using System.Globalization;
using LfrlAnvil.Numerics;

namespace LfrlAnvil.Tests.NumericsTests.PercentTests;

public class PercentTests : TestsBase
{
    [Fact]
    public void Zero_ShouldReturnCorrectResult()
    {
        var sut = Percent.Zero;
        sut.NormalizedValue.Should().Be( 0m );
    }

    [Fact]
    public void OneHundred_ShouldReturnCorrectResult()
    {
        var sut = Percent.OneHundred;
        sut.NormalizedValue.Should().Be( 1m );
    }

    [Fact]
    public void Create_WithInt64_ShouldReturnCorrectResult()
    {
        var sut = Percent.Create( 1234L );
        sut.NormalizedValue.Should().Be( 12.34m );
    }

    [Fact]
    public void Create_WithDouble_ShouldReturnCorrectResult()
    {
        var sut = Percent.Create( 1234.567 );
        sut.NormalizedValue.Should().Be( 12.34567m );
    }

    [Fact]
    public void Create_WithDecimal_ShouldReturnCorrectResult()
    {
        var sut = Percent.Create( 1234.567m );
        sut.NormalizedValue.Should().Be( 12.34567m );
    }

    [Fact]
    public void CreateNormalized_ShouldReturnCorrectResult()
    {
        var sut = Percent.CreateNormalized( 1234.567m );
        sut.NormalizedValue.Should().Be( 1234.567m );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 33 )]
    [InlineData( 99 )]
    [InlineData( 123 )]
    [InlineData( -123 )]
    public void Value_ShouldReturnNormalizedValueMultipliedByOneHundred(int value)
    {
        var sut = Percent.Create( value );
        sut.Value.Should().Be( value );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var expected = 123.4567m.ToString( "N2", NumberFormatInfo.CurrentInfo ) + "%";
        var sut = Percent.Create( 123.4567m );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = Percent.Create( 123 );
        var result = sut.GetHashCode();
        result.Should().Be( 1.23m.GetHashCode() );
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 123, 123 )]
    [InlineData( -123, 123 )]
    public void Abs_ShouldReturnCorrectResult(int value, int expected)
    {
        var sut = Percent.Create( value );
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
        var sut = Percent.Create( value );
        var result = sut.Truncate();
        result.Value.Should().Be( (decimal)expected );
    }

    [Theory]
    [InlineData( 123.4, 0, 123 )]
    [InlineData( 123.45, 2, 123.45 )]
    [InlineData( 123.456, 1, 123.5 )]
    [InlineData( 123.4618, 2, 123.46 )]
    public void Round_ShouldReturnCorrectResult(double value, int decimals, double expected)
    {
        var sut = Percent.Create( value );
        var result = sut.Round( decimals );
        result.Value.Should().Be( (decimal)expected );
    }

    [Theory]
    [InlineData( 123, 45, 168 )]
    [InlineData( -123, 45, -78 )]
    [InlineData( 123, -45, 78 )]
    [InlineData( -123, -45, -168 )]
    public void Offset_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = Percent.Create( left );
        var b = Percent.Create( right );

        var result = a.Offset( b );

        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, -123 )]
    [InlineData( -123, 123 )]
    public void NegateOperator_ShouldReturnCorrectResult(int value, int expected)
    {
        var sut = Percent.Create( value );
        var result = -sut;
        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 320 )]
    [InlineData( 200, 120, 440 )]
    [InlineData( 200, -60, 80 )]
    [InlineData( 200, -120, -40 )]
    [InlineData( -200, 60, -320 )]
    [InlineData( -200, 120, -440 )]
    [InlineData( -200, -60, -80 )]
    [InlineData( -200, -120, 40 )]
    public void AddOperator_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = Percent.Create( left );
        var b = Percent.Create( right );

        var result = a + b;

        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 80 )]
    [InlineData( 200, 120, -40 )]
    [InlineData( 200, -60, 320 )]
    [InlineData( 200, -120, 440 )]
    [InlineData( -200, 60, -80 )]
    [InlineData( -200, 120, 40 )]
    [InlineData( -200, -60, -320 )]
    [InlineData( -200, -120, -440 )]
    public void SubtractOperator_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = Percent.Create( left );
        var b = Percent.Create( right );

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
        var a = Percent.Create( left );
        var b = Percent.Create( right );

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
        var a = Percent.Create( left );
        var b = Percent.Create( right );

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
        var a = Percent.Create( left );
        var b = Percent.Create( right );

        var result = a % b;

        result.Value.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 320 )]
    [InlineData( 200, 120, 440 )]
    [InlineData( 200, -60, 80 )]
    [InlineData( 200, -120, -40 )]
    [InlineData( -200, 60, -320 )]
    [InlineData( -200, 120, -440 )]
    [InlineData( -200, -60, -80 )]
    [InlineData( -200, -120, 40 )]
    public void AddOperator_ForDecimal_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (decimal)left;
        var b = Percent.Create( right );

        var result = a + b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 80 )]
    [InlineData( 200, 120, -40 )]
    [InlineData( 200, -60, 320 )]
    [InlineData( 200, -120, 440 )]
    [InlineData( -200, 60, -80 )]
    [InlineData( -200, 120, 40 )]
    [InlineData( -200, -60, -320 )]
    [InlineData( -200, -120, -440 )]
    public void SubtractOperator_ForDecimal_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (decimal)left;
        var b = Percent.Create( right );

        var result = a - b;

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
    public void MultiplyOperator_ForDecimal_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (decimal)left;
        var b = Percent.Create( right );

        var result = a * b;

        result.Should().Be( expected );
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
    public void DivideOperator_ForDecimal_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (decimal)left;
        var b = Percent.Create( right );

        var result = a / b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 320 )]
    [InlineData( 200, 120, 440 )]
    [InlineData( 200, -60, 80 )]
    [InlineData( 200, -120, -40 )]
    [InlineData( -200, 60, -320 )]
    [InlineData( -200, 120, -440 )]
    [InlineData( -200, -60, -80 )]
    [InlineData( -200, -120, 40 )]
    public void AddOperator_ForDouble_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (double)left;
        var b = Percent.Create( right );

        var result = a + b;

        result.Should().BeApproximately( expected, 0.0000001 );
    }

    [Theory]
    [InlineData( 200, 60, 80 )]
    [InlineData( 200, 120, -40 )]
    [InlineData( 200, -60, 320 )]
    [InlineData( 200, -120, 440 )]
    [InlineData( -200, 60, -80 )]
    [InlineData( -200, 120, 40 )]
    [InlineData( -200, -60, -320 )]
    [InlineData( -200, -120, -440 )]
    public void SubtractOperator_ForDouble_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (double)left;
        var b = Percent.Create( right );

        var result = a - b;

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
    public void MultiplyOperator_ForDouble_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (double)left;
        var b = Percent.Create( right );

        var result = a * b;

        result.Should().BeApproximately( expected, 0.0000001 );
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
    public void DivideOperator_ForDouble_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (double)left;
        var b = Percent.Create( right );

        var result = a / b;

        result.Should().BeApproximately( expected, 0.0000001 );
    }

    [Theory]
    [InlineData( 200, 60, 320 )]
    [InlineData( 200, 120, 440 )]
    [InlineData( 200, -60, 80 )]
    [InlineData( 200, -120, -40 )]
    [InlineData( -200, 60, -320 )]
    [InlineData( -200, 120, -440 )]
    [InlineData( -200, -60, -80 )]
    [InlineData( -200, -120, 40 )]
    public void AddOperator_ForFloat_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (float)left;
        var b = Percent.Create( right );

        var result = a + b;

        result.Should().BeApproximately( expected, 0.0001 );
    }

    [Theory]
    [InlineData( 200, 60, 80 )]
    [InlineData( 200, 120, -40 )]
    [InlineData( 200, -60, 320 )]
    [InlineData( 200, -120, 440 )]
    [InlineData( -200, 60, -80 )]
    [InlineData( -200, 120, 40 )]
    [InlineData( -200, -60, -320 )]
    [InlineData( -200, -120, -440 )]
    public void SubtractOperator_ForFloat_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (float)left;
        var b = Percent.Create( right );

        var result = a - b;

        result.Should().BeApproximately( expected, 0.0001 );
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
    public void MultiplyOperator_ForFloat_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (float)left;
        var b = Percent.Create( right );

        var result = a * b;

        result.Should().BeApproximately( expected, 0.0001 );
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
    public void DivideOperator_ForFloat_ShouldReturnCorrectResult(int left, int right, int expected)
    {
        var a = (float)left;
        var b = Percent.Create( right );

        var result = a / b;

        result.Should().BeApproximately( expected, 0.0001 );
    }

    [Theory]
    [InlineData( 200, 60, 320 )]
    [InlineData( 200, 120, 440 )]
    [InlineData( 200, -60, 80 )]
    [InlineData( 200, -120, -40 )]
    [InlineData( -200, 60, -320 )]
    [InlineData( -200, 120, -440 )]
    [InlineData( -200, -60, -80 )]
    [InlineData( -200, -120, 40 )]
    public void AddOperator_ForInt64_ShouldReturnCorrectResult(long left, int right, long expected)
    {
        var b = Percent.Create( right );
        var result = left + b;
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 80 )]
    [InlineData( 200, 120, -40 )]
    [InlineData( 200, -60, 320 )]
    [InlineData( 200, -120, 440 )]
    [InlineData( -200, 60, -80 )]
    [InlineData( -200, 120, 40 )]
    [InlineData( -200, -60, -320 )]
    [InlineData( -200, -120, -440 )]
    public void SubtractOperator_ForInt64_ShouldReturnCorrectResult(long left, int right, long expected)
    {
        var b = Percent.Create( right );
        var result = left - b;
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
    public void MultiplyOperator_ForInt64_ShouldReturnCorrectResult(long left, int right, long expected)
    {
        var b = Percent.Create( right );
        var result = left * b;
        result.Should().Be( expected );
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
    public void DivideOperator_ForInt64_ShouldReturnCorrectResult(long left, int right, long expected)
    {
        var b = Percent.Create( right );
        var result = left / b;
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 320 )]
    [InlineData( 200, 120, 440 )]
    [InlineData( 200, -60, 80 )]
    [InlineData( 200, -120, -40 )]
    [InlineData( -200, 60, -320 )]
    [InlineData( -200, 120, -440 )]
    [InlineData( -200, -60, -80 )]
    [InlineData( -200, -120, 40 )]
    public void AddOperator_ForTimeSpan_ShouldReturnCorrectResult(long left, int right, long expected)
    {
        var a = TimeSpan.FromTicks( left );
        var b = Percent.Create( right );

        var result = a + b;

        result.Ticks.Should().Be( expected );
    }

    [Theory]
    [InlineData( 200, 60, 80 )]
    [InlineData( 200, 120, -40 )]
    [InlineData( 200, -60, 320 )]
    [InlineData( 200, -120, 440 )]
    [InlineData( -200, 60, -80 )]
    [InlineData( -200, 120, 40 )]
    [InlineData( -200, -60, -320 )]
    [InlineData( -200, -120, -440 )]
    public void SubtractOperator_ForTimeSpan_ShouldReturnCorrectResult(long left, int right, long expected)
    {
        var a = TimeSpan.FromTicks( left );
        var b = Percent.Create( right );

        var result = a - b;

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
    public void MultiplyOperator_ForTimeSpan_ShouldReturnCorrectResult(long left, int right, long expected)
    {
        var a = TimeSpan.FromTicks( left );
        var b = Percent.Create( right );

        var result = a * b;

        result.Ticks.Should().Be( expected );
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
    public void DivideOperator_ForTimeSpan_ShouldReturnCorrectResult(long left, int right, long expected)
    {
        var a = TimeSpan.FromTicks( left );
        var b = Percent.Create( right );

        var result = a / b;

        result.Ticks.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, false )]
    [InlineData( 2, 1, false )]
    public void EqualityOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Create( aValue );
        var b = Percent.Create( bValue );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, false )]
    [InlineData( 1, 2, true )]
    [InlineData( 2, 1, true )]
    public void InequalityOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Create( aValue );
        var b = Percent.Create( bValue );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, false )]
    [InlineData( 1, 2, true )]
    [InlineData( 2, 1, false )]
    public void LessThanOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Create( aValue );
        var b = Percent.Create( bValue );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, true )]
    [InlineData( 2, 1, false )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Create( aValue );
        var b = Percent.Create( bValue );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, false )]
    [InlineData( 1, 2, false )]
    [InlineData( 2, 1, true )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Create( aValue );
        var b = Percent.Create( bValue );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, false )]
    [InlineData( 2, 1, true )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int aValue, int bValue, bool expected)
    {
        var a = Percent.Create( aValue );
        var b = Percent.Create( bValue );

        var result = a >= b;

        result.Should().Be( expected );
    }
}
