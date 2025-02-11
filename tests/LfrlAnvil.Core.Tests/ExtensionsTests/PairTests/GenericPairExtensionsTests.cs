using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.PairTests;

public abstract class GenericPairExtensionsTests<T1, T2> : TestsBase
{
    [Fact]
    public void ToPair_WithTuple_ShouldReturnCorrectResult()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = Tuple.Create( first, second );

        var result = sut.ToPair();

        Assertion.All( result.First.TestEquals( first ), result.Second.TestEquals( second ) ).Go();
    }

    [Fact]
    public void ToPair_WithValueTuple_ShouldReturnCorrectResult()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = (First: first, Second: second);

        var result = sut.ToPair();

        Assertion.All( result.First.TestEquals( first ), result.Second.TestEquals( second ) ).Go();
    }

    [Fact]
    public void ToTuple_ShouldReturnCorrectResult()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = Pair.Create( first, second );

        var result = sut.ToTuple();

        Assertion.All( result.Item1.TestEquals( first ), result.Item2.TestEquals( second ) ).Go();
    }

    [Fact]
    public void ToValueTuple_ShouldReturnCorrectResult()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = Pair.Create( first, second );

        var result = sut.ToValueTuple();

        Assertion.All( result.Item1.TestEquals( first ), result.Item2.TestEquals( second ) ).Go();
    }

    [Fact]
    public void Deconstruct_ShouldReturnCorrectValues()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = Pair.Create( first, second );

        var (a, b) = sut;

        Assertion.All( a.TestEquals( first ), b.TestEquals( second ) ).Go();
    }
}
