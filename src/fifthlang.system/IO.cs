// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

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
    public static string write(object s)
    {
        var str = s?.ToString() ?? string.Empty;
        Console.WriteLine(str);
        return str;
    }


}
