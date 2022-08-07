using FluentAssertions.Execution;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Identifiers.Tests.IdentifierTests;

[TestClass( typeof( IdentifierTestsData ) )]
public class IdentifierTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = default( Identifier );

        using ( new AssertionScope() )
        {
            sut.Value.Should().Be( 0 );
            sut.High.Should().Be( 0 );
            sut.Low.Should().Be( 0 );
        }
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetCtorWithValueData ) )]
    public void Ctor_WithValue_ShouldReturnCorrectResult(ulong value, ulong expectedHigh, ushort expectedLow)
    {
        var sut = new Identifier( value );

        using ( new AssertionScope() )
        {
            sut.Value.Should().Be( value );
            sut.High.Should().Be( expectedHigh );
            sut.Low.Should().Be( expectedLow );
        }
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetCtorWithHighAndLowData ) )]
    public void Ctor_WithHighAndLow_ShouldReturnCorrectResult(ulong high, ushort low, ulong expectedValue)
    {
        var sut = new Identifier( high, low );

        using ( new AssertionScope() )
        {
            sut.Value.Should().Be( expectedValue );
            sut.High.Should().Be( high );
            sut.Low.Should().Be( low );
        }
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(ulong value, string expected)
    {
        var sut = new Identifier( value );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<ulong>();
        var sut = new Identifier( value );
        var expected = value.GetHashCode();

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(ulong v1, ulong v2, int expectedSign)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a.CompareTo( b );

        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Fact]
    public void UlongConversionOperator_ShouldReturnUnderlyingValue()
    {
        var value = Fixture.Create<ulong>();
        var sut = new Identifier( value );

        var result = (ulong)sut;

        result.Should().Be( value );
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.CreateGreaterThanComparisonTestData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.CreateGreaterThanOrEqualToComparisonTestData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a >= b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.CreateLessThanComparisonTestData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierTestsData.CreateLessThanOrEqualToComparisonTestData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(ulong v1, ulong v2, bool expected)
    {
        var a = new Identifier( v1 );
        var b = new Identifier( v2 );

        var result = a <= b;

        result.Should().Be( expected );
    }
}
