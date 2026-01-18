using LfrlAnvil.Internal;

namespace LfrlAnvil.Tests.Int31BoolPairTests;

public class Int31BoolPairTests : TestsBase
{
    [Theory]
    [InlineData( 123U, 123, false )]
    [InlineData( int.MaxValue + 124U, 123, true )]
    public void Ctor_ShouldCreateCorrectResult(uint data, int expectedInt, bool expectedBool)
    {
        var result = new Int31BoolPair( data );
        Assertion.All(
                result.Data.TestEquals( data ),
                result.IntValue.TestEquals( expectedInt ),
                result.BoolValue.TestEquals( expectedBool ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithInt_ShouldCreateCorrectResult()
    {
        var result = new Int31BoolPair( 123 );
        Assertion.All(
                result.Data.TestEquals( 123U ),
                result.IntValue.TestEquals( 123 ),
                result.BoolValue.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( 123, false, 123U )]
    [InlineData( 123, true, int.MaxValue + 124U )]
    public void Ctor_WithIntAndBool_ShouldCreateCorrectResult(int intValue, bool boolValue, uint expectedData)
    {
        var result = new Int31BoolPair( intValue, boolValue );
        Assertion.All(
                result.Data.TestEquals( expectedData ),
                result.IntValue.TestEquals( intValue ),
                result.BoolValue.TestEquals( boolValue ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new Int31BoolPair( 123, true );
        var result = sut.ToString();
        result.TestEquals( "Int = 123, Bool = True" ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = new Int31BoolPair( 123, true );
        var result = sut.GetHashCode();
        result.TestEquals( sut.Data.GetHashCode() ).Go();
    }

    [Theory]
    [InlineData( 123U, 123U, true )]
    [InlineData( 123U, 124U, false )]
    [InlineData( 124U, 123U, false )]
    public void Equals_ShouldReturnCorrectResult(uint val1, uint val2, bool expected)
    {
        var a = new Int31BoolPair( val1 );
        var b = new Int31BoolPair( val2 );

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Int31BoolPairConversionOperator_ShouldReturnCorrectResult()
    {
        var result = ( Int31BoolPair )123U;
        Assertion.All(
                result.Data.TestEquals( 123U ),
                result.IntValue.TestEquals( 123 ),
                result.BoolValue.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( 123U, 123U, true )]
    [InlineData( 123U, 124U, false )]
    [InlineData( 124U, 123U, false )]
    public void EqualityOperator_ShouldReturnCorrectResult(uint val1, uint val2, bool expected)
    {
        var a = new Int31BoolPair( val1 );
        var b = new Int31BoolPair( val2 );

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 123U, 123U, false )]
    [InlineData( 123U, 124U, true )]
    [InlineData( 124U, 123U, true )]
    public void InequalityOperator_ShouldReturnCorrectResult(uint val1, uint val2, bool expected)
    {
        var a = new Int31BoolPair( val1 );
        var b = new Int31BoolPair( val2 );

        var result = a != b;

        result.TestEquals( expected ).Go();
    }
}
