using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace AstSourceGenerators;

public struct AstTypeInfo
{
    public string Name { get; }
    public string FullName { get; }
    public string Namespace { get; }
    public bool IsAbstract { get; }
    public bool IsGeneric { get; }
    public ImmutableArray<AstPropertyInfo> Properties { get; }
    public ImmutableArray<string> BaseTypes { get; }

    public AstTypeInfo(string name, string fullName, string @namespace, bool isAbstract, bool isGeneric,
        ImmutableArray<AstPropertyInfo> properties, ImmutableArray<string> baseTypes)
    {
        Name = name;
        FullName = fullName;
        Namespace = @namespace;
        IsAbstract = isAbstract;
        IsGeneric = isGeneric;
        Properties = properties;
        BaseTypes = baseTypes;
    }
}

public struct AstPropertyInfo
{
    public string Name { get; }
    public string TypeName { get; }
    public string FullTypeName { get; }
    public bool IsCollection { get; }
    public bool IsLinkedListCollection { get; }
    public bool IsReadOnly { get; }
    public bool IsIgnored { get; }
    public bool IsIgnoredDuringVisit { get; }
    public bool IsIncludedInVisit { get; }
    public string? CollectionElementType { get; }
    public string DeclaringTypeName { get; }

    public AstPropertyInfo(string name, string typeName, string fullTypeName, bool isCollection,
        bool isLinkedListCollection, bool isReadOnly, bool isIgnored, bool isIgnoredDuringVisit,
        bool isIncludedInVisit, string? collectionElementType, string declaringTypeName)
    {
        Name = name;
        TypeName = typeName;
        FullTypeName = fullTypeName;
        IsCollection = isCollection;
        IsLinkedListCollection = isLinkedListCollection;
        IsReadOnly = isReadOnly;
        IsIgnored = isIgnored;
        IsIgnoredDuringVisit = isIgnoredDuringVisit;
        IsIncludedInVisit = isIncludedInVisit;
        CollectionElementType = collectionElementType;
        DeclaringTypeName = declaringTypeName;
    }
}

/// <summary>
/// Extracts AST type metadata using Roslyn semantic analysis instead of System.Reflection
/// </summary>
public static class AstMetadataExtractor
{
    public static ImmutableArray<AstTypeInfo> ExtractAstTypes(Compilation compilation, string namespaceName, string baseTypeName)
    {
        var builder = ImmutableArray.CreateBuilder<AstTypeInfo>();
        var baseTypeSymbol = FindTypeByName(compilation, baseTypeName);
        
        if (baseTypeSymbol == null)
            return builder.ToImmutable();

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            foreach (var typeDeclaration in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
                if (typeSymbol == null || !IsInNamespace(typeSymbol, namespaceName))
                    continue;

                if (IsSubtypeOf(typeSymbol, baseTypeSymbol) && !HasIgnoreAttribute(typeSymbol))
                {
                    var astTypeInfo = ExtractTypeInfo(typeSymbol, baseTypeSymbol);
                    builder.Add(astTypeInfo);
                }
            }
        }

