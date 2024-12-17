// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System;

namespace Fifth.System;
//[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Uses the naming conventions of fifth, not csharp")]
public static class IO
{
    [BuiltinFunction]
    public static string read()
    {
        return Console.ReadLine();
    }

    [BuiltinFunction]
    public static string write(string s)
    {
        Console.WriteLine(s);
        return s;
    }


}
//[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Uses the naming conventions of fifth, not csharp")]
public static class Math
{


    [BuiltinFunction]
    public static double sqrt(double f)
    {
        return global::System.Math.Sqrt(f);
    }

}
