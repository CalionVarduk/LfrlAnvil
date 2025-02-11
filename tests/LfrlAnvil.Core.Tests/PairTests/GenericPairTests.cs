using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Tests.PairTests;

[GenericTestClass( typeof( GenericPairTestsData<,> ) )]
public abstract class GenericPairTests<T1, T2> : TestsBase
{
    [Fact]
    public void Create_ShouldCreateCorrectPair()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = Pair.Create( first, second );

        Assertion.All( sut.First.TestEquals( first ), sut.Second.TestEquals( second ) ).Go();
    }

    [Fact]
    public void CtorWithValue_ShouldCreateWithCorrectValues()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new Pair<T1, T2>( first, second );

        Assertion.All( sut.First.TestEquals( first ), sut.Second.TestEquals( second ) ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnMixOfFirstAndSecond()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new Pair<T1, T2>( first, second );
        var expected = Hash.Default.Add( first ).Add( second ).Value;

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SetFirst_ShouldReturnWithNewFirstAndOldSecond()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();
        var other = Fixture.Create<double>();

        var sut = new Pair<T1, T2>( first, second );

        var result = sut.SetFirst( other );

        Assertion.All( result.First.TestEquals( other ), result.Second.TestEquals( second ) ).Go();
    }

    [Fact]
    public void SetSecond_ShouldReturnWithOldFirstAndNewSecond()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();
        var other = Fixture.Create<double>();

        var sut = new Pair<T1, T2>( first, second );

        var result = sut.SetSecond( other );

        Assertion.All( result.First.TestEquals( first ), result.Second.TestEquals( other ) ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericPairTestsData<T1, T2>.CreateEqualsTestData ) )]
    public void Equals_ShouldReturnCorrectResult(T1 first1, T2 second1, T1 first2, T2 second2, bool expected)
    {
        var a = new Pair<T1, T2>( first1, second1 );
        var b = new Pair<T1, T2>( first2, second2 );

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericPairTestsData<T1, T2>.CreateEqualsTestData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(T1 first1, T2 second1, T1 first2, T2 second2, bool expected)
    {
        var a = new Pair<T1, T2>( first1, second1 );
        var b = new Pair<T1, T2>( first2, second2 );

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericPairTestsData<T1, T2>.CreateNotEqualsTestData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(T1 first1, T2 second1, T1 first2, T2 second2, bool expected)
    {
        var a = new Pair<T1, T2>( first1, second1 );
        var b = new Pair<T1, T2>( first2, second2 );

        var result = a != b;

        result.TestEquals( expected ).Go();
    }
}
