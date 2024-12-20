using Fifth;

namespace ast_model.TypeSystem;

using static Maybe<FifthType>;
using Exp = System.Linq.Expressions.Expression;

public static class TypeHelpers
{
    public static ScopeAstThing GlobalScope(this IAstThing node)
    {
        // TODO: WARNING: This could return null (a non-scope AST Node), if the root is not a scope node
        if (node.Parent == null)
        {
            return node as ScopeAstThing;
        }

        return node?.Parent.GlobalScope();
    }

    public static Maybe<FifthType> Lookup(this TypeId tid)
    {
        if (TypeRegistry.DefaultRegistry.TryLookupType(tid, out FifthType ft))
        {
            return new Some(ft);
        }

        return new None();
    }

    public static FifthType LookupType(this Type type)
    {
        if (TypeRegistry.DefaultRegistry.TryLookupType(type, out var result))
        {
            return result;
        }

        throw new TypeCheckingException("no way to lookup non native types yet");
    }



    // tmp

    public static T PeekOrDefault<T>(this Stack<T> s)
    {
        if (s == null || s.Count == 0)
        {
            return default;
        }

        return s.Peek();
    }

    public static bool TryGetAttribute<T>(this Type t, out T attr)
    {
        attr = (T)t.GetCustomAttributes(true).FirstOrDefault(attr => attr is T);
        return attr != null;
    }

    public static bool TryGetAttribute<T>(this FieldInfo mi, out T attr)
    {
        attr = (T)mi.GetCustomAttributes(true).FirstOrDefault(attr => attr is T);
        return attr != null;
    }

    public static bool TryGetAttribute<T>(this MethodInfo mi, out T attr)
    {
        attr = (T)mi.GetCustomAttributes(true).FirstOrDefault(attr => attr is T);
        return attr != null;
    }

    public static bool TryGetMethodByName(this Type t, string name, out FuncWrapper fw)
    {
        var methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public);

        fw = methods
             .Where(m => m.Name == name)
             .Select(WrapMethodInfo)
             .FirstOrDefault();
        return fw != null;
    }

    public static bool TryGetNearestFifthTypeToListType(Type nt, out FifthType ft)
    {
        if (typeof(IList).IsAssignableFrom(nt) && nt.IsGenericType)
        {
            var typeParam = nt.GenericTypeArguments.First();
            if (!TryGetNearestFifthTypeToNativeType(typeParam, out var typeParamAsFifthType))
            {
                throw new TypeCheckingException("Unable to make sense of type param for list");
            }

            ft = new FifthType.TListOf(typeParamAsFifthType) { Name = TypeName.From($"[{typeParamAsFifthType.Name}]") };
            return true;
        }

        ft = null;
        return false;
    }

    public static bool TryGetNearestFifthTypeToNativeType(Type nt, out FifthType ft)
    {
        ft = nt.LookupType();
        return true;
    }

    public static bool TryPack(out ulong result, params ushort[] shorts)
    {
        result = 0L;
        if (shorts.Length > 4)
        {
            return false;
        }

        foreach (var s in shorts)
        {
            result ^= s;
            result <<= 16;
        }

        return true;
    }

    public static IEnumerable<Type> TypesImplementingInterface<TInterfaceType, TSampleType>()
    {
        var type = typeof(TInterfaceType);
        return AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetTypes())
                        .Where(p => type.IsAssignableFrom(p));
    }

    public static FuncWrapper Wrap(this MethodInfo method)
    {
        return WrapMethodInfo(method);
    }

    public static FuncWrapper WrapMethodInfo(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var formalParams = parameters.Select(p => Exp.Parameter(p.ParameterType, p.Name))
                                     .ToArray();
        var call = Exp.Call(null, method, formalParams);
        var result = new FuncWrapper(parameters.Select(p => p.ParameterType).ToList(), method.ReturnType,
            Exp.Lambda(call, formalParams).Compile(), method.MetadataToken);
        return result;
    }
}
