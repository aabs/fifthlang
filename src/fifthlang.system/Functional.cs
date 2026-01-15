// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Fifth.System;

public static class Functional
{
    // ------------------------------------------------------------
    // MAP
    // ------------------------------------------------------------
    /// <summary>
    /// Projects each element of a list into a new form and returns a list of the projected results.
    /// The selector is invoked once per element in order from index 0 to the end of the list.
    /// </summary>
    /// <typeparam name="T">The element type of the input list.</typeparam>
    /// <typeparam name="TResult">The element type of the output list.</typeparam>
    /// <param name="source">The input list to transform.</param>
    /// <param name="selector">The function that maps each source element to a result value.</param>
    /// <returns>A new list containing the mapped results, preserving the input order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    [BuiltinFunction]
    public static List<TResult> map<T, TResult>(
        IEnumerable<T> source,
        Func<T, TResult> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var result = new List<TResult>();

        foreach (var item in source)
            result.Add(selector(item));

        return result;
    }

    // ------------------------------------------------------------
    // FILTER
    // ------------------------------------------------------------
    /// <summary>
    /// Filters a list by applying a predicate to each element and returning only those that match.
    /// Evaluation occurs in index order, and the relative order of matching elements is preserved.
    /// </summary>
    /// <typeparam name="T">The element type of the input list.</typeparam>
    /// <param name="source">The input list to filter.</param>
    /// <param name="predicate">The predicate that determines whether an element is included.</param>
    /// <returns>A new list containing the elements for which <paramref name="predicate"/> returns true.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    [BuiltinFunction]
    public static List<T> filter<T>(
        IEnumerable<T> source,
        Func<T, bool> predicate)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var result = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
                result.Add(item);
        }

        return result;
    }

    // ------------------------------------------------------------
    // FOLD LEFT
    // ------------------------------------------------------------
    /// <summary>
    /// Aggregates a list from left to right by repeatedly applying a folder function to an accumulator.
    /// The seed is used as the initial accumulator value, and each element is processed in index order.
    /// </summary>
    /// <typeparam name="T">The element type of the input list.</typeparam>
    /// <typeparam name="TResult">The accumulator and result type.</typeparam>
    /// <param name="source">The input list to fold.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="folder">The function that combines the accumulator with the next element.</param>
    /// <returns>The final accumulator value after processing all elements.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="folder"/> is null.</exception>
    [BuiltinFunction]
    public static TResult foldleft<T, TResult>(
        IEnumerable<T> source,
        TResult seed,
        Func<TResult, T, TResult> folder)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (folder == null) throw new ArgumentNullException(nameof(folder));

        TResult acc = seed;

        foreach (var item in source)
            acc = folder(acc, item);

        return acc;
    }

    // ------------------------------------------------------------
    // FOLD RIGHT
    // ------------------------------------------------------------
    /// <summary>
    /// Aggregates a list from right to left by repeatedly applying a folder function to an accumulator.
    /// The seed is used as the initial accumulator value, and elements are processed in reverse index order.
    /// </summary>
    /// <typeparam name="T">The element type of the input list.</typeparam>
    /// <typeparam name="TResult">The accumulator and result type.</typeparam>
    /// <param name="source">The input list to fold.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="folder">The function that combines the current element with the accumulator.</param>
    /// <returns>The final accumulator value after processing all elements.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="folder"/> is null.</exception>
    [BuiltinFunction]
    public static TResult foldright<T, TResult>(
        IEnumerable<T> source,
        TResult seed,
        Func<T, TResult, TResult> folder)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (folder == null) throw new ArgumentNullException(nameof(folder));

        TResult acc = seed;

        var list = source as IList<T> ?? new List<T>(source);

        for (int i = list.Count - 1; i >= 0; i--)
            acc = folder(list[i], acc);

        return acc;
    }

    // ------------------------------------------------------------
    // FLATMAP
    // ------------------------------------------------------------
    /// <summary>
    /// Maps each element to a list of results and concatenates all results into a single list.
    /// Elements are processed in order; null inner lists are skipped.
    /// </summary>
    /// <typeparam name="T">The element type of the input list.</typeparam>
    /// <typeparam name="TResult">The element type of the flattened output list.</typeparam>
    /// <param name="source">The input list to transform and flatten.</param>
    /// <param name="selector">The function that maps each source element to a list of results.</param>
    /// <returns>A new list containing all mapped elements in their original order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    [BuiltinFunction]
    public static List<TResult> flatmap<T, TResult>(
        IEnumerable<T> source,
        Func<T, IEnumerable<TResult>> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var result = new List<TResult>();

        foreach (var item in source)
        {
            var inner = selector(item);
            if (inner == null) continue;

            foreach (var innerItem in inner)
                result.Add(innerItem);
        }

        return result;
    }

    // ------------------------------------------------------------
    // ZIP
    // ------------------------------------------------------------
    /// <summary>
    /// Combines two lists element-by-element using a zipper function.
    /// The result length is the smaller of the two input lengths.
    /// </summary>
    /// <typeparam name="T1">The element type of the first input list.</typeparam>
    /// <typeparam name="T2">The element type of the second input list.</typeparam>
    /// <typeparam name="TResult">The element type of the output list.</typeparam>
    /// <param name="a">The first input list.</param>
    /// <param name="b">The second input list.</param>
    /// <param name="zipper">The function that combines corresponding elements.</param>
    /// <returns>A new list containing the zipped results, in index order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="a"/>, <paramref name="b"/>, or <paramref name="zipper"/> is null.</exception>
    [BuiltinFunction]
    public static List<TResult> zip<T1, T2, TResult>(
        IEnumerable<T1> a,
        IEnumerable<T2> b,
        Func<T1, T2, TResult> zipper)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));
        if (zipper == null) throw new ArgumentNullException(nameof(zipper));

        var result = new List<TResult>();

        using var enumA = a.GetEnumerator();
        using var enumB = b.GetEnumerator();

        while (enumA.MoveNext() && enumB.MoveNext())
            result.Add(zipper(enumA.Current, enumB.Current));

        return result;
    }

    // ------------------------------------------------------------
    // COMPOSE
    // ------------------------------------------------------------
    /// <summary>
    /// Creates a new function that represents the composition of two functions, $f \circ g$.
    /// The returned function applies <paramref name="g"/> to its input and then applies <paramref name="f"/> to the result.
    /// </summary>
    /// <typeparam name="T">The input type of <paramref name="g"/> and the composed function.</typeparam>
    /// <typeparam name="TMid">The output type of <paramref name="g"/> and input type of <paramref name="f"/>.</typeparam>
    /// <typeparam name="TResult">The output type of <paramref name="f"/> and the composed function.</typeparam>
    /// <param name="f">The outer function applied last.</param>
    /// <param name="g">The inner function applied first.</param>
    /// <returns>A function equivalent to <c>x =&gt; f(g(x))</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="f"/> or <paramref name="g"/> is null.</exception>
    [BuiltinFunction]
    public static Func<T, TResult> compose<T, TMid, TResult>(
        Func<TMid, TResult> f,
        Func<T, TMid> g)
    {
        if (f == null) throw new ArgumentNullException(nameof(f));
        if (g == null) throw new ArgumentNullException(nameof(g));

        return x => f(g(x));
    }

    // ------------------------------------------------------------
    // CURRY
    // ------------------------------------------------------------
    /// <summary>
    /// Transforms a two-parameter function into a chain of single-parameter functions.
    /// This enables partial application by supplying the first argument and receiving a function for the second.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="f">The function to curry.</param>
    /// <returns>A function that takes the first argument and returns a function awaiting the second argument.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="f"/> is null.</exception>
    [BuiltinFunction]
    public static Func<T1, Func<T2, TResult>> curry<T1, T2, TResult>(
        Func<T1, T2, TResult> f)
    {
        if (f == null) throw new ArgumentNullException(nameof(f));

        return a => b => f(a, b);
    }

    // ------------------------------------------------------------
    // UNCURRY
    // ------------------------------------------------------------
    /// <summary>
    /// Transforms a curried two-parameter function into a single function that takes both arguments at once.
    /// This is the inverse of <see cref="curry{T1,T2,TResult}(Func{T1,T2,TResult})"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="f">The curried function to uncurry.</param>
    /// <returns>A function that accepts both arguments and returns the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="f"/> is null.</exception>
    [BuiltinFunction]
    public static Func<T1, T2, TResult> uncurry<T1, T2, TResult>(
        Func<T1, Func<T2, TResult>> f)
    {
        if (f == null) throw new ArgumentNullException(nameof(f));

        return (a, b) => f(a)(b);
    }

    // ------------------------------------------------------------
    // PARTIAL APPLICATION
    // ------------------------------------------------------------
    /// <summary>
    /// Partially applies a two-parameter function by fixing the first argument.
    /// The returned function accepts the remaining argument and produces the result.
    /// </summary>
    /// <typeparam name="T1">The type of the fixed argument.</typeparam>
    /// <typeparam name="T2">The type of the remaining argument.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="f">The function to partially apply.</param>
    /// <param name="fixedArg">The value to bind as the first argument.</param>
    /// <returns>A function that takes the second argument and returns the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="f"/> is null.</exception>
    [BuiltinFunction]
    public static Func<T2, TResult> partial<T1, T2, TResult>(
        Func<T1, T2, TResult> f,
        T1 fixedArg)
    {
        if (f == null) throw new ArgumentNullException(nameof(f));

        return b => f(fixedArg, b);
    }
}
