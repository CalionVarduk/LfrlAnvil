using System.Globalization;
using LfrlAnvil.Numerics;

namespace LfrlAnvil.Tests.NumericsTests.PercentTests;

public class PercentTests : TestsBase
{
    [Fact]
    public void Zero_ShouldReturnCorrectResult()
    {
        var sut = Percent.Zero;
        sut.Ratio.Should().Be( 0m );
    }

    [Fact]
    public void One_ShouldReturnCorrectResult()
    {
        var sut = Percent.One;
        sut.Ratio.Should().Be( 0.01m );
    }

    [Fact]
    public void OneHundred_ShouldReturnCorrectResult()
    {
        var sut = Percent.OneHundred;
        sut.Ratio.Should().Be( 1m );
    }

    [Fact]
    public void Ctor_ShouldAssignCorrectRatio()
    {
        var sut = new Percent( 1234.567m );
        sut.Ratio.Should().Be( 1234.567m );
    }

    [Fact]
    public void Normalize_WithInt64_ShouldReturnCorrectResult()
    {
        var sut = Percent.Normalize( 1234L );
        sut.Ratio.Should().Be( 12.34m );
    }

    [Fact]
    public void Normalize_WithDouble_ShouldReturnCorrectResult()
    {
        var sut = Percent.Normalize( 1234.567 );
        sut.Ratio.Should().Be( 12.34567m );
    }

