using ast;
using ast_model.Symbols;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Lowers lambda expressions to generated closure classes + instantiation,
/// and rewrites function-value calls f(...) into f.Apply(...).
///
/// IMPORTANT: This introduces no new AST node kinds (uses existing ClassDef/ObjectInitializerExp/MemberAccessExp/FuncCallExp).
/// </summary>
public sealed class LambdaClosureConversionRewriter : DefaultAstRewriter
{
    private int closureCounter;
    private ModuleDef? currentModule;
    private readonly List<ClassDef> generatedClosureClasses = new();

    public override RewriteResult VisitModuleDef(ModuleDef ctx)
    {
        currentModule = ctx;
        closureCounter = 0;
        generatedClosureClasses.Clear();

        var rr = base.VisitModuleDef(ctx);
        var rebuilt = (ModuleDef)rr.Node;

        if (generatedClosureClasses.Count > 0)
        {
            var combined = rebuilt.Classes.Concat(generatedClosureClasses).ToList();
            rebuilt = rebuilt with { Classes = combined };
        }

        currentModule = null;
        return new RewriteResult(rebuilt, rr.Prologue);
    }

    public override RewriteResult VisitFuncCallExp(FuncCallExp ctx)
    {
        var rr = base.VisitFuncCallExp(ctx);
        var result = (FuncCallExp)rr.Node;

        // If this call resolved to a real FunctionDef, keep as-is.
        if (result.FunctionDef != null)
        {
            return rr;
        }

        if (result.Annotations == null || !result.Annotations.TryGetValue("FunctionName", out var fnObj) || fnObj is not string fn || string.IsNullOrWhiteSpace(fn))
        {
            return rr;
        }

        // If there is a variable/param with this name and it has a function type, rewrite:
        //   f(a,b)  ==>  f.Apply(a,b)
        if (!TryResolveCallableSymbol(result, fn, out var lhsVarRef))
        {
            return rr;
        }

        var applyCall = new FuncCallExp
        {
            FunctionDef = null,
            InvocationArguments = result.InvocationArguments,
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "Apply" },
            Location = result.Location,
            Parent = null,
            Type = result.Type
        };

        var memberAccess = new MemberAccessExp
        {
            LHS = lhsVarRef,
            RHS = applyCall,
            Annotations = result.Annotations,
            Location = result.Location,
            Parent = null,
            Type = result.Type
        };