        return builder.ToImmutable();
    }

    private static AstTypeInfo ExtractTypeInfo(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        var properties = ExtractProperties(typeSymbol, baseTypeSymbol);
        var baseTypes = GetBaseTypeNames(typeSymbol);

        return new AstTypeInfo(
            typeSymbol.Name,
            typeSymbol.ToDisplayString(),
            typeSymbol.ContainingNamespace.ToDisplayString(),
            typeSymbol.IsAbstract,
            typeSymbol.IsGenericType,
            properties,
            baseTypes);
    }

    private static ImmutableArray<AstPropertyInfo> ExtractProperties(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        var builder = ImmutableArray.CreateBuilder<AstPropertyInfo>();

        // Get all properties including inherited ones
        var properties = GetAllProperties(typeSymbol);

        foreach (var property in properties)
        {
            if (property.DeclaredAccessibility != Accessibility.Public)
                continue;

            var isIgnored = HasIgnoreAttribute(property);
            var isIgnoredDuringVisit = HasIgnoreDuringVisitAttribute(property) && !HasIncludeInVisitAttribute(property);
            var isIncludedInVisit = HasIncludeInVisitAttribute(property);
            var isReadOnly = IsInitOnlyProperty(property);
            var (isCollection, isLinkedListCollection, elementType) = AnalyzeCollectionType(property.Type);

            var propertyInfo = new AstPropertyInfo(
                property.Name,
                GetTypeName(property.Type),
                property.Type.ToDisplayString(),
                isCollection,
                isLinkedListCollection,
                isReadOnly,
                isIgnored,
                isIgnoredDuringVisit,
                isIncludedInVisit,
                elementType,
                property.ContainingType.Name);

            builder.Add(propertyInfo);
        }

        return builder.ToImmutable();
    }

    private static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol typeSymbol)
    {
        var properties = new List<IPropertySymbol>();
        var currentType = typeSymbol;

        while (currentType != null)
        {
            properties.AddRange(currentType.GetMembers().OfType<IPropertySymbol>());
            currentType = currentType.BaseType;
        }

        return properties;
    }

    private static string GetTypeName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var genericTypeName = namedType.ConstructedFrom.ToDisplayString();
            var typeArgs = namedType.TypeArguments.Select(GetTypeName);
            return $"{genericTypeName.Split('<')[0]}<{string.Join(", ", typeArgs)}>";
        }

        return typeSymbol.ToDisplayString();
    }

    private static (bool isCollection, bool isLinkedList, string? elementType) AnalyzeCollectionType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType || !namedType.IsGenericType)
            return (false, false, null);

        var genericDefinition = namedType.ConstructedFrom.ToDisplayString();
        
        if (genericDefinition.Contains("System.Collections.Generic.List<T>"))
        {
            var elementType = namedType.TypeArguments.FirstOrDefault()?.ToDisplayString();
            return (true, false, elementType);
        }

        if (genericDefinition.Contains("System.Collections.Generic.LinkedList<T>"))
        {
            var elementType = namedType.TypeArguments.FirstOrDefault()?.ToDisplayString();
            return (true, true, elementType);
        }

        return (false, false, null);
    }

    private static bool IsInitOnlyProperty(IPropertySymbol property)
    {
        return property.SetMethod?.IsInitOnly == true || 
               HasRequiredMemberAttribute(property);
    }

    private static ImmutableArray<string> GetBaseTypeNames(INamedTypeSymbol typeSymbol)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var currentType = typeSymbol.BaseType;

        while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
        {
            builder.Add(currentType.Name);
            currentType = currentType.BaseType;
        }

        return builder.ToImmutable();
    }

    private static INamedTypeSymbol? FindTypeByName(Compilation compilation, string typeName)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            foreach (var typeDeclaration in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
                if (typeSymbol?.Name == typeName)
                    return typeSymbol;
            }
        }

        return null;
    }

    private static bool IsInNamespace(INamedTypeSymbol typeSymbol, string namespaceName)
    {
        return typeSymbol.ContainingNamespace.ToDisplayString() == namespaceName;
    }

    private static bool IsSubtypeOf(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        var currentType = typeSymbol;
        while (currentType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentType, baseTypeSymbol))
                return true;
            currentType = currentType.BaseType;
        }
        return false;
    }

    private static bool HasIgnoreAttribute(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(attr => 
            attr.AttributeClass?.Name == "IgnoreAttribute" || 
            attr.AttributeClass?.Name == "Ignore");
    }

    private static bool HasIgnoreDuringVisitAttribute(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(attr => 
            attr.AttributeClass?.Name == "IgnoreDuringVisitAttribute" ||
            attr.AttributeClass?.Name == "IgnoreDuringVisit");
    }

    private static bool HasIncludeInVisitAttribute(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(attr => 
            attr.AttributeClass?.Name == "IncludeInVisitAttribute" ||
            attr.AttributeClass?.Name == "IncludeInVisit");
    }

    private static bool HasRequiredMemberAttribute(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(attr => 
            attr.AttributeClass?.Name == "RequiredMemberAttribute");
    }
}