using System.Reflection;

namespace code_generator.Emit;

/// <summary>
/// Builds IL signatures for external method calls
/// </summary>
public static class SignatureFormatter
{
    /// <summary>
    /// Builds a fully qualified external call signature for a .NET method
    /// </summary>
    public static string BuildExternalCallSignature(MethodInfo methodInfo, Type fallbackType)
    {
        var declaring = methodInfo.DeclaringType ?? fallbackType;
        var asmName = declaring.Assembly.GetName().Name ?? "System.Runtime";
        var ns = declaring.Namespace ?? string.Empty;
        var typeName = (declaring.Name ?? fallbackType.Name ?? "Type").Replace('+', '.');

        var parameters = methodInfo.GetParameters();
        var paramTokens = parameters.Select(p => FormatTypeToken(p.ParameterType, asmName)).ToList();
        var returnToken = FormatTypeToken(methodInfo.ReturnType, asmName);

        var joinedParams = paramTokens.Count > 0 ? string.Join(',', paramTokens) : string.Empty;
        return $"extcall:Asm={asmName};Ns={ns};Type={typeName};Method={methodInfo.Name};Params={joinedParams};Return={returnToken}";
    }

    /// <summary>
    /// Builds a fallback external call signature when method resolution fails
    /// </summary>
    public static string BuildFallbackExternalSignature(Type extType, string methodName, int argCount)
    {
        var asmName = extType.Assembly.GetName().Name ?? "System.Runtime";
        var ns = extType.Namespace ?? string.Empty;
        var typeName = (extType.Name ?? "Type").Replace('+', '.');
        var paramList = argCount > 0 ? string.Join(',', Enumerable.Repeat("System.Object@" + asmName, argCount)) : string.Empty;
        return $"extcall:Asm={asmName};Ns={ns};Type={typeName};Method={methodName};Params={paramList};Return=System.Void";
    }

    /// <summary>
    /// Determines if the qualifier should be used as the receiver (first argument) for a method call
    /// </summary>
    public static bool ShouldUseQualifierAsReceiver(
        MethodInfo methodInfo, 
        ast.Expression? qualifier, 
        Type? receiverType, 
        ParameterInfo[] parameters, 
        int suppliedArgCount)
    {
        if (qualifier == null || parameters.Length == 0)
        {
            return false;
        }

        if (receiverType != null && parameters[0].ParameterType.IsAssignableFrom(receiverType))
        {
            return true;
        }

        // Check for extension method attribute
        if (methodInfo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), inherit: false))
        {
            return true;
        }

        // Alternative check: if parameters.Length is one more than suppliedArgCount,
        // treat the first parameter as the receiver (likely an extension method)
        if (parameters.Length == suppliedArgCount + 1)
        {
            return true;
        }

        return false;
    }

    private static string FormatTypeToken(Type t, string defaultAssembly)
    {
        if (t.IsByRef) t = t.GetElementType() ?? typeof(object);
        
        if (t == typeof(void)) return "System.Void";
        if (t == typeof(int)) return "System.Int32";
        if (t == typeof(string)) return "System.String";
        if (t == typeof(float)) return "System.Single";
        if (t == typeof(double)) return "System.Double";
        if (t == typeof(bool)) return "System.Boolean";
        if (t == typeof(long)) return "System.Int64";
        if (t == typeof(short)) return "System.Int16";
        if (t == typeof(byte)) return "System.Byte";
        if (t == typeof(char)) return "System.Char";
        if (t == typeof(object)) return "System.Object";
        if (t == typeof(decimal)) return "System.Decimal";

        var fullName = t.FullName?.Replace('+', '.') ?? t.Name;
        var ownerAsm = t.Assembly.GetName().Name ?? defaultAssembly;
        
        if ((t.Namespace ?? string.Empty).StartsWith("System", StringComparison.Ordinal))
        {
            return fullName ?? t.Name;
        }
        
        return $"{fullName}@{ownerAsm}";
    }
}
