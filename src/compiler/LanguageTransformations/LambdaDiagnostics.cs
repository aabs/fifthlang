namespace compiler.LanguageTransformations;

public static class LambdaDiagnostics
{
    public const string TooManyParameters = "ERR_TOO_MANY_LF_PARAMETERS";

    public static string FormatTooManyParameters(int maxArity, int actual)
        => $"{TooManyParameters}: Lambda functions support at most {maxArity} parameters, got {actual}";
}
