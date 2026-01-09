// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System;

namespace Fifth.System;

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

    [BuiltinFunction]
    public static List<T> op_PlusPlus<T>(IEnumerable<T> list1, IEnumerable<T> list2)
    {
        return concat(list1, list2);
    }
}
