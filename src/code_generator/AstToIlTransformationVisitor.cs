using ast;
using il_ast;
using il_ast_generated;

namespace code_generator;

/// <summary>
/// Transforms AST nodes from the Fifth language parser into IL metamodel representations.
/// This visitor follows the transformation stage pattern used in FifthParserManager.ApplyLanguageAnalysisPhases.
/// </summary>
public class AstToIlTransformationVisitor : DefaultRecursiveDescentVisitor
{
    private readonly Dictionary<string, TypeReference> _typeMap = new();
    private AssemblyDeclaration? _currentAssembly;
    private ModuleDeclaration? _currentModule;
    private ClassDefinition? _currentClass;
    
    public AstToIlTransformationVisitor()
    {
        InitializeBuiltinTypes();
    }
    
    private void InitializeBuiltinTypes()
    {
        _typeMap["int"] = new TypeReference { Namespace = "System", Name = "Int32" };
        _typeMap["string"] = new TypeReference { Namespace = "System", Name = "String" };
        _typeMap["float"] = new TypeReference { Namespace = "System", Name = "Single" };
        _typeMap["double"] = new TypeReference { Namespace = "System", Name = "Double" };
        _typeMap["bool"] = new TypeReference { Namespace = "System", Name = "Boolean" };
        _typeMap["void"] = new TypeReference { Namespace = "System", Name = "Void" };
    }

    /// <summary>
    /// Transform an AST assembly definition to IL assembly declaration
    /// </summary>
    public AssemblyDeclaration TransformAssembly(AssemblyDef astAssembly)
    {
        var ilAssembly = new AssemblyDeclaration
        {
            Name = astAssembly.Name.Value ?? "DefaultAssembly",
            Version = new il_ast.Version(1, 0, 0, 0),
            AssemblyReferences = CreateStandardAssemblyReferences()
        };

        _currentAssembly = ilAssembly;
        
        // Transform the first module (Fifth programs typically have one module)
        if (astAssembly.Modules.Count > 0)
        {
            ilAssembly.PrimeModule = TransformModule(astAssembly.Modules[0]);
        }
        
        return ilAssembly;
    }

    private List<AssemblyReference> CreateStandardAssemblyReferences()
    {
        return new List<AssemblyReference>
        {
            new AssemblyReference
            {
                Name = "System.Runtime",
                PublicKeyToken = "b03f5f7f11d50a3a",
                Version = new il_ast.Version(8, 0, 0, 0)
            },
            new AssemblyReference
            {
                Name = "System.Console", 
                PublicKeyToken = "b03f5f7f11d50a3a",
                Version = new il_ast.Version(8, 0, 0, 0)
            }
        };
    }

    private ModuleDeclaration TransformModule(ModuleDef astModule)
    {
        var ilModule = new ModuleDeclaration
        {
            FileName = astModule.OriginalModuleName ?? "main.dll"
        };

        _currentModule = ilModule;

        // Transform classes
        foreach (var astClass in astModule.Classes)
        {
            var ilClass = TransformClass(astClass);
            if (ilClass != null)
            {
                ilModule.Classes.Add(ilClass);
            }
        }

        // Transform top-level functions
        foreach (var astFunction in astModule.Functions)
        {
            var ilMethod = TransformFunction(astFunction);
            if (ilMethod != null)
            {
                ilModule.Functions.Add(ilMethod);
            }
        }

        return ilModule;
    }

    private ClassDefinition? TransformClass(ClassDef astClass)
    {
        var ilClass = new ClassDefinition
        {
            Name = astClass.Name.Value ?? "UnnamedClass",
            Namespace = "Fifth.Generated",
            Visibility = MemberAccessability.Public,
            ParentAssembly = _currentAssembly!
        };

        _currentClass = ilClass;

        // Transform members (properties, fields, methods)
        foreach (var astMember in astClass.MemberDefs)
        {
            switch (astMember)
            {
                case PropertyDef propertyDef:
                    var ilField = TransformPropertyToField(propertyDef);
                    if (ilField != null)
                    {
                        ilClass.Fields.Add(ilField);
                    }
                    break;
                case FieldDef fieldDef:
                    var ilFieldDef = TransformField(fieldDef);
                    if (ilFieldDef != null)
                    {
                        ilClass.Fields.Add(ilFieldDef);
                    }
                    break;
            }
        }

        return ilClass;
    }

