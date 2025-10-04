using static Fifth.DebugHelpers;

namespace code_generator;

/// <summary>
/// Utility functions for parsing and extracting information from method signatures
/// </summary>
internal static class SignatureUtilities
{
    /// <summary>
    /// Extract method name from a method signature
    /// </summary>
    public static string ExtractMethodName(string methodSignature)
    {
        // Simple extraction - assume method signature format like "methodName" or "Program::methodName"
        if (methodSignature.Contains("::"))
        {
            return methodSignature.Split("::").Last();
        }

        // If it looks like a simple method name, return as-is
        if (!methodSignature.Contains(" ") && !methodSignature.Contains("("))
        {
            return methodSignature;
        }

        // For more complex signatures, try to extract the method name before parentheses
        var parenIndex = methodSignature.IndexOf('(');
        if (parenIndex > 0)
        {
            var beforeParen = methodSignature.Substring(0, parenIndex).Trim();
            var spaceIndex = beforeParen.LastIndexOf(' ');
            if (spaceIndex >= 0)
            {
                return beforeParen.Substring(spaceIndex + 1);
            }
            return beforeParen;
        }

        return methodSignature;
    }

    /// <summary>
    /// Extract type name from a constructor signature like "instance void TypeName::.ctor()"
    /// </summary>
    public static string ExtractCtorTypeName(string ctorSignature)
    {
        try
        {
            var raw = (ctorSignature ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            // Handle bracketed assembly-qualified signatures: "void [Asm]Namespace.Type::.ctor()"
            if (raw.Contains('[') && raw.Contains(']') && raw.Contains("::"))
            {
                var lb = raw.IndexOf('[');
                var rb = raw.IndexOf(']');
                if (rb > lb)
                {
                    var after = raw.Substring(rb + 1).Trim();
                    var sep = after.IndexOf("::", StringComparison.Ordinal);
                    if (sep > 0)
                    {
                        var fullType = after.Substring(0, sep).Trim();
                        var simple = fullType.Contains('.') ? fullType.Split('.').Last() : fullType;
                        DebugLog($"DEBUG: ExtractCtorTypeName parsed bracketed signature -> fullType='{fullType}', simple='{simple}'");
                        return simple;
                    }
                }
            }

            // Handle common form: 'instance void Namespace.Type::.ctor()' or 'void Namespace.Type::.ctor()'
            var sepIndex = raw.IndexOf("::", StringComparison.Ordinal);
            if (sepIndex > 0)
            {
                // Find the token that denotes the start of the type name (skip return-type token if present)
                var lastSpaceBeforeSep = raw.LastIndexOf(' ', sepIndex);
                var start = lastSpaceBeforeSep >= 0 ? lastSpaceBeforeSep + 1 : 0;
                var typePart = raw.Substring(start, sepIndex - start).Trim();
                var simple = typePart.Contains('.') ? typePart.Split('.').Last() : typePart;
                DebugLog($"DEBUG: ExtractCtorTypeName parsed simple signature -> typePart='{typePart}', simple='{simple}'");
                return simple;
            }
        }
        catch (Exception ex)
        {
            DebugLog($"DEBUG: ExtractCtorTypeName error parsing '{ctorSignature}': {ex.Message}");
        }
        return string.Empty;
    }
}
