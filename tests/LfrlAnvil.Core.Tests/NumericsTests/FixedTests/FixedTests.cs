using System.Globalization;
using LfrlAnvil.Functional;
using LfrlAnvil.Numerics;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Tests.NumericsTests.FixedTests;

[TestClass( typeof( FixedTestsData ) )]
public class FixedTests : TestsBase
{
    [Fact]
    public void Zero_ShouldReturnCorrectResult()
    {
        var sut = Fixed.Zero;

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( 0 );
            sut.Precision.Should().Be( 0 );
        }
    }

    [Fact]
    public void MaxValue_ShouldReturnCorrectResult()
    {
        var sut = Fixed.MaxValue;

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( long.MaxValue );
            sut.Precision.Should().Be( 0 );
        }
    }

    [Fact]
    public void MinValue_ShouldReturnCorrectResult()
    {
        var sut = Fixed.MinValue;

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( long.MinValue );
            sut.Precision.Should().Be( 0 );
        }
    }

    [Fact]
    public void Epsilon_ShouldReturnCorrectResult()
    {
        var sut = Fixed.Epsilon;

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( 1 );
            sut.Precision.Should().Be( 18 );
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 10 )]
    [InlineData( 18 )]
    public void CreateZero_ShouldReturnCorrectResult(byte precision)
    {
        var sut = Fixed.CreateZero( precision );

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( 0 );
            sut.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 19 )]
    [InlineData( 20 )]
    public void CreateZero_ShouldThrowArgumentOutOfRangeException_WhenPrecisionAreGreaterThanEighteen(byte precision)
    {
        var action = Lambda.Of( () => Fixed.CreateZero( precision ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 10 )]
    [InlineData( 18 )]
    public void CreateMaxValue_ShouldReturnCorrectResult(byte precision)
    {
        var sut = Fixed.CreateMaxValue( precision );

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( long.MaxValue );
            sut.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 19 )]
    [InlineData( 20 )]
    public void CreateMaxValue_ShouldThrowArgumentOutOfRangeException_WhenPrecisionAreGreaterThanEighteen(byte precision)
    {
        var action = Lambda.Of( () => Fixed.CreateMaxValue( precision ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 10 )]
    [InlineData( 18 )]
    public void CreateMinValue_ShouldReturnCorrectResult(byte precision)
    {
        var sut = Fixed.CreateMinValue( precision );

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( long.MinValue );
            sut.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 19 )]
    [InlineData( 20 )]
    public void CreateMinValue_ShouldThrowArgumentOutOfRangeException_WhenPrecisionAreGreaterThanEighteen(byte precision)
    {
        var action = Lambda.Of( () => Fixed.CreateMinValue( precision ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 10 )]
    [InlineData( 18 )]
    public void CreateEpsilon_ShouldReturnCorrectResult(byte precision)
    {
        var sut = Fixed.CreateEpsilon( precision );

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( 1 );
            sut.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 19 )]
    [InlineData( 20 )]
    public void CreateEpsilon_ShouldThrowArgumentOutOfRangeException_WhenPrecisionAreGreaterThanEighteen(byte precision)
    {
        var action = Lambda.Of( () => Fixed.CreateEpsilon( precision ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetCreateRawData ) )]
    public void CreateRaw_ShouldReturnCorrectResult(long rawValue, byte precision)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( rawValue );
            sut.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 19 )]
    [InlineData( 20 )]
    public void CreateRaw_ShouldThrowArgumentOutOfRangeException_WhenPrecisionAreGreaterThanEighteen(byte precision)
    {
        var action = Lambda.Of( () => Fixed.CreateRaw( Fixture.Create<long>(), precision ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetCreateWithInt64Data ) )]
    public void Create_WithInt64_ShouldReturnCorrectResult(long value, byte precision, long expected)
    {
        var sut = Fixed.Create( value, precision );

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( expected );
            sut.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 123, 18 )]
    [InlineData( -123, 18 )]
    public void Create_WithInt64_ShouldThrowOverflowException_WhenValueAndPrecisionExceedLimits(long value, byte precision)
    {
        var action = Lambda.Of( () => Fixed.Create( value, precision ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 19 )]
    [InlineData( 20 )]
    public void Create_WithInt64_ShouldThrowArgumentOutOfRangeException_WhenPrecisionAreGreaterThanEighteen(byte precision)
    {
        var action = Lambda.Of( () => Fixed.Create( 0, precision ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetCreateWithDecimalData ) )]
    public void Create_WithDecimal_ShouldReturnCorrectResult(decimal value, byte precision, long expected)
    {
        var sut = Fixed.Create( value, precision );

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( expected );
            sut.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 19 )]
    [InlineData( 20 )]
    public void Create_WithDecimal_ShouldThrowArgumentOutOfRangeException_WhenPrecisionAreGreaterThanEighteen(byte precision)
    {
        var action = Lambda.Of( () => Fixed.Create( 0m, precision ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetCreateWithDoubleData ) )]
    public void Create_WithDouble_ShouldReturnCorrectResult(double value, byte precision, long expected)
    {
        var sut = Fixed.Create( value, precision );

        using ( new AssertionScope() )
        {
            sut.RawValue.Should().Be( expected );
            sut.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 19 )]
    [InlineData( 20 )]
    public void Create_WithDouble_ShouldThrowArgumentOutOfRangeException_WhenPrecisionAreGreaterThanEighteen(byte precision)
    {
        var action = Lambda.Of( () => Fixed.Create( 0.0, precision ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 6 )]
    [InlineData( 7 )]
    [InlineData( 8 )]
    [InlineData( 9 )]
    [InlineData( 10 )]
    [InlineData( 11 )]
    [InlineData( 12 )]
    [InlineData( 13 )]
    [InlineData( 14 )]
    [InlineData( 15 )]
    [InlineData( 16 )]
    [InlineData( 17 )]
    [InlineData( 18 )]
    public void GetScale_ShouldReturnCorrectResult(byte precision)
    {
        var expected = (long)Math.Pow( 10, precision );
        var result = Fixed.GetScale( precision );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 19 )]
    [InlineData( 20 )]
    public void GetScale_ShouldThrowIndexOutOfRangeException_WhenPrecisionAreGreaterThanEighteen(byte precision)
    {
        var action = Lambda.Of( () => Fixed.GetScale( precision ) );
        action.Should().ThrowExactly<IndexOutOfRangeException>();
    }

    [Theory]
    [InlineData( 123, 0, "N0" )]
    [InlineData( 123.45, 2, "N2" )]
    [InlineData( 123.456789, 6, "N6" )]
    public void ToString_ShouldReturnCorrectResult(double value, byte precision, string expectedFormat)
    {
        var dec = (decimal)value;
        var sut = Fixed.Create( dec, precision );
        var expected = dec.ToString( expectedFormat, NumberFormatInfo.CurrentInfo );

        var result = sut.ToString();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var rawValue = Fixture.Create<long>();
        var sut = Fixed.CreateRaw( rawValue, 10 );

        var result = sut.GetHashCode();

        result.Should().Be( ((decimal)sut).GetHashCode() );
    }

    [Theory]
    [InlineData( 0, 1, 0 )]
    [InlineData( 123, 5, 12300000 )]
    [InlineData( -123, 11, 12300000000000 )]
    public void Abs_ShouldReturnCorrectResult(int value, byte precision, long expected)
    {
        var sut = Fixed.Create( value, precision );
        var result = sut.Abs();

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 0, 1, 0 )]
    [InlineData( 123, 5, 12300000 )]
    [InlineData( 123.456, 11, 12300000000000 )]
    [InlineData( -123, 2, -12300 )]
    [InlineData( -123.456, 7, -1230000000 )]
    public void Truncate_ShouldReturnCorrectResult(double value, byte precision, long expected)
    {
        var sut = Fixed.Create( value, precision );
        var result = sut.Truncate();

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetRoundData ) )]
    public void Round_ShouldReturnCorrectResult(decimal value, byte precision, int precisionToRoundTo, long expected)
    {
        var sut = Fixed.Create( value, precision );
        var result = sut.Round( precisionToRoundTo );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Fact]
    public void Round_ShouldThrowArgumentOutOfRangeException_WhenPrecisionIsNegative()
    {
        var sut = Fixed.CreateRaw( Fixture.Create<long>(), 10 );
        var action = Lambda.Of( () => sut.Round( -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetFloorData ) )]
    public void Floor_ShouldReturnCorrectResult(decimal value, byte precision, long expected)
    {
        var sut = Fixed.Create( value, precision );
        var result = sut.Floor();

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetCeilingData ) )]
    public void Ceiling_ShouldReturnCorrectResult(decimal value, byte precision, long expected)
    {
        var sut = Fixed.Create( value, precision );
        var result = sut.Ceiling();

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetCreateRawData ) )]
    public void SetRawValue_ShouldReturnCorrectResult(long rawValue, byte precision)
    {
        var sut = Fixed.CreateRaw( Fixture.Create<long>(), precision );
        var result = sut.SetRawValue( rawValue );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( rawValue );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetCreateWithInt64Data ) )]
    public void SetValue_WithInt64_ShouldReturnCorrectResult(long value, byte precision, long expected)
    {
        var sut = Fixed.CreateRaw( Fixture.Create<long>(), precision );
        var result = sut.SetValue( value );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetCreateWithDecimalData ) )]
    public void SetValue_WithDecimal_ShouldReturnCorrectResult(decimal value, byte precision, long expected)
    {
        var sut = Fixed.CreateRaw( Fixture.Create<long>(), precision );
        var result = sut.SetValue( value );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetCreateWithDoubleData ) )]
    public void SetValue_WithDouble_ShouldReturnCorrectResult(double value, byte precision, long expected)
    {
        var sut = Fixed.CreateRaw( Fixture.Create<long>(), precision );
        var result = sut.SetValue( value );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 0, 0, 0, 0 )]
    [InlineData( 0, 1, 10, 0 )]
    [InlineData( 0, 10, 1, 0 )]
    [InlineData( 123, 2, 2, 123 )]
    [InlineData( 123, 2, 5, 123000 )]
    [InlineData( 123000, 5, 2, 123 )]
    [InlineData( 123499, 5, 2, 123 )]
    [InlineData( 123500, 5, 2, 124 )]
    public void SetPrecision_ShouldReturnCorrectResult(long rawValue, byte precision, byte newPrecision, long expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = sut.SetPrecision( newPrecision );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( newPrecision );
        }
    }

    [Theory]
    [InlineData( long.MaxValue )]
    [InlineData( long.MinValue )]
    public void SetPrecision_ShouldThrowOverflowException_WhenPrecisionIncreaseCausesAnOverflow(long rawValue)
    {
        var sut = Fixed.CreateRaw( rawValue, 0 );
        var action = Lambda.Of( () => sut.SetPrecision( 1 ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 19 )]
    [InlineData( 20 )]
    public void SetPrecision_ShouldThrowArgumentOutOfRangeException_WhenPrecisionAreGreaterThanEighteen(byte precision)
    {
        var sut = Fixed.CreateRaw( Fixture.Create<long>(), 10 );
        var action = Lambda.Of( () => sut.SetPrecision( precision ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 0, 0, 0 )]
    [InlineData( 0, 1, 1, 1 )]
    [InlineData( 0, 1, -1, -1 )]
    [InlineData( 123, 7, 456, 579 )]
    [InlineData( -123, 7, -456, -579 )]
    [InlineData( 123, 7, -456, -333 )]
    [InlineData( -123, 7, 456, 333 )]
    public void AddRaw_ShouldReturnCorrectResult(long rawValue, byte precision, long toAdd, long expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = sut.AddRaw( toAdd );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, 1 )]
    [InlineData( long.MinValue, -1 )]
    public void AddRaw_ShouldThrowOverflowException_WhenResultOverflows(long rawValue, long toAdd)
    {
        var sut = Fixed.CreateRaw( rawValue, 0 );
        var action = Lambda.Of( () => sut.AddRaw( toAdd ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 0, 0, 0 )]
    [InlineData( 0, 1, 1, -1 )]
    [InlineData( 0, 1, -1, 1 )]
    [InlineData( 123, 7, 456, -333 )]
    [InlineData( -123, 7, -456, 333 )]
    [InlineData( 123, 7, -456, 579 )]
    [InlineData( -123, 7, 456, -579 )]
    public void SubtractRaw_ShouldReturnCorrectResult(long rawValue, byte precision, long toSubtract, long expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = sut.SubtractRaw( toSubtract );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, -1 )]
    [InlineData( long.MinValue, 1 )]
    public void SubtractRaw_ShouldThrowOverflowException_WhenResultOverflows(long rawValue, long toSubtract)
    {
        var sut = Fixed.CreateRaw( rawValue, 0 );
        var action = Lambda.Of( () => sut.SubtractRaw( toSubtract ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 0, 0, 0 )]
    [InlineData( 0, 1, 1, 0 )]
    [InlineData( 0, 1, -1, 0 )]
    [InlineData( 123, 7, 456, 56088 )]
    [InlineData( -123, 7, -456, 56088 )]
    [InlineData( 123, 7, -456, -56088 )]
    [InlineData( -123, 7, 456, -56088 )]
    public void MultiplyRaw_ShouldReturnCorrectResult(long rawValue, byte precision, long toMultiply, long expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = sut.MultiplyRaw( toMultiply );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, 2 )]
    [InlineData( long.MinValue, -2 )]
    public void MultiplyRaw_ShouldThrowOverflowException_WhenResultOverflows(long rawValue, long toMultiply)
    {
        var sut = Fixed.CreateRaw( rawValue, 0 );
        var action = Lambda.Of( () => sut.MultiplyRaw( toMultiply ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 1, 1, 0 )]
    [InlineData( 0, 1, -1, 0 )]
    [InlineData( 456, 7, 123, 3 )]
    [InlineData( -456, 7, -123, 3 )]
    [InlineData( 456, 7, -123, -3 )]
    [InlineData( -456, 7, 123, -3 )]
    public void DivideRaw_ShouldReturnCorrectResult(long rawValue, byte precision, long toDivide, long expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = sut.DivideRaw( toDivide );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Fact]
    public void DivideRaw_ShouldThrowDivideByZeroException_WhenDivisorIsEqualToZero()
    {
        var sut = Fixed.CreateRaw( Fixture.Create<long>(), 0 );
        var action = Lambda.Of( () => sut.DivideRaw( 0 ) );
        action.Should().ThrowExactly<DivideByZeroException>();
    }

    [Theory]
    [InlineData( 0, 1, 1, 0 )]
    [InlineData( 0, 1, -1, 0 )]
    [InlineData( 456, 7, 123, 87 )]
    [InlineData( -456, 7, -123, -87 )]
    [InlineData( 456, 7, -123, 87 )]
    [InlineData( -456, 7, 123, -87 )]
    public void ModuloRaw_ShouldReturnCorrectResult(long rawValue, byte precision, long toModulo, long expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = sut.ModuloRaw( toModulo );

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Fact]
    public void ModuloRaw_ShouldThrowDivideByZeroException_WhenDivisorIsEqualToZero()
    {
        var sut = Fixed.CreateRaw( Fixture.Create<long>(), 0 );
        var action = Lambda.Of( () => sut.ModuloRaw( 0 ) );
        action.Should().ThrowExactly<DivideByZeroException>();
    }

    [Theory]
    [InlineData( 0, 0, 0 )]
    [InlineData( 123, 0, 123 )]
    [InlineData( 1234, 1, 123.4 )]
    [InlineData( 123456789, 7, 12.3456789 )]
    [InlineData( -123, 0, -123 )]
    [InlineData( -1234, 1, -123.4 )]
    [InlineData( -123456789, 7, -12.3456789 )]
    public void DecimalConversionOperator_ShouldReturnCorrectResult(long rawValue, byte precision, double expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = (decimal)sut;
        result.Should().Be( (decimal)expected );
    }

    [Theory]
    [InlineData( 0, 0, 0 )]
    [InlineData( 123, 0, 123 )]
    [InlineData( 1234, 1, 123.4 )]
    [InlineData( 123456789, 7, 12.3456789 )]
    [InlineData( -123, 0, -123 )]
    [InlineData( -1234, 1, -123.4 )]
    [InlineData( -123456789, 7, -12.3456789 )]
    public void DoubleConversionOperator_ShouldReturnCorrectResult(long rawValue, byte precision, double expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = (double)sut;
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, 0, 0 )]
    [InlineData( 123, 0, 123 )]
    [InlineData( 1234, 1, 123 )]
    [InlineData( 1235, 1, 123 )]
    [InlineData( 123456789, 7, 12 )]
    [InlineData( -123, 0, -123 )]
    [InlineData( -1234, 1, -123 )]
    [InlineData( -1235, 1, -123 )]
    [InlineData( -123456789, 7, -12 )]
    public void Int64ConversionOperator_ShouldReturnCorrectResult(long rawValue, byte precision, long expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = (long)sut;
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123, 0, -123 )]
    [InlineData( -123, 0, 123 )]
    [InlineData( 1234567, 5, -1234567 )]
    [InlineData( -1234567, 5, 1234567 )]
    public void NegateOperator_ShouldReturnCorrectResult(long rawValue, byte precision, long expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = -sut;

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 123, 0, 124 )]
    [InlineData( -123, 0, -122 )]
    [InlineData( 123456, 3, 124456 )]
    [InlineData( -123456, 3, -122456 )]
    public void IncrementOperator_ShouldReturnCorrectResult(long rawValue, byte precision, long expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = ++sut;

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 123, 0, 122 )]
    [InlineData( -123, 0, -124 )]
    [InlineData( 123456, 3, 122456 )]
    [InlineData( -123456, 3, -124456 )]
    public void DecrementOperator_ShouldReturnCorrectResult(long rawValue, byte precision, long expected)
    {
        var sut = Fixed.CreateRaw( rawValue, precision );
        var result = --sut;

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 0, 0, 0, 0, 0, 0 )]
    [InlineData( 0, 1, 1, 1, 1, 1 )]
    [InlineData( 0, 1, -1, 1, -1, 1 )]
    [InlineData( 123, 7, 456, 7, 579, 7 )]
    [InlineData( -123, 7, -456, 7, -579, 7 )]
    [InlineData( 123, 7, -456, 7, -333, 7 )]
    [InlineData( -123, 7, 456, 7, 333, 7 )]
    [InlineData( 123456, 3, 789123, 5, 13134723, 5 )]
    [InlineData( -123456, 3, -789123, 5, -13134723, 5 )]
    [InlineData( 123456, 3, -789123, 5, 11556477, 5 )]
    [InlineData( -123456, 3, 789123, 5, -11556477, 5 )]
    [InlineData( 789123, 5, 123456, 3, 13134723, 5 )]
    [InlineData( -789123, 5, -123456, 3, -13134723, 5 )]
    [InlineData( 789123, 5, -123456, 3, -11556477, 5 )]
    [InlineData( -789123, 5, 123456, 3, 11556477, 5 )]
    public void AddOperator_ShouldReturnCorrectResult(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision,
        long expectedRawValue,
        byte expectedPrecision)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a + b;

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expectedRawValue );
            result.Precision.Should().Be( expectedPrecision );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, 0, 0, 1 )]
    [InlineData( 0, 1, long.MinValue, 0 )]
    [InlineData( long.MaxValue, 0, 1, 0 )]
    public void AddOperator_ShouldThrowOverflowException_WhenOperationOverflows(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var action = Lambda.Of( () => a + b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 0, 0, 0, 0, 0 )]
    [InlineData( 0, 1, 1, 1, -1, 1 )]
    [InlineData( 0, 1, -1, 1, 1, 1 )]
    [InlineData( 123, 7, 456, 7, -333, 7 )]
    [InlineData( -123, 7, -456, 7, 333, 7 )]
    [InlineData( 123, 7, -456, 7, 579, 7 )]
    [InlineData( -123, 7, 456, 7, -579, 7 )]
    [InlineData( 123456, 3, 789123, 5, 11556477, 5 )]
    [InlineData( -123456, 3, -789123, 5, -11556477, 5 )]
    [InlineData( 123456, 3, -789123, 5, 13134723, 5 )]
    [InlineData( -123456, 3, 789123, 5, -13134723, 5 )]
    [InlineData( 789123, 5, 123456, 3, -11556477, 5 )]
    [InlineData( -789123, 5, -123456, 3, 11556477, 5 )]
    [InlineData( 789123, 5, -123456, 3, 13134723, 5 )]
    [InlineData( -789123, 5, 123456, 3, -13134723, 5 )]
    public void SubtractOperator_ShouldReturnCorrectResult(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision,
        long expectedRawValue,
        byte expectedPrecision)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a - b;

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expectedRawValue );
            result.Precision.Should().Be( expectedPrecision );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, 0, 0, 1 )]
    [InlineData( 0, 1, long.MinValue, 0 )]
    [InlineData( long.MaxValue, 0, -1, 0 )]
    public void SubtractOperator_ShouldThrowOverflowException_WhenOperationOverflows(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var action = Lambda.Of( () => a - b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 0, 0, 0, 0, 0 )]
    [InlineData( 100, 0, 200, 0, 20000, 0 )]
    [InlineData( 0, 1, 1, 1, 0, 1 )]
    [InlineData( 0, 1, -1, 1, 0, 1 )]
    [InlineData( 1234, 2, 4567, 2, 56357, 2 )]
    [InlineData( -1234, 2, -4567, 2, 56357, 2 )]
    [InlineData( 1234, 2, -4567, 2, -56357, 2 )]
    [InlineData( -1234, 2, 4567, 2, -56357, 2 )]
    [InlineData( 1234, 2, 4566, 2, 56344, 2 )]
    [InlineData( -1234, 2, -4566, 2, 56344, 2 )]
    [InlineData( 1234, 2, -4566, 2, -56344, 2 )]
    [InlineData( -1234, 2, 4566, 2, -56344, 2 )]
    [InlineData( 3, 1, 2, 1, 1, 1 )]
    [InlineData( 2, 1, 2, 1, 0, 1 )]
    [InlineData( 30, 2, 20, 2, 6, 2 )]
    [InlineData( 20, 2, 20, 2, 4, 2 )]
    [InlineData( -3, 1, 2, 1, -1, 1 )]
    [InlineData( -2, 1, 2, 1, 0, 1 )]
    [InlineData( -30, 2, 20, 2, -6, 2 )]
    [InlineData( -20, 2, 20, 2, -4, 2 )]
    [InlineData( 3, 1, -2, 1, -1, 1 )]
    [InlineData( 2, 1, -2, 1, 0, 1 )]
    [InlineData( 30, 2, -20, 2, -6, 2 )]
    [InlineData( 20, 2, -20, 2, -4, 2 )]
    [InlineData( -3, 1, -2, 1, 1, 1 )]
    [InlineData( -2, 1, -2, 1, 0, 1 )]
    [InlineData( -30, 2, -20, 2, 6, 2 )]
    [InlineData( -20, 2, -20, 2, 4, 2 )]
    [InlineData( 123_4567890, 7, 42_123, 3, 5200_3703230, 7 )]
    [InlineData( 123_4567890, 7, 42_122, 3, 5200_2468663, 7 )]
    [InlineData( -123_4567890, 7, 42_123, 3, -5200_3703230, 7 )]
    [InlineData( -123_4567890, 7, 42_122, 3, -5200_2468663, 7 )]
    [InlineData( 123_4567890, 7, -42_123, 3, -5200_3703230, 7 )]
    [InlineData( 123_4567890, 7, -42_122, 3, -5200_2468663, 7 )]
    [InlineData( -123_4567890, 7, -42_123, 3, 5200_3703230, 7 )]
    [InlineData( -123_4567890, 7, -42_122, 3, 5200_2468663, 7 )]
    [InlineData( 42_123, 3, 123_4567890, 7, 5200_3703230, 7 )]
    [InlineData( 42_122, 3, 123_4567890, 7, 5200_2468663, 7 )]
    [InlineData( -42_123, 3, 123_4567890, 7, -5200_3703230, 7 )]
    [InlineData( -42_122, 3, 123_4567890, 7, -5200_2468663, 7 )]
    [InlineData( 42_123, 3, -123_4567890, 7, -5200_3703230, 7 )]
    [InlineData( 42_122, 3, -123_4567890, 7, -5200_2468663, 7 )]
    [InlineData( -42_123, 3, -123_4567890, 7, 5200_3703230, 7 )]
    [InlineData( -42_122, 3, -123_4567890, 7, 5200_2468663, 7 )]
    [InlineData( 2_000000000000, 12, 3_500000000000, 12, 7_000000000000, 12 )]
    [InlineData( long.MaxValue, 10, 1250000000, 10, 115292150_4606846976, 10 )]
    [InlineData( 188232082384791343, 0, 49, 0, long.MaxValue, 0 )]
    [InlineData( -1L << 61, 0, 4, 0, long.MinValue, 0 )]
    public void MultiplyOperator_ShouldReturnCorrectResult(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision,
        long expectedRawValue,
        byte expectedPrecision)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a * b;

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expectedRawValue );
            result.Precision.Should().Be( expectedPrecision );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, 0, 0, 1 )]
    [InlineData( 0, 1, long.MinValue, 0 )]
    [InlineData( 1L << 61, 0, 4, 0 )]
    [InlineData( -341606371735362067, 0, 27, 0 )]
    [InlineData( 1L << 62, 0, 4, 0 )]
    [InlineData( 145295143558111, 1, 1269605, 1 )]
    public void MultiplyOperator_ShouldThrowOverflowException_WhenOperationOverflows(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var action = Lambda.Of( () => a * b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 0, 1, 0, 0, 0 )]
    [InlineData( 1, 0, 1, 0, 1, 0 )]
    [InlineData( -1, 0, 1, 0, -1, 0 )]
    [InlineData( 1, 0, -1, 0, -1, 0 )]
    [InlineData( -1, 0, -1, 0, 1, 0 )]
    [InlineData( 7, 0, 3, 0, 2, 0 )]
    [InlineData( 8, 0, 3, 0, 3, 0 )]
    [InlineData( 9, 0, 3, 0, 3, 0 )]
    [InlineData( 70, 1, 30, 1, 23, 1 )]
    [InlineData( 80, 1, 30, 1, 27, 1 )]
    [InlineData( 90, 1, 30, 1, 30, 1 )]
    [InlineData( 10_0, 1, 3, 1, 33_3, 1 )]
    [InlineData( 10_1, 1, 3, 1, 33_7, 1 )]
    [InlineData( 100_00, 2, 10_00, 2, 10_00, 2 )]
    [InlineData( 123_4567890, 7, 42_123, 3, 2_9308641, 7 )]
    [InlineData( 123_4567890, 7, 42_122, 3, 2_9309337, 7 )]
    [InlineData( -123_4567890, 7, 42_123, 3, -2_9308641, 7 )]
    [InlineData( -123_4567890, 7, 42_122, 3, -2_9309337, 7 )]
    [InlineData( 123_4567890, 7, -42_123, 3, -2_9308641, 7 )]
    [InlineData( 123_4567890, 7, -42_122, 3, -2_9309337, 7 )]
    [InlineData( -123_4567890, 7, -42_123, 3, 2_9308641, 7 )]
    [InlineData( -123_4567890, 7, -42_122, 3, 2_9309337, 7 )]
    [InlineData( 42_123, 3, 123_4567890, 7, 3411963, 7 )]
    [InlineData( 42_122, 3, 123_9876543, 7, 3397274, 7 )]
    [InlineData( -42_123, 3, 123_4567890, 7, -3411963, 7 )]
    [InlineData( -42_122, 3, 123_9876543, 7, -3397274, 7 )]
    [InlineData( 42_123, 3, -123_4567890, 7, -3411963, 7 )]
    [InlineData( 42_122, 3, -123_9876543, 7, -3397274, 7 )]
    [InlineData( -42_123, 3, -123_4567890, 7, 3411963, 7 )]
    [InlineData( -42_122, 3, -123_9876543, 7, 3397274, 7 )]
    [InlineData( 3_500000000000, 12, 2_000000000000, 12, 1_750000000000, 12 )]
    [InlineData( long.MaxValue, 10, 8_0000000000, 10, 115292150_4606846976, 10 )]
    [InlineData( long.MaxValue, 0, 1, 0, long.MaxValue, 0 )]
    [InlineData( long.MinValue, 0, 1, 0, long.MinValue, 0 )]
    public void DivideOperator_ShouldReturnCorrectResult(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision,
        long expectedRawValue,
        byte expectedPrecision)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a / b;

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expectedRawValue );
            result.Precision.Should().Be( expectedPrecision );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, 0, 1, 1 )]
    [InlineData( 1, 1, long.MinValue, 0 )]
    [InlineData( 1L << 62, 1, 5, 1 )]
    [InlineData( -485440633518672411, 18, 52631578947368421, 18 )]
    [InlineData( 1L << 62, 2, 25, 2 )]
    public void DivideOperator_ShouldThrowOverflowException_WhenOperationOverflows(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var action = Lambda.Of( () => a / b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 1, 0 )]
    [InlineData( 0, 0 )]
    public void DivideOperator_ShouldThrowDivideByZeroException_WhenDivisorIsEqualToZero(byte aPrecision, byte bPrecision)
    {
        var a = Fixed.CreateRaw( Fixture.Create<long>(), aPrecision );
        var b = Fixed.CreateRaw( 0, bPrecision );

        var action = Lambda.Of( () => a / b );

        action.Should().ThrowExactly<DivideByZeroException>();
    }

    [Theory]
    [InlineData( 0, 1, 1, 1, 0, 1 )]
    [InlineData( 0, 1, -1, 1, 0, 1 )]
    [InlineData( 456, 7, 123, 7, 87, 7 )]
    [InlineData( -456, 7, -123, 7, -87, 7 )]
    [InlineData( 456, 7, -123, 7, 87, 7 )]
    [InlineData( -456, 7, 123, 7, -87, 7 )]
    [InlineData( 123456, 3, 789123, 5, 508755, 5 )]
    [InlineData( -123456, 3, -789123, 5, -508755, 5 )]
    [InlineData( 123456, 3, -789123, 5, 508755, 5 )]
    [InlineData( -123456, 3, 789123, 5, -508755, 5 )]
    [InlineData( 78912345, 5, 12345, 3, 1138845, 5 )]
    [InlineData( -78912345, 5, -12345, 3, -1138845, 5 )]
    [InlineData( 78912345, 5, -12345, 3, 1138845, 5 )]
    [InlineData( -78912345, 5, 12345, 3, -1138845, 5 )]
    public void ModuloOperator_ShouldReturnCorrectResult(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision,
        long expectedRawValue,
        byte expectedPrecision)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a % b;

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expectedRawValue );
            result.Precision.Should().Be( expectedPrecision );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, 0, 0, 1 )]
    [InlineData( 0, 1, long.MinValue, 0 )]
    public void ModuloOperator_ShouldThrowOverflowException_WhenOperationOverflows(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var action = Lambda.Of( () => a % b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 1, 0 )]
    [InlineData( 0, 0 )]
    public void ModuloOperator_ShouldThrowDivideByZeroException_WhenDivisorIsEqualToZero(byte aPrecision, byte bPrecision)
    {
        var a = Fixed.CreateRaw( Fixture.Create<long>(), aPrecision );
        var b = Fixed.CreateRaw( 0, bPrecision );

        var action = Lambda.Of( () => a % b );

        action.Should().ThrowExactly<DivideByZeroException>();
    }

    [Theory]
    [InlineData( 200, 0, 60, 120 )]
    [InlineData( 200, 1, 120, 240 )]
    [InlineData( 200, 2, -60, -120 )]
    [InlineData( 200, 3, -120, -240 )]
    [InlineData( -200, 4, 60, -120 )]
    [InlineData( -200, 5, 120, -240 )]
    [InlineData( -200, 6, -60, 120 )]
    [InlineData( -200, 7, -120, 240 )]
    [InlineData( 3, 1, 50, 2 )]
    [InlineData( 3, 2, 49, 1 )]
    public void MultiplyOperator_ForPercentRight_ShouldReturnCorrectResult(long rawValue, byte precision, int right, long expected)
    {
        var a = Fixed.CreateRaw( rawValue, precision );
        var b = Percent.Normalize( right );

        var result = a * b;

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [InlineData( 200, 0, 60, 120 )]
    [InlineData( 200, 1, 120, 240 )]
    [InlineData( 200, 2, -60, -120 )]
    [InlineData( 200, 3, -120, -240 )]
    [InlineData( -200, 4, 60, -120 )]
    [InlineData( -200, 5, 120, -240 )]
    [InlineData( -200, 6, -60, 120 )]
    [InlineData( -200, 7, -120, 240 )]
    [InlineData( 3, 1, 50, 2 )]
    [InlineData( 3, 2, 49, 1 )]
    public void MultiplyOperator_ForLeft_ShouldReturnCorrectResult(long rawValue, byte precision, int right, long expected)
    {
        var a = Fixed.CreateRaw( rawValue, precision );
        var b = Percent.Normalize( right );

        var result = b * a;

        using ( new AssertionScope() )
        {
            result.RawValue.Should().Be( expected );
            result.Precision.Should().Be( precision );
        }
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(long aRawValue, byte aPrecision, long bRawValue, byte bPrecision, bool expected)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision,
        bool expected)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetLessThanData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(long aRawValue, byte aPrecision, long bRawValue, byte bPrecision, bool expected)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetLessThanOrEqualToData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision,
        bool expected)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetGreaterThanData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision,
        bool expected)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FixedTestsData.GetGreaterThanOrEqualToData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(
        long aRawValue,
        byte aPrecision,
        long bRawValue,
        byte bPrecision,
        bool expected)
    {
        var a = Fixed.CreateRaw( aRawValue, aPrecision );
        var b = Fixed.CreateRaw( bRawValue, bPrecision );

        var result = a >= b;

        result.Should().Be( expected );
    }
}