    private FieldDefinition? TransformPropertyToField(PropertyDef astProperty)
    {
        return new FieldDefinition
        {
            Name = astProperty.Name.Value ?? "UnnamedField",
            TheType = MapType(astProperty.TypeName.Value?.ToString() ?? "object"),
            ParentClass = _currentClass!,
            TypeOfMember = MemberType.Field,
            Visibility = MemberAccessability.Public,
            IsStatic = false
        };
    }

    private FieldDefinition? TransformField(FieldDef astField)
    {
        return new FieldDefinition
        {
            Name = astField.Name.Value ?? "UnnamedField",
            TheType = MapType(astField.TypeName.Value?.ToString() ?? "object"),
            ParentClass = _currentClass!,
            TypeOfMember = MemberType.Field,
            Visibility = MemberAccessability.Public,
            IsStatic = false
        };
    }

    private MethodDefinition? TransformFunction(FunctionDef astFunction)
    {
        var ilMethod = new MethodDefinition
        {
            Name = astFunction.Name.Value ?? "UnnamedMethod",
            ParentClass = _currentClass,
            TypeOfMember = MemberType.Method,
            Visibility = MemberAccessability.Public,
            IsStatic = _currentClass == null || astFunction.IsStatic,
            Header = new MethodHeader
            {
                FunctionKind = astFunction.Name.Value == "main" ? FunctionKind.Normal : FunctionKind.Normal,
                IsEntrypoint = astFunction.Name.Value == "main"
            },
            Signature = CreateMethodSignature(astFunction),
            Impl = new MethodImpl
            {
                IsManaged = true,
                Body = TransformBlock(astFunction.Body)
            },
            CodeTypeFlags = CodeTypeFlag.cil
        };

        return ilMethod;
    }

    private MethodSignature CreateMethodSignature(FunctionDef astFunction)
    {
        var returnTypeName = astFunction.ReturnType?.Name.Value ?? "void";
        
        var signature = new MethodSignature
        {
            CallingConvention = MethodCallingConvention.Default,
            ReturnTypeSignature = MapType(returnTypeName),
            NumberOfParameters = (ushort)(astFunction.Params?.Count ?? 0)
        };

        if (astFunction.Params != null)
        {
            foreach (var param in astFunction.Params)
            {
                signature.ParameterSignatures.Add(new ParameterSignature
                {
                    Name = param.Name ?? "param",
                    TypeReference = MapType(param.TypeName.Value?.ToString() ?? "object"),
                    InOut = InOutFlag.In
                });
            }
        }

        return signature;
    }

    private Block TransformBlock(BlockStatement? astBlock)
    {
        var ilBlock = new Block();
        
        if (astBlock?.Statements != null)
        {
            foreach (var astStatement in astBlock.Statements)
            {
                var ilStatement = TransformStatement(astStatement);
                if (ilStatement != null)
                {
                    ilBlock.Statements.Add(ilStatement);
                }
            }
        }

        return ilBlock;
    }

    private il_ast.Statement? TransformStatement(ast.Statement astStatement)
    {
        return astStatement switch
        {
            VarDeclStatement varDecl => TransformVariableDeclaration(varDecl),
            ast.ReturnStatement returnStmt => TransformReturn(returnStmt),
            IfElseStatement ifStmt => TransformIfElse(ifStmt),
            ast.WhileStatement whileStmt => TransformWhile(whileStmt),
            AssignmentStatement assignment => TransformAssignment(assignment),
            ExpStatement exprStmt => new il_ast.ExpressionStatement 
            { 
                Expression = TransformExpression(exprStmt.RHS) 
            },
            _ => null
        };
    }

