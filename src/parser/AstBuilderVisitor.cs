using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ast;
using ast_generated;
using ast_model.TypeSystem;
using Operator = ast.Operator;

namespace compiler.LangProcessingPhases;

public class AstBuilderVisitor : FifthParserBaseVisitor<IAstThing>
{
    public static readonly FifthType Void = new FifthType.TVoidType() { Name = TypeName.From("void") };

    #region Helper Functions

    private TAstType CreateLiteral<TAstType, TBaseType>(ParserRuleContext ctx, Func<string, TBaseType> x)
        where TAstType : LiteralExpression<TBaseType>, new()
    {
        return new TAstType()
        {
            Annotations = [],
            Location = GetLocationDetails(ctx),
            Parent = null,
            Type = new FifthType.TDotnetType(typeof(TBaseType)) { Name = TypeName.From(typeof(TBaseType).FullName) },
            Value = x(ctx.GetText())
        };
    }

    private SourceLocationMetadata GetLocationDetails(ParserRuleContext context)
    {
        var text = context.GetText();
        var len = Math.Min(text.Length, 10);
        return new SourceLocationMetadata(
            context.Start.Column,
            string.Empty,
            context.Start.Line,
            text[..len]
        );
    }

    private FifthType ResolveTypeFromName(string typeName)
    {
        // Ensure the TypeRegistry is initialized with primitive types
        TypeRegistry.DefaultRegistry.RegisterPrimitiveTypes();

        // Create a mapping from Fifth language type names to .NET type names
        var typeNameMapping = new Dictionary<string, string>
        {
            { "int", "Int32" },
            { "float", "Single" },
            { "double", "Double" },
            { "bool", "Boolean" },
            { "string", "String" },
            { "byte", "Byte" },
            { "char", "Char" },
            { "long", "Int64" },
            { "short", "Int16" },
            { "decimal", "Decimal" }
        };

        // Try to map the language type name to .NET type name
        string dotnetTypeName = typeNameMapping.TryGetValue(typeName, out string mappedName) ? mappedName : typeName;

        // Try to resolve the type name using the TypeRegistry
        if (TypeRegistry.DefaultRegistry.TryGetTypeByName(dotnetTypeName, out FifthType resolvedType) && resolvedType != null)
        {
            return resolvedType;
        }

        // Fall back to UnknownType if the type cannot be resolved
        return new FifthType.UnknownType() { Name = TypeName.From(typeName) };
    }

    #endregion Helper Functions

    public override IAstThing VisitAssignment_statement([NotNull] FifthParser.Assignment_statementContext context)
    {
        var b = new AssignmentStatementBuilder()
            .WithAnnotations([])
            .WithLValue((Expression)Visit(context.lvalue))
            .WithRValue((Expression)Visit(context.rvalue));
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExpression_statement([NotNull] FifthParser.Expression_statementContext context)
    {
        var exprCtx = context.expression();
        if (exprCtx != null)
        {
            var expr = (Expression)Visit(exprCtx);
            return new ExpStatement
            {
                Annotations = [],
                RHS = expr,
                Location = GetLocationDetails(context),
                Type = Void
            };
        }
        // Represent empty ';' as a no-op expression statement
        var noop = new Int32LiteralExp
        {
            Annotations = [],
            Location = GetLocationDetails(context),
            Parent = null,
            Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From(typeof(int).FullName) },
            Value = 0
        };
        return new ExpStatement
        {
            Annotations = [],
            RHS = noop,
            Location = GetLocationDetails(context),
            Type = Void
        };
    }

