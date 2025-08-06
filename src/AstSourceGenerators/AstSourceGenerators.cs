using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace AstSourceGenerators;

[Generator]
public class AstBuildersGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register to generate sources when the compilation changes
        context.RegisterPostInitializationOutput(static ctx =>
        {
            // We'll generate placeholder files for now to demonstrate the infrastructure works
            // The actual generation will happen in the source output phase
        });

        // Get all types from the compilation and referenced assemblies
        var compilationProvider = context.CompilationProvider.Select((compilation, cancellationToken) =>
        {
            var astTypes = new List<TypeInfo>();
            var debugInfo = new StringBuilder();
            
            debugInfo.AppendLine($"// Current compilation assembly: {compilation.Assembly.Name}");
            
            // Look in all referenced assemblies for AST types
            foreach (var reference in compilation.References)
            {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
                {
                    debugInfo.AppendLine($"// Checking assembly: {assembly.Name}");
                    CollectAstTypesFromAssembly(assembly, astTypes);
                }
            }
            
            debugInfo.AppendLine($"// Checking current compilation assembly");
            // Also check the current compilation
            CollectAstTypesFromAssembly(compilation.Assembly, astTypes);
            
            debugInfo.AppendLine($"// Total AST types found: {astTypes.Count}");
            
            // Store debug info regardless of whether AST types are found
            astTypes.Add(new TypeInfo("DEBUG_INFO", debugInfo.ToString(), "", false, false, ImmutableArray<PropertyInfo>.Empty));
            
            return astTypes.ToImmutableArray();
        });

        context.RegisterSourceOutput(compilationProvider, GenerateSources);
    }

    private static void CollectAstTypesFromAssembly(IAssemblySymbol assembly, List<TypeInfo> astTypes)
    {
        foreach (var module in assembly.Modules)
        {
            CollectAstTypes(module.GlobalNamespace, astTypes);
        }
    }

    private static void GenerateSources(SourceProductionContext context, ImmutableArray<TypeInfo> astTypes)
    {
        // Generate diagnostic output
        var diagnosticSb = new StringBuilder();
        diagnosticSb.AppendLine("// Diagnostic output from AstBuildersGenerator");
        diagnosticSb.AppendLine($"// Found {astTypes.Length} types total");
        diagnosticSb.AppendLine();

        foreach (var type in astTypes)
        {
            if (type.Name == "DEBUG_INFO")
            {
                diagnosticSb.AppendLine(type.FullName); // Contains debug info
                continue;
            }
            
            diagnosticSb.AppendLine($"// Type: {type.Name} ({type.FullName})");
            diagnosticSb.AppendLine($"//   Namespace: {type.Namespace}");
            diagnosticSb.AppendLine($"//   IsAbstract: {type.IsAbstract}");
            diagnosticSb.AppendLine($"//   Properties: {type.Properties.Length}");
            foreach (var prop in type.Properties)
            {
                diagnosticSb.AppendLine($"//     - {prop.Name}: {prop.TypeName} (Collection: {prop.IsCollection})");
            }
            diagnosticSb.AppendLine();
        }

        context.AddSource("diagnostic.generated.cs", SourceText.From(diagnosticSb.ToString(), Encoding.UTF8));

        var concreteTypes = astTypes.Where(t => !t.IsAbstract).ToImmutableArray();
        
        if (concreteTypes.Length > 0)
        {
            var source = GenerateBuilders(concreteTypes);
            context.AddSource("builders.generated.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static void CollectAstTypes(INamespaceSymbol namespaceSymbol, List<TypeInfo> astTypes)
    {
        // Check all types in this namespace
        foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
        {
            // Add debug info for ast namespace types
            if (namespaceSymbol.ToDisplayString() == "ast")
            {
                var debugInfo = new TypeInfo(
                    name: $"DEBUG_{typeSymbol.Name}",
                    fullName: $"Found type {typeSymbol.Name} in ast namespace, IsClass: {typeSymbol.TypeKind == TypeKind.Class}, IsAbstract: {typeSymbol.IsAbstract}, BaseType: {typeSymbol.BaseType?.Name ?? "null"}",
                    @namespace: "debug",
                    isAbstract: false,
                    isAstType: false,
                    properties: ImmutableArray<PropertyInfo>.Empty);
                astTypes.Add(debugInfo);
            }
            
            if (IsAstType(typeSymbol))
            {
                var properties = GetAllProperties(typeSymbol)
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public && 
                               (p.SetMethod?.IsInitOnly == true || p.SetMethod == null))
                    .Select(p => new PropertyInfo(
                        name: p.Name,
                        typeName: p.Type.ToDisplayString(),
                        isCollection: IsCollectionType(p.Type),
                        declaringTypeName: GetDeclaringTypeName(p.ContainingType)))
                    .ToImmutableArray();

                var typeInfo = new TypeInfo(
                    name: typeSymbol.Name,
                    fullName: typeSymbol.ToDisplayString(),
                    @namespace: typeSymbol.ContainingNamespace.ToDisplayString(),
                    isAbstract: typeSymbol.IsAbstract,
                    isAstType: true,
                    properties: properties);
                
                astTypes.Add(typeInfo);
            }
        }

        // Recursively check nested namespaces
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            CollectAstTypes(nestedNamespace, astTypes);
        }
    }

    private static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol typeSymbol)
    {
        var properties = new List<IPropertySymbol>();
        var visitedTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        
        var currentType = typeSymbol;
        while (currentType != null && visitedTypes.Add(currentType))
        {
            foreach (var member in currentType.GetMembers())
            {
                if (member is IPropertySymbol property)
                {
                    properties.Add(property);
                }
            }
            currentType = currentType.BaseType;
        }

        return properties;
    }

    private static string GetDeclaringTypeName(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.Name;
    }

    private static bool IsAstType(INamedTypeSymbol typeSymbol)
    {
        // Check if type is in ast namespace
        if (typeSymbol.ContainingNamespace.ToDisplayString() == "ast")
            return true;

        // Check if inherits from AstThing
        var currentType = typeSymbol.BaseType;
        while (currentType != null)
        {
            if (currentType.Name == "AstThing")
                return true;
            currentType = currentType.BaseType;
        }

        return false;
    }

    private static bool IsCollectionType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var typeName = namedType.ConstructedFrom.ToDisplayString();
            return typeName.Contains("List<") || typeName.Contains("LinkedList<");
        }
        return false;
    }

    private static string GenerateBuilders(ImmutableArray<TypeInfo> astTypes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Generated by AstBuildersGenerator");
        sb.AppendLine();
        sb.AppendLine("namespace ast_generated;");
        sb.AppendLine("using ast_generated;");
        sb.AppendLine("using ast;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("#nullable disable");
        sb.AppendLine();

        foreach (var type in astTypes.Where(t => !t.IsAbstract))
        {
            GenerateBuilder(sb, type);
        }

        sb.AppendLine("#nullable restore");
        return sb.ToString();
    }

    private static void GenerateBuilder(StringBuilder sb, TypeInfo type)
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

        var first = true;
        foreach (var prop in type.Properties)
        {
            var separator = first ? " " : ",";
            sb.AppendLine($"           {separator} {prop.Name} = this._{prop.Name} // from {prop.DeclaringTypeName}");
            first = false;
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
            if (prop.IsCollection)
            {
                var elementType = GetCollectionElementType(prop.TypeName);
                sb.AppendLine($"    public {type.Name}Builder AddingItemTo{prop.Name}({elementType} value){{");
                sb.AppendLine($"        _{prop.Name}  ??= [];");
                sb.AppendLine($"        _{prop.Name}.Add(value);");
                sb.AppendLine("        return this;");
                sb.AppendLine("    }");
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static string GetCollectionElementType(string typeName)
    {
        // Extract element type from List<T> or LinkedList<T>
        var start = typeName.IndexOf('<');
        var end = typeName.LastIndexOf('>');
        if (start > 0 && end > start)
        {
            return typeName.Substring(start + 1, end - start - 1);
        }
        return "object";
    }

    private struct TypeInfo
    {
        public string Name { get; }
        public string FullName { get; }
        public string Namespace { get; }
        public bool IsAbstract { get; }
        public bool IsAstType { get; }
        public ImmutableArray<PropertyInfo> Properties { get; }

        public TypeInfo(string name, string fullName, string @namespace, bool isAbstract, bool isAstType, ImmutableArray<PropertyInfo> properties)
        {
            Name = name;
            FullName = fullName;
            Namespace = @namespace;
            IsAbstract = isAbstract;
            IsAstType = isAstType;
            Properties = properties;
        }
    }

    private struct PropertyInfo
    {
        public string Name { get; }
        public string TypeName { get; }
        public bool IsCollection { get; }
        public string DeclaringTypeName { get; }

        public PropertyInfo(string name, string typeName, bool isCollection, string declaringTypeName)
        {
            Name = name;
            TypeName = typeName;
            IsCollection = isCollection;
            DeclaringTypeName = declaringTypeName;
        }
    }
}