    private VariableDeclarationStatement TransformVariableDeclaration(VarDeclStatement astVarDecl)
    {
        return new VariableDeclarationStatement
        {
            Name = astVarDecl.VariableDecl.Name ?? "temp",
            TypeName = astVarDecl.VariableDecl.TypeName.Value?.ToString() ?? "object",
            InitialisationExpression = astVarDecl.InitialValue != null 
                ? TransformExpression(astVarDecl.InitialValue)
                : null
        };
    }

    private il_ast.ReturnStatement TransformReturn(ast.ReturnStatement astReturn)
    {
        return new il_ast.ReturnStatement
        {
            Exp = TransformExpression(astReturn.ReturnValue)
        };
    }

    private IfStatement TransformIfElse(IfElseStatement astIfElse)
    {
        return new IfStatement
        {
            Conditional = TransformExpression(astIfElse.Condition),
            IfBlock = TransformBlock(astIfElse.ThenBlock),
            ElseBlock = TransformBlock(astIfElse.ElseBlock)
        };
    }

    private il_ast.WhileStatement TransformWhile(ast.WhileStatement astWhile)
    {
        return new il_ast.WhileStatement
        {
            Conditional = TransformExpression(astWhile.Condition),
            LoopBlock = TransformBlock(astWhile.Body)
        };
    }

    private VariableAssignmentStatement TransformAssignment(AssignmentStatement astAssignment)
    {
        // For simplicity, we assume LValue is a variable reference
        var lvalueName = "temp";
        if (astAssignment.LValue is VarRefExp varRef)
        {
            lvalueName = varRef.VarName;
        }

        return new VariableAssignmentStatement
        {
            LHS = lvalueName,
            RHS = TransformExpression(astAssignment.RValue)
        };
    }

    private il_ast.Expression TransformExpression(ast.Expression astExpression)
    {
        return astExpression switch
        {
            Int32LiteralExp intLit => new IntLiteral(intLit.Value),
            Float8LiteralExp doubleLit => new DoubleLiteral(doubleLit.Value),
            Float4LiteralExp floatLit => new FloatLiteral(floatLit.Value),
            StringLiteralExp stringLit => new StringLiteral(stringLit.Value),
            BooleanLiteralExp boolLit => new BoolLiteral(boolLit.Value),
            BinaryExp binaryExp => new BinaryExpression(
                GetOperatorString(binaryExp.Operator),
                TransformExpression(binaryExp.LHS),
                TransformExpression(binaryExp.RHS)
            ),
            UnaryExp unaryExp => new UnaryExpression(
                GetOperatorString(unaryExp.Operator),
                TransformExpression(unaryExp.Operand)
            ),
            VarRefExp varRef => new VariableReferenceExpression
            {
                Name = varRef.VarName
            },
            ast.FuncCallExp funcCall => new il_ast.FuncCallExp
            {
                Name = funcCall.FunctionDef?.Name.Value ?? "unknown",
                Args = funcCall.InvocationArguments?.Select(TransformExpression).ToList() ?? new List<il_ast.Expression>(),
                ReturnType = "object"
            },
            _ => new VariableReferenceExpression { Name = "unknown" }
        };
    }

    private string GetOperatorString(Operator op)
    {
        return op switch
        {
            Operator.ArithmeticAdd => "+",
            Operator.ArithmeticSubtract => "-",
            Operator.ArithmeticMultiply => "*",
            Operator.ArithmeticDivide => "/",
            Operator.Equal => "==",
            Operator.NotEqual => "!=",
            Operator.LessThan => "<",
            Operator.GreaterThan => ">",
            Operator.LessThanOrEqual => "<=",
            Operator.GreaterThanOrEqual => ">=",
            Operator.ArithmeticNegative => "-",
            Operator.LogicalNot => "!",
            _ => "+"
        };
    }

    private TypeReference MapType(string typeName)
    {
        if (_typeMap.TryGetValue(typeName, out var mappedType))
        {
            return mappedType;
        }

        return new TypeReference
        {
            Namespace = "Fifth.Generated",
            Name = typeName
        };
    }
}