        return new RewriteResult(memberAccess, rr.Prologue);
    }

    public override RewriteResult VisitLambdaExp(LambdaExp ctx)
    {
        // First rewrite inside the functor/apply body so nested lambdas are handled.
        var functorRR = Rewrite(ctx.FunctorDef);
        var functor = (FunctorDef)functorRR.Node;

        var apply = functor.InvocationFuncDev;

        // Compute captured variables by name (after VarRefResolver).
        var declaredNames = LambdaCaptureUtils.GetDeclaredNames(apply);
        var captureNames = LambdaCaptureUtils.GetCaptureNames(apply);

        // Preserve deterministic capture ordering.
        var orderedCaptures = captureNames.OrderBy(n => n, StringComparer.Ordinal).ToList();

        var closureClassName = GetFreshClosureClassName(ctx);

        // Build closure class members: fields for captures, ctor, and Apply method.
        var members = new List<MemberDef>();

        foreach (var capName in orderedCaptures)
        {
            // Prefer inferred type from a reference in the lambda body; fallback to 'object'.
            var capType = FindCapturedVarType(apply, capName);
            var capTypeName = TypeName.From(capType);

            members.Add(new FieldDef
            {
                Name = MemberName.From(capName),
                TypeName = capTypeName,
                CollectionType = CollectionType.SingleInstance,
                IsReadOnly = true,
                AccessConstraints = [],
                Annotations = [],
                Location = ctx.Location,
                Parent = null,
                Type = new FifthType.UnknownType { Name = TypeName.From("unknown") },
                Visibility = Visibility.Public
            });
        }

        if (orderedCaptures.Count > 0)
        {
            var typedCaps = orderedCaptures
                .Select(n => (Name: n, TypeName: FindCapturedVarType(apply, n)))
                .ToList();
            members.Add(BuildCaptureConstructor(ctx, closureClassName, typedCaps));
        }

        // Wrap Apply as a class method
        members.Add(new MethodDef
        {
            Name = MemberName.From("Apply"),
            TypeName = TypeName.From(apply.ReturnType.Name.Value),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = true,
            Annotations = [],
            Location = apply.Location,
            Parent = null,
            Type = new FifthType.UnknownType { Name = TypeName.From("unknown") },
            Visibility = Visibility.Public,
            FunctionDef = apply
        });

        var baseInterface = ComputeClosureInterfaceName(apply);

        var closureClass = new ClassDef
        {
            Name = TypeName.From(closureClassName),
            TypeParameters = [],
            MemberDefs = members,
            BaseClasses = baseInterface != null ? new List<string> { baseInterface } : [],
            AliasScope = null,
            Annotations = [],
            Location = ctx.Location,
            Parent = null,
            Type = new FifthType.UnknownType { Name = TypeName.From("unknown") },
            Visibility = Visibility.Public
        };

        generatedClosureClasses.Add(closureClass);

        // Replace the lambda expression with: new <closureClassName>(<captured values...>)
        var ctorArgs = new List<Expression>();
        foreach (var cap in orderedCaptures)
        {
            ctorArgs.Add(new VarRefExp
            {
                VarName = cap,
                VariableDecl = null,
                Annotations = [],
                Location = ctx.Location,
                Parent = null,
                Type = new FifthType.UnknownType { Name = TypeName.From("unknown") }
            });
        }

        var inst = new ObjectInitializerExp
        {
            TypeToInitialize = new FifthType.TType { Name = TypeName.From(closureClassName) },
            ConstructorArguments = ctorArgs,
            PropertyInitialisers = [],
            ResolvedConstructor = null,
            Annotations = [],
            Location = ctx.Location,
            Parent = null,
            Type = new FifthType.TType { Name = TypeName.From(closureClassName) }
        };

        var prologue = new List<Statement>();
        prologue.AddRange(functorRR.Prologue);
        return new RewriteResult(inst, prologue);
    }

    private string GetFreshClosureClassName(AstThing ctx)
    {
        var baseName = "__lambda_closure_" + closureCounter++;

        // Avoid collisions with existing classes.
        var existing = currentModule?.Classes.Any(c => c.Name.Value == baseName) == true;
        if (!existing)
        {
            return baseName;
        }

        var i = 0;
        while (true)
        {
            var name = baseName + "_" + i++;
            if (currentModule?.Classes.Any(c => c.Name.Value == name) != true)
            {
                return name;
            }
        }
    }

    private static string? ComputeClosureInterfaceName(FunctionDef apply)
    {
        if (apply.ReturnType is FifthType.TVoidType)
        {
            if (apply.Params.Count == 0)
            {
                return "Fifth.System.Runtime.IActionClosure";
            }

            var args = string.Join(", ", apply.Params.Select(p => MapFifthTypeNameForInterface(p.TypeName.Value)));
            return $"Fifth.System.Runtime.IActionClosure<{args}>";
        }

        // Non-void
        var ret = MapFifthTypeNameForInterface(apply.ReturnType.Name.Value);
        if (apply.Params.Count == 0)
        {
            return $"Fifth.System.Runtime.IClosure<{ret}>";
        }

        var inArgs = string.Join(", ", apply.Params.Select(p => MapFifthTypeNameForInterface(p.TypeName.Value)));
        return $"Fifth.System.Runtime.IClosure<{inArgs}, {ret}>";
    }

    private static string MapFifthTypeNameForInterface(string fifthTypeName)
    {
        // Keep as Fifth-friendly name; Roslyn translator will map primitives/collections.
        // For nested list syntax like [int], this is fine.
        return fifthTypeName;
    }

    private static string FindCapturedVarType(FunctionDef apply, string captureName)
    {
        foreach (var vr in FindVarRefs(apply.Body))
        {
            if (string.Equals(vr.VarName, captureName, StringComparison.Ordinal) && vr.Type != null)
            {
                return vr.Type.Name.Value;
            }
        }

        return "object";
    }

    private static IEnumerable<VarRefExp> FindVarRefs(AstThing node)
    {
        var stack = new Stack<AstThing>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            if (cur is VarRefExp vr)
            {
                yield return vr;
            }

            foreach (var child in GetChildren(cur))
            {
                stack.Push(child);
            }
        }
    }

    private static MethodDef BuildCaptureConstructor(LambdaExp lambda, string className, List<(string Name, string TypeName)> captures)
    {
        var ctorParams = new List<ParamDef>();
        foreach (var cap in captures)
        {
            ctorParams.Add(new ParamDef
            {
                Name = cap.Name,
                TypeName = TypeName.From(cap.TypeName),
                CollectionType = CollectionType.SingleInstance,
                Visibility = Visibility.Public,
                DestructureDef = null,
                ParameterConstraint = null,
                Annotations = [],
                Location = lambda.Location,
                Parent = null,
                Type = new FifthType.UnknownType { Name = TypeName.From("unknown") }
            });
        }

        var stmts = new List<Statement>();
        foreach (var cap in captures)
        {
            stmts.Add(new AssignmentStatement
            {
                LValue = new VarRefExp
                {
                    VarName = cap.Name,
                    VariableDecl = null,
                    Annotations = [],
                    Location = lambda.Location,
                    Parent = null,
                    Type = new FifthType.UnknownType { Name = TypeName.From("unknown") }
                },
                RValue = new VarRefExp
                {
                    VarName = cap.Name,
                    VariableDecl = null,
                    Annotations = [],
                    Location = lambda.Location,
                    Parent = null,
                    Type = new FifthType.UnknownType { Name = TypeName.From("unknown") }
                },
                Annotations = [],
                Location = lambda.Location,
                Parent = null,
                Type = new FifthType.UnknownType { Name = TypeName.From("unknown") }
            });
        }

        var ctorFunc = new FunctionDef
        {
            TypeParameters = [],
            Params = ctorParams,
            Body = new BlockStatement
            {
                Statements = stmts,
                Annotations = [],
                Location = lambda.Location,
                Parent = null,
                Type = new FifthType.UnknownType { Name = TypeName.From("unknown") }
            },
            ReturnType = new FifthType.TVoidType { Name = TypeName.From("void") },
            Name = MemberName.From(className),
            IsStatic = false,
            IsConstructor = true,
            BaseCall = null,
            Annotations = [],
            Location = lambda.Location,
            Parent = null,
            Type = new FifthType.UnknownType { Name = TypeName.From("unknown") },
            Visibility = Visibility.Public
        };

        return new MethodDef
        {
            Name = MemberName.From(className),
            TypeName = TypeName.From("void"),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = true,
            Annotations = [],
            Location = lambda.Location,
            Parent = null,
            Type = new FifthType.UnknownType { Name = TypeName.From("unknown") },
            Visibility = Visibility.Public,
            FunctionDef = ctorFunc
        };
    }

    private static IEnumerable<AstThing> GetChildren(AstThing node)
    {
        return node switch
        {
            AssemblyDef a => a.Modules,
            ModuleDef m => m.Classes.Concat<AstThing>(m.Functions),
            ClassDef c => c.MemberDefs,
            MethodDef md => new AstThing[] { md.FunctionDef },
            FunctionDef f => f.Params.Concat<AstThing>(new AstThing[] { f.Body }),
            BlockStatement b => b.Statements,
            VarDeclStatement vds => new AstThing[] { vds.VariableDecl }.Concat(vds.InitialValue != null ? new AstThing[] { vds.InitialValue } : Array.Empty<AstThing>()),
            ExpStatement es => new AstThing[] { es.RHS },
            ReturnStatement rs => new AstThing[] { rs.ReturnValue },
            AssignmentStatement a => new AstThing[] { a.LValue, a.RValue },
            IfElseStatement i => new AstThing[] { i.Condition, i.ThenBlock, i.ElseBlock },
            WhileStatement w => new AstThing[] { w.Condition, w.Body },
            ForeachStatement fe => new AstThing[] { fe.LoopVariable, fe.Collection, fe.Body },
            BinaryExp be => new AstThing[] { be.LHS, be.RHS },
            UnaryExp ue => new AstThing[] { ue.Operand },
            MemberAccessExp ma => new AstThing[] { ma.LHS }.Concat(ma.RHS != null ? new AstThing[] { ma.RHS } : Array.Empty<AstThing>()),
            FuncCallExp fc => fc.InvocationArguments,
            LambdaExp le => new AstThing[] { le.FunctorDef },
            FunctorDef fd => new AstThing[] { fd.InvocationFuncDev },
            ListLiteral ll => ll.ElementExpressions,
            ListComprehension lc => new AstThing[] { lc.Projection, lc.Source }.Concat(lc.Constraints),
            IndexerExpression ie => new AstThing[] { ie.IndexExpression, ie.OffsetExpression },
            ObjectInitializerExp oi => oi.ConstructorArguments.Concat<AstThing>(oi.PropertyInitialisers),
            PropertyInitializerExp pi => new AstThing[] { pi.RHS },
            _ => Array.Empty<AstThing>()
        };
    }

    private static bool TryResolveCallableSymbol(AstThing callSite, string name, out VarRefExp varRef)
    {
        varRef = null!;

        // Resolve by name at the call site scope.
        var scope = callSite.NearestScope();
        if (scope == null)
        {
            return false;
        }

        if (!scope.TryResolveByName(name, out var entry) || entry == null)
        {
            return false;
        }

        // Only allow variable-like symbols.
        if (!LambdaCaptureUtils.IsVariableLike(entry.Symbol.Kind))
        {
            return false;
        }

        // Must look like a function type.
        if (!TryGetTypeName(entry.OriginatingAstThing, out var typeName) || !LooksLikeFunctionType(typeName))
        {
            return false;
        }

        varRef = new VarRefExp
        {
            VarName = name,
            VariableDecl = entry.OriginatingAstThing as VariableDecl,
            Annotations = [],
            Location = callSite.Location,
            Parent = null,
            Type = new FifthType.UnknownType { Name = TypeName.From("unknown") }
        };

        return true;
    }

    private static bool LooksLikeFunctionType(string typeName) => typeName.Contains("->", StringComparison.Ordinal);

    private static bool TryGetTypeName(IAstThing astThing, out string typeName)
    {
        switch (astThing)
        {
            case VariableDecl vd:
                typeName = vd.TypeName.Value;
                return true;
            case ParamDef pd:
                typeName = pd.TypeName.Value;
                return true;
            case PropertyBindingDef pb:
                // property bindings inherit property type; not supported here
                typeName = string.Empty;
                return false;
            default:
                typeName = string.Empty;
                return false;
        }
    }
}