    public override IAstThing VisitBlock(FifthParser.BlockContext context)
    {
        var b = new BlockStatementBuilder()
            .WithAnnotations([]);

        foreach (var stmt in context.statement())
        {
            b.AddingItemToStatements((Statement)Visit(stmt));
        }

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitGraphAssertionBlock(FifthParser.GraphAssertionBlockContext context)
    {
        // Build a BlockStatement from inner statements
        var inner = new List<Statement>();
        foreach (var s in context.statement())
        {
            inner.Add((Statement)Visit(s));
        }

        var block = new BlockStatement
        {
            Annotations = [],
            Statements = inner,
            Location = GetLocationDetails(context),
            Type = Void
        };

        var exp = new GraphAssertionBlockExp
        {
            Annotations = [],
            Content = block,
            Location = GetLocationDetails(context),
            Type = null
        };
        return exp;
    }

    public override IAstThing VisitGraph_assertion_statement(FifthParser.Graph_assertion_statementContext context)
    {
        var exp = (GraphAssertionBlockExp)VisitGraphAssertionBlock(context.graphAssertionBlock());
        var stmt = new GraphAssertionBlockStatement
        {
            Annotations = [],
            Content = exp,
            Location = GetLocationDetails(context),
            Type = Void
        };
        return stmt;
    }

    public override IAstThing VisitClass_definition(FifthParser.Class_definitionContext context)
    {
        var b = new ClassDefBuilder();
        foreach (var fctx in context._functions)
        {
            var f = Visit(fctx);
            b.AddingItemToMemberDefs((MemberDef)f);
        }

        foreach (var pctx in context._properties)
        {
            var prop = Visit(pctx);
            b.AddingItemToMemberDefs((MemberDef)prop);
        }

        b.WithVisibility(Visibility.Public);
        b.WithName(TypeName.From(context.name.Text));
        b.WithAnnotations([]);
        // Set optional features prior to building so they appear in the result
        if (context.superClass is not null)
        {
            b.AddingItemToBaseClasses(context.superClass.GetText());
        }

        if (context.aliasScope is not null)
        {
            b.WithAliasScope(context.aliasScope.GetText());
        }

        var built = b.Build();
        var result = built with
        {
            Location = GetLocationDetails(context),
            Type = new FifthType.TType() { Name = TypeName.From(context.name.Text) }
        };
        return result;
    }

    public override IAstThing VisitDeclaration([NotNull] FifthParser.DeclarationContext context)
    {
        var b = new VarDeclStatementBuilder()
                        .WithAnnotations([]);
        b.WithVariableDecl((VariableDecl)VisitVar_decl(context.var_decl()));
        if (context.expression() is not null)
        {
            Console.WriteLine($"DEBUG: VisitDeclaration found expression context: {context.expression().GetType().Name}");
            var exp = context.expression();
            var e = base.Visit(exp);
            Console.WriteLine($"DEBUG: VisitDeclaration visited expression, result type: {e?.GetType().Name ?? "null"}");
            b.WithInitialValue((Expression)e);
        }
        else
        {
            Console.WriteLine("DEBUG: VisitDeclaration - no expression found in context");
        }
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitDestructure_binding([NotNull] FifthParser.Destructure_bindingContext context)
    {
        var b = new PropertyBindingDefBuilder()
            .WithAnnotations([])
            .WithVisibility(Visibility.Public)
            .WithIntroducedVariable(MemberName.From(context.name.Text))
            .WithReferencedPropertyName(MemberName.From(context.propname.Text));
        if (context.destructuring_decl() is not null)
        {
            b.WithDestructureDef((ParamDestructureDef)VisitDestructuring_decl(context.destructuring_decl()));
        }

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };

        // Set constraint if present (WithConstraint method not generated for nullable properties)
        if (context.variable_constraint() is not null)
        {
            result = result with { Constraint = (Expression)Visit(context.variable_constraint()) };
        }

        return result;
    }

    public override IAstThing VisitDestructuring_decl([NotNull] FifthParser.Destructuring_declContext context)
    {
        var b = new ParamDestructureDefBuilder()
            .WithAnnotations([])
            .WithVisibility(Visibility.Public);
        foreach (var pb in context._bindings)
        {
            b.AddingItemToBindings((PropertyBindingDef)VisitDestructure_binding(pb));
        }
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_add(FifthParser.Exp_addContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);
        var op = context.add_op.Type switch
        {
            FifthParser.PLUS => Operator.ArithmeticAdd,
            FifthParser.MINUS => Operator.ArithmeticSubtract,
            FifthParser.OR => Operator.BitwiseOr,
            FifthParser.LOGICAL_XOR => Operator.LogicalXor,
            _ => Operator.ArithmeticAdd
        };

        // ANTLR visitor dispatch issue workaround: manually create FuncCallExp when needed
        var lhs = context.lhs is FifthParser.Exp_funccallContext lhsFunc
            ? CreateFuncCallExp(lhsFunc)
            : (Expression)Visit(context.lhs);

        var rhs = context.rhs is FifthParser.Exp_funccallContext rhsFunc
            ? CreateFuncCallExp(rhsFunc)
            : (Expression)Visit(context.rhs);

        b.WithOperator(op)
            .WithLHS(lhs)
            .WithRHS(rhs)
            ;

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_funccall([NotNull] FifthParser.Exp_funccallContext context)
    {
        // Build a FuncCallExp with arguments and stash the name for resolution in linkage
        var functionName = context.funcname.Text;
        var arguments = new List<Expression>();

        if (context.expressionList() != null)
        {
            foreach (var exp in context.expressionList()._expressions)
            {
                arguments.Add((Expression)Visit(exp));
            }
        }

        return new FuncCallExp
        {
            FunctionDef = null,
            InvocationArguments = arguments,
            Annotations = new Dictionary<string, object> { ["FunctionName"] = functionName },
            Location = GetLocationDetails(context),
            Parent = null,
            Type = null
        };
    }

    private FuncCallExp CreateFuncCallExp(FifthParser.Exp_funccallContext context)
    {
        var functionName = context.funcname.Text;
        var arguments = new List<Expression>();

        // Handle expression list manually to avoid type issues
        if (context.expressionList() != null)
        {
            foreach (var exp in context.expressionList()._expressions)
            {
                arguments.Add((Expression)Visit(exp));
            }
        }

        return new FuncCallExp()
        {
            FunctionDef = null, // Will be resolved during linking phase
            InvocationArguments = arguments,
            // Store the function name in annotations temporarily
            Annotations = new Dictionary<string, object> { ["FunctionName"] = functionName },
            Location = GetLocationDetails(context),
            Parent = null,
            Type = null // Will be inferred later
        };
    }

    public override IAstThing VisitExp_and(FifthParser.Exp_andContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);
        b.WithOperator(Operator.LogicalAnd)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_exp(FifthParser.Exp_expContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);

        b.WithOperator(Operator.ArithmeticPow)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_member_access([NotNull] FifthParser.Exp_member_accessContext context)
    {
        var b = new MemberAccessExpBuilder()
            .WithAnnotations([]);
        b.WithLHS((Expression)Visit(context.lhs));
        if (context.rhs is not null)
            b.WithRHS((Expression)Visit(context.rhs));
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_mul(FifthParser.Exp_mulContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);
        var op = context.mul_op.Type switch
        {
            FifthParser.LSHIFT => Operator.BitwiseLeftShift,
            FifthParser.RSHIFT => Operator.BitwiseRightShift,
            FifthParser.AMPERSAND => Operator.BitwiseAnd,
            FifthParser.STAR => Operator.ArithmeticMultiply,
            FifthParser.DIV => Operator.ArithmeticDivide,
            FifthParser.MOD => Operator.ArithmeticMod,
            FifthParser.STAR_STAR => Operator.ArithmeticPow,
            _ => Operator.ArithmeticMultiply
        };
        b.WithOperator(op)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_or(FifthParser.Exp_orContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);
        b.WithOperator(Operator.LogicalOr)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_rel(FifthParser.Exp_relContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);
        var op = context.rel_op.Type switch
        {
            FifthParser.EQUALS => Operator.Equal,
            FifthParser.NOT_EQUALS => Operator.NotEqual,
            FifthParser.LESS => Operator.LessThan,
            FifthParser.LESS_OR_EQUALS => Operator.LessThanOrEqual,
            FifthParser.GREATER => Operator.GreaterThan,
            FifthParser.GREATER_OR_EQUALS => Operator.GreaterThanOrEqual,
            _ => Operator.Equal
        };
        b.WithOperator(op)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_unary(FifthParser.Exp_unaryContext context)
    {
        var b = new UnaryExpBuilder()
            .WithAnnotations([]);
        var op = context.unary_op.Type switch
        {
            FifthParser.PLUS => Operator.ArithmeticAdd,
            FifthParser.MINUS => Operator.ArithmeticNegative,
            FifthParser.LOGICAL_NOT => Operator.LogicalNot,
            _ => Operator.ArithmeticAdd
        };
        b.WithOperator(op)
            .WithOperand((Expression)Visit(context.expression()));

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_operand([NotNull] FifthParser.Exp_operandContext context)
    {
        Console.WriteLine($"DEBUG: VisitExp_operand called, operand type: {context.operand().GetType().Name}");
        var operandContext = context.operand();

        // Check what type of operand this is and route appropriately
        if (operandContext.object_instantiation_expression() != null)
        {
            Console.WriteLine("DEBUG: Found object_instantiation_expression in operand, calling base.Visit");
            try
            {
                var objInstContext = operandContext.object_instantiation_expression();
                Console.WriteLine($"DEBUG: About to visit object instantiation context of type: {objInstContext.GetType().Name}");
                var result = base.Visit(objInstContext);
                Console.WriteLine($"DEBUG: base.Visit returned: {result?.GetType().Name ?? "null"}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception in base.Visit: {ex.Message}");
                return null;
            }
        }
        else if (operandContext.literal() != null)
        {
            Console.WriteLine("DEBUG: Found literal in operand");
        }
        else if (operandContext.var_name() != null)
        {
            Console.WriteLine("DEBUG: Found var_name in operand");
        }
        else if (operandContext.list() != null)
        {
            Console.WriteLine("DEBUG: Found list in operand");
        }
        else if (operandContext.L_PAREN() != null && operandContext.R_PAREN() != null)
        {
            Console.WriteLine("DEBUG: Found parenthesized expression in operand; visiting inner expression");
            return Visit(operandContext.expression());
        }
        else
        {
            Console.WriteLine("DEBUG: Found other operand type");
        }

        return Visit(operandContext);
    }

    public override IAstThing VisitFifth([NotNull] FifthParser.FifthContext context)
    {
        var b = new AssemblyDefBuilder();
        b.WithVisibility(Visibility.Public)
            .WithPublicKeyToken("abc123") // TODO: need ways to define this
            .WithAnnotations([])
            .WithAssemblyRefs([])
            .WithName(AssemblyName.anonymous)
            .WithVersion("0.0.0.0")
            ;
        var mb = new ModuleDefBuilder();
        if (context._classes.Count == 0)
        {
            mb.WithClasses([]);
        }
        else
        {
            foreach (var @class in context._classes)
            {
                mb.AddingItemToClasses((ClassDef)Visit(@class));
            }
        }

        if (context._functions.Count == 0)
        {
            mb.WithFunctions([]);
        }
        else
        {
            foreach (var @func in context._functions)
            {
                mb.AddingItemToFunctions((FunctionDef)Visit(@func));
            }
        }

        // Build the module so we can attach annotations for store declarations
        var module = mb.Build();
        // Ensure annotations dictionary is initialized (builder may leave it null)
        if (module.Annotations == null)
        {
            module = module with { Annotations = new Dictionary<string, object>() };
        }

        // Parse store declarations: STORE <name> = sparql_store(<iri>);
        // Attach to module annotations for downstream phases
        try
        {
            var storeDecls = context.store_decl();
            if (storeDecls != null && storeDecls.Length > 0)
            {
                var stores = new Dictionary<string, string>(StringComparer.Ordinal);
                string defaultStore = null;
                foreach (var s in storeDecls)
                {
                    var name = s.store_name?.Text ?? "";
                    var uri = s.iri()?.GetText() ?? "";
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(uri))
                    {
                        stores[name] = uri;
                        defaultStore ??= name; // First declared becomes default
                    }
                }

                if (stores.Count > 0)
                {
                    module.Annotations["GraphStores"] = stores;
                    if (defaultStore == null)
                    {
                        foreach (var k in stores.Keys)
                        {
                            defaultStore = k;
                            break;
                        }
                    }
                    module.Annotations["DefaultGraphStore"] = defaultStore;
                }
            }
        }
        catch (System.Exception ex)
        {
            Console.Error.WriteLine($"DEBUG: Failed to capture store declarations: {ex.Message}");
        }

        b.AddingItemToModules(module);

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitFunction_declaration(FifthParser.Function_declarationContext context)
    {
        var returnType = ResolveTypeFromName(context.result_type.GetText());

        var b = new FunctionDefBuilder();
        b.WithName(MemberName.From(context.name.GetText()))
            .WithBody((BlockStatement)VisitBlock(context.body.block()))
            .WithReturnType(returnType)
            .WithAnnotations([])
            .WithVisibility(Visibility.Public) // todo: grammar needs support for member visibility
            ;
        foreach (var paramdeclContext in context.paramdecl())
        {
            b.AddingItemToParams((ParamDef)VisitParamdecl(paramdeclContext));
        }

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitIf_statement([NotNull] FifthParser.If_statementContext context)
    {
        var b = new IfElseStatementBuilder();
        b.WithAnnotations([])
            .WithCondition((Expression)Visit(context.condition));

        var thenAst = Visit(context.ifpart);
        var thenBlock = thenAst as BlockStatement ?? new BlockStatement
        {
            Annotations = [],
            Statements = [(Statement)thenAst],
            Location = GetLocationDetails(context.ifpart),
            Type = Void
        };
        b.WithThenBlock(thenBlock);

        if (context.elsepart is not null)
        {
            var elseAst = Visit(context.elsepart);
            var elseBlock = elseAst as BlockStatement ?? new BlockStatement
            {
                Annotations = [],
                Statements = [(Statement)elseAst],
                Location = GetLocationDetails(context.elsepart),
                Type = Void
            };
            b.WithElseBlock(elseBlock);
        }
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitList([NotNull] FifthParser.ListContext context)
    {
        return base.VisitList_body(context.list_body());
    }

    public override IAstThing VisitList_body([NotNull] FifthParser.List_bodyContext context)
    {
        if (context.list_literal() is not null)
        {
            return VisitList_literal(context.list_literal());
        }
        else if (context.list_comprehension() is not null)
        {
            return VisitList_comprehension(context.list_comprehension());
        }
        return base.VisitList_body(context);
    }

    public override IAstThing VisitList_comprehension([NotNull] FifthParser.List_comprehensionContext context)
    {
        var b = new ListComprehensionBuilder()
            .WithAnnotations([]);
        b.WithMembershipConstraint((Expression)Visit(context.constraint))
            .WithVarName(context.var_name().GetText())
            .WithSourceName(context.source.GetText());
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitList_literal([NotNull] FifthParser.List_literalContext context)
    {
        var b = new ListLiteralBuilder()
            .WithAnnotations([]);
        foreach (var exp in context.expressionList()._expressions)
        {
            b.AddingItemToElementExpressions((Expression)Visit(exp));
        }
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitLit_bool(FifthParser.Lit_boolContext context)
    {
        return CreateLiteral<BooleanLiteralExp, bool>(context, bool.Parse);
    }

    public override IAstThing VisitLit_float(FifthParser.Lit_floatContext context)
    {
        if (context.GetText().EndsWith("d", StringComparison.InvariantCultureIgnoreCase))
        {
            return CreateLiteral<Float8LiteralExp, double>(context, s => double.Parse(s.Substring(0, s.Length - 1)));
        }

        if (context.GetText().EndsWith("c", StringComparison.InvariantCultureIgnoreCase))
        {
            return CreateLiteral<Float16LiteralExp, decimal>(context, decimal.Parse);
        }

        return CreateLiteral<Float4LiteralExp, float>(context, float.Parse);
    }

    public override IAstThing VisitLit_int(FifthParser.Lit_intContext context)
    {
        // Delegate to specific number literal handlers based on actual child alternative
        var intCtx = context.integer();
        if (intCtx is FifthParser.Num_decimalContext dec)
        {
            return VisitNum_decimal(dec);
        }
        if (intCtx is FifthParser.Num_binaryContext bin)
        {
            return VisitNum_binary(bin);
        }
        if (intCtx is FifthParser.Num_octalContext oct)
        {
            return VisitNum_octal(oct);
        }
        if (intCtx is FifthParser.Num_hexContext hex)
        {
            return VisitNum_hex(hex);
        }
        if (intCtx is FifthParser.Num_imaginaryContext img)
        {
            return VisitNum_imaginary(img);
        }
        // Fallback
        return CreateLiteral<Int32LiteralExp, int>(context, int.Parse);
    }

    public override IAstThing VisitLit_string(FifthParser.Lit_stringContext context)
    {
        return CreateLiteral<StringLiteralExp, string>(context, x => x);
    }

    public override IAstThing VisitNum_decimal(FifthParser.Num_decimalContext context)
    {
        return context.suffix?.Type switch
        {
            FifthParser.SUF_SHORT => CreateLiteral<Int16LiteralExp, short>(context, short.Parse),
            FifthParser.SUF_LONG => CreateLiteral<Int64LiteralExp, long>(context, long.Parse),
            _ => CreateLiteral<Int32LiteralExp, int>(context, int.Parse)
        };
    }

    public override IAstThing VisitNum_binary(FifthParser.Num_binaryContext context)
    {
        var text = context.GetText();
        // Strip 0b/0B and underscores
        var payload = text.StartsWith("0b", StringComparison.OrdinalIgnoreCase)
            ? text.Substring(2)
            : text;
        payload = payload.Replace("_", string.Empty);
        return new Int32LiteralExp
        {
            Annotations = [],
            Location = GetLocationDetails(context),
            Parent = null,
            Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From(typeof(int).FullName) },
            Value = Convert.ToInt32(payload, 2)
        };
    }

    public override IAstThing VisitNum_octal(FifthParser.Num_octalContext context)
    {
        var text = context.GetText();
        // Strip leading 0 or 0o/0O and underscores
        var payload = text.StartsWith("0o", StringComparison.OrdinalIgnoreCase) ? text.Substring(2) : text;
        if (payload.Length > 0 && payload[0] == '0') payload = payload.Substring(1);
        payload = payload.Replace("_", string.Empty);
        return new Int32LiteralExp
        {
            Annotations = [],
            Location = GetLocationDetails(context),
            Parent = null,
            Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From(typeof(int).FullName) },
            Value = Convert.ToInt32(payload.Length == 0 ? "0" : payload, 8)
        };
    }

    public override IAstThing VisitNum_hex(FifthParser.Num_hexContext context)
    {
        var text = context.GetText();
        // Strip 0x/0X and underscores
        var payload = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? text.Substring(2)
            : text;
        payload = payload.Replace("_", string.Empty);
        return new Int32LiteralExp
        {
            Annotations = [],
            Location = GetLocationDetails(context),
            Parent = null,
            Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From(typeof(int).FullName) },
            Value = Convert.ToInt32(payload, 16)
        };
    }

    public override IAstThing VisitNum_imaginary(FifthParser.Num_imaginaryContext context)
    {
        var text = context.GetText();
        if (string.IsNullOrWhiteSpace(text))
        {
            return new Int32LiteralExp
            {
                Annotations = [],
                Location = GetLocationDetails(context),
                Parent = null,
                Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From(typeof(int).FullName) },
                Value = 0
            };
        }

        // Strip imaginary suffix 'i'/'I' and underscores
        var trimmed = text.EndsWith("i", StringComparison.OrdinalIgnoreCase)
            ? text.Substring(0, text.Length - 1)
            : text;
        trimmed = trimmed.Replace("_", string.Empty);

        int value;
        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            value = Convert.ToInt32(trimmed.Substring(2), 16);
        }
        else if (trimmed.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            value = Convert.ToInt32(trimmed.Substring(2), 2);
        }
        else if (trimmed.StartsWith("0o", StringComparison.OrdinalIgnoreCase))
        {
            value = Convert.ToInt32(trimmed.Substring(2), 8);
        }
        else
        {
            // Decimal fallback; handle leading 0 for octal-like forms as decimal here
            value = int.Parse(trimmed);
        }

        return new Int32LiteralExp
        {
            Annotations = [],
            Location = GetLocationDetails(context),
            Parent = null,
            Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From(typeof(int).FullName) },
            Value = value
        };
    }

    public override IAstThing VisitParamdecl(FifthParser.ParamdeclContext context)
    {
        var b = new ParamDefBuilder()
                .WithVisibility(Visibility.Public)
                .WithAnnotations([])
                .WithName(context.var_name().GetText())
                .WithTypeName(TypeName.From(context.type_name().GetText()))
            ;
        if (context.destructuring_decl() is not null)
        {
            var destructuringDef = VisitDestructuring_decl(context.destructuring_decl());
            b.WithDestructureDef((ParamDestructureDef)destructuringDef);
        }

        if (context.variable_constraint() is not null)
        {
            var constraint = Visit(context.variable_constraint()) as Expression;
            if (constraint is not null)
            {
                b.WithParameterConstraint(constraint);
            }
        }

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitProperty_declaration(FifthParser.Property_declarationContext context)
    {
        var b = new PropertyDefBuilder();
        b.WithName(MemberName.From(context.name.Text))
         .WithTypeName(TypeName.From(context.type.Text))
         .WithVisibility(Visibility.Public)
         .WithAccessConstraints([AccessConstraint.None])
         .WithIsReadOnly(false)
         .WithIsWriteOnly(false);
        // todo:  There's a lot more detail that could be filled in here, and a lot more
        // sophistication needed in the grammar of the decl
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitReturn_statement([NotNull] FifthParser.Return_statementContext context)
    {
        var returnExpr = (Expression)Visit(context.expression());

        var b = new ReturnStatementBuilder()
            .WithAnnotations([])
            .WithReturnValue(returnExpr);
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };

        return result;
    }

    public override IAstThing VisitVar_decl(FifthParser.Var_declContext context)
    {
        var b = new VariableDeclBuilder()
            .WithAnnotations([]);
        b.WithName(context.var_name().GetText());

        if (context.type_name() is not null)
        {
            b.WithTypeName(TypeName.From(context.type_name().GetText()));
            b.WithCollectionType(CollectionType.SingleInstance);
        }
        else if (context.list_type_signature() is not null)
        {
            b.WithTypeName(TypeName.From(context.list_type_signature().type_name().GetText()));
            b.WithCollectionType(CollectionType.List);
        }
        else if (context.array_type_signature() is not null)
        {
            b.WithTypeName(TypeName.From(context.array_type_signature().type_name().GetText()));
            b.WithCollectionType(CollectionType.Array);
        }
        else if (context.generic_type_signature() is not null)
        {
            // Handle generic syntax like list<int>
            var genericName = context.generic_type_signature().generic_name?.Text ?? string.Empty;
            var innerType = context.generic_type_signature().inner?.GetText() ?? "object";

            if (string.Equals(genericName, "list", StringComparison.OrdinalIgnoreCase))
            {
                b.WithTypeName(TypeName.From(innerType));
                b.WithCollectionType(CollectionType.List);
            }
            else if (string.Equals(genericName, "array", StringComparison.OrdinalIgnoreCase))
            {
                b.WithTypeName(TypeName.From(innerType));
                b.WithCollectionType(CollectionType.Array);
            }
            else
            {
                // Fallback: treat as single instance of the inner type if unknown generic
                b.WithTypeName(TypeName.From(innerType));
                b.WithCollectionType(CollectionType.SingleInstance);
            }
        }
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitVar_name(FifthParser.Var_nameContext context)
    {
        var b = new VarRefExpBuilder()
            .WithVarName(context.GetText())
            .WithAnnotations([]);
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitVariable_constraint([NotNull] FifthParser.Variable_constraintContext context)
    {
        return base.Visit(context.constraint);
    }

    public override IAstThing VisitWhile_statement([NotNull] FifthParser.While_statementContext context)
    {
        var b = new WhileStatementBuilder();
        b.WithAnnotations([])
            .WithCondition((Expression)Visit(context.condition));

        var bodyAst = Visit(context.looppart);
        var bodyBlock = bodyAst as BlockStatement ?? new BlockStatement
        {
            Annotations = [],
            Statements = [(Statement)bodyAst],
            Location = GetLocationDetails(context.looppart),
            Type = Void
        };
        b.WithBody(bodyBlock);
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    // Handle indexing expressions: numbers[0]
    public override IAstThing VisitExp_index([NotNull] FifthParser.Exp_indexContext context)
    {
        // Represent index access as a MemberAccessExp with RHS being the index expression in annotations
        var target = (Expression)Visit(context.lhs);
        var indexCtx = context.index();
        var indexExpr = (Expression)Visit(indexCtx.expression());

        // Create a temporary VarRefExp for the index and attach metadata to LHS for later lowering phases
        var synthetic = new VarRefExp { VarName = "[index]", Annotations = new Dictionary<string, object> { ["IndexExpression"] = indexExpr }, Location = GetLocationDetails(context), Type = Void };

        var b = new MemberAccessExpBuilder().WithAnnotations([]).WithLHS(target).WithRHS(synthetic);
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitObject_instantiation_expression([NotNull] FifthParser.Object_instantiation_expressionContext context)
    {
        Console.WriteLine($"DEBUG: FINALLY ENTERING VisitObject_instantiation_expression!!!");
        Console.WriteLine($"DEBUG: Context type: {context?.GetType().Name ?? "null"}");
        Console.WriteLine($"DEBUG: Type name method exists: {context?.type_name() != null}");

        if (context?.type_name() != null)
        {
            Console.WriteLine($"DEBUG: Type name: {context.type_name().GetText()}");
        }

        // Extract the type name
        var typeName = context.type_name()?.GetText() ?? "object";
        Console.WriteLine($"DEBUG: Creating ObjectInitializerExp for type: {typeName}");

        // Create the type reference
        var typeToInitialize = new FifthType.TType() { Name = TypeName.From(typeName) };

        // Extract property initializers
        var propertyInitializers = new List<PropertyInitializerExp>();
        var propertyAssignments = context.initialiser_property_assignment();
        if (propertyAssignments != null && propertyAssignments.Length > 0)
        {
            Console.WriteLine($"DEBUG: Found {propertyAssignments.Length} property initializers");
            foreach (var propContext in propertyAssignments)
            {
                Console.WriteLine($"DEBUG: Processing property assignment context: {propContext?.GetType().Name ?? "null"}");

                // Try to explicitly cast to the specific context type and call the right visitor method
                if (propContext is FifthParser.Initialiser_property_assignmentContext propAssignmentContext)
                {
                    Console.WriteLine($"DEBUG: Processing property assignment for proper dispatch");
                    try
                    {
                        // Use direct method call since it's working now
                        var propResult = VisitInitialiser_property_assignment(propAssignmentContext);
                        Console.WriteLine($"DEBUG: VisitInitialiser_property_assignment returned: {propResult?.GetType().Name ?? "null"}");

                        var propInit = propResult as PropertyInitializerExp;
                        if (propInit != null)
                        {
                            propertyInitializers.Add(propInit);
                            Console.WriteLine($"DEBUG: Added property initializer for: {propInit.PropertyToInitialize?.Property?.Name.Value ?? "unknown"}");
                        }
                        else
                        {
                            Console.WriteLine($"DEBUG: Could not cast to PropertyInitializerExp. Actual type: {propResult?.GetType().Name ?? "null"}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DEBUG: Exception: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"DEBUG: Cast to Initialiser_property_assignmentContext failed");
                    Console.WriteLine($"DEBUG: Using base Visit method");
                    var visitResult = Visit(propContext);
                    Console.WriteLine($"DEBUG: Visit result type: {visitResult?.GetType().Name ?? "null"}");
                    var propInit = visitResult as PropertyInitializerExp;
                    if (propInit != null)
                    {
                        propertyInitializers.Add(propInit);
                        Console.WriteLine($"DEBUG: Added property initializer for: {propInit.PropertyToInitialize?.Property?.Name.Value ?? "unknown"}");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: Failed to cast visit result to PropertyInitializerExp");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine($"DEBUG: No property initializers found or array is empty");
        }

        // Create the ObjectInitializerExp
        var result = new ObjectInitializerExp
        {
            TypeToInitialize = typeToInitialize,
            PropertyInitialisers = propertyInitializers,
            Annotations = new Dictionary<string, object>(),
            Location = GetLocationDetails(context),
            Type = typeToInitialize // The result type is the same as the type being initialized
        };

        Console.WriteLine($"DEBUG: Created ObjectInitializerExp with {propertyInitializers.Count} property initializers");
        return result;
    }

    protected override IAstThing DefaultResult { get; }

    public override IAstThing VisitInitialiser_property_assignment([NotNull] FifthParser.Initialiser_property_assignmentContext context)
    {
        Console.WriteLine($"DEBUG: VisitInitialiser_property_assignment START");

        var propertyName = context.var_name().GetText();
        Console.WriteLine($"DEBUG: Got property name: {propertyName}");

        var expression = Visit(context.expression()) as Expression;
        Console.WriteLine($"DEBUG: VisitInitialiser_property_assignment called for property: {propertyName}");
        Console.WriteLine($"DEBUG: Expression visit result: {expression?.GetType().Name ?? "null"}");

        // Create PropertyRef manually since the builder seems incomplete
        var propertyRef = new PropertyRef
        {
            Property = new PropertyDef
            {
                Name = MemberName.From(propertyName),
                AccessConstraints = [],
                IsWriteOnly = false,
                IsReadOnly = false,
                BackingField = null,
                Getter = null,
                Setter = null,
                CtorOnlySetter = false,
                Visibility = Visibility.Public,
                TypeName = TypeName.From("object") // We'll resolve this later
            }
        };

        var result = new PropertyInitializerExp
        {
            Annotations = new Dictionary<string, object>(),
            PropertyToInitialize = propertyRef,
            RHS = expression ?? new StringLiteralExp { Value = "" },
            Location = GetLocationDetails(context),
            Type = expression?.Type ?? new FifthType.UnknownType() { Name = TypeName.From("unknown") }
        };
        Console.WriteLine($"DEBUG: VisitInitialiser_property_assignment created PropertyInitializerExp for {propertyName}");
        Console.WriteLine($"DEBUG: About to return result of type: {result.GetType().Name}");
        return result;
    }
}
