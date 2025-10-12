namespace compiler;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using ast;
using ast_model.TypeSystem;

/// <summary>
/// Roslyn-based translator that converts lowered AST modules into C# syntax trees.
/// Uses Microsoft.CodeAnalysis APIs to directly build syntax trees and emit assemblies
/// without intermediate string-based source generation.
/// </summary>
public enum BackendCompatibilityMode
{
    LegacyShim,
    Strict
}

public class TranslatorOptions
{
    public bool EmitDebugInfo { get; set; } = true;
    public BackendCompatibilityMode BackendCompatibilityMode { get; set; } = BackendCompatibilityMode.LegacyShim;
    public IReadOnlyList<string>? AdditionalReferences { get; set; }
}

public class LoweredAstToRoslynTranslator : IBackendTranslator
{
    /// <summary>
    /// Translate AssemblyDef (from IBackendTranslator interface)
    /// </summary>
    public TranslationResult Translate(AssemblyDef assembly)
    {
        return TranslateAssembly(assembly, null);
    }

    /// <summary>
    /// Translate LoweredAstModule (for backward compatibility with tests)
    /// </summary>
    public TranslationResult Translate(LoweredAstModule module, TranslatorOptions? options = null)
    {
        options ??= new TranslatorOptions();

        var mapping = new MappingTable();
        var sources = new List<string>();
        var diagnostics = new List<Diagnostic>();

        try
        {
            // Build C# syntax tree using Roslyn APIs
            var syntaxTree = BuildSyntaxTree(module, mapping);

            // Convert syntax tree to source text for the Sources list
            var sourceText = syntaxTree.GetText().ToString();
            sources.Add(sourceText);
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticLevel.Error,
                $"Translation failed: {ex.Message}",
                module.ModuleName,
                "TRANS001"));
        }

        return new TranslationResult(sources, mapping, diagnostics);
    }

    /// <summary>
    /// Translate AssemblyDef to TranslationResult with C# sources
    /// </summary>
    private TranslationResult TranslateAssembly(AssemblyDef assembly, TranslatorOptions? options)
    {
        options ??= new TranslatorOptions();

        var mapping = new MappingTable();
        var sources = new List<string>();
        var diagnostics = new List<Diagnostic>();

        try
        {
            // Process each module in the assembly
            foreach (var module in assembly.Modules)
            {
                var syntaxTree = BuildSyntaxTreeFromModule(module, mapping);
                var sourceText = syntaxTree.GetText().ToString();
                sources.Add(sourceText);
            }
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticLevel.Error,
                $"Translation failed: {ex.Message}",
                assembly.Name.ToString(),
                "TRANS001"));
        }

        return new TranslationResult(sources, mapping, diagnostics);
    }

    private SyntaxTree BuildSyntaxTreeFromModule(ModuleDef module, MappingTable mapping)
    {
        // Create using directives
        var usingDirectives = new[]
        {
            UsingDirective(IdentifierName("System")),
            UsingDirective(IdentifierName("System.Collections.Generic")),
            UsingDirective(IdentifierName("Fifth.System")),
            UsingDirective(IdentifierName("VDS.RDF"))
        };

        // Get namespace name (or use a default)
        // Handle potentially uninitialized NamespaceDecl value object
        string namespaceName;
        try
        {
            namespaceName = module.NamespaceDecl != null && !string.IsNullOrEmpty(module.NamespaceDecl.ToString())
                ? module.NamespaceDecl.ToString()
                : (module.OriginalModuleName ?? "GeneratedNamespace");
        }
        catch
        {
            // Fallback if NamespaceDecl is uninitialized value object
            namespaceName = module.OriginalModuleName ?? "GeneratedNamespace";
        }

        // Create namespace declaration
        var namespaceDeclaration = NamespaceDeclaration(
            IdentifierName(SanitizeIdentifier(namespaceName)))
            .AddUsings(usingDirectives);

        // Add classes to namespace
        foreach (var classDef in module.Classes)
        {
            var classDecl = BuildClassDeclaration(classDef, mapping);
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDecl);
        }

        // Always add a static Program class to host top-level functions and a fallback Main
        var program = ClassDeclaration("Program")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));

        foreach (var funcDef in module.Functions.OfType<FunctionDef>())
        {
            var methodDecl = BuildMethodDeclaration(funcDef, mapping);
            program = program.AddMembers(methodDecl);
        }

        // Ensure there is a suitable entry point even if the pipeline failed to surface a 'main'.
        // Detect presence of a generated Main method on the Program class to be robust to Name.ToString() differences.
        var hasMainMethod = program.Members
            .OfType<MethodDeclarationSyntax>()
            .Any(m => string.Equals(m.Identifier.Text, "Main", StringComparison.Ordinal));

        if (!hasMainMethod)
        {
            var stubMain = MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.IntKeyword)),
                    Identifier("Main"))
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                .WithBody(Block(ReturnStatement(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))));
            program = program.AddMembers(stubMain);
        }

        namespaceDeclaration = namespaceDeclaration.AddMembers(program);

        // Create compilation unit
        var compilationUnit = CompilationUnit()
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        // Create syntax tree with file path for debugging
        return CSharpSyntaxTree.Create(
            compilationUnit,
            path: $"{namespaceName}.g.cs",
            encoding: System.Text.Encoding.UTF8);
    }

    private ClassDeclarationSyntax BuildClassDeclaration(ClassDef classDef, MappingTable mapping)
    {
        var className = SanitizeIdentifier(classDef.Name.ToString());
        var classDecl = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword));

        // Add members (which can be properties, methods, or fields)
        foreach (var member in classDef.MemberDefs)
        {
            if (member is PropertyDef prop)
            {
                var propDecl = BuildPropertyDeclaration(prop);
                classDecl = classDecl.AddMembers(propDecl);
            }
            else if (member is MethodDef method)
            {
                var methodDecl = BuildMethodDeclarationFromMethodDef(method, mapping);
                classDecl = classDecl.AddMembers(methodDecl);
            }
            else if (member is FieldDef field)
            {
                var fieldDecl = BuildFieldDeclaration(field);
                classDecl = classDecl.AddMembers(fieldDecl);
            }
        }

        return classDecl;
    }

    private FieldDeclarationSyntax BuildFieldDeclaration(FieldDef field)
    {
        var fieldName = SanitizeIdentifier(field.Name.ToString());
        var typeName = MapTypeName(field.TypeName.ToString());

        return FieldDeclaration(
            VariableDeclaration(IdentifierName(typeName))
                .AddVariables(VariableDeclarator(Identifier(fieldName))))
            .AddModifiers(Token(SyntaxKind.PublicKeyword));
    }

    private PropertyDeclarationSyntax BuildPropertyDeclaration(PropertyDef prop)
    {
        var propName = SanitizeIdentifier(prop.Name.ToString());
        var typeName = MapTypeName(prop.TypeName.ToString());

        return PropertyDeclaration(
            IdentifierName(typeName),
            Identifier(propName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
    }

    private MethodDeclarationSyntax BuildMethodDeclarationFromMethodDef(MethodDef methodDef, MappingTable mapping)
    {
        // MethodDef wraps a FunctionDef, so delegate to it
        if (methodDef.FunctionDef != null)
        {
            var funcMethod = BuildMethodDeclaration(methodDef.FunctionDef, mapping);

            // Methods in classes are typically instance methods unless marked static in the FunctionDef
            if (!methodDef.FunctionDef.IsStatic)
            {
                funcMethod = funcMethod.WithModifiers(
                    SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)));
            }

            return funcMethod;
        }

        // Fallback for methods without FunctionDef
        var methodName = SanitizeIdentifier(methodDef.Name.ToString());
        return MethodDeclaration(
            IdentifierName("void"),
            Identifier(methodName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithBody(Block());
    }

    private BlockSyntax BuildBlockStatement(BlockStatement block)
    {
        var statements = new List<StatementSyntax>();

        foreach (var stmt in block.Statements)
        {
            var translatedStmt = TranslateStatement(stmt);
            if (translatedStmt != null)
            {
                statements.Add(translatedStmt);
            }
        }

        return Block(statements);
    }

    private StatementSyntax? TranslateStatement(Statement stmt)
    {
        return stmt switch
        {
            ReturnStatement retStmt => TranslateReturnStatement(retStmt),
            VarDeclStatement varDecl => TranslateVarDeclStatement(varDecl),
            ExpStatement expStmt => TranslateExpStatement(expStmt),
            IfElseStatement ifStmt => TranslateIfElseStatement(ifStmt),
            _ => null // Unsupported statement types return null for now
        };
    }

    private StatementSyntax TranslateReturnStatement(ReturnStatement retStmt)
    {
        if (retStmt.ReturnValue == null)
        {
            return ReturnStatement();
        }

        var expr = TranslateExpression(retStmt.ReturnValue);
        return ReturnStatement(expr);
    }

    private StatementSyntax TranslateVarDeclStatement(VarDeclStatement varDecl)
    {
        var varName = SanitizeIdentifier(varDecl.VariableDecl.Name.ToString());
        // TypeName is a value object, so ToString() should work
        var typeName = varDecl.VariableDecl.TypeName != null
            ? MapTypeName(varDecl.VariableDecl.TypeName.ToString())
            : "var";

        if (varDecl.InitialValue != null)
        {
            var initExpr = TranslateExpression(varDecl.InitialValue);
            return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(typeName))
                    .AddVariables(
                        VariableDeclarator(Identifier(varName))
                            .WithInitializer(EqualsValueClause(initExpr))));
        }
        else
        {
            // Default initialize to avoid CS0165 (use of unassigned local)
            return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(typeName))
                    .AddVariables(
                        VariableDeclarator(Identifier(varName))
                            .WithInitializer(
                                EqualsValueClause(
                                    DefaultExpression(IdentifierName(typeName))))));
        }
    }

    private StatementSyntax TranslateExpStatement(ExpStatement expStmt)
    {
        var expr = TranslateExpression(expStmt.RHS);
        if (expr is InvocationExpressionSyntax || expr is AssignmentExpressionSyntax ||
            expr is PostfixUnaryExpressionSyntax || expr is PrefixUnaryExpressionSyntax ||
            expr is ObjectCreationExpressionSyntax)
        {
            return ExpressionStatement(expr);
        }
        // Fallback: assign to a discard variable to permit expression statements
        return ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                IdentifierName("__discard"),
                expr));
    }

    private StatementSyntax TranslateIfElseStatement(IfElseStatement ifStmt)
    {
        var condition = TranslateExpression(ifStmt.Condition);
        var thenBlock = BuildBlockStatement(ifStmt.ThenBlock);

        if (ifStmt.ElseBlock != null)
        {
            var elseBlock = BuildBlockStatement(ifStmt.ElseBlock);
            return IfStatement(condition, thenBlock)
                .WithElse(ElseClause(elseBlock));
        }

        return IfStatement(condition, thenBlock);
    }

    private ExpressionSyntax TranslateExpression(Expression expr)
    {
        return expr switch
        {
            Int32LiteralExp intLit => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(intLit.Value)),
            Float8LiteralExp floatLit => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(floatLit.Value)),
            StringLiteralExp strLit => LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(NormalizeStringLiteral(strLit.Value))),
            VarRefExp varRef => IdentifierName(SanitizeIdentifier(varRef.VarName)),
            BinaryExp binExp => TranslateBinaryExpression(binExp),
            FuncCallExp funcCall => TranslateFuncCallExpression(funcCall),
            MemberAccessExp memberAccess => TranslateMemberAccessExpression(memberAccess),
            _ => LiteralExpression(SyntaxKind.NullLiteralExpression) // Fallback for unsupported expressions
        };
    }

    private ExpressionSyntax TranslateBinaryExpression(BinaryExp binExp)
    {
        var left = TranslateExpression(binExp.LHS);
        var right = TranslateExpression(binExp.RHS);

        var kind = binExp.Operator switch
        {
            Operator.ArithmeticAdd => SyntaxKind.AddExpression,
            Operator.ArithmeticSubtract => SyntaxKind.SubtractExpression,
            Operator.ArithmeticMultiply => SyntaxKind.MultiplyExpression,
            Operator.ArithmeticDivide => SyntaxKind.DivideExpression,
            Operator.Equal => SyntaxKind.EqualsExpression,
            Operator.NotEqual => SyntaxKind.NotEqualsExpression,
            Operator.LessThan => SyntaxKind.LessThanExpression,
            Operator.LessThanOrEqual => SyntaxKind.LessThanOrEqualExpression,
            Operator.GreaterThan => SyntaxKind.GreaterThanExpression,
            Operator.GreaterThanOrEqual => SyntaxKind.GreaterThanOrEqualExpression,
            Operator.LogicalAnd => SyntaxKind.LogicalAndExpression,
            Operator.LogicalOr => SyntaxKind.LogicalOrExpression,
            _ => SyntaxKind.AddExpression // Default fallback
        };

        return BinaryExpression(kind, left, right);
    }

    private ExpressionSyntax TranslateFuncCallExpression(FuncCallExp funcCall)
    {
        // External (static) call annotated by TreeLinkageVisitor?
        if (funcCall.Annotations != null &&
            funcCall.Annotations.TryGetValue("ExternalType", out var extTypeObj) && extTypeObj is Type extType)
        {
            var typeName = extType.FullName ?? extType.Name;
            var methodName = funcCall.Annotations.TryGetValue("ExternalMethodName", out var extNameObj) && extNameObj is string extMethod
                ? SanitizeIdentifier(extMethod)
                : (funcCall.FunctionDef != null ? SanitizeIdentifier(funcCall.FunctionDef.Name.ToString()) : "UnknownFunction");

            var argList = new List<ArgumentSyntax>();
            foreach (var arg in funcCall.InvocationArguments)
            {
                argList.Add(Argument(TranslateExpression(arg)));
            }

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(typeName),
                        IdentifierName(methodName)))
                .WithArgumentList(ArgumentList(SeparatedList(argList)));
        }

        // Regular function call
        string funcName = funcCall.FunctionDef != null
            ? SanitizeIdentifier(funcCall.FunctionDef.Name.ToString())
            : "UnknownFunction";

        var arguments = new List<ArgumentSyntax>();
        foreach (var arg in funcCall.InvocationArguments)
        {
            var argExpr = TranslateExpression(arg);
            arguments.Add(Argument(argExpr));
        }

        return InvocationExpression(IdentifierName(funcName))
            .WithArgumentList(ArgumentList(SeparatedList(arguments)));
    }

    private ExpressionSyntax TranslateMemberAccessExpression(MemberAccessExp memberAccess)
    {
        var objExpr = TranslateExpression(memberAccess.LHS);

        // RHS can be a VarRefExp or FuncCallExp
        if (memberAccess.RHS is VarRefExp varRef)
        {
            var memberName = SanitizeIdentifier(varRef.VarName);
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                objExpr,
                IdentifierName(memberName));
        }
        else if (memberAccess.RHS is FuncCallExp funcCall)
        {
            // Static external call? Use annotations to emit Type.Method(args)
            if (funcCall.Annotations != null &&
                funcCall.Annotations.TryGetValue("ExternalType", out var extTypeObj) && extTypeObj is Type extType)
            {
                var typeName = extType.FullName ?? extType.Name;
                var resolvedMethodName = funcCall.Annotations.TryGetValue("ExternalMethodName", out var extNameObj) && extNameObj is string extMethod
                    ? SanitizeIdentifier(extMethod)
                    : (funcCall.FunctionDef != null ? SanitizeIdentifier(funcCall.FunctionDef.Name.ToString()) : "UnknownFunction");

                var arguments = new List<ArgumentSyntax>();
                foreach (var arg in funcCall.InvocationArguments)
                {
                    arguments.Add(Argument(TranslateExpression(arg)));
                }

                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(typeName),
                            IdentifierName(resolvedMethodName)))
                    .WithArgumentList(ArgumentList(SeparatedList(arguments)));
            }

            // Instance call: obj.Method(args)
            string methodName = funcCall.FunctionDef != null
                ? SanitizeIdentifier(funcCall.FunctionDef.Name.ToString())
                : "UnknownFunction";

            var argumentsInst = new List<ArgumentSyntax>();
            foreach (var arg in funcCall.InvocationArguments)
            {
                var argExpr = TranslateExpression(arg);
                argumentsInst.Add(Argument(argExpr));
            }

            var member = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                objExpr,
                IdentifierName(methodName));

            return InvocationExpression(member)
                .WithArgumentList(ArgumentList(SeparatedList(argumentsInst)));
        }

        // Fallback
        return objExpr;
    }

    private string MapTypeName(string? fifthTypeName)
    {
        if (fifthTypeName == null) return "object";

        return fifthTypeName switch
        {
            "int" => "int",
            "Int32" => "int",
            "float" => "float",
            "Float" => "float",
            "double" => "double",
            "Double" => "double",
            "string" => "string",
            "String" => "string",
            "bool" => "bool",
            "Boolean" => "bool",
            "datetime" => "System.DateTime",
            "graph" => "VDS.RDF.IGraph",
            "triple" => "VDS.RDF.Triple",
            "void" => "void",
            "var" => "var",
            _ => fifthTypeName // Keep custom types as-is
        };
    }

    private SyntaxTree BuildSyntaxTree(LoweredAstModule module, MappingTable mapping)
    {
        // Create using directives
        var usingDirectives = new[]
        {
            UsingDirective(IdentifierName("System"))
        };

        // Create namespace declaration
        var namespaceDeclaration = NamespaceDeclaration(
            IdentifierName(SanitizeIdentifier(module.ModuleName)))
            .AddUsings(usingDirectives);

        // Create class declaration
        var classDeclaration = ClassDeclaration("GeneratedProgram")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));

        // Add methods to class
        foreach (var method in module.Methods)
        {
            var methodDecl = BuildMethodDeclarationFromLoweredMethod(method, mapping);
            classDeclaration = classDeclaration.AddMembers(methodDecl);
        }

        // Add class to namespace
        namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);

        // Create compilation unit
        var compilationUnit = CompilationUnit()
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        // Create syntax tree with file path for debugging
        return CSharpSyntaxTree.Create(
            compilationUnit,
            path: $"{module.ModuleName}.g.cs",
            encoding: System.Text.Encoding.UTF8);
    }

    private MethodDeclarationSyntax BuildMethodDeclarationFromLoweredMethod(LoweredMethod method, MappingTable mapping)
    {
        // Create a simple stub method for the LoweredMethod
        var methodName = SanitizeIdentifier(method.Name);
        var returnStatement = ReturnStatement();

        var body = Block(SingletonList<StatementSyntax>(returnStatement));

        var methodDecl = MethodDeclaration(
            PredefinedType(Token(SyntaxKind.VoidKeyword)),
            Identifier(methodName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .WithBody(body);

        // Add mapping entry
        var syntaxText = methodDecl.NormalizeWhitespace().ToFullString();
        var lines = syntaxText.Split('\n');

        mapping.Add(new MappingEntry(
            method.NodeId,
            0,
            1,
            1,
            lines.Length,
            lines.LastOrDefault()?.Length ?? 1));

        return methodDecl;
    }

    private MethodDeclarationSyntax BuildMethodDeclaration(FunctionDef funcDef, MappingTable mapping)
    {
        var functionName = funcDef.Name.ToString();
        // Translate 'main' to 'Main' for C# entry point convention
        var methodName = functionName == "main" ? "Main" : SanitizeIdentifier(functionName);
        var returnTypeName = ExtractTypeName(funcDef.ReturnType);

        // Build parameters
        var parameters = new List<ParameterSyntax>();
        foreach (var param in funcDef.Params)
        {
            var paramName = SanitizeIdentifier(param.Name.ToString());
            // TypeName is a value object
            var paramTypeName = param.TypeName != null
                ? MapTypeName(param.TypeName.ToString())
                : "object";
            parameters.Add(
                Parameter(Identifier(paramName))
                    .WithType(IdentifierName(paramTypeName)));
        }

        // Build method body
        var body = BuildBlockStatement(funcDef.Body);

        // Add a discard local: object __discard = default(object);
        var discardDecl = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("object"))
                .AddVariables(
                    VariableDeclarator(Identifier("__discard"))
                        .WithInitializer(
                            EqualsValueClause(
                                DefaultExpression(IdentifierName("object"))))));
        body = body.WithStatements(body.Statements.Insert(0, discardDecl));

        var methodDecl = MethodDeclaration(
            IdentifierName(returnTypeName),
            Identifier(methodName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .WithParameterList(ParameterList(SeparatedList(parameters)))
            .WithBody(body);

        // Add mapping entry for this method
        // Generate a unique node ID if not available
        var nodeId = $"func_{methodName}_{Guid.NewGuid().ToString("N")[..8]}";
        var syntaxText = methodDecl.NormalizeWhitespace().ToFullString();
        var lines = syntaxText.Split('\n');

        mapping.Add(new MappingEntry(
            nodeId,
            0, // First source file
            1, // Start line (approximate)
            1, // Start column
            lines.Length, // End line (approximate)
            lines.LastOrDefault()?.Length ?? 1)); // End column (approximate)

        return methodDecl;
    }

    /// <summary>
    /// Extract a simple type name string from a FifthType
    /// </summary>
    private string ExtractTypeName(FifthType? fifthType)
    {
        if (fifthType == null)
            return "void";

        try
        {
            // FifthType has a Name property that is a TypeName
            var typeName = fifthType.Name.ToString();
            return MapTypeName(typeName);
        }
        catch
        {
            // Fallback for complex types or errors
            return fifthType switch
            {
                FifthType.TVoidType => "void",
                FifthType.TDotnetType dotnetType => dotnetType.TheType.Name,
                _ => "object"
            };
        }
    }

    private static string SanitizeIdentifier(string identifier)
    {
        // Basic sanitization: replace invalid characters with underscores
        var result = identifier.Replace('-', '_').Replace('.', '_');

        // Ensure it starts with a letter or underscore
        if (result.Length > 0 && !char.IsLetter(result[0]) && result[0] != '_')
        {
            result = "_" + result;
        }

        return result;
    }

    private static string NormalizeStringLiteral(string value)
    {
        // If the parsed value includes surrounding quotes, strip them
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
        {
            value = value.Substring(1, value.Length - 2);
        }

        // Escape backslashes and quotes for safe C# literal emission
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }
}
