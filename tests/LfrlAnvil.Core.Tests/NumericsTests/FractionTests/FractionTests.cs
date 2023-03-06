using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Numerics;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Tests.NumericsTests.FractionTests;

[TestClass( typeof( FractionTestsData ) )]
public class FractionTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = default( Fraction );

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( 0 );
            sut.Denominator.Should().Be( 1 );
        }
    }

    [Fact]
    public void Zero_ShouldReturnCorrectResult()
    {
        var sut = Fraction.Zero;

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( 0 );
            sut.Denominator.Should().Be( 1 );
        }
    }

    [Fact]
    public void One_ShouldReturnCorrectResult()
    {
        var sut = Fraction.One;

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( 1 );
            sut.Denominator.Should().Be( 1 );
        }
    }

    [Fact]
    public void MinValue_ShouldReturnCorrectResult()
    {
        var sut = Fraction.MinValue;

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( long.MinValue );
            sut.Denominator.Should().Be( 1 );
        }
    }

    [Fact]
    public void MaxValue_ShouldReturnCorrectResult()
    {
        var sut = Fraction.MaxValue;

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( long.MaxValue );
            sut.Denominator.Should().Be( 1 );
        }
    }

    [Fact]
    public void Epsilon_ShouldReturnCorrectResult()
    {
        var sut = Fraction.Epsilon;

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( 1 );
            sut.Denominator.Should().Be( ulong.MaxValue );
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( -1 )]
    [InlineData( 123 )]
    [InlineData( -123 )]
    public void Ctor_ShouldReturnCorrectInteger(long value)
    {
        var sut = new Fraction( value );

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( value );
            sut.Denominator.Should().Be( 1 );
        }
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 0, 15 )]
    [InlineData( 1, 1 )]
    [InlineData( 1, 25 )]
    [InlineData( -1, 1 )]
    [InlineData( -1, 40 )]
    [InlineData( 123, 1 )]
    [InlineData( 123, 55 )]
    [InlineData( -123, 1 )]
    [InlineData( -123, 60 )]
    public void Ctor_WithDenominator_ShouldReturnCorrectResult(long numerator, ulong denominator)
    {
        var sut = new Fraction( numerator, denominator );

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( numerator );
            sut.Denominator.Should().Be( denominator );
        }
    }

    [Fact]
    public void Ctor_WithDenominator_ShouldThrowArgumentOutOfRangeException_WhenDenominatorEqualsZero()
    {
        var action = Lambda.Of( () => new Fraction( Fixture.Create<long>(), denominator: 0 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 100, 0 )]
    [InlineData( 1, 100, 100 )]
    [InlineData( 1.01, 100, 101 )]
    [InlineData( 12.3456789, 10000000, 123456789 )]
    [InlineData( 12.3456789, 500, 6173 )]
    [InlineData( -1, 100, -100 )]
    [InlineData( -1.01, 100, -101 )]
    [InlineData( -12.3456789, 10000000, -123456789 )]
    [InlineData( -12.3456789, 500, -6173 )]
    [InlineData( 1.5, 2, 3 )]
    public void Create_WithDecimal_ShouldReturnCorrectResult(double value, ulong denominator, long expectedNumerator)
    {
        var sut = Fraction.Create( (decimal)value, denominator );

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( expectedNumerator );
            sut.Denominator.Should().Be( denominator );
        }
    }

    [Fact]
    public void Create_WithDecimal_ShouldThrowOverflowException_WhenNumeratorIsTooLarge()
    {
        var action = Lambda.Of( () => Fraction.Create( 1.0000001m, ulong.MaxValue ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 100, 0 )]
    [InlineData( 1, 100, 100 )]
    [InlineData( 1.01, 100, 101 )]
    [InlineData( 12.3456789, 10000000, 123456789 )]
    [InlineData( 12.3456789, 500, 6173 )]
    [InlineData( -1, 100, -100 )]
    [InlineData( -1.01, 100, -101 )]
    [InlineData( -12.3456789, 10000000, -123456789 )]
    [InlineData( -12.3456789, 500, -6173 )]
    [InlineData( 1.5, 2, 3 )]
    public void Create_WithDouble_ShouldReturnCorrectResult(double value, ulong denominator, long expectedNumerator)
    {
        var sut = Fraction.Create( value, denominator );

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( expectedNumerator );
            sut.Denominator.Should().Be( denominator );
        }
    }

    [Fact]
    public void Create_WithDouble_ShouldThrowOverflowException_WhenNumeratorIsTooLarge()
    {
        var action = Lambda.Of( () => Fraction.Create( 1.0000001, ulong.MaxValue ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 0, 1 )]
    [InlineData( 0, 5, 100000 )]
    [InlineData( long.MaxValue, 4, 10000 )]
    [InlineData( long.MinValue, 3, 1000 )]
    public void Create_WithFixed_ShouldReturnCorrectResult(long rawValue, byte precision, ulong expectedDenominator)
    {
        var sut = Fraction.Create( Fixed.CreateRaw( rawValue, precision ) );

        using ( new AssertionScope() )
        {
            sut.Numerator.Should().Be( rawValue );
            sut.Denominator.Should().Be( expectedDenominator );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new Fraction( 1234, 567 );
        var result = sut.ToString();
        result.Should().Be( "1234 / 567" );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var a = new Fraction( 1234, 100 );
        var b = new Fraction( 123400, 10000 );

        var aHash = a.GetHashCode();
        var bHash = b.GetHashCode();

        aHash.Should().Be( bHash );
    }

    [Theory]
    [InlineData( 0, 100, 0 )]
    [InlineData( 15, 200, 15 )]
    [InlineData( -123, 150, 123 )]
    public void Abs_ShouldReturnCorrectResult(long numerator, ulong denominator, long expectedNumerator)
    {
        var sut = new Fraction( numerator, denominator );
        var result = sut.Abs();

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( sut.Denominator );
        }
    }

    [Fact]
    public void Abs_ShouldThrowOverflowException_WhenNumeratorEqualsMinValue()
    {
        var sut = new Fraction( long.MinValue, 1 );
        var action = Lambda.Of( () => sut.Abs() );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 10, 0 )]
    [InlineData( 20, 10, 2 )]
    [InlineData( -20, 10, -2 )]
    [InlineData( 123, 90, 1 )]
    [InlineData( 185, 80, 2 )]
    [InlineData( -123, 90, -1 )]
    [InlineData( -185, 80, -2 )]
    [InlineData( 0, ulong.MaxValue, 0 )]
    [InlineData( long.MaxValue, 1, long.MaxValue )]
    [InlineData( long.MinValue, 1, long.MinValue )]
    [InlineData( long.MaxValue, ulong.MaxValue, 0 )]
    [InlineData( long.MinValue, ulong.MaxValue, 0 )]
    public void Truncate_ShouldReturnCorrectResult(long numerator, ulong denominator, long expectedNumerator)
    {
        var sut = new Fraction( numerator, denominator );
        var result = sut.Truncate();

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( 1 );
        }
    }

    [Theory]
    [InlineData( 0, 10, 0 )]
    [InlineData( 20, 10, 2 )]
    [InlineData( -20, 10, -2 )]
    [InlineData( 123, 90, 1 )]
    [InlineData( 185, 80, 2 )]
    [InlineData( -123, 90, -2 )]
    [InlineData( -185, 80, -3 )]
    [InlineData( 0, ulong.MaxValue, 0 )]
    [InlineData( long.MaxValue, 1, long.MaxValue )]
    [InlineData( long.MinValue, 1, long.MinValue )]
    [InlineData( long.MaxValue, ulong.MaxValue, 0 )]
    [InlineData( long.MinValue + 2, long.MaxValue, -1 )]
    public void Floor_ShouldReturnCorrectResult(long numerator, ulong denominator, long expectedNumerator)
    {
        var sut = new Fraction( numerator, denominator );
        var result = sut.Floor();

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( 1 );
        }
    }

    [Fact]
    public void Floor_ShouldThrowOverflowException_WhenNumeratorDropsBelowMinPossibleValue()
    {
        var sut = new Fraction( long.MinValue, 3 );
        var action = Lambda.Of( () => sut.Floor() );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 10, 0 )]
    [InlineData( 20, 10, 2 )]
    [InlineData( -20, 10, -2 )]
    [InlineData( 123, 90, 2 )]
    [InlineData( 185, 80, 3 )]
    [InlineData( -123, 90, -1 )]
    [InlineData( -185, 80, -2 )]
    [InlineData( 0, ulong.MaxValue, 0 )]
    [InlineData( long.MaxValue, 1, long.MaxValue )]
    [InlineData( long.MinValue, 1, long.MinValue )]
    [InlineData( long.MaxValue - 1, long.MaxValue, 1 )]
    [InlineData( long.MinValue, ulong.MaxValue, 0 )]
    public void Ceiling_ShouldReturnCorrectResult(long numerator, ulong denominator, long expectedNumerator)
    {
        var sut = new Fraction( numerator, denominator );
        var result = sut.Ceiling();

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( 1 );
        }
    }

    [Fact]
    public void Ceiling_ShouldThrowOverflowException_WhenNumeratorRisesAboveMaxPossibleValue()
    {
        var sut = new Fraction( long.MaxValue, 2 );
        var action = Lambda.Of( () => sut.Ceiling() );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 20, 10, 10, 20 )]
    [InlineData( -20, 10, -10, 20 )]
    [InlineData( 123, 90, 90, 123 )]
    [InlineData( 185, 80, 80, 185 )]
    [InlineData( -123, 90, -90, 123 )]
    [InlineData( -185, 80, -80, 185 )]
    [InlineData( long.MaxValue, 1, 1, long.MaxValue )]
    [InlineData( long.MinValue, 1, -1, (ulong)long.MaxValue + 1 )]
    [InlineData( 1, long.MaxValue, long.MaxValue, 1 )]
    [InlineData( -1, long.MaxValue, long.MinValue + 1, 1 )]
    public void Reciprocal_ShouldReturnCorrectResult(long numerator, ulong denominator, long expectedNumerator, ulong expectedDenominator)
    {
        var sut = new Fraction( numerator, denominator );
        var result = sut.Reciprocal();

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( expectedDenominator );
        }
    }

    [Fact]
    public void Reciprocal_ShouldThrowOverflowException_WhenDenominatorIsTooLarge()
    {
        var sut = new Fraction( 1, (ulong)long.MaxValue + 1 );
        var action = Lambda.Of( () => sut.Reciprocal() );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void Reciprocal_ShouldThrowArgumentOutOfRangeException_WhenNumeratorEqualsZero()
    {
        var sut = new Fraction( 0, 123 );
        var action = Lambda.Of( () => sut.Reciprocal() );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 1, 0, 1 )]
    [InlineData( 0, 100, 0, 1 )]
    [InlineData( 1, 1, 1, 1 )]
    [InlineData( -1, 1, -1, 1 )]
    [InlineData( 7, 3, 7, 3 )]
    [InlineData( -3, 7, -3, 7 )]
    [InlineData( 200, 100, 2, 1 )]
    [InlineData( -200, 100, -2, 1 )]
    [InlineData( 123, 246, 1, 2 )]
    [InlineData( -123, 246, -1, 2 )]
    [InlineData( 150, 100, 3, 2 )]
    [InlineData( -150, 100, -3, 2 )]
    [InlineData( long.MinValue, 4, long.MinValue / 4, 1 )]
    [InlineData( long.MaxValue, 7, long.MaxValue / 7, 1 )]
    public void Simplify_ShouldDivideNumeratorAndDenominatorByTheirGreatestCommonDivisor(
        long numerator,
        ulong denominator,
        long expectedNumerator,
        ulong expectedDenominator)
    {
        var sut = new Fraction( numerator, denominator );
        var result = sut.Simplify();

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( expectedDenominator );
        }
    }

    [Theory]
    [InlineData( 0, 1, 0 )]
    [InlineData( 20, 15, 5 )]
    [InlineData( 20, 15, -5 )]
    [InlineData( -20, 15, -5 )]
    [InlineData( -20, 15, 5 )]
    [InlineData( long.MaxValue, 7, long.MinValue )]
    [InlineData( long.MinValue, 7, long.MaxValue )]
    public void SetNumerator_ShouldReturnCorrectResult(long numerator, ulong denominator, long value)
    {
        var sut = new Fraction( numerator, denominator );
        var result = sut.SetNumerator( value );

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( value );
            result.Denominator.Should().Be( sut.Denominator );
        }
    }

    [Theory]
    [InlineData( 0, 1, 1 )]
    [InlineData( 20, 15, 5 )]
    [InlineData( -20, 15, 5 )]
    [InlineData( long.MaxValue, 7, ulong.MaxValue )]
    public void SetDenominator_ShouldReturnCorrectResult(long numerator, ulong denominator, ulong value)
    {
        var sut = new Fraction( numerator, denominator );
        var result = sut.SetDenominator( value );

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( sut.Numerator );
            result.Denominator.Should().Be( value );
        }
    }

    [Fact]
    public void SetDenominator_ShouldThrowArgumentOutOfRangeException_WhenValueEqualsZero()
    {
        var sut = new Fraction( 123, 456 );
        var action = Lambda.Of( () => sut.SetDenominator( 0 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 1, 1, 0 )]
    [InlineData( 123, 456, 456, 123 )]
    [InlineData( -123, 456, 456, -123 )]
    [InlineData( 0, 1, 10, 0 )]
    [InlineData( 1, 1, 2, 2 )]
    [InlineData( -1, 1, 2, -2 )]
    [InlineData( 3, 2, 50, 75 )]
    [InlineData( -3, 2, 50, -75 )]
    [InlineData( 123456789, 10000000, 500, 6173 )]
    [InlineData( -123456789, 10000000, 500, -6173 )]
    [InlineData( 6173, 500, 10000000, 123460000 )]
    [InlineData( -6173, 500, 10000000, -123460000 )]
    public void Round_ShouldReturnCorrectResult(long numerator, ulong denominator, ulong newDenominator, long expectedNumerator)
    {
        var sut = new Fraction( numerator, denominator );
        var result = sut.Round( newDenominator );

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( newDenominator );
        }
    }

    [Fact]
    public void Round_ShouldThrowArgumentOutOfRangeException_WhenDenominatorEqualsZero()
    {
        var sut = new Fraction( 123, 456 );
        var action = Lambda.Of( () => sut.Round( 0 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Round_ShouldThrowOverflowException_WhenNumeratorDropsBelowMinPossibleValue()
    {
        var sut = new Fraction( long.MinValue / 2 - 1, 1 );
        var action = Lambda.Of( () => sut.Round( 2 ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void Round_ShouldThrowOverflowException_WhenNumeratorRisesAboveMaxPossibleValue()
    {
        var sut = new Fraction( long.MaxValue / 2 + 1, 1 );
        var action = Lambda.Of( () => sut.Round( 2 ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void FromFixedConversionOperator_ShouldReturnCorrectResult()
    {
        var value = Fixed.CreateRaw( 1234, 5 );
        var result = (Fraction)value;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( value.RawValue );
            result.Denominator.Should().Be( 100000 );
        }
    }

    [Fact]
    public void DoubleConversionOperator_ShouldReturnCorrectResult()
    {
        var sut = new Fraction( 125, 50 );
        var result = (double)sut;
        result.Should().Be( 2.5 );
    }

    [Fact]
    public void DecimalConversionOperator_ShouldReturnCorrectResult()
    {
        var sut = new Fraction( 125, 50 );
        var result = (decimal)sut;
        result.Should().Be( 2.5m );
    }

    [Theory]
    [InlineData( 0, 1, 0 )]
    [InlineData( 123, 45, -123 )]
    [InlineData( -123, 45, 123 )]
    [InlineData( long.MaxValue, 100, -long.MaxValue )]
    [InlineData( long.MinValue + 1, 100, long.MaxValue )]
    public void NegateOperator_ShouldReturnCorrectResult(long numerator, ulong denominator, long expectedNumerator)
    {
        var sut = new Fraction( numerator, denominator );
        var result = -sut;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( sut.Denominator );
        }
    }

    [Fact]
    public void NegateOperator_ShouldThrowOverflowException_WhenNumeratorEqualsMinValue()
    {
        var sut = new Fraction( long.MinValue, 1 );
        var action = Lambda.Of( () => -sut );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 1, 1 )]
    [InlineData( 0, 10, 10 )]
    [InlineData( 30, 10, 40 )]
    [InlineData( -30, 5, -25 )]
    [InlineData( long.MinValue, 1, long.MinValue + 1 )]
    [InlineData( long.MaxValue - 2, 2, long.MaxValue )]
    public void IncrementOperator_ShouldReturnCorrectResult(long numerator, ulong denominator, long expectedNumerator)
    {
        var sut = new Fraction( numerator, denominator );
        var result = ++sut;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( sut.Denominator );
        }
    }

    [Fact]
    public void IncrementOperator_ShouldThrowOverflowException_WhenDenominatorIsTooLarge()
    {
        var sut = new Fraction( 123, (ulong)long.MaxValue + 1 );
        var action = Lambda.Of( () => ++sut );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void IncrementOperator_ShouldThrowOverflowException_WhenNumeratorRisesAboveMaxPossibleValue()
    {
        var sut = new Fraction( long.MaxValue, 1 );
        var action = Lambda.Of( () => ++sut );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 1, -1 )]
    [InlineData( 0, 10, -10 )]
    [InlineData( 30, 10, 20 )]
    [InlineData( -30, 5, -35 )]
    [InlineData( long.MinValue + 2, 2, long.MinValue )]
    [InlineData( long.MaxValue, 1, long.MaxValue - 1 )]
    public void DecrementOperator_ShouldReturnCorrectResult(long numerator, ulong denominator, long expectedNumerator)
    {
        var sut = new Fraction( numerator, denominator );
        var result = --sut;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( sut.Denominator );
        }
    }

    [Fact]
    public void DecrementOperator_ShouldThrowOverflowException_WhenNumeratorDropsBelowMinPossibleValue()
    {
        var sut = new Fraction( long.MinValue, 1 );
        var action = Lambda.Of( () => --sut );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void DecrementOperator_ShouldThrowOverflowException_WhenDenominatorIsTooLarge()
    {
        var sut = new Fraction( 123, (ulong)long.MaxValue + 1 );
        var action = Lambda.Of( () => --sut );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 1, 0, 1, 0, 1 )]
    [InlineData( 15, 5, 20, 5, 35, 5 )]
    [InlineData( 15, 5, -20, 5, -5, 5 )]
    [InlineData( -15, 5, 20, 5, 5, 5 )]
    [InlineData( -15, 5, -20, 5, -35, 5 )]
    [InlineData( 123, 50, 456, 20, 2526, 100 )]
    [InlineData( 123, 50, -456, 20, -2034, 100 )]
    [InlineData( -123, 50, 456, 20, 2034, 100 )]
    [InlineData( -123, 50, -456, 20, -2526, 100 )]
    public void AddOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, long exn, ulong exd)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a + b;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( exn );
            result.Denominator.Should().Be( exd );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, 123, 1, 123 )]
    [InlineData( long.MinValue, 123, -1, 123 )]
    [InlineData( long.MaxValue, 3, 1, 2 )]
    [InlineData( long.MinValue, 3, -1, 2 )]
    public void AddOperator_ShouldThrowOverflowException_WhenNumeratorIsOutsideOfAllowedValuesRange(long n1, ulong d1, long n2, ulong d2)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var action = Lambda.Of( () => a + b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void AddOperator_ShouldThrowOverflowException_WhenDenominatorRisesAboveMaxPossibleValue()
    {
        var a = new Fraction( 1, ulong.MaxValue / 2 );
        var b = new Fraction( 1, 3 );

        var action = Lambda.Of( () => a + b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 1, 0, 1, 0, 1 )]
    [InlineData( 15, 5, 20, 5, -5, 5 )]
    [InlineData( 15, 5, -20, 5, 35, 5 )]
    [InlineData( -15, 5, 20, 5, -35, 5 )]
    [InlineData( -15, 5, -20, 5, 5, 5 )]
    [InlineData( 123, 50, 456, 20, -2034, 100 )]
    [InlineData( 123, 50, -456, 20, 2526, 100 )]
    [InlineData( -123, 50, 456, 20, -2526, 100 )]
    [InlineData( -123, 50, -456, 20, 2034, 100 )]
    public void SubtractOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, long exn, ulong exd)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a - b;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( exn );
            result.Denominator.Should().Be( exd );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, 123, -1, 123 )]
    [InlineData( long.MinValue, 123, 1, 123 )]
    [InlineData( long.MaxValue, 3, -1, 2 )]
    [InlineData( long.MinValue, 3, 1, 2 )]
    public void SubtractOperator_ShouldThrowOverflowException_WhenNumeratorIsOutsideOfAllowedValuesRange(
        long n1,
        ulong d1,
        long n2,
        ulong d2)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var action = Lambda.Of( () => a - b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void SubtractOperator_ShouldThrowOverflowException_WhenDenominatorRisesAboveMaxPossibleValue()
    {
        var a = new Fraction( 1, ulong.MaxValue / 2 );
        var b = new Fraction( 1, 3 );

        var action = Lambda.Of( () => a - b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 1, 0, 1, 0, 1 )]
    [InlineData( 0, 2, 0, 10, 0, 1 )]
    [InlineData( 15, 5, 20, 5, 12, 1 )]
    [InlineData( 15, 5, -20, 5, -12, 1 )]
    [InlineData( -15, 5, 20, 5, -12, 1 )]
    [InlineData( -15, 5, -20, 5, 12, 1 )]
    [InlineData( 123, 50, 456, 20, 28044, 500 )]
    [InlineData( 123, 50, -456, 20, -28044, 500 )]
    [InlineData( -123, 50, 456, 20, -28044, 500 )]
    [InlineData( -123, 50, -456, 20, 28044, 500 )]
    public void MultiplyOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, long exn, ulong exd)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a * b;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( exn );
            result.Denominator.Should().Be( exd );
        }
    }

    [Theory]
    [InlineData( long.MaxValue, 123, 2, 123 )]
    [InlineData( long.MinValue, 123, 2, 123 )]
    public void MultiplyOperator_ShouldThrowOverflowException_WhenNumeratorIsOutsideOfAllowedValuesRange(
        long n1,
        ulong d1,
        long n2,
        ulong d2)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var action = Lambda.Of( () => a * b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void MultiplyOperator_ShouldThrowOverflowException_WhenDenominatorRisesAboveMaxPossibleValue()
    {
        var a = new Fraction( 1, ulong.MaxValue / 2 );
        var b = new Fraction( 1, 3 );

        var action = Lambda.Of( () => a * b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 1, 0, 0 )]
    [InlineData( 0, 1, 100, 0 )]
    [InlineData( 15, 5, 50, 8 )]
    [InlineData( 15, 5, -50, -8 )]
    [InlineData( -15, 5, 50, -8 )]
    [InlineData( -15, 5, -50, 8 )]
    [InlineData( 123, 50, 456, 561 )]
    [InlineData( 123, 50, -456, -561 )]
    [InlineData( -123, 50, 456, -561 )]
    [InlineData( -123, 50, -456, 561 )]
    public void MultiplyOperator_WithPercentRight_ShouldReturnCorrectResult(
        long numerator,
        ulong denominator,
        long percent,
        long expectedNumerator)
    {
        var a = new Fraction( numerator, denominator );
        var b = Percent.Normalize( percent );

        var result = a * b;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( a.Denominator );
        }
    }

    [Theory]
    [InlineData( 0, 1, 0, 0 )]
    [InlineData( 0, 1, 100, 0 )]
    [InlineData( 15, 5, 50, 8 )]
    [InlineData( 15, 5, -50, -8 )]
    [InlineData( -15, 5, 50, -8 )]
    [InlineData( -15, 5, -50, 8 )]
    [InlineData( 123, 50, 456, 561 )]
    [InlineData( 123, 50, -456, -561 )]
    [InlineData( -123, 50, 456, -561 )]
    [InlineData( -123, 50, -456, 561 )]
    public void MultiplyOperator_WithPercentLeft_ShouldReturnCorrectResult(
        long numerator,
        ulong denominator,
        long percent,
        long expectedNumerator)
    {
        var a = new Fraction( numerator, denominator );
        var b = Percent.Normalize( percent );

        var result = b * a;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( expectedNumerator );
            result.Denominator.Should().Be( a.Denominator );
        }
    }

    [Theory]
    [InlineData( 0, 1, 2, 1, 0, 1 )]
    [InlineData( 0, 2, 3, 10, 0, 1 )]
    [InlineData( 15, 5, 20, 5, 3, 4 )]
    [InlineData( 15, 5, -20, 5, -3, 4 )]
    [InlineData( -15, 5, 20, 5, -3, 4 )]
    [InlineData( -15, 5, -20, 5, 3, 4 )]
    [InlineData( 123, 50, 456, 20, 82, 760 )]
    [InlineData( 123, 50, -456, 20, -82, 760 )]
    [InlineData( -123, 50, 456, 20, -82, 760 )]
    [InlineData( -123, 50, -456, 20, 82, 760 )]
    public void DivideOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, long exn, ulong exd)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a / b;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( exn );
            result.Denominator.Should().Be( exd );
        }
    }

    [Fact]
    public void DivideOperator_ShouldThrowDivideByZeroException_WhenDivisorEqualsZero()
    {
        var a = new Fraction( 123, 456 );
        var b = new Fraction( 0, 1 );

        var action = Lambda.Of( () => a / b );

        action.Should().ThrowExactly<DivideByZeroException>();
    }

    [Theory]
    [InlineData( long.MaxValue, 123, 123, 2 )]
    [InlineData( long.MinValue, 123, 123, 2 )]
    public void DivideOperator_ShouldThrowOverflowException_WhenNumeratorIsOutsideOfAllowedValuesRange(long n1, ulong d1, long n2, ulong d2)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var action = Lambda.Of( () => a / b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void DivideOperator_ShouldThrowOverflowException_WhenDenominatorRisesAboveMaxPossibleValue()
    {
        var a = new Fraction( 1, ulong.MaxValue / 2 );
        var b = new Fraction( 3, 1 );

        var action = Lambda.Of( () => a / b );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 1, 2, 1, 0, 1 )]
    [InlineData( 0, 2, 3, 10, 0, 2 )]
    [InlineData( 20, 5, 15, 5, 5, 5 )]
    [InlineData( 20, 5, -15, 5, -10, 5 )]
    [InlineData( -20, 5, 15, 5, 10, 5 )]
    [InlineData( -20, 5, -15, 5, -5, 5 )]
    [InlineData( 456, 20, 123, 50, 66, 100 )]
    [InlineData( -456, 20, -123, 50, -66, 100 )]
    [InlineData( 456, 20, -123, 50, -36, 20 )]
    [InlineData( -456, 20, 123, 50, 36, 20 )]
    public void ModuloOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, long exn, ulong exd)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a % b;

        using ( new AssertionScope() )
        {
            result.Numerator.Should().Be( exn );
            result.Denominator.Should().Be( exd );
        }
    }

    [Fact]
    public void ModuloOperator_ShouldThrowDivideByZeroException_WhenDivisorEqualsZero()
    {
        var a = new Fraction( 123, 456 );
        var b = new Fraction( 0, 1 );

        var action = Lambda.Of( () => a % b );

        action.Should().ThrowExactly<DivideByZeroException>();
    }

    [Theory]
    [MethodData( nameof( FractionTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, bool expected)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FractionTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, bool expected)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FractionTestsData.GetGreaterThanOrEqualToData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, bool expected)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a >= b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FractionTestsData.GetGreaterThanData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, bool expected)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FractionTestsData.GetLessThanOrEqualToData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, bool expected)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FractionTestsData.GetLessThanData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(long n1, ulong d1, long n2, ulong d2, bool expected)
    {
        var a = new Fraction( n1, d1 );
        var b = new Fraction( n2, d2 );

        var result = a < b;

        result.Should().Be( expected );
    }
}
