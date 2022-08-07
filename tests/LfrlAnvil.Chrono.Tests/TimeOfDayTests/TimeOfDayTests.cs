using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Chrono.Tests.TimeOfDayTests;

[TestClass( typeof( TimeOfDayTestsData ) )]
public class TimeOfDayTests : TestsBase
{
    [Fact]
    public void Start_ShouldReturnMidnight()
    {
        var result = TimeOfDay.Start;

        using ( new AssertionScope() )
        {
            result.Hour.Should().Be( 0 );
            result.Minute.Should().Be( 0 );
            result.Second.Should().Be( 0 );
            result.Millisecond.Should().Be( 0 );
            result.Tick.Should().Be( 0 );
        }
    }

    [Fact]
    public void Mid_ShouldReturnNoon()
    {
        var result = TimeOfDay.Mid;

        using ( new AssertionScope() )
        {
            result.Hour.Should().Be( 12 );
            result.Minute.Should().Be( 0 );
            result.Second.Should().Be( 0 );
            result.Millisecond.Should().Be( 0 );
            result.Tick.Should().Be( 0 );
        }
    }

    [Fact]
    public void End_ShouldReturnOneTickBeforeMidnight()
    {
        var result = TimeOfDay.End;

        using ( new AssertionScope() )
        {
            result.Hour.Should().Be( 23 );
            result.Minute.Should().Be( 59 );
            result.Second.Should().Be( 59 );
            result.Millisecond.Should().Be( 999 );
            result.Tick.Should().Be( 9999 );
        }
    }

