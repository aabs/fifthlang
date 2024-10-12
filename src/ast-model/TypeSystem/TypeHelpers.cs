using ast_model.TypeSystem.PrimitiveTypes;
using Fifth;

namespace ast_model.TypeSystem;

using Exp = System.Linq.Expressions.Expression;

public static class TypeHelpers
{
    public static IFunctionSignature GetFunctionSignature(this FunctionDef fd)
    {
        TypeRegistry.DefaultRegistry.TryGetTypeByName(fd.Type.Symbol.Name, out var returnType);
        var paramTypes = new List<TypeId>();
        foreach (var paramTypeName in fd.Params.Select(item => item.Type.Symbol.Name))
        {
            if (TypeRegistry.DefaultRegistry.TryGetTypeByName(paramTypeName, out var paramTid))
            {
                paramTypes.Add(paramTid.Id);
            }
        }

        TypeId? declaringType = null;

        if (fd.Parent is AstThing n)
        {
            declaringType = n.Type.Id;
        }
        return new FunctionSignature
        {
            Name = fd.Name,
            FormalParameterTypes = paramTypes.ToArray(),
            GenericTypeParameters = [],
            ReturnType = returnType.TypeId,
            DeclaringType = declaringType
        };
    }

    public static IFunctionSignature GetFuncType(this MethodInfo method)
    {
        return new FunctionSignature
        {
            Name = MemberName.From(method.Name),
            FormalParameterTypes = method.GetParameters().Select(p => p.ParameterType.LookupType()).ToArray(),
            GenericTypeParameters = [],
            ReturnType = method.ReturnType.LookupType(),
            DeclaringType = method.ReturnType.LookupType() // TODO: not sure what the difference is between the return type and the typeid
        };
    }

    public static IFunctionSignature GetFuncType(this FuncWrapper method)
    {
        return new FunctionSignature
        {
            Name = MemberName.anonymous,
            FormalParameterTypes = method.ArgTypes.Select(p => p.LookupType()).ToArray(),
            GenericTypeParameters = [],
            ReturnType = method.ResultType.LookupType(),
            DeclaringType = method.ResultType.LookupType() // TODO: not sure what the difference is between the return type and the typeid
        };
    }

    /// <summary>
    /// try to resolve the type of the value and get its internal value
    /// </summary>
    /// <returns>Value if it can find it, as an object</returns>
    public static object GetValueOfValueObject(this object vo)
    {
        var pi = vo.GetType().GetProperty("Value");

        if (pi?.CanRead ?? false)
        {
            return pi!.GetMethod!.Invoke(vo, new object[] { });
        }

        return null;
    }

    public static ScopeAstThing GlobalScope(this IAstThing node)
    {
        // TODO: WARNING: This could return null (a non-scope AST Node), if the root is not a scope node
        if (node.Parent == null)
        {
            return node as ScopeAstThing;
        }

        return node?.Parent.GlobalScope();
    }

    public static bool Implements<TInterface>(this Type t)
    {
        return t.GetInterfaces().Contains(typeof(TInterface));
    }

    public static bool IsBuiltinType(string typename)
    {
        return LookupBuiltinType(typename) != null;
    }

    public static FifthType Lookup(this TypeId tid)
    {
        if (TypeRegistry.DefaultRegistry.TryGetType(tid, out var ft))
        {
            return ft;
        }

        return null;
    }

    public static TypeId? LookupBuiltinType(string typename)
    {
        if (TypeRegistry.DefaultRegistry.TryGetTypeByName(typename, out var type))
        {
            return type.Id;
        }

        return null;
    }

    public static string LookupOperationName(Operator op)
    {
        var opName = Enum.GetName(typeof(Operator), op);
        if (typeof(Operator).GetField(opName!).TryGetAttribute<OpAttribute>(out var attr))
        {
            return attr.CommonName;
        }

        throw new TypeCheckingException("Unrecognised Operation");
    }

    public static FifthType? LookupType(string typename)
    {
        if (IsBuiltinType(typename))
        {
            return LookupBuiltinType(typename);
        }

        throw new TypeCheckingException("no way to lookup non native types yet");
    }

    public static FifthType LookupType(this Type type)
    {
        if (NewTypeRegistry.DefaultRegistry.TryLookupType(type, out var result))
        {
            return result.Id;
        }

        throw new TypeCheckingException("no way to lookup non native types yet");
    }

    public static IEnumerable<MethodInfo> MethodsHavingAttribute<TAttribute>(this Type t)
    {
        return t.GetMethods().Where(mi => mi.GetCustomAttributes(true).Any(attr => attr is TAttribute));
    }

    public static ScopeAstThing NearestScope(this IAstThing node)
    {
        if (node is ScopeAstThing astNode)
        {
            return astNode;
        }

        return node?.Parent.NearestScope();
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

            ft = new FifthType.TListOf(typeParamAsFifthType);
            ft = new PrimitiveList();
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
