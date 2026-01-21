namespace compiler;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
public class TranslatorOptions
{
    public bool EmitDebugInfo { get; set; } = true;
    public IReadOnlyList<string>? AdditionalReferences { get; set; }
}

public class LoweredAstToRoslynTranslator : IBackendTranslator
{
    // Track variables that have been declared in the current method scope
    private HashSet<string> _declaredVariables = new HashSet<string>();

    // Track class-level property/field names for the current class being translated
    private HashSet<string> _classMembers = new HashSet<string>();

    // Track the current containing class name for expression translation decisions
    private string? _currentContainingClassName;

    // Track top-level (module) function names so closure classes can qualify them as Program.<fn>
    private HashSet<string> _moduleLevelFunctionNames = new HashSet<string>(StringComparer.Ordinal);

    // Track the return type of the current function being translated
    private FifthType? _currentReturnType;

    private readonly Dictionary<string, string> _closureInterfaceByClass = new(StringComparer.Ordinal);

    private static string GetModuleNamespace(ModuleDef module)
    {
        string namespaceName;
        try
        {
            namespaceName = module.NamespaceDecl.ToString() ?? string.Empty;
        }
        catch
        {
            namespaceName = string.Empty;
        }

        if (string.IsNullOrWhiteSpace(namespaceName) || namespaceName.Equals("anonymous", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return namespaceName;
    }

    private static IReadOnlyList<string> GetImportedNamespaces(ModuleDef module)
    {
        if (module.Annotations == null)
        {
            return Array.Empty<string>();
        }

        if (module.Annotations.TryGetValue(ModuleAnnotationKeys.ResolvedImports, out var resolvedObj))
        {
            if (resolvedObj is IReadOnlyList<string> resolvedList)
            {
                return resolvedList
                    .Where(ns => !string.IsNullOrWhiteSpace(ns))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }

            if (resolvedObj is List<string> resolvedList2)
            {
                return resolvedList2
                    .Where(ns => !string.IsNullOrWhiteSpace(ns))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }
        }

        if (module.Annotations.TryGetValue(ModuleAnnotationKeys.ImportDirectives, out var importsObj))
        {
            if (importsObj is IReadOnlyList<NamespaceImportDirective> importsList)
            {
                return importsList
                    .Select(i => i.Namespace)
                    .Where(ns => !string.IsNullOrWhiteSpace(ns))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }

            if (importsObj is List<NamespaceImportDirective> importsList2)
            {
                return importsList2
                    .Select(i => i.Namespace)
                    .Where(ns => !string.IsNullOrWhiteSpace(ns))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }
        }

        return Array.Empty<string>();
    }

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
            var hasAnyMain = assembly.Modules
                .SelectMany(m => m.Functions)
                .OfType<FunctionDef>()
                .Any(f => string.Equals(f.Name.Value, "main", StringComparison.Ordinal));
            var isSingleModule = assembly.Modules.Count == 1;

            // Process each module in the assembly
            foreach (var module in assembly.Modules)
            {
                var hasModuleMain = module.Functions
                    .OfType<FunctionDef>()
                    .Any(f => string.Equals(f.Name.Value, "main", StringComparison.Ordinal));

                var emitStubMain = !hasModuleMain && !hasAnyMain && isSingleModule;

                var syntaxTree = BuildSyntaxTreeFromModule(module, mapping, emitStubMain);
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

    private SyntaxTree BuildSyntaxTreeFromModule(ModuleDef module, MappingTable mapping, bool emitStubMain)
    {
        _moduleLevelFunctionNames = module.Functions
            .OfType<FunctionDef>()
            .Select(f => SanitizeIdentifier(f.Name.ToString() == "main" ? "Main" : f.Name.ToString()))
            .ToHashSet(StringComparer.Ordinal);

        CacheClosureInterfaces(module);

        // Create using directives
        var usingDirectives = new List<UsingDirectiveSyntax>
        {
            UsingDirective(IdentifierName("System")),
            UsingDirective(IdentifierName("System.Collections.Generic")),
            UsingDirective(IdentifierName("Fifth.System")),
            UsingDirective(IdentifierName("Fifth.System.Functional")).WithStaticKeyword(Token(SyntaxKind.StaticKeyword)),
            UsingDirective(IdentifierName("Fifth.System.List")).WithStaticKeyword(Token(SyntaxKind.StaticKeyword)),
            UsingDirective(IdentifierName("Fifth.System.IO")).WithStaticKeyword(Token(SyntaxKind.StaticKeyword)),
            UsingDirective(IdentifierName("Fifth.System.Math")).WithStaticKeyword(Token(SyntaxKind.StaticKeyword)),
            UsingDirective(IdentifierName("VDS.RDF")),
            UsingDirective(IdentifierName("VDS.RDF.Query"))
        };

        var importNamespaces = GetImportedNamespaces(module);
        var usingTracker = new HashSet<string>(StringComparer.Ordinal);
        foreach (var importNamespace in importNamespaces)
        {
            if (string.IsNullOrWhiteSpace(importNamespace))
            {
                continue;
            }

            if (usingTracker.Add($"ns:{importNamespace}"))
            {
                usingDirectives.Add(UsingDirective(ParseName(importNamespace)));
            }

            if (usingTracker.Add($"static:{importNamespace}.Program"))
            {
                usingDirectives.Add(
                    UsingDirective(ParseName($"{importNamespace}.Program"))
                        .WithStaticKeyword(Token(SyntaxKind.StaticKeyword)));
            }
        }

        var namespaceName = GetModuleNamespace(module);
        var fileNamespaceName = string.IsNullOrWhiteSpace(namespaceName)
            ? module.OriginalModuleName ?? "GeneratedModule"
            : namespaceName;

        var members = new List<MemberDeclarationSyntax>();

        // Add classes
        foreach (var classDef in module.Classes)
        {
            var classDecl = BuildClassDeclaration(classDef, mapping);
            members.Add(classDecl);
        }

        // Always add a static partial Program class to host top-level functions
        var program = ClassDeclaration("Program")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword));

        var priorContaining = _currentContainingClassName;
        _currentContainingClassName = "Program";

        foreach (var funcDef in module.Functions.OfType<FunctionDef>())
        {
            var methodDecl = BuildMethodDeclaration(funcDef, mapping);
            program = program.AddMembers(methodDecl);
        }

        _currentContainingClassName = priorContaining;

        // Ensure there is a suitable entry point even if the pipeline failed to surface a 'main'.
        // Detect presence of a generated Main method on the Program class to be robust to Name.ToString() differences.
        var hasMainMethod = program.Members
            .OfType<MethodDeclarationSyntax>()
            .Any(m => string.Equals(m.Identifier.Text, "Main", StringComparison.Ordinal));

        if (!hasMainMethod && emitStubMain)
        {
            var stubMain = MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.IntKeyword)),
                    Identifier("Main"))
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                .WithBody(Block(ReturnStatement(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))));
            program = program.AddMembers(stubMain);
        }

        members.Add(program);

        CompilationUnitSyntax compilationUnit;
        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            compilationUnit = CompilationUnit()
                .AddUsings(usingDirectives.ToArray())
                .AddMembers(members.ToArray())
                .NormalizeWhitespace();
        }
        else
        {
            var namespaceDeclaration = NamespaceDeclaration(ParseName(namespaceName))
                .AddUsings(usingDirectives.ToArray())
                .AddMembers(members.ToArray());

            compilationUnit = CompilationUnit()
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace();
        }

        // Create syntax tree with file path for debugging
        return CSharpSyntaxTree.Create(
            compilationUnit,
                path: $"{fileNamespaceName}.g.cs",
            encoding: System.Text.Encoding.UTF8);
    }

    private ClassDeclarationSyntax BuildClassDeclaration(ClassDef classDef, MappingTable mapping)
    {
        var priorContaining = _currentContainingClassName;

        // Clear and populate class members (properties and fields) to prevent shadowing
        _classMembers.Clear();
        foreach (var member in classDef.MemberDefs)
        {
            if (member is PropertyDef prop)
            {
                _classMembers.Add(SanitizeIdentifier(prop.Name.ToString()));
            }
            else if (member is FieldDef field)
            {
                _classMembers.Add(SanitizeIdentifier(field.Name.ToString()));
            }
        }

        var className = SanitizeIdentifier(classDef.Name.ToString());
        _currentContainingClassName = className;
        var classDecl = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword));

        // Add base classes (Inheritance)
        if (classDef.BaseClasses != null && classDef.BaseClasses.Count > 0)
        {
            var baseTypes = classDef.BaseClasses
                .Select(b => SimpleBaseType(ParseTypeName(MapTypeName(b))))
                .ToArray();
            classDecl = classDecl.WithBaseList(BaseList(SeparatedList<BaseTypeSyntax>(baseTypes)));
        }

        // Add type parameters if the class is generic (T093)
        if (classDef.TypeParameters.Count > 0)
        {
            var typeParams = classDef.TypeParameters
                .Select(tp => TypeParameter(SanitizeIdentifier(tp.Name.Value)))
                .ToArray();
            classDecl = classDecl.AddTypeParameterListParameters(typeParams);

            // Add constraints for type parameters (T094)
            foreach (var typeParam in classDef.TypeParameters)
            {
                var constraints = BuildTypeParameterConstraints(typeParam);
                if (constraints != null)
                {
                    classDecl = classDecl.AddConstraintClauses(constraints);
                }
            }
        }

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
                var memberDecl = BuildMemberFromMethodDef(method, mapping);

                // If the member is a method (not constructor) and redeclares generic parameters at the class level,
                // strip the method-level generic parameter list
                if (memberDecl is MethodDeclarationSyntax methodDecl &&
                    methodDecl.TypeParameterList != null &&
                    classDef.TypeParameters.Count > 0)
                {
                    var classTypeParamNames = classDef.TypeParameters.Select(tp => SanitizeIdentifier(tp.Name.Value)).ToHashSet();
                    var methodTypeParamNames = methodDecl.TypeParameterList.Parameters.Select(p => p.Identifier.Text).ToList();

                    if (methodTypeParamNames.All(n => classTypeParamNames.Contains(n)))
                    {
                        // Remove generic parameter list & any constraint clauses tied to them
                        memberDecl = methodDecl.WithTypeParameterList(null)
                                               .WithConstraintClauses(new SyntaxList<TypeParameterConstraintClauseSyntax>());
                    }
                }
                classDecl = classDecl.AddMembers(memberDecl);
            }
            else if (member is FieldDef field)
            {
                var fieldDecl = BuildFieldDeclaration(field);
                classDecl = classDecl.AddMembers(fieldDecl);
            }
        }

        _currentContainingClassName = priorContaining;

        return classDecl;
    }

    /// <summary>
    /// Builds type parameter constraints for Roslyn (T094).
    /// Maps Fifth constraints to C# constraints:
    /// - InterfaceConstraint → TypeConstraint
    /// - BaseClassConstraint → ClassOrStructConstraint
    /// - ConstructorConstraint → ConstructorConstraint
    /// </summary>
    private TypeParameterConstraintClauseSyntax? BuildTypeParameterConstraints(TypeParameterDef typeParam)
    {
        if (typeParam.Constraints.Count == 0)
            return null;

        var constraintList = new List<TypeParameterConstraintSyntax>();

        foreach (var constraint in typeParam.Constraints)
        {
            switch (constraint)
            {
                case InterfaceConstraint interfaceConstraint:
                    // Interface constraint: where T : IComparable
                    var interfaceType = IdentifierName(SanitizeIdentifier(interfaceConstraint.InterfaceName.Value));
                    constraintList.Add(TypeConstraint(interfaceType));
                    break;

                case BaseClassConstraint baseClassConstraint:
                    // Base class constraint: where T : BaseClass
                    var baseType = IdentifierName(SanitizeIdentifier(baseClassConstraint.BaseClassName.Value));
                    constraintList.Add(TypeConstraint(baseType));
                    break;

                case ConstructorConstraint _:
                    // Constructor constraint: where T : new()
                    constraintList.Add(SyntaxFactory.ConstructorConstraint());
                    break;
            }
        }

        if (constraintList.Count == 0)
            return null;

        return TypeParameterConstraintClause(
            IdentifierName(SanitizeIdentifier(typeParam.Name.Value)),
            SeparatedList(constraintList)
        );
    }

    private FieldDeclarationSyntax BuildFieldDeclaration(FieldDef field)
    {
        var fieldName = SanitizeIdentifier(field.Name.ToString());
        var fieldType = CreateTypeSyntax(field.TypeName.ToString(), field.CollectionType);

        return FieldDeclaration(
            VariableDeclaration(fieldType)
                .AddVariables(VariableDeclarator(Identifier(fieldName))))
            .AddModifiers(Token(SyntaxKind.PublicKeyword));
    }

    private PropertyDeclarationSyntax BuildPropertyDeclaration(PropertyDef prop)
    {
        var propName = SanitizeIdentifier(prop.Name.ToString());
        var propType = CreateTypeSyntax(prop.TypeName.ToString(), prop.CollectionType);

        return PropertyDeclaration(
            propType,
            Identifier(propName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
    }

    private MemberDeclarationSyntax BuildMemberFromMethodDef(MethodDef methodDef, MappingTable mapping)
    {
        // MethodDef wraps a FunctionDef, so delegate to it
        if (methodDef.FunctionDef != null)
        {
            // Check if this is a constructor
            if (methodDef.FunctionDef.IsConstructor)
            {
                return BuildConstructorDeclaration(methodDef.FunctionDef, mapping);
            }

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
            EmptyStatement _ => null, // Empty statements are elided (no code generated)
            ExpStatement expStmt => TranslateExpStatement(expStmt),
            IfElseStatement ifStmt => TranslateIfElseStatement(ifStmt),
            WhileStatement whileStmt => TranslateWhileStatement(whileStmt),
            ForeachStatement foreachStmt => TranslateForeachStatement(foreachStmt),
            AssignmentStatement assignStmt => TranslateAssignmentStatement(assignStmt),
            TryStatement tryStmt => TranslateTryStatement(tryStmt),
            ThrowStatement throwStmt => TranslateThrowStatement(throwStmt),
            _ => null // Unsupported statement types return null for now
        };
    }

    private StatementSyntax TranslateReturnStatement(ReturnStatement retStmt)
    {
        if (retStmt.ReturnValue == null)
        {
            return ReturnStatement();
        }

        var expr = TranslateExpression(retStmt.ReturnValue, _currentReturnType);
        return ReturnStatement(expr);
    }

    private StatementSyntax TranslateVarDeclStatement(VarDeclStatement varDecl)
    {
        var varName = SanitizeIdentifier(varDecl.VariableDecl.Name.ToString());
        // Track that this variable has been declared
        _declaredVariables.Add(varName);

        // Use CreateTypeSyntax which already handles collection types and the [T] -> List<T> mapping
        var declaredType = CreateTypeSyntax(varDecl.VariableDecl.TypeName.ToString(), varDecl.VariableDecl.CollectionType);

        var useVarType = varDecl.InitialValue is List;

        if (varDecl.InitialValue != null)
        {
            var initExpr = TranslateExpression(varDecl.InitialValue, varDecl.VariableDecl.Type);
            return LocalDeclarationStatement(
                VariableDeclaration(declaredType)
                    .AddVariables(
                        VariableDeclarator(Identifier(varName))
                            .WithInitializer(EqualsValueClause(initExpr))));
        }
        else
        {
            var defaultType = useVarType ? ParseTypeName("object") : declaredType;

            // Default initialize to avoid CS0165 (use of unassigned local)
            return LocalDeclarationStatement(
                VariableDeclaration(defaultType)
                    .AddVariables(
                        VariableDeclarator(Identifier(varName))
                            .WithInitializer(
                                EqualsValueClause(
                                    DefaultExpression(defaultType)))));
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
        // Fifth uses int for bool (0=false, non-zero=true), but comparison operators return bool
        // Only convert if the condition is not already a comparison/relational expression
        condition = EnsureBooleanExpression(condition, ifStmt.Condition);
        var thenBlock = BuildBlockStatement(ifStmt.ThenBlock);

        if (ifStmt.ElseBlock != null)
        {
            var elseBlock = BuildBlockStatement(ifStmt.ElseBlock);
            return IfStatement(condition, thenBlock)
                .WithElse(ElseClause(elseBlock));
        }

        return IfStatement(condition, thenBlock);
    }

    private StatementSyntax TranslateWhileStatement(WhileStatement whileStmt)
    {
        var condition = TranslateExpression(whileStmt.Condition);
        // Fifth uses int for bool (0=false, non-zero=true), but comparison operators return bool
        condition = EnsureBooleanExpression(condition, whileStmt.Condition);
        var body = BuildBlockStatement(whileStmt.Body);

        return WhileStatement(condition, body);
    }

    private StatementSyntax TranslateForeachStatement(ForeachStatement foreachStmt)
    {
        // Generate: foreach (var loopVar in collection) { body }
        var loopVarName = SanitizeIdentifier(foreachStmt.LoopVariable.Name.ToString());
        var loopVarTypeName = ExtractTypeName(foreachStmt.LoopVariable.Type);
        var loopVarType = ParseTypeName(loopVarTypeName);

        var collection = TranslateExpression(foreachStmt.Collection);
        var body = BuildBlockStatement(foreachStmt.Body);

        return ForEachStatement(
            loopVarType,
            Identifier(loopVarName),
            collection,
            body);
    }

    private StatementSyntax TranslateAssignmentStatement(AssignmentStatement assignStmt)
    {
        // Check if LValue is a simple variable reference (not member access or indexer)
        if (assignStmt.LValue is VarRefExp varRef)
        {
            var varName = SanitizeIdentifier(varRef.VarName);

            // If this is a class member (property/field), generate this.PropertyName assignment
            if (_classMembers.Contains(varName))
            {
                var memberTarget = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName(varName));
                var memberValue = TranslateExpression(assignStmt.RValue, assignStmt.LValue.Type);

                return ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        memberTarget,
                        memberValue));
            }

            // If this variable hasn't been declared yet, create a declaration with var
            if (!_declaredVariables.Contains(varName))
            {
                _declaredVariables.Add(varName);
                var value = TranslateExpression(assignStmt.RValue);

                return LocalDeclarationStatement(
                    VariableDeclaration(IdentifierName("var"))
                        .AddVariables(
                            VariableDeclarator(Identifier(varName))
                                .WithInitializer(EqualsValueClause(value))));
            }
        }

        // Regular assignment for already-declared variables or complex lvalues
        var target = TranslateExpression(assignStmt.LValue);
        var valueExpr = TranslateExpression(assignStmt.RValue, assignStmt.LValue.Type);

        return ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                target,
                valueExpr));
    }

    private StatementSyntax TranslateTryStatement(TryStatement tryStmt)
    {
        // Build the try block
        var tryBlock = BuildBlockStatement(tryStmt.TryBlock);

        // Build catch clauses
        var catchClauses = new List<CatchClauseSyntax>();
        foreach (var catchClause in tryStmt.CatchClauses)
        {
            var catchClauseSyntax = TranslateCatchClause(catchClause);
            if (catchClauseSyntax != null)
            {
                catchClauses.Add(catchClauseSyntax);
            }
        }

        // Build finally block if present
        FinallyClauseSyntax? finallyClause = null;
        if (tryStmt.FinallyBlock != null)
        {
            var finallyBlock = BuildBlockStatement(tryStmt.FinallyBlock);
            finallyClause = FinallyClause(finallyBlock);
        }

        // Create the try statement with all components
        return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TryStatement(
            tryBlock,
            List(catchClauses),
            finallyClause);
    }

    private CatchClauseSyntax? TranslateCatchClause(CatchClause catchClause)
    {
        var catchBody = BuildBlockStatement(catchClause.Body);

        // Handle catch-all (no type specified)
        if (catchClause.ExceptionType == null && catchClause.ExceptionIdentifier == null)
        {
            return CatchClause()
                .WithBlock(catchBody);
        }

        // Determine the exception type
        TypeSyntax? exceptionType = null;
        if (catchClause.ExceptionType != null)
        {
            var typeName = MapTypeName(catchClause.ExceptionType.Name.ToString());
            exceptionType = ParseTypeName(typeName);
        }
        else
        {
            // Default to System.Exception if not specified but identifier is
            exceptionType = ParseTypeName("System.Exception");
        }

        // Build catch declaration
        CatchDeclarationSyntax? catchDecl = null;
        if (catchClause.ExceptionIdentifier != null)
        {
            var identifier = SanitizeIdentifier(catchClause.ExceptionIdentifier);
            catchDecl = CatchDeclaration(exceptionType)
                .WithIdentifier(Identifier(identifier));
        }
        else if (exceptionType != null)
        {
            // Type specified but no identifier
            catchDecl = CatchDeclaration(exceptionType);
        }

        // Build filter if present
        CatchFilterClauseSyntax? filterClause = null;
        if (catchClause.Filter != null)
        {
            var filterExpr = TranslateExpression(catchClause.Filter);
            filterExpr = EnsureBooleanExpression(filterExpr, catchClause.Filter);
            filterClause = CatchFilterClause(filterExpr);
        }

        var catchSyntax = CatchClause()
            .WithBlock(catchBody);

        if (catchDecl != null)
        {
            catchSyntax = catchSyntax.WithDeclaration(catchDecl);
        }

        if (filterClause != null)
        {
            catchSyntax = catchSyntax.WithFilter(filterClause);
        }

        return catchSyntax;
    }

    private StatementSyntax TranslateThrowStatement(ThrowStatement throwStmt)
    {
        if (throwStmt.Exception == null)
        {
            // Rethrow (bare throw;)
            return ThrowStatement();
        }

        var exceptionExpr = TranslateExpression(throwStmt.Exception);
        return ThrowStatement(exceptionExpr);
    }

    private ExpressionSyntax TranslateThrowExpression(ThrowExp throwExp)
    {
        var exceptionExpr = TranslateExpression(throwExp.Exception);
        return ThrowExpression(exceptionExpr);
    }

    private ExpressionSyntax TranslateExpression(Expression expr, FifthType? targetType = null)
    {
        return expr switch
        {
            Int32LiteralExp intLit => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(intLit.Value)),
            Int64LiteralExp longLit => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(longLit.Value)),
            Float8LiteralExp floatLit => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(floatLit.Value)),
            Float4LiteralExp float32Lit => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(float32Lit.Value)),
            BooleanLiteralExp boolLit => LiteralExpression(
                boolLit.Value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression),
            StringLiteralExp strLit => strLit.Annotations != null && strLit.Annotations.ContainsKey("TriGContent")
                ? LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(strLit.Value))  // Don't normalize TriG content - use verbatim
                : LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(NormalizeStringLiteral(strLit.Value))),
            UriLiteralExp uriLit => ObjectCreationExpression(IdentifierName("Uri"))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                        Literal(uriLit.Value.ToString())))))),
            VarRefExp varRef => IdentifierName(SanitizeIdentifier(varRef.VarName)),
            BinaryExp binExp => TranslateBinaryExpression(binExp),
            FuncCallExp funcCall => TranslateFuncCallExpression(funcCall),
            MemberAccessExp memberAccess => TranslateMemberAccessExpression(memberAccess),
            IndexerExpression indexer => TranslateIndexerExpression(indexer),
            List list => TranslateListExpression(list, targetType),
            ObjectInitializerExp objInit => TranslateObjectInitializerExpression(objInit),
            UnaryExp unary => TranslateUnaryExpression(unary),
            TripleLiteralExp triple => TranslateTripleLiteralExpression(triple),
            ThrowExp throwExp => TranslateThrowExpression(throwExp),
            _ => DefaultExpression(IdentifierName("object")) // Fallback for unsupported expressions
        };
    }

    /// <summary>
    /// Creates a qualified name from a dotted string (e.g., "Fifth.System.KG")
    /// </summary>
    private NameSyntax CreateQualifiedName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName) || !fullName.Contains('.'))
        {
            return IdentifierName(fullName ?? "object");
        }

        var parts = fullName.Split('.');
        NameSyntax result = IdentifierName(parts[0]);
        for (int i = 1; i < parts.Length; i++)
        {
            result = QualifiedName(result, IdentifierName(parts[i]));
        }
        return result;
    }

    /// <summary>
    /// Get operator precedence level for Fifth operators.
    /// Higher numbers = higher precedence (binds tighter).
    /// Based on standard mathematical operator precedence.
    /// </summary>
    private int GetOperatorPrecedence(Operator op)
    {
        return op switch
        {
            // Highest precedence
            Operator.ArithmeticPow => 100,  // Power (^)

            // Multiplicative
            Operator.ArithmeticMultiply => 90,
            Operator.ArithmeticDivide => 90,
            Operator.ArithmeticRem => 90,
            Operator.ArithmeticMod => 90,
            Operator.BitwiseLeftShift => 90,
            Operator.BitwiseRightShift => 90,
            Operator.BitwiseAnd => 90,

            // Additive
            Operator.ArithmeticAdd => 80,
            Operator.ArithmeticSubtract => 80,
            Operator.BitwiseOr => 80,
            Operator.LogicalXor => 80,

            // Relational
            Operator.LessThan => 70,
            Operator.LessThanOrEqual => 70,
            Operator.GreaterThan => 70,
            Operator.GreaterThanOrEqual => 70,

            // Equality
            Operator.Equal => 60,
            Operator.NotEqual => 60,

            // Logical AND
            Operator.LogicalAnd => 50,

            // Logical OR (lowest)
            Operator.LogicalOr => 40,

            _ => 0  // Unknown operators get lowest precedence
        };
    }

    private ExpressionSyntax TranslateBinaryExpression(BinaryExp binExp)
    {
        // Special-case graph/triple operators to avoid emitting raw '+'/'-' for graphs
        // This is a defensive fallback; the normal pipeline should lower these earlier.
        bool lhsIsGraphLike = IsGraphType(binExp.LHS);
        bool rhsIsGraphLike = IsGraphType(binExp.RHS);

        if (binExp.Operator == Operator.ArithmeticAdd && (lhsIsGraphLike || rhsIsGraphLike))
        {
            // Emit: KG.Merge(KG.CopyGraph(lhsGraph), rhsGraph)
            var lhsExpr = TranslateExpression(binExp.LHS);
            var rhsExpr = TranslateExpression(binExp.RHS);
            var copyCall = InvocationExpression(
                MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                CreateQualifiedName("Fifth.System.KG"),
                IdentifierName("CopyGraph")))
            .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(lhsExpr))));
            return InvocationExpression(
                MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                CreateQualifiedName("Fifth.System.KG"),
                IdentifierName("Merge")))
            .WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(copyCall), Argument(rhsExpr) })));
        }
        if (binExp.Operator == Operator.ArithmeticSubtract && (lhsIsGraphLike || rhsIsGraphLike))
        {
            // Emit: KG.Difference(lhsGraph, rhsGraph)
            var lhsExpr = TranslateExpression(binExp.LHS);
            var rhsExpr = TranslateExpression(binExp.RHS);
            return InvocationExpression(
                MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                CreateQualifiedName("Fifth.System.KG"),
                IdentifierName("Difference")))
            .WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(lhsExpr), Argument(rhsExpr) })));
        }

        // Handle power operator (^) by calling System.Math.Pow
        if (binExp.Operator == Operator.ArithmeticPow)
        {
            var lhsExpr = TranslateExpression(binExp.LHS);
            var rhsExpr = TranslateExpression(binExp.RHS);

            // Emit: System.Math.Pow(base, exponent)
            // Math.Pow accepts double arguments and C# will auto-convert ints
            var powCall = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("System"),
                        IdentifierName("Math")),
                    IdentifierName("Pow")))
                .WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(lhsExpr), Argument(rhsExpr) })));

            // System.Math.Pow returns double, so cast to int for integer expressions
            return CastExpression(
                PredefinedType(Token(SyntaxKind.IntKeyword)),
                powCall);
        }


        // Handle concatenation (++) by calling Fifth.System.List.op_PlusPlus
        if (binExp.Operator == Operator.Concatenate)
        {
            var lhsExpr = TranslateExpression(binExp.LHS);
            var rhsExpr = TranslateExpression(binExp.RHS);

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Fifth.System.List"),
                        IdentifierName("op_PlusPlus")))
                .WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(lhsExpr), Argument(rhsExpr) })));
        }

        var left = TranslateExpression(binExp.LHS);
        var right = TranslateExpression(binExp.RHS);

        // Wrap child binary expressions in parentheses if they have lower precedence
        // This ensures correct evaluation order (e.g., (1 + 2) * 3 stays parenthesized)
        var parentPrecedence = GetOperatorPrecedence(binExp.Operator);

        if (binExp.LHS is BinaryExp leftBinExp)
        {
            var leftPrecedence = GetOperatorPrecedence(leftBinExp.Operator);
            // Add parentheses if child has lower precedence (or equal for right-associative)
            if (leftPrecedence < parentPrecedence)
            {
                left = ParenthesizedExpression(left);
            }
        }

        if (binExp.RHS is BinaryExp rightBinExp)
        {
            var rightPrecedence = GetOperatorPrecedence(rightBinExp.Operator);
            // Add parentheses if child has lower precedence
            // For right side, also parenthesize if equal precedence (left-associative default)
            if (rightPrecedence < parentPrecedence ||
                (rightPrecedence == parentPrecedence && binExp.Operator != Operator.ArithmeticPow))
            {
                right = ParenthesizedExpression(right);
            }
        }

        var kind = binExp.Operator switch
        {
            Operator.ArithmeticAdd => SyntaxKind.AddExpression,
            Operator.ArithmeticSubtract => SyntaxKind.SubtractExpression,
            Operator.ArithmeticMultiply => SyntaxKind.MultiplyExpression,
            Operator.ArithmeticDivide => SyntaxKind.DivideExpression,
            Operator.ArithmeticMod => SyntaxKind.ModuloExpression,
            Operator.BitwiseAnd => SyntaxKind.BitwiseAndExpression,
            Operator.BitwiseOr => SyntaxKind.BitwiseOrExpression,
            Operator.BitwiseLeftShift => SyntaxKind.LeftShiftExpression,
            Operator.BitwiseRightShift => SyntaxKind.RightShiftExpression,
            Operator.LogicalXor => SyntaxKind.ExclusiveOrExpression,
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

        // For logical operators, convert int operands to bool (Fifth uses int for bool: 0=false, non-zero=true)
        if (binExp.Operator == Operator.LogicalAnd || binExp.Operator == Operator.LogicalOr)
        {
            left = EnsureBooleanExpression(left, binExp.LHS);
            right = EnsureBooleanExpression(right, binExp.RHS);
        }

        return BinaryExpression(kind, left, right);
    }

    private bool IsGraphType(Expression expr)
    {
        if (expr == null) return false;

        // Check if the expression's type is "graph" or "IGraph"
        var typeName = expr.Type?.ToString() ?? "";
        return typeName.Contains("graph", StringComparison.OrdinalIgnoreCase) ||
               typeName.Contains("IGraph", StringComparison.Ordinal);
    }

    /// <summary>
    /// Ensure an expression is suitable for use in a boolean context.
    /// Comparison operators already return bool, but int expressions need conversion.
    /// </summary>
    private ExpressionSyntax EnsureBooleanExpression(ExpressionSyntax expr, Expression originalExpr)
    {
        // Check if the original expression is a comparison/relational that already returns bool
        if (originalExpr is BinaryExp binExpr)
        {
            switch (binExpr.Operator)
            {
                case Operator.Equal:
                case Operator.NotEqual:
                case Operator.LessThan:
                case Operator.GreaterThan:
                case Operator.LessThanOrEqual:
                case Operator.GreaterThanOrEqual:
                    // These already return bool, no conversion needed
                    return expr;

                case Operator.LogicalAnd:
                case Operator.LogicalOr:
                    // These will be handled by TranslateBinaryExpression which already
                    // converts their operands, so the result is bool
                    return expr;
            }
        }

        // Check if the original expression is a unary logical NOT
        if (originalExpr is UnaryExp unaryExpr && unaryExpr.Operator == Operator.LogicalNot)
        {
            // Logical NOT already returns bool, no conversion needed
            return expr;
        }

        // For boolean literals, no conversion needed
        if (originalExpr is BooleanLiteralExp)
        {
            return expr;
        }

        // For other expressions (likely integers), add the != 0 check
        return IntToBoolConversion(expr);
    }

    private ExpressionSyntax TranslateFuncCallExpression(FuncCallExp funcCall)
    {
        // Instance method call (e.g., list.Add(item))?
        if (funcCall.Annotations != null &&
            funcCall.Annotations.TryGetValue("IsInstanceMethod", out var isInstanceObj) &&
            isInstanceObj is bool isInstance && isInstance &&
            funcCall.Annotations.TryGetValue("Target", out var targetObj) && targetObj is Expression targetExpr)
        {
            string methodName;
            if (funcCall.Annotations.TryGetValue("ExternalMethodName", out var extNameObj) && extNameObj is string extMethod)
            {
                methodName = SanitizeIdentifier(extMethod);
            }
            else
            {
                methodName = "UnknownMethod";
            }

            var candidateMethods = targetExpr.Type is FifthType.TDotnetType targetDotnet
                ? GetExternalMethodCandidates(targetDotnet.TheType, methodName, funcCall.InvocationArguments.Count, isStatic: false)
                : Array.Empty<MethodInfo>();

            var argList = new List<ArgumentSyntax>();
            for (var i = 0; i < funcCall.InvocationArguments.Count; i++)
            {
                var arg = funcCall.InvocationArguments[i];
                var argExpr = TranslateExpression(arg);

                argExpr = MaybeWrapExternalClosureArgument(candidateMethods, i, arg, argExpr);

                argList.Add(Argument(argExpr));
            }

            // Emit instance method call: target.Method(args)
            var targetSyntax = TranslateExpression(targetExpr);
            return InvocationExpression(
                MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                targetSyntax,
                CreateMethodNameSyntax(methodName, funcCall.TypeArguments)))
            .WithArgumentList(ArgumentList(SeparatedList(argList)));
        }

        // External (static) call annotated by TreeLinkageVisitor?
        if (funcCall.Annotations != null &&
            funcCall.Annotations.TryGetValue("ExternalType", out var extTypeObj) && extTypeObj is Type extType)
        {
            var typeName = extType.FullName ?? extType.Name;
            // Resolve method name preference order:
            // 1) Explicit ExternalMethodName (set by linkage)
            // 2) Resolved FunctionDef name
            // 3) Parser-provided FunctionName annotation
            // 4) Fallback: UnknownFunction (should be rare)
            string methodName;
            if (funcCall.Annotations.TryGetValue("ExternalMethodName", out var extNameObj) && extNameObj is string extMethod)
            {
                methodName = SanitizeIdentifier(extMethod);
            }
            else if (funcCall.FunctionDef != null)
            {
                methodName = SanitizeIdentifier(funcCall.FunctionDef.Name.ToString());
            }
            else if (funcCall.Annotations != null && funcCall.Annotations.TryGetValue("FunctionName", out var fnAnno) && fnAnno is string fnStr && !string.IsNullOrWhiteSpace(fnStr))
            {
                methodName = SanitizeIdentifier(fnStr);
            }
            else
            {
                methodName = "UnknownFunction";
            }

            var candidateMethods = GetExternalMethodCandidates(extType, methodName, funcCall.InvocationArguments.Count, isStatic: true);

            var argList = new List<ArgumentSyntax>();
            for (var i = 0; i < funcCall.InvocationArguments.Count; i++)
            {
                var arg = funcCall.InvocationArguments[i];
                var argExpr = TranslateExpression(arg);

                argExpr = MaybeWrapExternalClosureArgument(candidateMethods, i, arg, argExpr);

                argList.Add(Argument(argExpr));
            }

            // Always emit fully-qualified static call: Type.Method(args)
            var typeNameSyntax = CreateQualifiedName(typeName);
            var callMethodName = CreateMethodNameSyntax(methodName, funcCall.TypeArguments);

            return InvocationExpression(
                    MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    typeNameSyntax,
                    callMethodName))
                .WithArgumentList(ArgumentList(SeparatedList(argList)));
        }

        // Regular function call
        // Best-effort resolution for unbound calls: prefer FunctionDef, then FunctionName annotation
        string funcName;
        if (funcCall.FunctionDef != null)
        {
            funcName = SanitizeIdentifier(funcCall.FunctionDef.Name.ToString());
        }
        else if (funcCall.Annotations != null && funcCall.Annotations.TryGetValue("FunctionName", out var fnAnno2) && fnAnno2 is string fnStr2 && !string.IsNullOrWhiteSpace(fnStr2))
        {
            funcName = SanitizeIdentifier(fnStr2);
        }
        else
        {
            funcName = "UnknownFunction";
        }

        var arguments = new List<ArgumentSyntax>();
        for (var i = 0; i < funcCall.InvocationArguments.Count; i++)
        {
            var arg = funcCall.InvocationArguments[i];
            var argExpr = TranslateExpression(arg);

            if (funcCall.FunctionDef == null)
            {
                argExpr = MaybeWrapUnresolvedExternalClosureArgument(arg, argExpr);
            }
            else
            {
                argExpr = MaybeWrapExternalDelegateAsClosureArgument(funcCall, i, arg, argExpr);
            }

            arguments.Add(Argument(argExpr));
        }

        ExpressionSyntax callee;

        // If we're inside a non-Program class (e.g., a generated lambda closure),
        // module-level functions live on the static Program class.
        var inProgram = string.Equals(_currentContainingClassName, "Program", StringComparison.Ordinal);
        var methodNameSyntax = CreateMethodNameSyntax(funcName, funcCall.TypeArguments);
        if (!inProgram && _moduleLevelFunctionNames.Contains(funcName))
        {
            callee = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("Program"),
                methodNameSyntax);
        }
        else
        {
            callee = methodNameSyntax;
        }

        return InvocationExpression(callee)
            .WithArgumentList(ArgumentList(SeparatedList(arguments)));
    }

    private SimpleNameSyntax CreateMethodNameSyntax(string methodName, IReadOnlyList<FifthType>? typeArguments)
    {
        if (typeArguments == null || typeArguments.Count == 0)
            return IdentifierName(methodName);

        var typeSyntaxes = typeArguments
            .Select(typeArg => ParseTypeName(ExtractTypeName(typeArg)))
            .ToArray();

        return GenericName(Identifier(methodName))
            .WithTypeArgumentList(TypeArgumentList(SeparatedList(typeSyntaxes)));
    }

    private static SimpleNameSyntax CreateGenericMethodNameSyntax(string methodName, IReadOnlyList<string> typeArguments)
    {
        if (typeArguments.Count == 0)
            return IdentifierName(methodName);

        var typeSyntaxes = typeArguments
            .Select(typeArg => ParseTypeName(typeArg))
            .ToArray();

        return GenericName(Identifier(methodName))
            .WithTypeArgumentList(TypeArgumentList(SeparatedList(typeSyntaxes)));
    }

    private static IReadOnlyList<MethodInfo> GetExternalMethodCandidates(
        Type externalType,
        string methodName,
        int argumentCount,
        bool isStatic)
    {
        var flags = BindingFlags.Public | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
        return externalType
            .GetMethods(flags)
            .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal) && m.GetParameters().Length == argumentCount)
            .ToList();
    }

    private ExpressionSyntax MaybeWrapExternalClosureArgument(
        IReadOnlyList<MethodInfo> candidateMethods,
        int argIndex,
        Expression arg,
        ExpressionSyntax argExpr)
    {
        if (arg.Type is FifthType.TDotnetType dotnetType && typeof(Delegate).IsAssignableFrom(dotnetType.TheType))
            return argExpr;

        if (!TryBuildDelegateTypeName(arg, out var delegateTypeName))
            return argExpr;

        if (candidateMethods.Count > 0)
        {
            var expectsDelegate = candidateMethods
                .Select(m => m.GetParameters())
                .Where(parameters => argIndex < parameters.Length)
                .Any(parameters => ParameterExpectsDelegate(parameters[argIndex]));

            if (!expectsDelegate)
                return argExpr;
        }

        return WrapClosureAsDelegate(argExpr, delegateTypeName);
    }

    private ExpressionSyntax MaybeWrapUnresolvedExternalClosureArgument(Expression arg, ExpressionSyntax argExpr)
    {
        return MaybeWrapExternalClosureArgument(Array.Empty<MethodInfo>(), 0, arg, argExpr);
    }

    private static bool ParameterExpectsDelegate(ParameterInfo parameter)
    {
        var paramType = parameter.ParameterType;
        return typeof(Delegate).IsAssignableFrom(paramType);
    }

    private bool TryBuildDelegateTypeName(Expression arg, out string delegateTypeName)
    {
        delegateTypeName = string.Empty;

        if (arg.Type is FifthType.TFunc funcType)
        {
            var inputs = funcType.InputTypes.Select(ExtractTypeName).ToList();
            if (funcType.OutputType is FifthType.TVoidType)
            {
                delegateTypeName = inputs.Count == 0
                    ? "System.Action"
                    : $"System.Action<{string.Join(", ", inputs)}>";
                return true;
            }

            var output = ExtractTypeName(funcType.OutputType);
            delegateTypeName = inputs.Count == 0
                ? $"System.Func<{output}>"
                : $"System.Func<{string.Join(", ", inputs)}, {output}>";
            return true;
        }

        if (TryGetClosureInterfaceSignature(arg, out var inputTypes, out var outputType, out var isAction))
        {
            var mappedInputs = inputTypes.Select(MapTypeName).ToList();

            if (isAction || string.Equals(outputType, "void", StringComparison.Ordinal))
            {
                delegateTypeName = mappedInputs.Count == 0
                    ? "System.Action"
                    : $"System.Action<{string.Join(", ", mappedInputs)}>";
                return true;
            }

            if (string.IsNullOrWhiteSpace(outputType))
                return false;

            var mappedOutput = MapTypeName(outputType);
            delegateTypeName = mappedInputs.Count == 0
                ? $"System.Func<{mappedOutput}>"
                : $"System.Func<{string.Join(", ", mappedInputs)}, {mappedOutput}>";
            return true;
        }

        return false;
    }

    private ExpressionSyntax WrapClosureAsDelegate(ExpressionSyntax closureExpr, FifthType.TFunc funcType)
    {
        var inputs = funcType.InputTypes.Select(ExtractTypeName).ToList();

        if (funcType.OutputType is FifthType.TVoidType)
        {
            var actionTypeName = inputs.Count == 0
                ? "System.Action"
                : $"System.Action<{string.Join(", ", inputs)}>";

            return ObjectCreationExpression(ParseTypeName(actionTypeName))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                    Argument(MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        closureExpr,
                        IdentifierName("Apply"))))));
        }

        var output = ExtractTypeName(funcType.OutputType);
        var funcTypeName = inputs.Count == 0
            ? $"System.Func<{output}>"
            : $"System.Func<{string.Join(", ", inputs)}, {output}>";

        return ObjectCreationExpression(ParseTypeName(funcTypeName))
            .WithArgumentList(ArgumentList(SingletonSeparatedList(
                Argument(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    closureExpr,
                    IdentifierName("Apply"))))));
    }

    private static ExpressionSyntax WrapClosureAsDelegate(ExpressionSyntax closureExpr, string delegateTypeName)
    {
        return ObjectCreationExpression(ParseTypeName(delegateTypeName))
            .WithArgumentList(ArgumentList(SingletonSeparatedList(
                Argument(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    closureExpr,
                    IdentifierName("Apply"))))));
    }

    private void CacheClosureInterfaces(ModuleDef module)
    {
        _closureInterfaceByClass.Clear();

        foreach (var classDef in module.Classes)
        {
            if (classDef.BaseClasses == null || classDef.BaseClasses.Count == 0)
                continue;

            var closureInterface = classDef.BaseClasses.FirstOrDefault(baseClass =>
                baseClass.StartsWith("Fifth.System.Runtime.IClosure", StringComparison.Ordinal) ||
                baseClass.StartsWith("Fifth.System.Runtime.IActionClosure", StringComparison.Ordinal));

            if (!string.IsNullOrWhiteSpace(closureInterface))
            {
                var className = SanitizeIdentifier(classDef.Name.ToString());
                _closureInterfaceByClass[className] = closureInterface;
            }
        }
    }

    private bool TryGetClosureInterfaceSignature(Expression arg, out List<string> inputTypes, out string? outputType, out bool isAction)
    {
        inputTypes = new List<string>();
        outputType = null;
        isAction = false;

        if (arg.Type is not FifthType.TType type)
            return false;

        var className = SanitizeIdentifier(type.Name.ToString());
        if (!_closureInterfaceByClass.TryGetValue(className, out var interfaceName))
            return false;

        isAction = interfaceName.StartsWith("Fifth.System.Runtime.IActionClosure", StringComparison.Ordinal);

        var openIndex = interfaceName.IndexOf('<');
        if (openIndex < 0)
        {
            return isAction;
        }

        var closeIndex = interfaceName.LastIndexOf('>');
        if (closeIndex <= openIndex)
            return false;

        var args = interfaceName.Substring(openIndex + 1, closeIndex - openIndex - 1);
        var typeParts = SplitTopLevelCommaSeparated(args).Select(t => t.Trim()).Where(t => t.Length > 0).ToList();

        if (isAction)
        {
            inputTypes.AddRange(typeParts);
            return true;
        }

        if (typeParts.Count == 0)
            return false;

        if (typeParts.Count == 1)
        {
            outputType = typeParts[0];
            return true;
        }

        outputType = typeParts[^1];
        inputTypes.AddRange(typeParts.Take(typeParts.Count - 1));
        return true;
    }


    private ExpressionSyntax MaybeWrapExternalDelegateAsClosureArgument(
        FuncCallExp funcCall,
        int argIndex,
        Expression arg,
        ExpressionSyntax argExpr)
    {
        if (funcCall.FunctionDef == null)
            return argExpr;

        if (argIndex >= funcCall.FunctionDef.Params.Count)
            return argExpr;

        var param = funcCall.FunctionDef.Params[argIndex];
        if (param.TypeName == null)
            return argExpr;

        if (!TryParseFunctionType(param.TypeName.ToString(), out var inputTypes, out var outputType))
            return argExpr;

        if (arg.Type is not FifthType.TDotnetType dotnetType)
            return argExpr;

        var delegateType = dotnetType.TheType;
        if (!typeof(Delegate).IsAssignableFrom(delegateType))
            return argExpr;

        var mappedInputs = inputTypes.Select(MapTypeName).ToList();
        var mappedOutput = MapTypeName(outputType);

        var adapterTypeName = "Fifth.System.Runtime.ClosureAdapters";
        var methodName = string.Equals(outputType.Trim(), "void", StringComparison.Ordinal)
            ? "FromAction"
            : "FromFunc";

        SimpleNameSyntax adapterMethodName;
        if (string.Equals(outputType.Trim(), "void", StringComparison.Ordinal))
        {
            adapterMethodName = mappedInputs.Count == 0
                ? IdentifierName(methodName)
                : CreateGenericMethodNameSyntax(methodName, mappedInputs);
        }
        else
        {
            var typeArgs = mappedInputs.Concat(new[] { mappedOutput }).ToList();
            adapterMethodName = CreateGenericMethodNameSyntax(methodName, typeArgs);
        }

        var adapterCallee = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            CreateQualifiedName(adapterTypeName),
            adapterMethodName);

        return InvocationExpression(adapterCallee)
            .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(argExpr))));
    }

    private bool IsListOrArrayType(FifthType? type)
    {
        if (type == null) return false;
        return type is FifthType.TArrayOf || type is FifthType.TListOf ||
               (type is FifthType.TGenericInstance genericInst &&
                genericInst.GenericTypeDefinition.ToString() == "List");
    }

    private ExpressionSyntax TranslateMemberAccessExpression(MemberAccessExp memberAccess)
    {
        var objExpr = TranslateExpression(memberAccess.LHS);

        // RHS can be a VarRefExp or FuncCallExp
        if (memberAccess.RHS is VarRefExp varRef)
        {
            var memberName = SanitizeIdentifier(varRef.VarName);

            // Map Length to Count for Lists (which we use for arrays now)
            if (memberName == "Length" && IsListOrArrayType(memberAccess.LHS.Type))
            {
                memberName = "Count";
            }

            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                objExpr,
                IdentifierName(memberName));
        }
        else if (memberAccess.RHS is FuncCallExp funcCall)
        {
            // Static external call? Use annotations to emit Type.Method(args)
            // For extension methods in a MemberAccessExp, the LHS is the target object
            if (funcCall.Annotations != null &&
                funcCall.Annotations.TryGetValue("ExternalType", out var extTypeObj) && extTypeObj is Type extType)
            {
                var typeName = extType.FullName ?? extType.Name;
                // Resolve external method name with sane fallbacks (ExternalMethodName -> FunctionDef -> FunctionName)
                string resolvedMethodName;
                if (funcCall.Annotations.TryGetValue("ExternalMethodName", out var extNameObj) && extNameObj is string extMethod)
                {
                    resolvedMethodName = SanitizeIdentifier(extMethod);
                }
                else if (funcCall.FunctionDef != null)
                {
                    resolvedMethodName = SanitizeIdentifier(funcCall.FunctionDef.Name.ToString());
                }
                else if (funcCall.Annotations != null && funcCall.Annotations.TryGetValue("FunctionName", out var fnAnno3) && fnAnno3 is string fnStr3 && !string.IsNullOrWhiteSpace(fnStr3))
                {
                    resolvedMethodName = SanitizeIdentifier(fnStr3);
                }
                else
                {
                    resolvedMethodName = "UnknownFunction";
                }

                // Check if the LHS is a type reference (e.g., VarRefExp with VarName = "KG" or resolved type name)
                bool lhsIsTypeReference = memberAccess.LHS is VarRefExp varRefLhs &&
                    (varRefLhs.VarName == "KG" || varRefLhs.VarName == extType.Name || varRefLhs.VarName == "std");

                var typeNameSyntax = CreateQualifiedName(typeName);

                if (lhsIsTypeReference)
                {
                    // Static method call: KG.Method(args)
                    var arguments = new List<ArgumentSyntax>();
                    var candidateMethods = GetExternalMethodCandidates(extType, resolvedMethodName, funcCall.InvocationArguments.Count, isStatic: true);
                    for (var i = 0; i < funcCall.InvocationArguments.Count; i++)
                    {
                        var arg = funcCall.InvocationArguments[i];
                        var argExpr = TranslateExpression(arg);
                        argExpr = MaybeWrapExternalClosureArgument(candidateMethods, i, arg, argExpr);
                        arguments.Add(Argument(argExpr));
                    }

                    return InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                typeNameSyntax,
                                IdentifierName(resolvedMethodName)))
                        .WithArgumentList(ArgumentList(SeparatedList(arguments)));
                }
                else
                {
                    // For external calls on an instance (extension methods), emit static invocation
                    // Special handling for Fifth.System.KG extension methods: KG.Method(obj, args)
                    var arguments = new List<ArgumentSyntax>();
                    // First argument is the instance (for extension methods)
                    arguments.Add(Argument(objExpr));
                    var candidateMethods = GetExternalMethodCandidates(extType, resolvedMethodName, funcCall.InvocationArguments.Count + 1, isStatic: true);
                    for (var i = 0; i < funcCall.InvocationArguments.Count; i++)
                    {
                        var arg = funcCall.InvocationArguments[i];
                        var argExpr = TranslateExpression(arg);
                        argExpr = MaybeWrapExternalClosureArgument(candidateMethods, i + 1, arg, argExpr);
                        arguments.Add(Argument(argExpr));
                    }

                    return InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                typeNameSyntax,
                                IdentifierName(resolvedMethodName)))
                        .WithArgumentList(ArgumentList(SeparatedList(arguments)));
                }
            }

            // Instance call: obj.Method(args)
            // Instance call fallback: prefer FunctionDef name, then FunctionName annotation
            string methodName;
            if (funcCall.FunctionDef != null)
            {
                methodName = SanitizeIdentifier(funcCall.FunctionDef.Name.ToString());
            }
            else if (funcCall.Annotations != null && funcCall.Annotations.TryGetValue("FunctionName", out var fnAnno4) && fnAnno4 is string fnStr4 && !string.IsNullOrWhiteSpace(fnStr4))
            {
                methodName = SanitizeIdentifier(fnStr4);
            }
            else
            {
                methodName = "UnknownFunction";
            }

            var argumentsInst2 = new List<ArgumentSyntax>();
            foreach (var arg in funcCall.InvocationArguments)
            {
                var argExpr = TranslateExpression(arg);
                argumentsInst2.Add(Argument(argExpr));
            }

            // Special case: translate collection Item(idx) access to indexer syntax obj[idx]
            if (string.Equals(methodName, "Item", StringComparison.Ordinal) && argumentsInst2.Count == 1)
            {
                return ElementAccessExpression(objExpr)
                    .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(argumentsInst2[0])));
            }

            var member2 = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                objExpr,
                IdentifierName(methodName));

            return InvocationExpression(member2)
                .WithArgumentList(ArgumentList(SeparatedList(argumentsInst2)));
        }

        // Fallback
        return objExpr;
    }

    private ExpressionSyntax TranslateIndexerExpression(IndexerExpression indexer)
    {
        var target = TranslateExpression(indexer.IndexExpression);
        var index = TranslateExpression(indexer.OffsetExpression);

        // Generate: target[index]
        return ElementAccessExpression(target)
            .WithArgumentList(
                BracketedArgumentList(
                    SingletonSeparatedList(Argument(index))));
    }

    private ExpressionSyntax TranslateListExpression(List list, FifthType? targetType)
    {
        // Handle specific List subtypes
        if (list is ListLiteral listLiteral)
        {
            // Determine if we should generate a List<T> or T[]
            bool generateList = false;
            FifthType? elementType = null;

            if (targetType is FifthType.TListOf listOf)
            {
                generateList = true;
                elementType = listOf.ElementType;
            }
            else if (targetType is FifthType.TArrayOf arrayOf)
            {
                generateList = true;
                elementType = arrayOf.ElementType;
            }
            else if (targetType is FifthType.TGenericInstance genericInst &&
                     genericInst.GenericTypeDefinition.ToString() == "List")
            {
                // Handle explicit List<T> if it comes through as generic instance
                generateList = true;
                if (genericInst.TypeArguments.Count > 0)
                {
                    elementType = genericInst.TypeArguments[0];
                }
            }

            var elements = new List<ExpressionSyntax>();
            foreach (var item in listLiteral.ElementExpressions)
            {
                // Pass the element type as target type for recursive translation
                // This ensures nested lists know their expected type
                elements.Add(TranslateExpression(item, elementType));
            }

            if (generateList && elementType != null)
            {
                // Generate: new List<T> { ... }
                var elementTypeName = ExtractTypeName(elementType);
                var listType = GenericName(Identifier("List"))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(ParseTypeName(elementTypeName))));

                return ObjectCreationExpression(listType)
                    .WithArgumentList(ArgumentList())
                    .WithInitializer(
                        InitializerExpression(
                            SyntaxKind.CollectionInitializerExpression,
                            SeparatedList(elements)));
            }
            else
            {
                // Use implicit array creation: new[] { ... }
                return ImplicitArrayCreationExpression(
                    InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SeparatedList(elements)));
            }
        }

        // Fallback for unsupported list types
        return DefaultExpression(IdentifierName("object"));
    }

    private ExpressionSyntax TranslateObjectInitializerExpression(ObjectInitializerExp objInit)
    {
        // Get the type name - TypeToInitialize is a FifthType, need to extract the name properly
        if (objInit.TypeToInitialize is FifthType.TArrayOf arrayType)
        {
            return TranslateArrayCreationExpression(objInit, arrayType);
        }

        // Also handle TListOf which is used for Fifth's [T] syntax
        if (objInit.TypeToInitialize is FifthType.TListOf listType)
        {
            return TranslateListCreationExpression(objInit, listType);
        }

        string typeName2;
        try
        {
            // Try to get the type name from the FifthType
            if (objInit.TypeToInitialize != null)
            {
                typeName2 = SanitizeIdentifier(objInit.TypeToInitialize.Name.ToString());
            }
            else
            {
                typeName2 = "object";
            }
        }
        catch
        {
            typeName2 = "object";
        }

        // Build constructor argument list if present
        var ctorArgs = new List<ArgumentSyntax>();
        if (objInit.ConstructorArguments != null && objInit.ConstructorArguments.Count > 0)
        {
            foreach (var arg in objInit.ConstructorArguments)
            {
                var argExpr = TranslateExpression(arg);
                ctorArgs.Add(Argument(argExpr));
            }
        }

        var argList = ArgumentList(SeparatedList(ctorArgs));

        // If there are no property initializers, create simple object creation: new TypeName(args)
        if (objInit.PropertyInitialisers == null || objInit.PropertyInitialisers.Count == 0)
        {
            return ObjectCreationExpression(IdentifierName(typeName2))
                .WithArgumentList(argList);
        }

        // Create object creation with initializers: new TypeName(args) { Prop = Value, ... }
        var initializers = new List<ExpressionSyntax>();
        foreach (var propInit in objInit.PropertyInitialisers)
        {
            // Extract property name from PropertyDef to avoid potential stack overflow from ToString()
            string propName;
            try
            {
                if (propInit.PropertyToInitialize?.Property != null)
                {
                    propName = SanitizeIdentifier(propInit.PropertyToInitialize.Property.Name.ToString());
                }
                else
                {
                    // Fallback - try ToString() on PropertyToInitialize
                    propName = SanitizeIdentifier(propInit.PropertyToInitialize.ToString());
                }
            }
            catch
            {
                propName = "UnknownProperty";
            }

            var valueExpr = TranslateExpression(propInit.RHS);

            var assignment = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                IdentifierName(propName),
                valueExpr);

            initializers.Add(assignment);
        }

        return ObjectCreationExpression(IdentifierName(typeName2))
            .WithArgumentList(argList)
            .WithInitializer(
                InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression,
                    SeparatedList(initializers)));
    }

    private ExpressionSyntax TranslateArrayCreationExpression(ObjectInitializerExp objInit, FifthType.TArrayOf arrayType)
    {
        // Treat Fifth array syntax [T] as List<T> for dynamic operations (Add, RemoveAt, Item). Generate new List<T>() with optional initializer.
        var elementTypeName = ExtractTypeName(arrayType.ElementType);
        var listType = GenericName(Identifier("List"))
            .WithTypeArgumentList(
                TypeArgumentList(
                    SingletonSeparatedList<TypeSyntax>(ParseTypeName(elementTypeName))));

        // Check if we have a size argument in ConstructorArguments OR in Annotations["ArraySize"]
        ExpressionSyntax? sizeExpr = null;
        if (objInit.ConstructorArguments != null && objInit.ConstructorArguments.Count == 1)
        {
            sizeExpr = TranslateExpression(objInit.ConstructorArguments[0]);
        }
        else if (objInit.Annotations != null && objInit.Annotations.TryGetValue("ArraySize", out var sizeObj) && sizeObj is Expression sizeExpNode)
        {
            sizeExpr = TranslateExpression(sizeExpNode);
        }

        if (sizeExpr != null)
        {
            // Generate: new List<T>(new T[size]) to initialize with default values
            var arrayCreation = ArrayCreationExpression(
                ArrayType(ParseTypeName(elementTypeName))
                    .WithRankSpecifiers(
                        SingletonList(
                            ArrayRankSpecifier(
                                SingletonSeparatedList(sizeExpr)))));

            return ObjectCreationExpression(listType)
                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(arrayCreation))));
        }

        ObjectCreationExpressionSyntax creation = ObjectCreationExpression(listType)
            .WithArgumentList(ArgumentList());

        if (objInit.PropertyInitialisers != null && objInit.PropertyInitialisers.Count > 0)
        {
            var elements = new List<ExpressionSyntax>();
            foreach (var propInit in objInit.PropertyInitialisers)
            {
                if (propInit.RHS != null)
                {
                    elements.Add(TranslateExpression(propInit.RHS));
                }
            }
            creation = creation.WithInitializer(
                InitializerExpression(
                    SyntaxKind.CollectionInitializerExpression,
                    SeparatedList(elements)));
        }

        return creation;
    }

    private ExpressionSyntax TranslateListCreationExpression(ObjectInitializerExp objInit, FifthType.TListOf listType)
    {
        // Handle TListOf same as TArrayOf - generate new List<T>()
        var elementTypeName = ExtractTypeName(listType.ElementType);
        var genericListType = GenericName(Identifier("List"))
            .WithTypeArgumentList(
                TypeArgumentList(
                    SingletonSeparatedList<TypeSyntax>(ParseTypeName(elementTypeName))));

        ObjectCreationExpressionSyntax creation = ObjectCreationExpression(genericListType)
            .WithArgumentList(ArgumentList());

        if (objInit.PropertyInitialisers != null && objInit.PropertyInitialisers.Count > 0)
        {
            var elements = new List<ExpressionSyntax>();
            foreach (var propInit in objInit.PropertyInitialisers)
            {
                if (propInit.RHS != null)
                {
                    elements.Add(TranslateExpression(propInit.RHS));
                }
            }
            creation = creation.WithInitializer(
                InitializerExpression(
                    SyntaxKind.CollectionInitializerExpression,
                    SeparatedList(elements)));
        }

        return creation;
    }

    private ExpressionSyntax TranslateUnaryExpression(UnaryExp unary)
    {
        var operand = TranslateExpression(unary.Operand);

        // Wrap binary expressions in parentheses to ensure correct precedence
        // For example: !(1 < 2) should not become !1 < 2
        if (unary.Operand is BinaryExp)
        {
            operand = ParenthesizedExpression(operand);
        }

        var kind = unary.Operator switch
        {
            Operator.ArithmeticNegative => SyntaxKind.UnaryMinusExpression,
            Operator.LogicalNot => SyntaxKind.LogicalNotExpression,
            _ => SyntaxKind.UnaryPlusExpression
        };

        return PrefixUnaryExpression(kind, operand);
    }

    private ExpressionSyntax TranslateTripleLiteralExpression(TripleLiteralExp triple)
    {
        // Translate <subj, pred, obj> to Fifth.System.KG.CreateTriple(KG.CreateUriNode(...), KG.CreateUriNode(...), KG.CreateLiteralNode(...))
        ExpressionSyntax TranslateNode(Expression e)
        {
            // If it's a URI literal, wrap with KG.CreateUriNode(value)
            if (e is UriLiteralExp)
            {
                var uriExpr = TranslateExpression(e);
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateQualifiedName("Fifth.System.KG"),
                            IdentifierName("CreateUriNode")))
                    .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(uriExpr))));
            }

            // For primitives, create literal nodes
            if (e is Int32LiteralExp || e is Int64LiteralExp || e is Float8LiteralExp || e is Float4LiteralExp
                || e is BooleanLiteralExp || e is StringLiteralExp)
            {
                var valExpr = TranslateExpression(e);
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateQualifiedName("Fifth.System.KG"),
                            IdentifierName("CreateLiteralNode")))
                    .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(valExpr))));
            }

            // Fallback: assume it's already an INode-producing expression (e.g., a variable of INode)
            // Pass through without wrapping to avoid type mismatches
            return TranslateExpression(e);
        }

        var subjNode = TranslateNode(triple.SubjectExp);
        var predNode = TranslateNode(triple.PredicateExp);
        var objNode = TranslateNode(triple.ObjectExp);

        var args = new List<ArgumentSyntax>
        {
            Argument(subjNode),
            Argument(predNode),
            Argument(objNode)
        };

        return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Fifth.System.KG"),
                    IdentifierName("CreateTriple")))
            .WithArgumentList(ArgumentList(SeparatedList(args)));
    }

    private string MapTypeName(string? fifthTypeName)
    {
        if (fifthTypeName == null) return "object";

        // Handle function types: [T1, T2] -> R
        // Map to runtime closure interfaces: IClosure<..., R> or IActionClosure<...>
        if (TryParseFunctionType(fifthTypeName, out var inputTypes, out var outputType))
        {
            var mappedInputs = inputTypes.Select(MapTypeName).ToList();
            var mappedOutput = MapTypeName(outputType);

            if (string.Equals(outputType.Trim(), "void", StringComparison.Ordinal))
            {
                if (mappedInputs.Count == 0)
                {
                    return "Fifth.System.Runtime.IActionClosure";
                }
                return $"Fifth.System.Runtime.IActionClosure<{string.Join(", ", mappedInputs)}>";
            }

            if (mappedInputs.Count == 0)
            {
                return $"Fifth.System.Runtime.IClosure<{mappedOutput}>";
            }

            return $"Fifth.System.Runtime.IClosure<{string.Join(", ", mappedInputs)}, {mappedOutput}>";
        }

        // Handle list/array types - Fifth uses [T] syntax, C# uses T[]
        // Examples: [int] -> int[], [[int]] -> int[][]
        if (fifthTypeName.StartsWith("[") && fifthTypeName.EndsWith("]"))
        {
            // Recursively convert bracketed types to nested List<...>
            var innerType = fifthTypeName.Substring(1, fifthTypeName.Length - 2);
            var mappedInner = MapTypeName(innerType);
            return $"System.Collections.Generic.List<{mappedInner}>";
        }

        // Handle C# array syntax T[] -> List<T> to unify type system
        if (fifthTypeName.EndsWith("[]"))
        {
            var innerType = fifthTypeName.Substring(0, fifthTypeName.Length - 2);
            var mappedInner = MapTypeName(innerType);
            return $"System.Collections.Generic.List<{mappedInner}>";
        }

        // Also handle legacy list<T> syntax for compatibility
        if (fifthTypeName.StartsWith("list<") && fifthTypeName.EndsWith(">"))
        {
            var innerType = fifthTypeName.Substring(5, fifthTypeName.Length - 6);
            var mappedInner = MapTypeName(innerType);
            return $"System.Collections.Generic.List<{mappedInner}>";
        }

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
            "datetime" => "System.DateTimeOffset",
            "graph" => "VDS.RDF.IGraph",
            "triple" => "Fifth.System.Triple",
            "store" => "Fifth.System.Store",
            "Store" => "Fifth.System.Store",
            "void" => "void",
            "var" => "var",
            "unknown" => "object", // Map unknown types to object to allow compilation
            _ => fifthTypeName // Keep custom types as-is
        };
    }

    private static bool TryParseFunctionType(string input, out List<string> inputTypes, out string outputType)
    {
        inputTypes = [];
        outputType = string.Empty;

        var s = input.Trim();
        var arrowIndex = s.IndexOf("->", StringComparison.Ordinal);
        if (arrowIndex < 0)
        {
            return false;
        }

        var left = s.Substring(0, arrowIndex).Trim();
        var right = s.Substring(arrowIndex + 2).Trim();

        if (!left.StartsWith("[", StringComparison.Ordinal) || !left.EndsWith("]", StringComparison.Ordinal))
        {
            return false;
        }

        var args = left.Substring(1, left.Length - 2).Trim();
        outputType = right;

        if (string.IsNullOrWhiteSpace(args))
        {
            return true;
        }

        foreach (var part in SplitTopLevelCommaSeparated(args))
        {
            var t = part.Trim();
            if (!string.IsNullOrWhiteSpace(t))
            {
                inputTypes.Add(t);
            }
        }

        return true;
    }

    private static IEnumerable<string> SplitTopLevelCommaSeparated(string s)
    {
        var start = 0;
        var angle = 0;
        var square = 0;

        for (var i = 0; i < s.Length; i++)
        {
            var ch = s[i];
            switch (ch)
            {
                case '<':
                    angle++;
                    break;
                case '>':
                    angle = Math.Max(0, angle - 1);
                    break;
                case '[':
                    square++;
                    break;
                case ']':
                    square = Math.Max(0, square - 1);
                    break;
                case ',':
                    if (angle == 0 && square == 0)
                    {
                        yield return s.Substring(start, i - start);
                        start = i + 1;
                    }
                    break;
            }
        }

        if (start <= s.Length)
        {
            yield return s.Substring(start);
        }
    }

    private TypeSyntax CreateTypeSyntax(string typeName, ast.CollectionType collectionType)
    {
        var mappedType = MapTypeName(typeName);

        // If collection type is Array or List, wrap as List<T>
        // Note: We intentionally wrap even if mappedType is already a List<T> to support nested lists (e.g. [[int]])
        if (collectionType == ast.CollectionType.Array || collectionType == ast.CollectionType.List)
        {
            mappedType = $"System.Collections.Generic.List<{mappedType}>";
        }

        return ParseTypeName(mappedType);
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
            // Use CreateTypeSyntax to handle collection types correctly
            var paramTypeSyntax = param.TypeName != null
                ? CreateTypeSyntax(param.TypeName.ToString(), param.CollectionType)
                : ParseTypeName("object");

            parameters.Add(
                Parameter(Identifier(paramName))
                    .WithType(paramTypeSyntax));
        }

        // Build method body
        // Clear the declared variables set for this method
        _declaredVariables.Clear();
        // Add parameters to declared variables
        foreach (var param in funcDef.Params)
        {
            _declaredVariables.Add(SanitizeIdentifier(param.Name.ToString()));
        }

        var priorReturnType = _currentReturnType;
        _currentReturnType = funcDef.ReturnType;
        var body = BuildBlockStatement(funcDef.Body);
        _currentReturnType = priorReturnType;

        // Add a discard local: object __discard = default(object);
        var discardDecl = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("object"))
                .AddVariables(
                    VariableDeclarator(Identifier("__discard"))
                        .WithInitializer(
                            EqualsValueClause(
                                DefaultExpression(IdentifierName("object"))))));
        body = body.WithStatements(body.Statements.Insert(0, discardDecl));

        // For non-void functions, ensure all code paths return a value by adding a fallback throw
        // This handles guard clauses that may be exhaustive but the C# compiler can't verify
        if (returnTypeName != "void")
        {
            var lastStatement = body.Statements.LastOrDefault();
            var endsWithReturn = lastStatement is ReturnStatementSyntax;

            if (!endsWithReturn)
            {
                // Add: throw new InvalidOperationException("No matching guard clause");
                var throwStmt = ThrowStatement(
                    ObjectCreationExpression(
                        IdentifierName("InvalidOperationException"))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal("No matching guard clause")))))));

                body = body.AddStatements(throwStmt);
            }
        }

        var methodDecl = MethodDeclaration(
            ParseTypeName(returnTypeName),
            Identifier(methodName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .WithParameterList(ParameterList(SeparatedList(parameters)))
            .WithBody(body);

        // Add type parameters if the function is generic (T093, T095)
        if (funcDef.TypeParameters.Count > 0)
        {
            var typeParams = funcDef.TypeParameters
                .Select(tp => TypeParameter(SanitizeIdentifier(tp.Name.Value)))
                .ToArray();
            methodDecl = methodDecl.AddTypeParameterListParameters(typeParams);

            // Add constraints for type parameters (T094)
            foreach (var typeParam in funcDef.TypeParameters)
            {
                var constraints = BuildTypeParameterConstraints(typeParam);
                if (constraints != null)
                {
                    methodDecl = methodDecl.AddConstraintClauses(constraints);
                }
            }
        }

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

    private MemberDeclarationSyntax BuildConstructorDeclaration(FunctionDef funcDef, MappingTable mapping)
    {
        var constructorName = SanitizeIdentifier(funcDef.Name.ToString());

        // Build parameters
        var parameters = new List<ParameterSyntax>();
        foreach (var param in funcDef.Params)
        {
            var paramName = SanitizeIdentifier(param.Name.ToString());
            // Use CreateTypeSyntax to handle collection types correctly
            var paramTypeSyntax = param.TypeName != null
                ? CreateTypeSyntax(param.TypeName.ToString(), param.CollectionType)
                : ParseTypeName("object");

            parameters.Add(
                Parameter(Identifier(paramName))
                    .WithType(paramTypeSyntax));
        }

        // Build constructor body
        _declaredVariables.Clear();
        foreach (var param in funcDef.Params)
        {
            _declaredVariables.Add(SanitizeIdentifier(param.Name.ToString()));
        }

        var body = BuildBlockStatement(funcDef.Body);

        // Build base constructor initializer if present
        ConstructorInitializerSyntax? initializer = null;
        if (funcDef.BaseCall != null && funcDef.BaseCall.Arguments != null)
        {
            var baseArgs = new List<ArgumentSyntax>();
            foreach (var arg in funcDef.BaseCall.Arguments)
            {
                var argExpr = TranslateExpression(arg);
                if (argExpr != null)
                {
                    baseArgs.Add(Argument(argExpr));
                }
            }
            initializer = ConstructorInitializer(
                SyntaxKind.BaseConstructorInitializer,
                ArgumentList(SeparatedList(baseArgs)));
        }

        // Create constructor declaration
        var constructorDecl = ConstructorDeclaration(Identifier(constructorName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithParameterList(ParameterList(SeparatedList(parameters)))
            .WithBody(body);

        if (initializer != null)
        {
            constructorDecl = constructorDecl.WithInitializer(initializer);
        }

        // Add mapping entry
        var nodeId = $"ctor_{constructorName}_{Guid.NewGuid().ToString("N")[..8]}";
        var syntaxText = constructorDecl.NormalizeWhitespace().ToFullString();
        var lines = syntaxText.Split('\n');

        mapping.Add(new MappingEntry(
            nodeId,
            0,
            1,
            1,
            lines.Length,
            lines.LastOrDefault()?.Length ?? 1));

        return constructorDecl;
    }

    /// <summary>
    /// Extract a simple type name string from a FifthType
    /// </summary>
    private string ExtractTypeName(FifthType? fifthType)
    {
        if (fifthType == null)
            return "void";

        // Handle specific FifthType discriminated union cases first
        switch (fifthType)
        {
            case FifthType.TVoidType:
                return "void";

            case FifthType.TDotnetType dotnetType:
                return dotnetType.TheType.Name;

            case FifthType.TGenericParameter genericParam:
                // For generic parameters like T, extract the parameter name
                return SanitizeIdentifier(genericParam.ParameterName.Value);

            case FifthType.TArrayOf arrayOf:
                // For arrays like [T], recursively extract element type and add []
                var elementTypeName = ExtractTypeName(arrayOf.ElementType);
                return $"System.Collections.Generic.List<{elementTypeName}>";

            case FifthType.TListOf listOf:
                // For lists, treat as arrays for C# generation
                var listElementTypeName = ExtractTypeName(listOf.ElementType);
                return $"System.Collections.Generic.List<{listElementTypeName}>";

            case FifthType.TGenericInstance genericInstance:
                // For generic instantiations like Stack<int>, format with type arguments
                var baseTypeName = SanitizeIdentifier(genericInstance.GenericTypeDefinition.ToString());
                var typeArgs = string.Join(", ", genericInstance.TypeArguments.Select(ExtractTypeName));
                return $"{baseTypeName}<{typeArgs}>";

            case FifthType.TFunc func:
                // Map to runtime closure interfaces
                if (func.OutputType is FifthType.TVoidType)
                {
                    if (func.InputTypes.Count == 0)
                    {
                        return "Fifth.System.Runtime.IActionClosure";
                    }
                    var inArgs = string.Join(", ", func.InputTypes.Select(ExtractTypeName));
                    return $"Fifth.System.Runtime.IActionClosure<{inArgs}>";
                }
                else
                {
                    var ret = ExtractTypeName(func.OutputType);
                    if (func.InputTypes.Count == 0)
                    {
                        return $"Fifth.System.Runtime.IClosure<{ret}>";
                    }
                    var inArgs = string.Join(", ", func.InputTypes.Select(ExtractTypeName));
                    return $"Fifth.System.Runtime.IClosure<{inArgs}, {ret}>";
                }
        }

        // Fallback: try to use the Name property
        try
        {
            var typeName = fifthType.Name.ToString();
            return MapTypeName(typeName);
        }
        catch
        {
            // Last resort fallback
            return "object";
        }
    }

    /// <summary>
    /// Convert an integer expression to a boolean expression for use in C# boolean contexts.
    /// In Fifth, 0 is false and non-zero is true.
    /// </summary>
    private static ExpressionSyntax IntToBoolConversion(ExpressionSyntax expr)
    {
        // Generate: (expr) != 0
        return BinaryExpression(
            SyntaxKind.NotEqualsExpression,
            ParenthesizedExpression(expr),
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)));
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
