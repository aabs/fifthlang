using ast_model;
using System.Reflection;

namespace ast_generator;

public class TemplateModel
{
    public ITypeProvider TypeProvider { get; set; }
    public string NamespaceScope { get; set; }
    public IEnumerable<Type> AllAstTypes { get; set; }
    public IEnumerable<Type> AllTypes { get; set; }
    public IEnumerable<Type> ConcreteTypes { get; set; }
    public IEnumerable<Type> NonIgnoredTypes { get; set; }
    public Dictionary<Type, IEnumerable<PropertyInfo>> TypeProperties { get; set; }
    public Dictionary<Type, IEnumerable<PropertyInfo>> VisitableProperties { get; set; }

    public TemplateModel(ITypeProvider typeProvider)
    {
        TypeProvider = typeProvider;
        NamespaceScope = typeProvider.NamespaceScope;
        AllAstTypes = typeProvider.AllAstTypes;
        AllTypes = typeProvider.AllTypes;
        ConcreteTypes = typeProvider.ConcreteTypes;
        NonIgnoredTypes = typeProvider.NonIgnoredTypes;
        
        // Pre-compute properties to avoid needing extension methods in templates
        TypeProperties = ConcreteTypes.ToDictionary(t => t, t => t.InitialisedProperties());
        VisitableProperties = ConcreteTypes.ToDictionary(t => t, t => t.VisitableProperties(typeProvider.BaseType));
    }
}