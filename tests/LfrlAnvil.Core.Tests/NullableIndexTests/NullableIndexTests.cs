using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Tests.NullableIndexTests;

public class NullableIndexTests : TestsBase
{
    [Fact]
    public void Null_ShouldNotHaveValue()
    {
        var sut = NullableIndex.Null;

        Assertion.All(
                sut.HasValue.TestFalse(),
                sut.ToString().TestEquals( "NULL" ) )
            .Go();
    }

    [Theory]
    [InlineData( int.MinValue )]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 123 )]
    [InlineData( int.MaxValue - 1 )]
    public void Create_ShouldCreateIndex(int value)
    {
        var sut = NullableIndex.Create( value );

        Assertion.All(
                sut.HasValue.TestTrue(),
                sut.Value.TestEquals( value ),
                sut.ToString().TestEquals( value.ToString() ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldAllowToCreateNull()
    {
        var sut = NullableIndex.Create( NullableIndex.NullValue );

        Assertion.All(
                sut.HasValue.TestFalse(),
                sut.ToString().TestEquals( "NULL" ) )
            .Go();
    }

    [Theory]
    [InlineData( int.MinValue )]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 123 )]
    [InlineData( int.MaxValue - 1 )]
    public void Create_FromNullableInt_ShouldCreateIndex(int? value)
    {
        var sut = NullableIndex.Create( value );

        Assertion.All(
                sut.HasValue.TestTrue(),
                sut.Value.ToNullable().TestEquals( value ),
                sut.ToString().TestEquals( value.ToString() ) )
            .Go();
    }

    [Fact]
    public void Create_FromNullableInt_ShouldAllowToCreateNull()
    {
        var sut = NullableIndex.Create( null );

        Assertion.All(
                sut.HasValue.TestFalse(),
                sut.ToString().TestEquals( "NULL" ) )
            .Go();
    }

    [Theory]
    [InlineData( 123 )]
    [InlineData( NullableIndex.NullValue )]
    public void GetHashCode_ShouldReturnCorrectResult(int value)
    {
        var sut = NullableIndex.Create( value );
        var result = sut.GetHashCode();
        result.TestEquals( value.GetHashCode() ).Go();
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 124, false )]
    [InlineData( 124, 123, false )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, true )]
    [InlineData( NullableIndex.NullValue, 123, false )]
    [InlineData( 123, NullableIndex.NullValue, false )]
    public void Equals_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.Create( val1 );
        var b = NullableIndex.Create( val2 );

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, 0 )]
    [InlineData( 123, 124, -1 )]
    [InlineData( 124, 123, 1 )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, 0 )]
    [InlineData( NullableIndex.NullValue, 123, 1 )]
    [InlineData( 123, NullableIndex.NullValue, -1 )]
    public void CompareTo_ShouldReturnCorrectResult(int val1, int val2, int expectedSign)
    {
        var a = NullableIndex.Create( val1 );
        var b = NullableIndex.Create( val2 );

        var result = a.CompareTo( b );

        Math.Sign( result ).TestEquals( expectedSign ).Go();
    }

    [Theory]
    [InlineData( 123, 123 )]
    [InlineData( NullableIndex.NullValue, null )]
    public void NullableIntConversionOperator_ShouldReturnCorrectResult(int value, int? expected)
    {
        var sut = NullableIndex.Create( value );
        var result = ( int? )sut;
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 124 )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue )]
    public void IncrementOperator_ShouldReturnCorrectResult(int value, int expected)
    {
        var sut = NullableIndex.Create( value );
        sut++;
        sut.TestEquals( NullableIndex.Create( expected ) ).Go();
    }

    [Theory]
    [InlineData( 123, 122 )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue )]
    public void DecrementOperator_ShouldReturnCorrectResult(int value, int expected)
    {
        var sut = NullableIndex.Create( value );
        sut--;
        sut.TestEquals( NullableIndex.Create( expected ) ).Go();
    }

    [Theory]
    [InlineData( 123, 17, 140 )]
    [InlineData( 123, NullableIndex.NullValue, NullableIndex.NullValue )]
    [InlineData( NullableIndex.NullValue, 123, NullableIndex.NullValue )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, NullableIndex.NullValue )]
    public void AddOperator_ShouldReturnCorrectResult(int val1, int val2, int expected)
    {
        var a = NullableIndex.Create( val1 );
        var b = NullableIndex.Create( val2 );

        var result = a + b;

        result.TestEquals( NullableIndex.Create( expected ) ).Go();
    }

    [Theory]
    [InlineData( 123, 13, 110 )]
    [InlineData( 123, NullableIndex.NullValue, NullableIndex.NullValue )]
    [InlineData( NullableIndex.NullValue, 123, NullableIndex.NullValue )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, NullableIndex.NullValue )]
    public void SubtractOperator_ShouldReturnCorrectResult(int val1, int val2, int expected)
    {
        var a = NullableIndex.Create( val1 );
        var b = NullableIndex.Create( val2 );

        var result = a - b;

        result.TestEquals( NullableIndex.Create( expected ) ).Go();
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 124, false )]
    [InlineData( 124, 123, false )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, true )]
    [InlineData( NullableIndex.NullValue, 123, false )]
    [InlineData( 123, NullableIndex.NullValue, false )]
    public void EqualityOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.Create( val1 );
        var b = NullableIndex.Create( val2 );

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 124, true )]
    [InlineData( 124, 123, true )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, false )]
    [InlineData( NullableIndex.NullValue, 123, true )]
    [InlineData( 123, NullableIndex.NullValue, true )]
    public void InequalityOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.Create( val1 );
        var b = NullableIndex.Create( val2 );

        var result = a != b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 124, false )]
    [InlineData( 124, 123, true )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, false )]
    [InlineData( NullableIndex.NullValue, 123, true )]
    [InlineData( 123, NullableIndex.NullValue, false )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.Create( val1 );
        var b = NullableIndex.Create( val2 );

        var result = a > b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 124, true )]
    [InlineData( 124, 123, false )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, true )]
    [InlineData( NullableIndex.NullValue, 123, false )]
    [InlineData( 123, NullableIndex.NullValue, true )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.Create( val1 );
        var b = NullableIndex.Create( val2 );

        var result = a <= b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, false )]
    [InlineData( 123, 124, true )]
    [InlineData( 124, 123, false )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, false )]
    [InlineData( NullableIndex.NullValue, 123, false )]
    [InlineData( 123, NullableIndex.NullValue, true )]
    public void LessThanOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.Create( val1 );
        var b = NullableIndex.Create( val2 );

        var result = a < b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123, 123, true )]
    [InlineData( 123, 124, false )]
    [InlineData( 124, 123, true )]
    [InlineData( NullableIndex.NullValue, NullableIndex.NullValue, true )]
    [InlineData( NullableIndex.NullValue, 123, true )]
    [InlineData( 123, NullableIndex.NullValue, false )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int val1, int val2, bool expected)
    {
        var a = NullableIndex.Create( val1 );
        var b = NullableIndex.Create( val2 );

        var result = a >= b;

        result.TestEquals( expected ).Go();
    }
}
