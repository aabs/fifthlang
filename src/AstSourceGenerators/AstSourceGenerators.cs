using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AstSourceGenerators;

[Generator]
public class AstBuildersGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register to generate sources when the compilation changes
        var compilationProvider = context.CompilationProvider.Select((compilation, cancellationToken) =>
        {
            var astTypes = ExtractAstTypesFromReferences(compilation);
            return astTypes;
        });

        context.RegisterSourceOutput(compilationProvider, GenerateSources);
    }

    private static List<AstTypeModel> ExtractAstTypesFromReferences(Compilation compilation)
    {
        var astTypes = new List<AstTypeModel>();
        var debugInfo = new StringBuilder();
        
        debugInfo.AppendLine($"// Compilation: {compilation.Assembly.Name}");
        debugInfo.AppendLine($"// References: {compilation.References.Count()}");
        
        // Check referenced assemblies for AST types
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
            {
                debugInfo.AppendLine($"// Checking assembly: {assembly.Name}");
                ExtractAstTypesFromAssembly(assembly, astTypes, debugInfo);
            }
        }
        
        // Also check the current compilation
        debugInfo.AppendLine($"// Checking current compilation");
        ExtractAstTypesFromAssembly(compilation.Assembly, astTypes, debugInfo);
        
        debugInfo.AppendLine($"// Total AST types found: {astTypes.Count}");
        
        // Add debug info as a special type
        astTypes.Add(new AstTypeModel 
        { 
            Name = "DEBUG_INFO", 
            FullName = debugInfo.ToString(),
            Namespace = "debug",
            IsAbstract = false,
            Properties = new List<AstPropertyModel>()
        });
        
        return astTypes;
    }

    private static void ExtractAstTypesFromAssembly(IAssemblySymbol assembly, List<AstTypeModel> astTypes, StringBuilder debugInfo)
    {
        foreach (var module in assembly.Modules)
        {
            ExtractAstTypesFromNamespace(module.GlobalNamespace, astTypes, debugInfo);
        }
    }

    private static void ExtractAstTypesFromNamespace(INamespaceSymbol namespaceSymbol, List<AstTypeModel> astTypes, StringBuilder debugInfo)
    {
        // Check for types in 'ast' namespace specifically
        if (namespaceSymbol.ToDisplayString() == "ast")
        {
            debugInfo.AppendLine($"//   Found ast namespace with {namespaceSymbol.GetTypeMembers().Length} types");
            
            foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
            {
                if (typeSymbol.TypeKind == TypeKind.Class || typeSymbol.TypeKind == TypeKind.Struct)
                {
                    debugInfo.AppendLine($"//     Type: {typeSymbol.Name}, Kind: {typeSymbol.TypeKind}, Abstract: {typeSymbol.IsAbstract}");
                    
                    var properties = ExtractProperties(typeSymbol);
                    var astType = new AstTypeModel
                    {
                        Name = typeSymbol.Name,
                        FullName = typeSymbol.ToDisplayString(),
                        Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                        IsAbstract = typeSymbol.IsAbstract,
                        Properties = properties
                    };
                    
                    astTypes.Add(astType);
                }
            }
        }

        // Recursively check nested namespaces
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ExtractAstTypesFromNamespace(nestedNamespace, astTypes, debugInfo);
        }
    }

    private static List<AstPropertyModel> ExtractProperties(INamedTypeSymbol typeSymbol)
    {
        var properties = new List<AstPropertyModel>();
        var visitedTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        
        var currentType = typeSymbol;
        while (currentType != null && visitedTypes.Add(currentType))
        {
            foreach (var member in currentType.GetMembers())
            {
                if (member is IPropertySymbol property && 
                    property.DeclaredAccessibility == Accessibility.Public)
                {
                    var (isCollection, elementType) = AnalyzeCollectionType(property.Type);
                    
                    properties.Add(new AstPropertyModel
                    {
                        Name = property.Name,
                        TypeName = property.Type.ToDisplayString(),
                        IsCollection = isCollection,
                        CollectionElementType = elementType,
                        DeclaringTypeName = property.ContainingType.Name,
                        IsVisitable = !HasIgnoreAttribute(property)
                    });
                }
            }
            currentType = currentType.BaseType;
        }

        return properties;
    }

    private static (bool isCollection, string? elementType) AnalyzeCollectionType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var typeName = namedType.ConstructedFrom.ToDisplayString();
            if (typeName.Contains("List<") || typeName.Contains("LinkedList<"))
            {
                var elementType = namedType.TypeArguments.FirstOrDefault()?.ToDisplayString();
                return (true, elementType);
            }
        }
        return (false, null);
    }

    private static bool HasIgnoreAttribute(IPropertySymbol property)
    {
        return property.GetAttributes().Any(attr => 
            attr.AttributeClass?.Name.Contains("Ignore") == true);
    }

    private static void GenerateSources(SourceProductionContext context, List<AstTypeModel> astTypes)
    {
        // Generate diagnostic output
        var diagnostic = GenerateDiagnostic(astTypes);
        context.AddSource("diagnostic.generated.cs", SourceText.From(diagnostic, Encoding.UTF8));

        var concreteTypes = astTypes.Where(t => !t.IsAbstract && t.Name != "DEBUG_INFO").ToList();
        
        if (concreteTypes.Count > 0)
        {
            // Generate builders using template
            var buildersSource = GenerateFromTemplate("BuildersTemplate.sbn", new { 
                @namespace = "ast", 
                concrete_types = concreteTypes 
            });
            context.AddSource("builders.generated.cs", SourceText.From(buildersSource, Encoding.UTF8));

            // Generate visitors using template  
            var visitorsSource = GenerateFromTemplate("VisitorsTemplate.sbn", new { 
                @namespace = "ast", 
                concrete_types = concreteTypes 
            });
            context.AddSource("visitors.generated.cs", SourceText.From(visitorsSource, Encoding.UTF8));

            // Generate type inference using template
            var typeInferenceSource = GenerateFromTemplate("TypeCheckerTemplate.sbn", new { 
                @namespace = "ast", 
                concrete_types = concreteTypes 
            });
            context.AddSource("typeinference.generated.cs", SourceText.From(typeInferenceSource, Encoding.UTF8));
        }
    }

    private static string GenerateDiagnostic(List<AstTypeModel> astTypes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Diagnostic output from AstBuildersGenerator");
        sb.AppendLine($"// Found {astTypes.Count} types total");
        sb.AppendLine();

        foreach (var type in astTypes)
        {
            if (type.Name == "DEBUG_INFO")
            {
                sb.AppendLine(type.FullName);
                continue;
            }
            
            sb.AppendLine($"// Type: {type.Name} ({type.FullName})");
            sb.AppendLine($"//   Namespace: {type.Namespace}");
            sb.AppendLine($"//   IsAbstract: {type.IsAbstract}");
            sb.AppendLine($"//   Properties: {type.Properties.Count}");
            foreach (var prop in type.Properties)
            {
                sb.AppendLine($"//     - {prop.Name}: {prop.TypeName} (Collection: {prop.IsCollection})");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GenerateFromTemplate(string templateName, object model)
    {
        try
        {
            // For now, use inline templates until we can get resource loading working
            if (templateName == "BuildersTemplate.sbn")
            {
                return GenerateBuildersInline(model);
            }
            else if (templateName == "VisitorsTemplate.sbn")
            {
                return GenerateVisitorsInline(model);
            }
            else if (templateName == "TypeCheckerTemplate.sbn")
            {
                return GenerateTypeInferenceInline(model);
            }
            
            return $"// Template {templateName} not implemented";
        }
        catch (Exception ex)
        {
            return $"// Error generating from template {templateName}: {ex.Message}";
        }
    }

    private static string GenerateBuildersInline(object model)
    {
        var data = (dynamic)model;
        var types = (List<AstTypeModel>)data.concrete_types;
        var namespaceName = (string)data.@namespace;
        
        var sb = new StringBuilder();
        sb.AppendLine($"namespace {namespaceName}_generated;");
        sb.AppendLine($"using ast_generated;");
        sb.AppendLine($"using {namespaceName};");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("#nullable disable");
        sb.AppendLine();

        foreach (var type in types)
        {
            sb.AppendLine($"public class {type.Name}Builder : IBuilder<{type.FullName}>");
            sb.AppendLine("{");
            sb.AppendLine();

            // Generate private fields
            foreach (var prop in type.Properties)
            {
                sb.AppendLine($"    private {prop.TypeName} _{prop.Name};");
            }

            sb.AppendLine();
            sb.AppendLine($"    public {type.FullName} Build()");
            sb.AppendLine("    {");
            sb.AppendLine($"        return new {type.FullName}(){{");

            for (int i = 0; i < type.Properties.Count; i++)
            {
                var prop = type.Properties[i];
                var prefix = i == 0 ? "             " : "           , ";
                sb.AppendLine($"{prefix}{prop.Name} = this._{prop.Name} // from {prop.DeclaringTypeName}");
            }

            sb.AppendLine("        };");
            sb.AppendLine("    }");

            // Generate With methods
            foreach (var prop in type.Properties)
            {
                sb.AppendLine($"    public {type.Name}Builder With{prop.Name}({prop.TypeName} value){{");
                sb.AppendLine($"        _{prop.Name} = value;");
                sb.AppendLine("        return this;");
                sb.AppendLine("    }");
                sb.AppendLine();

                // Generate AddingItemTo methods for collections
                if (prop.IsCollection && prop.CollectionElementType != null)
                {
                    sb.AppendLine($"    public {type.Name}Builder AddingItemTo{prop.Name}({prop.CollectionElementType} value){{");
                    sb.AppendLine($"        _{prop.Name} ??= new {prop.TypeName}();");
                    sb.AppendLine($"        _{prop.Name}.Add(value);");
                    sb.AppendLine("        return this;");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        sb.AppendLine("#nullable restore");
        return sb.ToString();
    }

    private static string GenerateVisitorsInline(object model)
    {
        var data = (dynamic)model;
        var types = (List<AstTypeModel>)data.concrete_types;
        var namespaceName = (string)data.@namespace;
        
        var sb = new StringBuilder();
        sb.AppendLine($"namespace {namespaceName}_generated;");
        sb.AppendLine($"using {namespaceName};");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();

        // Generate IAstVisitor interface
        sb.AppendLine("public interface IAstVisitor");
        sb.AppendLine("{");
        foreach (var type in types)
        {
            sb.AppendLine($"    public void Enter{type.Name}({type.Name} ctx);");
            sb.AppendLine($"    public void Leave{type.Name}({type.Name} ctx);");
        }
        sb.AppendLine("}");
        sb.AppendLine();

        // Generate BaseAstVisitor
        sb.AppendLine("public partial class BaseAstVisitor : IAstVisitor");
        sb.AppendLine("{");
        foreach (var type in types)
        {
            sb.AppendLine($"    public virtual void Enter{type.Name}({type.Name} ctx){{}}");
            sb.AppendLine($"    public virtual void Leave{type.Name}({type.Name} ctx){{}}");
        }
        sb.AppendLine("}");
        sb.AppendLine();

        return sb.ToString();
    }

    private static string GenerateTypeInferenceInline(object model)
    {
        var data = (dynamic)model;
        var types = (List<AstTypeModel>)data.concrete_types;
        var namespaceName = (string)data.@namespace;
        
        var sb = new StringBuilder();
        sb.AppendLine($"namespace {namespaceName}_generated;");
        sb.AppendLine($"using {namespaceName};");
        sb.AppendLine("using ast_model.Symbols;");
        sb.AppendLine("using ast_model.TypeSystem;");
        sb.AppendLine();

        // Generate ITypeChecker interface
        sb.AppendLine("public interface ITypeChecker");
        sb.AppendLine("{");
        foreach (var type in types)
        {
            sb.AppendLine($"    public FifthType Infer(ScopeAstThing scope, {type.Name} node);");
        }
        sb.AppendLine("}");
        sb.AppendLine();

        // Generate abstract base class
        sb.AppendLine("public abstract class FunctionalTypeChecker : ITypeChecker");
        sb.AppendLine("{");
        sb.AppendLine("    public FifthType Infer(AstThing exp)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (exp == null) return default;");
        sb.AppendLine("        var scope = exp.NearestScope();");
        sb.AppendLine("        return exp switch");
        sb.AppendLine("        {");
        foreach (var type in types)
        {
            sb.AppendLine($"            {type.Name} node => Infer(scope, node),");
        }
        sb.AppendLine("            { } node => throw new ast_model.TypeCheckingException(\"Unrecognised type\")");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();

        foreach (var type in types)
        {
            sb.AppendLine($"    public abstract FifthType Infer(ScopeAstThing scope, {type.Name} node);");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}

public class AstTypeModel
{
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public bool IsAbstract { get; set; }
    public List<AstPropertyModel> Properties { get; set; } = new();
}

public class AstPropertyModel
{
    public string Name { get; set; } = "";
    public string TypeName { get; set; } = "";
    public bool IsCollection { get; set; }
    public string? CollectionElementType { get; set; }
    public string DeclaringTypeName { get; set; } = "";
    public bool IsVisitable { get; set; } = true;
}