    [Fact]
    public void Normalize_WithDecimal_ShouldReturnCorrectResult()
    {
        var sut = Percent.Normalize( 1234.567m );
        sut.Ratio.Should().Be( 12.34567m );
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var sut = Percent.Create( 1234.567m );
        sut.Ratio.Should().Be( 1234.567m );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 33 )]
    [InlineData( 99 )]
    [InlineData( 123 )]
    [InlineData( -123 )]
    public void Value_ShouldReturnRatioMultipliedByOneHundred(int value)
    {
        var sut = Percent.Normalize( value );
        sut.Value.Should().Be( value );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var expected = 1.234567m.ToString( "P2", NumberFormatInfo.CurrentInfo );
        var sut = Percent.Normalize( 123.4567m );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = Percent.Normalize( 123 );
        var result = sut.GetHashCode();
        result.Should().Be( 1.23m.GetHashCode() );
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 123, 123 )]
    [InlineData( -123, 123 )]
    public void Abs_ShouldReturnCorrectResult(int value, int expected)
    {
        var sut = Percent.Normalize( value );
        var result = sut.Abs();
        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 123, 123 )]
    [InlineData( 123.456, 123 )]
    [InlineData( -123, -123 )]
    [InlineData( -123.456, -123 )]
    public void Truncate_ShouldReturnCorrectResult(double value, double expected)
    {
        var sut = Percent.Normalize( value );
        var result = sut.Truncate();
        result.Value.Should().Be( (decimal)expected );
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 123, 123 )]
    [InlineData( 123.456, 123 )]
    [InlineData( -123, -123 )]
    [InlineData( -123.456, -124 )]
    public void Floor_ShouldReturnCorrectResult(double value, double expected)
    {
        var sut = Percent.Normalize( value );
        var result = sut.Floor();
        result.Value.Should().Be( (decimal)expected );
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 123, 123 )]
    [InlineData( 123.456, 124 )]
    [InlineData( -123, -123 )]
    [InlineData( -123.456, -123 )]
    public void Ceiling_ShouldReturnCorrectResult(double value, double expected)
    {
        var sut = Percent.Normalize( value );
        var result = sut.Ceiling();
        result.Value.Should().Be( (decimal)expected );
    }

    [Theory]
    [InlineData( 123.4, 0, 123 )]
    [InlineData( 123.45, 2, 123.45 )]
    [InlineData( 123.456, 1, 123.5 )]
    [InlineData( 123.4618, 2, 123.46 )]
    public void Round_ShouldReturnCorrectResult(double value, int decimals, double expected)
    {
        var sut = Percent.Normalize( value );
        var result = sut.Round( decimals );
        result.Value.Should().Be( (decimal)expected );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 123 )]
    [InlineData( -123 )]
    public void DecimalConversionOperator_ShouldReturnCorrectResult(int value)
    {
        var sut = Percent.Normalize( value );
        var result = (decimal)sut;
        result.Should().Be( sut.Ratio );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 123 )]
    [InlineData( -123 )]
    public void DoubleConversionOperator_ShouldReturnCorrectResult(int value)
    {
        var sut = Percent.Normalize( value );
        var result = (double)sut;
        result.Should().Be( (double)sut.Ratio );
    }

    [Theory]
    [InlineData( 123, -123 )]
    [InlineData( -123, 123 )]
    public void NegateOperator_ShouldReturnCorrectResult(int value, int expected)
    {
        var sut = Percent.Normalize( value );
        var result = -sut;
        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, 124 )]
    [InlineData( -123, -122 )]
    public void IncrementOperator_ShouldReturnCorrectResult(int value, int expected)
    {
        var sut = Percent.Normalize( value );
        var result = ++sut;
        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, 122 )]
    [InlineData( -123, -124 )]
    public void DecrementOperator_ShouldReturnCorrectResult(int value, int expected)
    {
        var sut = Percent.Normalize( value );
        var result = --sut;
        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 260 )]
    [InlineData( 200, 120, 320 )]
    [InlineData( 200, -60, 140 )]
    [InlineData( 200, -120, 80 )]
    [InlineData( -200, 60, -140 )]
    [InlineData( -200, 120, -80 )]
    [InlineData( -200, -60, -260 )]
    [InlineData( -200, -120, -320 )]
    public void AddOperator_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = Percent.Normalize( left );
        var b = Percent.Normalize( right );

        var result = a + b;

        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 140 )]
    [InlineData( 200, 120, 80 )]
    [InlineData( 200, -60, 260 )]
    [InlineData( 200, -120, 320 )]
    [InlineData( -200, 60, -260 )]
    [InlineData( -200, 120, -320 )]
    [InlineData( -200, -60, -140 )]
    [InlineData( -200, -120, -80 )]
    public void SubtractOperator_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = Percent.Normalize( left );
        var b = Percent.Normalize( right );

        var result = a - b;

        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = Percent.Normalize( left );
        var b = Percent.Normalize( right );

        var result = a * b;

        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 180, 60, 300 )]
    [InlineData( 240, 120, 200 )]
    [InlineData( 180, -60, -300 )]
    [InlineData( 240, -120, -200 )]
    [InlineData( -180, 60, -300 )]
    [InlineData( -240, 120, -200 )]
    [InlineData( -180, -60, 300 )]
    [InlineData( -240, -120, 200 )]
    public void DivideOperator_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = Percent.Normalize( left );
        var b = Percent.Normalize( right );

        var result = a / b;

        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 20 )]
    [InlineData( 200, 120, 80 )]
    [InlineData( 200, -60, 20 )]
    [InlineData( 200, -120, 80 )]
    [InlineData( -200, 60, -20 )]
    [InlineData( -200, 120, -80 )]
    [InlineData( -200, -60, -20 )]
    [InlineData( -200, -120, -80 )]
    public void ModuloOperator_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = Percent.Normalize( left );
        var b = Percent.Normalize( right );

        var result = a % b;

        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ForDecimalLeft_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (decimal)left;
        var b = Percent.Normalize( right );

        var result = a * b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ForDecimalRight_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (decimal)right;
        var b = Percent.Normalize( left );

        var result = b * a;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ForDoubleLeft_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (double)left;
        var b = Percent.Normalize( right );

        var result = a * b;

        result.Should().BeApproximately( expected, 0.0000001 );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ForDoubleRight_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (double)right;
        var b = Percent.Normalize( left );

        var result = b * a;

        result.Should().BeApproximately( expected, 0.0000001 );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ForFloatLeft_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (float)left;
        var b = Percent.Normalize( right );

        var result = a * b;

        result.Should().BeApproximately( expected, 0.0001f );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ForFloatRight_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (float)right;
        var b = Percent.Normalize( left );

        var result = b * a;

        result.Should().BeApproximately( expected, 0.0001f );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ForInt64Left_ShouldReturnCorrectResult(long left, int right, long expected)
    {
        var b = Percent.Normalize( right );
        var result = left * b;
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ForInt64Right_ShouldReturnCorrectResult(int left, long right, long expected)
    {
        var b = Percent.Normalize( left );
        var result = b * right;
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ForTimeSpanLeft_ShouldReturnCorrectResult(long left, int right, long expected)
    {
        var a = TimeSpan.FromTicks( left );
        var b = Percent.Normalize( right );

        var result = a * b;

        result.Ticks.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 120 )]
    [InlineData( 200, 120, 240 )]
    [InlineData( 200, -60, -120 )]
    [InlineData( 200, -120, -240 )]
    [InlineData( -200, 60, -120 )]
    [InlineData( -200, 120, -240 )]
    [InlineData( -200, -60, 120 )]
    [InlineData( -200, -120, 240 )]
    public void MultiplyOperator_ForTimeSpanRight_ShouldReturnCorrectResult(int left, long right, long expected)
    {
        var a = TimeSpan.FromTicks( right );
        var b = Percent.Normalize( left );

        var result = b * a;

        result.Ticks.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, false )]
    [InlineData( 2, 1, false )]
    public void EqualityOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Normalize( aValue );
        var b = Percent.Normalize( bValue );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, false )]
    [InlineData( 1, 2, true )]
    [InlineData( 2, 1, true )]
    public void InequalityOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Normalize( aValue );
        var b = Percent.Normalize( bValue );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, false )]
    [InlineData( 1, 2, true )]
    [InlineData( 2, 1, false )]
    public void LessThanOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Normalize( aValue );
        var b = Percent.Normalize( bValue );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, true )]
    [InlineData( 2, 1, false )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Normalize( aValue );
        var b = Percent.Normalize( bValue );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, false )]
    [InlineData( 1, 2, false )]
    [InlineData( 2, 1, true )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Normalize( aValue );
        var b = Percent.Normalize( bValue );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, false )]
    [InlineData( 2, 1, true )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Normalize( aValue );
        var b = Percent.Normalize( bValue );

        var result = a >= b;

        result.Should().Be( expected );
    }
}