    [Fact]
    public void Default_ShouldReturnMidnight()
    {
        var sut = default( TimeOfDay );

        using ( new AssertionScope() )
        {
            sut.Hour.Should().Be( 0 );
            sut.Minute.Should().Be( 0 );
            sut.Second.Should().Be( 0 );
            sut.Millisecond.Should().Be( 0 );
            sut.Tick.Should().Be( 0 );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithHourPrecisionData ) )]
    public void Ctor_WithHourPrecision_ShouldReturnCorrectResult(int hour)
    {
        var sut = new TimeOfDay( hour );

        using ( new AssertionScope() )
        {
            sut.Hour.Should().Be( hour );
            sut.Minute.Should().Be( 0 );
            sut.Second.Should().Be( 0 );
            sut.Millisecond.Should().Be( 0 );
            sut.Tick.Should().Be( 0 );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithHourPrecisionThrowData ) )]
    public void Ctor_WithHourPrecision_ShouldThrow_WhenHourIsInvalid(int hour)
    {
        var action = Lambda.Of( () => new TimeOfDay( hour ) );
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithMinutePrecisionData ) )]
    public void Ctor_WithMinutePrecision_ShouldReturnCorrectResult(int hour, int minute)
    {
        var sut = new TimeOfDay( hour, minute );

        using ( new AssertionScope() )
        {
            sut.Hour.Should().Be( hour );
            sut.Minute.Should().Be( minute );
            sut.Second.Should().Be( 0 );
            sut.Millisecond.Should().Be( 0 );
            sut.Tick.Should().Be( 0 );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithMinutePrecisionThrowData ) )]
    public void Ctor_WithMinutePrecision_ShouldThrow_WhenParamsAreInvalid(int hour, int minute)
    {
        var action = Lambda.Of( () => new TimeOfDay( hour, minute ) );
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithSecondPrecisionData ) )]
    public void Ctor_WithSecondPrecision_ShouldReturnCorrectResult(int hour, int minute, int second)
    {
        var sut = new TimeOfDay( hour, minute, second );

        using ( new AssertionScope() )
        {
            sut.Hour.Should().Be( hour );
            sut.Minute.Should().Be( minute );
            sut.Second.Should().Be( second );
            sut.Millisecond.Should().Be( 0 );
            sut.Tick.Should().Be( 0 );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithSecondPrecisionThrowData ) )]
    public void Ctor_WithSecondPrecision_ShouldThrow_WhenParamsAreInvalid(int hour, int minute, int second)
    {
        var action = Lambda.Of( () => new TimeOfDay( hour, minute, second ) );
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithMsPrecisionData ) )]
    public void Ctor_WithMsPrecision_ShouldReturnCorrectResult(int hour, int minute, int second, int ms)
    {
        var sut = new TimeOfDay( hour, minute, second, ms );

        using ( new AssertionScope() )
        {
            sut.Hour.Should().Be( hour );
            sut.Minute.Should().Be( minute );
            sut.Second.Should().Be( second );
            sut.Millisecond.Should().Be( ms );
            sut.Tick.Should().Be( 0 );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithMsPrecisionThrowData ) )]
    public void Ctor_WithMsPrecision_ShouldThrowArgumentOutOfRangeException_WhenParamsAreInvalid(
        int hour,
        int minute,
        int second,
        int ms)
    {
        var action = Lambda.Of( () => new TimeOfDay( hour, minute, second, ms ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithTickPrecisionData ) )]
    public void Ctor_WithTickPrecision_ShouldReturnCorrectResult(int hour, int minute, int second, int ms, int tick)
    {
        var sut = new TimeOfDay( hour, minute, second, ms, tick );

        using ( new AssertionScope() )
        {
            sut.Hour.Should().Be( hour );
            sut.Minute.Should().Be( minute );
            sut.Second.Should().Be( second );
            sut.Millisecond.Should().Be( ms );
            sut.Tick.Should().Be( tick );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithTickPrecisionThrowData ) )]
    public void Ctor_WithTickPrecision_ShouldThrowArgumentOutOfRangeException_WhenParamsAreInvalid(
        int hour,
        int minute,
        int second,
        int ms,
        int tick)
    {
        var action = Lambda.Of( () => new TimeOfDay( hour, minute, second, ms, tick ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithTimeSpanData ) )]
    public void Ctor_WithTimeSpan_ShouldReturnCorrectResult(long ticks)
    {
        var timeSpan = TimeSpan.FromTicks( ticks );
        var sut = new TimeOfDay( timeSpan );

        using ( new AssertionScope() )
        {
            sut.Hour.Should().Be( timeSpan.Hours );
            sut.Minute.Should().Be( timeSpan.Minutes );
            sut.Second.Should().Be( timeSpan.Seconds );
            sut.Millisecond.Should().Be( timeSpan.Milliseconds );
            sut.Tick.Should().Be( (int)(timeSpan.Ticks % ChronoConstants.TicksPerMillisecond) );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithTimeSpanThrowData ) )]
    public void Ctor_WithTimeSpan_ShouldThrowArgumentOutOfRangeException_WhenParamsAreInvalid(long ticks)
    {
        var timeSpan = TimeSpan.FromTicks( ticks );
        var action = Lambda.Of( () => new TimeOfDay( timeSpan ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(long ticks, string expected)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetGetHashCodeData ) )]
    public void GetHashCode_ShouldReturnCorrectResult(long ticks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var result = sut.GetHashCode();
        result.Should().Be( ticks.GetHashCode() );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(long ticks1, long ticks2, int expectedSign)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a.CompareTo( b );

        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Fact]
    public void Invert_ShouldDoNothing_ForMidnight()
    {
        var sut = TimeOfDay.Start;
        var result = sut.Invert();
        result.Should().Be( sut );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetInvertData ) )]
    public void Invert_ShouldReturnCorrectResult_ForNonMidnight(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.Invert();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSubtractData ) )]
    public void Subtract_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var other = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = sut.Subtract( other );

        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetTrimToMillisecondData ) )]
    public void TrimToMillisecond_ShouldResetTicksToZero(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.TrimToMillisecond();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetTrimToSecondData ) )]
    public void TrimToSecond_ShouldResetMillisecondsAndTicksToZero(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.TrimToSecond();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetTrimToMinuteData ) )]
    public void TrimToMinute_ShouldResetSecondsAndMillisecondsAndTicksToZero(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.TrimToMinute();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetTrimToHourData ) )]
    public void TrimToHour_ShouldResetMinutesAndSecondsAndMillisecondsAndTicksToZero(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.TrimToHour();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetTickThrowData ) )]
    public void SetTick_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetTick( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetTickData ) )]
    public void SetTick_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetTick( value );

        using ( new AssertionScope() )
        {
            result.Tick.Should().Be( value );
            result.Should().Be( expected );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetMillisecondThrowData ) )]
    public void SetMillisecond_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetMillisecond( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetMillisecondData ) )]
    public void SetMillisecond_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetMillisecond( value );

        using ( new AssertionScope() )
        {
            result.Millisecond.Should().Be( value );
            result.Should().Be( expected );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetSecondThrowData ) )]
    public void SetSecond_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetSecond( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetSecondData ) )]
    public void SetSecond_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetSecond( value );

        using ( new AssertionScope() )
        {
            result.Second.Should().Be( value );
            result.Should().Be( expected );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetMinuteThrowData ) )]
    public void SetMinute_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetMinute( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetMinuteData ) )]
    public void SetMinute_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetMinute( value );

        using ( new AssertionScope() )
        {
            result.Minute.Should().Be( value );
            result.Should().Be( expected );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetHourThrowData ) )]
    public void SetHour_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetHour( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetHourData ) )]
    public void SetHour_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetHour( value );

        using ( new AssertionScope() )
        {
            result.Hour.Should().Be( value );
            result.Should().Be( expected );
        }
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetConversionOperatorData ) )]
    public void TimeSpanConversionOperator_ShouldReturnCorrectResult(long ticks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var result = (TimeSpan)sut;
        result.Ticks.Should().Be( ticks );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetConversionOperatorData ) )]
    public void DurationConversionOperator_ShouldReturnCorrectResult(long ticks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var result = (Duration)sut;
        result.Ticks.Should().Be( ticks );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSubtractData ) )]
    public void SubtractOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var other = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = sut - other;

        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetGreaterThanComparisonData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetLessThanOrEqualToComparisonData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetLessThanComparisonData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetGreaterThanOrEqualToComparisonData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a >= b;

        result.Should().Be( expected );
    }
}
