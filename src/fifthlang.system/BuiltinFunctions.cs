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
    public static string write(object s)
    {
        var str = s?.ToString() ?? string.Empty;
        Console.WriteLine(str);
        return str;
    }


}
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

public static class List
{
    // len gives the length of any ienumerable
    [BuiltinFunction]
    public static int len<T>(IEnumerable<T> list)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        return list.Count();
    }

    // head
    [BuiltinFunction]
    public static T head<T>(IEnumerable<T> list)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        return list.First();
    }
    // tail
    [BuiltinFunction]
    public static List<T> tail<T>(IEnumerable<T> list)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        return list.Skip(1).ToList();
    }

    [BuiltinFunction]
    public static List<T> concat<T>(IEnumerable<T> list1, IEnumerable<T> list2)
    {
        return list1.Concat(list2).ToList();
    }
}
