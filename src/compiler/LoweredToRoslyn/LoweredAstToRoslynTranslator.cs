namespace compiler;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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

public class LoweredAstToRoslynTranslator
{
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
            var methodDecl = BuildMethodDeclaration(method, mapping);
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

    private MethodDeclarationSyntax BuildMethodDeclaration(LoweredMethod method, MappingTable mapping)
    {
        // Create method body with a simple comment stub
        var comment = Comment($"// Generated method stub for {method.Name}");
        var returnStatement = ReturnStatement();
        
        var body = Block(
            SingletonList<StatementSyntax>(returnStatement));

        // Create method declaration
        var methodDecl = MethodDeclaration(
            PredefinedType(Token(SyntaxKind.VoidKeyword)),
            Identifier(SanitizeIdentifier(method.Name)))
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword))
            .WithBody(body);

        // Calculate line/column positions in the generated syntax tree
        // For now, we'll use approximate positions - more accurate mapping would require
        // analyzing the final formatted output
        var syntaxText = methodDecl.NormalizeWhitespace().ToFullString();
        var lines = syntaxText.Split('\n');
        
        // Add mapping entry for this method
        mapping.Add(new MappingEntry(
            method.NodeId,
            0, // First source file
            1, // Start line (approximate)
            1, // Start column
            lines.Length, // End line (approximate)
            lines.LastOrDefault()?.Length ?? 1)); // End column (approximate)

        return methodDecl;
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
}
