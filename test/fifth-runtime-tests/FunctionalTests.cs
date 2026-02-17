using System;
using System.Collections.Generic;
using FluentAssertions;
using Fifth.System;

namespace fifth_runtime_tests;

public class FunctionalTests
{
    [Fact]
    public void Map_NullArguments_Throw()
    {
        IEnumerable<int> source = new List<int> { 1, 2 };
        Assert.Throws<ArgumentNullException>(() => Functional.map<int, int>(null!, x => x));
        Assert.Throws<ArgumentNullException>(() => Functional.map<int, int>(source, null!));
    }

    [Fact]
    public void Map_EmptySource_ReturnsEmptyList()
    {
        IEnumerable<int> source = Array.Empty<int>();
        var result = Functional.map(source, x => x * 2);

        result.Should().BeEmpty();
        result.Should().NotBeSameAs(source);
    }

    [Fact]
    public void Map_TransformsAllElements_InOrder()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = Functional.map(source, x => x * 10);

        result.Should().Equal(10, 20, 30);
    }

    [Fact]
    public void Filter_NullArguments_Throw()
    {
        IEnumerable<int> source = new List<int> { 1, 2 };
        Assert.Throws<ArgumentNullException>(() => Functional.filter<int>(null!, x => x > 0));
        Assert.Throws<ArgumentNullException>(() => Functional.filter(source, null!));
    }

    [Fact]
    public void Filter_EmptySource_ReturnsEmptyList()
    {
        IEnumerable<int> source = Array.Empty<int>();
        var result = Functional.filter(source, x => x > 0);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Filter_FiltersElements_PreservesOrder()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3, 4, 5 };
        var result = Functional.filter(source, x => x % 2 == 1);

        result.Should().Equal(1, 3, 5);
    }

    [Fact]
    public void FoldLeft_NullArguments_Throw()
    {
        IEnumerable<int> source = new List<int> { 1, 2 };
        Assert.Throws<ArgumentNullException>(() => Functional.foldleft<int, int>(null!, 0, (acc, x) => acc + x));
        Assert.Throws<ArgumentNullException>(() => Functional.foldleft(source, 0, null!));
    }

    [Fact]
    public void FoldLeft_EmptySource_ReturnsSeed()
    {
        IEnumerable<int> source = Array.Empty<int>();
        var result = Functional.foldleft(source, 42, (acc, x) => acc + x);

        result.Should().Be(42);
    }

    [Fact]
    public void FoldLeft_ProcessesInLeftToRightOrder()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = Functional.foldleft(source, 0, (acc, x) => acc * 10 + x);

        result.Should().Be(123);
    }

    [Fact]
    public void FoldRight_NullArguments_Throw()
    {
        IEnumerable<int> source = new List<int> { 1, 2 };
        Assert.Throws<ArgumentNullException>(() => Functional.foldright<int, int>(null!, 0, (x, acc) => acc + x));
        Assert.Throws<ArgumentNullException>(() => Functional.foldright(source, 0, null!));
    }

    [Fact]
    public void FoldRight_EmptySource_ReturnsSeed()
    {
        IEnumerable<int> source = Array.Empty<int>();
        var result = Functional.foldright(source, 42, (x, acc) => acc + x);

        result.Should().Be(42);
    }

    [Fact]
    public void FoldRight_ProcessesInRightToLeftOrder()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = Functional.foldright(source, 0, (x, acc) => acc * 10 + x);

        result.Should().Be(321);
    }

    [Fact]
    public void FlatMap_NullArguments_Throw()
    {
        IEnumerable<int> source = new List<int> { 1, 2 };
        Assert.Throws<ArgumentNullException>(() => Functional.flatmap<int, int>(null!, x => new[] { x }));
        Assert.Throws<ArgumentNullException>(() => Functional.flatmap<int, int>(source, null!));
    }

    [Fact]
    public void FlatMap_EmptySource_ReturnsEmptyList()
    {
        IEnumerable<int> source = Array.Empty<int>();
        var result = Functional.flatmap(source, x => new[] { x, x });

        result.Should().BeEmpty();
    }

    [Fact]
    public void FlatMap_ConcatenatesInnerLists_InOrder()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = Functional.flatmap(source, x => new[] { x, x * 10 });

        result.Should().Equal(1, 10, 2, 20, 3, 30);
    }

    [Fact]
    public void FlatMap_SkipsNullInnerLists()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = Functional.flatmap(source, x => x == 2 ? null! : new[] { x });

        result.Should().Equal(1, 3);
    }

    [Fact]
    public void Zip_NullArguments_Throw()
    {
        IEnumerable<int> a = new List<int> { 1, 2 };
        IEnumerable<int> b = new List<int> { 3, 4 };

        Assert.Throws<ArgumentNullException>(() => Functional.zip<int, int, int>(null!, b, (x, y) => x + y));
        Assert.Throws<ArgumentNullException>(() => Functional.zip<int, int, int>(a, null!, (x, y) => x + y));
        Assert.Throws<ArgumentNullException>(() => Functional.zip<int, int, int>(a, b, null!));
    }

    [Fact]
    public void Zip_EmptyInputs_ReturnsEmptyList()
    {
        IEnumerable<int> a = Array.Empty<int>();
        IEnumerable<int> b = Array.Empty<int>();
        var result = Functional.zip(a, b, (x, y) => x + y);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Zip_UnevenLengths_StopsAtShortest()
    {
        IEnumerable<int> a = new List<int> { 1, 2, 3 };
        IEnumerable<int> b = new List<int> { 10 };
        var result = Functional.zip(a, b, (x, y) => x + y);

        result.Should().Equal(11);
    }

    [Fact]
    public void Compose_NullArguments_Throw()
    {
        Func<int, int> f = x => x + 1;
        Func<int, int> g = x => x * 2;

        Assert.Throws<ArgumentNullException>(() => Functional.compose<int, int, int>(null!, g));
        Assert.Throws<ArgumentNullException>(() => Functional.compose<int, int, int>(f, null!));
    }

    [Fact]
    public void Compose_ComposesInCorrectOrder()
    {
        Func<int, int> f = x => x + 1;
        Func<int, int> g = x => x * 2;

        var h = Functional.compose(f, g);
        h(3).Should().Be(7);
    }

    [Fact]
    public void Compose_InvokesBothFunctionsOnce()
    {
        int fCalls = 0;
        int gCalls = 0;
        Func<int, int> f = x =>
        {
            fCalls++;
            return x + 10;
        };
        Func<int, int> g = x =>
        {
            gCalls++;
            return x * 2;
        };

        var h = Functional.compose(f, g);
        h(5).Should().Be(20);
        fCalls.Should().Be(1);
        gCalls.Should().Be(1);
    }

    [Fact]
    public void Curry_NullArgument_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => Functional.curry<int, int, int>(null!));
    }

    [Fact]
    public void Curry_ReturnsFunctionWithCapturedArguments()
    {
        Func<int, int, int> add = (a, b) => a + b;
        var curried = Functional.curry(add);

        curried(2)(3).Should().Be(5);
    }

    [Fact]
    public void Uncurry_NullArgument_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => Functional.uncurry<int, int, int>(null!));
    }

    [Fact]
    public void Uncurry_ReturnsFunctionWithTwoArguments()
    {
        Func<int, Func<int, int>> add = a => b => a + b;
        var uncurried = Functional.uncurry(add);

        uncurried(2, 3).Should().Be(5);
    }

    [Fact]
    public void CurryUncurry_Roundtrip_PreservesBehavior()
    {
        Func<int, int, int> mul = (a, b) => a * b;
        var curried = Functional.curry(mul);
        var roundtrip = Functional.uncurry(curried);

        roundtrip(4, 5).Should().Be(20);
    }

    [Fact]
    public void Partial_NullArgument_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => Functional.partial<int, int, int>(null!, 1));
    }

    [Fact]
    public void Partial_FixesFirstArgument()
    {
        Func<int, int, int> add = (a, b) => a + b;
        var add10 = Functional.partial(add, 10);

        add10(5).Should().Be(15);
    }

    [Fact]
    public void Partial_WorksWithReferenceTypes()
    {
        Func<string, string, string> concat = (a, b) => a + b;
        var hello = Functional.partial(concat, "hello ");

        hello("world").Should().Be("hello world");
    }
}
