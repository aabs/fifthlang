// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Fifth.System;

//[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Uses the naming conventions of fifth, not csharp")]
public static class Math
{

    [BuiltinFunction]
    public static int to_int(double f)
    {
        return (int)f;
    }
    [BuiltinFunction]
    public static double sqrt(double f)
    {
        return global::System.Math.Sqrt(f);
    }

}
