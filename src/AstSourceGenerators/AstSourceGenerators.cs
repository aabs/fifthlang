using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
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
        // Get all syntax trees that contain AST types
        var astTypesProvider = context.CompilationProvider
            .Select((compilation, cancellationToken) =>
            {
                var astTypes = AstMetadataExtractor.ExtractAstTypes(compilation, "ast", "AstThing");
                var ilAstTypes = AstMetadataExtractor.ExtractAstTypes(compilation, "il_ast", "AstThing");
                return (astTypes, ilAstTypes);
            });

        // Generate builders for both AST types
        context.RegisterSourceOutput(astTypesProvider, (sourceContext, astTypesData) =>
        {
            var (astTypes, ilAstTypes) = astTypesData;

            // Generate builders for main AST
            if (!astTypes.IsEmpty)
            {
                var buildersSource = GenerateBuildersCode("ast", astTypes);
                sourceContext.AddSource("builders.generated.cs", SourceText.From(buildersSource, Encoding.UTF8));
            }

            // Generate builders for IL AST
            if (!ilAstTypes.IsEmpty)
            {
                var ilBuildersSource = GenerateBuildersCode("il_ast", ilAstTypes);
                sourceContext.AddSource("il.builders.generated.cs", SourceText.From(ilBuildersSource, Encoding.UTF8));
            }
        });
    }

    private static string GenerateBuildersCode(string namespaceName, ImmutableArray<AstTypeInfo> astTypes)
    {
        var concreteTypes = astTypes.Where(t => !t.IsAbstract).ToArray();
        
        var templateModel = new
        {
            @namespace = namespaceName,
            concrete_types = concreteTypes.Select(t => new
            {
                name = t.Name,
                full_name = t.FullName,
                properties = t.Properties.Where(p => !p.IsIgnored).Select(p => new
                {
                    name = p.Name,
                    type_name = p.TypeName,
                    declaring_type_name = p.DeclaringTypeName,
                    is_collection = p.IsCollection,
                    is_linked_list_collection = p.IsLinkedListCollection,
                    collection_element_type = p.CollectionElementType
                }).ToArray()
            }).ToArray()
        };

        var template = LoadTemplate("BuildersTemplate.sbn");
        return template.Render(templateModel);
    }

    private static Template LoadTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"AstSourceGenerators.Templates.{templateName}";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            // Fallback to embedded template if resource not found
            return Template.Parse(GetEmbeddedTemplate(templateName));
        }
        
        using var reader = new StreamReader(stream);
        var templateContent = reader.ReadToEnd();
        return Template.Parse(templateContent);
    }

    private static string GetEmbeddedTemplate(string templateName)
    {
        // Fallback embedded templates
        return templateName switch
        {
            "BuildersTemplate.sbn" => BuildersTemplateContent,
            _ => throw new InvalidOperationException($"Template {templateName} not found")
        };
    }

    private const string BuildersTemplateContent = @"
namespace {{ namespace }}_generated;
using ast_generated;
using {{ namespace }};
using System.Collections.Generic;
#nullable disable

{{~ for type in concrete_types ~}}
public class {{ type.name }}Builder : IBuilder<{{ type.full_name }}>
{

{{~ for prop in type.properties ~}}
    private {{ prop.type_name }} _{{ prop.name }};
{{~ end ~}}
    
    public {{ type.full_name }} Build()
    {
        return new {{ type.full_name }}(){
{{~ sep = "" ~}}
{{~ for prop in type.properties ~}}
           {{ sep }} {{ prop.name }} = this._{{ prop.name }} // from {{ prop.declaring_type_name }}
{{~ sep = "","" ~}}
{{~ end ~}}
        };
    }
{{~ for prop in type.properties ~}}
    public {{ type.name }}Builder With{{ prop.name }}({{ prop.type_name }} value){
        _{{ prop.name }} = value;
        return this;
    }

{{~ if prop.is_collection ~}}
{{~ ll_adjust = prop.is_linked_list_collection ? ""Last"" : """" ~}}
    public {{ type.name }}Builder AddingItemTo{{ prop.name }}({{ prop.collection_element_type }} value){
        _{{ prop.name }}  ??= [];
        _{{ prop.name }}.Add{{ ll_adjust }}(value);
        return this;
    }
{{~ end ~}}
{{~ end ~}}
}
{{~ end ~}}

#nullable restore";
}

[Generator]
public class AstVisitorsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var astTypesProvider = context.CompilationProvider
            .Select((compilation, cancellationToken) =>
            {
                var astTypes = AstMetadataExtractor.ExtractAstTypes(compilation, "ast", "AstThing");
                var ilAstTypes = AstMetadataExtractor.ExtractAstTypes(compilation, "il_ast", "AstThing");
                return (astTypes, ilAstTypes);
            });

        context.RegisterSourceOutput(astTypesProvider, (sourceContext, astTypesData) =>
        {
            var (astTypes, ilAstTypes) = astTypesData;

            if (!astTypes.IsEmpty)
            {
                var visitorsSource = GenerateVisitorsCode("ast", astTypes);
                sourceContext.AddSource("visitors.generated.cs", SourceText.From(visitorsSource, Encoding.UTF8));
            }

            if (!ilAstTypes.IsEmpty)
            {
                var ilVisitorsSource = GenerateVisitorsCode("il_ast", ilAstTypes);
                sourceContext.AddSource("il.visitors.generated.cs", SourceText.From(ilVisitorsSource, Encoding.UTF8));
            }
        });
    }

    private static string GenerateVisitorsCode(string namespaceName, ImmutableArray<AstTypeInfo> astTypes)
    {
        var concreteTypes = astTypes.Where(t => !t.IsAbstract).ToArray();
        
        // TODO: Implement visitor generation logic similar to builders
        // For now, return a basic implementation
        return $@"
namespace {namespaceName}_generated;
using {namespaceName};
using System.Collections.Generic;

public interface IAstVisitor
{{
    // TODO: Generate visitor methods
}}";
    }
}

[Generator]
public class AstTypeCheckerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var astTypesProvider = context.CompilationProvider
            .Select((compilation, cancellationToken) =>
            {
                var astTypes = AstMetadataExtractor.ExtractAstTypes(compilation, "ast", "AstThing");
                return astTypes;
            });

        context.RegisterSourceOutput(astTypesProvider, (sourceContext, astTypes) =>
        {
            if (!astTypes.IsEmpty)
            {
                var typeCheckerSource = GenerateTypeCheckerCode("ast", astTypes);
                sourceContext.AddSource("typeinference.generated.cs", SourceText.From(typeCheckerSource, Encoding.UTF8));
            }
        });
    }

    private static string GenerateTypeCheckerCode(string namespaceName, ImmutableArray<AstTypeInfo> astTypes)
    {
        var concreteTypes = astTypes.Where(t => !t.IsAbstract).ToArray();
        
        // TODO: Implement type checker generation logic
        return $@"
namespace {namespaceName}_generated;
using {namespaceName};
using ast_model.Symbols;
using ast_model.TypeSystem;

public interface ITypeChecker
{{
    // TODO: Generate type checker methods
}}";
    }
}