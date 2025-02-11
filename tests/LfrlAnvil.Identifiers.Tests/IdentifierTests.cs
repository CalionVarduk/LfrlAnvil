using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Identifiers.Tests;

[TestClass( typeof( IdentifierTestsData ) )]
public class IdentifierTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = default( Identifier );

        Assertion.All(
                sut.Value.TestEquals( 0UL ),
                sut.High.TestEquals( 0UL ),
                sut.Low.TestEquals( ( ushort )0 ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetCtorWithValueData ) )]
    public void Ctor_WithValue_ShouldReturnCorrectResult(ulong value, ulong expectedHigh, ushort expectedLow)
    {
        var sut = new Identifier( value );

        Assertion.All(
                sut.Value.TestEquals( value ),
                sut.High.TestEquals( expectedHigh ),
                sut.Low.TestEquals( expectedLow ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetCtorWithHighAndLowData ) )]
    public void Ctor_WithHighAndLow_ShouldReturnCorrectResult(ulong high, ushort low, ulong expectedValue)
    {
        var sut = new Identifier( high, low );

        Assertion.All(
                sut.Value.TestEquals( expectedValue ),
                sut.High.TestEquals( high ),
                sut.Low.TestEquals( low ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(ulong value, string expected)
    {
        var sut = new Identifier( value );
        var result = sut.ToString();
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<ulong>();
        var sut = new Identifier( value );
        var expected = value.GetHashCode();

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(ulong v1, ulong v2, int expectedSign)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a.CompareTo( b );

        Math.Sign( result ).TestEquals( expectedSign ).Go();
    }

    [Fact]
    public void UlongConversionOperator_ShouldReturnUnderlyingValue()
    {
        var value = Fixture.Create<ulong>();
        var sut = new Identifier( value );

        var result = ( ulong )sut;

        result.TestEquals( value ).Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a != b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.CreateGreaterThanComparisonTestData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a > b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.CreateGreaterThanOrEqualToComparisonTestData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a >= b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.CreateLessThanComparisonTestData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a < b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.CreateLessThanOrEqualToComparisonTestData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a <= b;

        result.TestEquals( expected ).Go();
    }
}
