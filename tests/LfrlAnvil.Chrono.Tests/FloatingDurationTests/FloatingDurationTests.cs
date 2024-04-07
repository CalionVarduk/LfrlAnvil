using LfrlAnvil.Functional;
using LfrlAnvil.Numerics;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Chrono.Tests.FloatingDurationTests;

[TestClass( typeof( FloatingDurationTestsData ) )]
public class FloatingDurationTests : TestsBase
{
    [Fact]
    public void Zero_ShouldReturnResultWithZeroTicks()
    {
        var sut = FloatingDuration.Zero;
        sut.Ticks.Should().Be( 0 );
    }

    [Fact]
    public void MinValue_ShouldReturnResultWithMinLongTicks()
    {
        var sut = FloatingDuration.MinValue;
        sut.Ticks.Should().Be( decimal.MinValue );
    }

    [Fact]
    public void MaxValue_ShouldReturnResultWithMaxLongTicks()
    {
        var sut = FloatingDuration.MaxValue;
        sut.Ticks.Should().Be( decimal.MaxValue );
    }

    [Fact]
    public void Default_ShouldReturnResultWithZeroTicks()
    {
        var sut = default( FloatingDuration );
        sut.Ticks.Should().Be( 0 );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTicksData ) )]
    public void Ctor_WithTicks_ShouldCreateCorrectly(decimal ticks)
    {
        var sut = new FloatingDuration( ticks );
        sut.Ticks.Should().Be( ticks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetCtorWithTicksPrecisionData ) )]
    public void Ctor_WithTicksPrecision_ShouldCreateCorrectly(
        int hours,
        int minutes,
        int seconds,
        int milliseconds,
        int microseconds,
        decimal ticks,
        decimal expectedTicks)
    {
        var sut = new FloatingDuration( hours, minutes, seconds, milliseconds, microseconds, ticks );
        sut.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetCtorWithTimeSpanData ) )]
    public void Ctor_WithTimeSpan_ShouldCreateCorrectly(TimeSpan timeSpan)
    {
        var sut = new FloatingDuration( timeSpan );
        sut.Ticks.Should().Be( timeSpan.Ticks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetCtorWithDurationData ) )]
    public void Ctor_WithDuration_ShouldCreateCorrectly(Duration duration)
    {
        var sut = new FloatingDuration( duration );
        sut.Ticks.Should().Be( duration.Ticks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTicksData ) )]
    public void FullTicks_ShouldReturnCorrectResult(decimal ticks)
    {
        var sut = new FloatingDuration( ticks );
        sut.FullTicks.Should().Be( ( long )ticks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetFullMicrosecondsData ) )]
    public void FullMicroseconds_ShouldReturnCorrectResult(int microseconds, int ticks, long expected)
    {
        var sut = new FloatingDuration( 0, 0, 0, 0, microseconds, ticks );
        sut.FullMicroseconds.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetFullMillisecondsData ) )]
    public void FullMilliseconds_ShouldReturnCorrectResult(int milliseconds, int microseconds, int ticks, long expected)
    {
        var sut = new FloatingDuration( 0, 0, 0, milliseconds, microseconds, ticks );
        sut.FullMilliseconds.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetFullSecondsData ) )]
    public void FullSeconds_ShouldReturnCorrectResult(int seconds, int milliseconds, int microseconds, int ticks, long expected)
    {
        var sut = new FloatingDuration( 0, 0, seconds, milliseconds, microseconds, ticks );
        sut.FullSeconds.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetFullMinutesData ) )]
    public void FullMinutes_ShouldReturnCorrectResult(
        int minutes,
        int seconds,
        int milliseconds,
        int microseconds,
        int ticks,
        long expected)
    {
        var sut = new FloatingDuration( 0, minutes, seconds, milliseconds, microseconds, ticks );
        sut.FullMinutes.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetFullHoursData ) )]
    public void FullHours_ShouldReturnCorrectResult(
        int hours,
        int minutes,
        int seconds,
        int milliseconds,
        int microseconds,
        int ticks,
        long expected)
    {
        var sut = new FloatingDuration( hours, minutes, seconds, milliseconds, microseconds, ticks );
        sut.FullHours.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTicksInMicrosecondData ) )]
    public void TicksInMicrosecond_ShouldReturnCorrectResult(int microseconds, decimal ticks, decimal expected)
    {
        var sut = new FloatingDuration( 0, 0, 0, 0, microseconds, ticks );
        sut.TicksInMicrosecond.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetMicrosecondsInMillisecondData ) )]
    public void MicrosecondsInMillisecond_ShouldReturnCorrectResult(int milliseconds, int microseconds, int ticks, int expected)
    {
        var sut = new FloatingDuration( 0, 0, 0, milliseconds, microseconds, ticks );
        sut.MicrosecondsInMillisecond.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetMillisecondsInSecondData ) )]
    public void MillisecondsInSecond_ShouldReturnCorrectResult(int seconds, int milliseconds, int microseconds, int ticks, int expected)
    {
        var sut = new FloatingDuration( 0, 0, seconds, milliseconds, microseconds, ticks );
        sut.MillisecondsInSecond.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSecondsInMinuteData ) )]
    public void SecondsInMinute_ShouldReturnCorrectResult(
        int minutes,
        int seconds,
        int milliseconds,
        int microseconds,
        int ticks,
        int expected)
    {
        var sut = new FloatingDuration( 0, minutes, seconds, milliseconds, microseconds, ticks );
        sut.SecondsInMinute.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetMinutesInHourData ) )]
    public void MinutesInHour_ShouldReturnCorrectResult(
        int hours,
        int minutes,
        int seconds,
        int milliseconds,
        int microseconds,
        int ticks,
        int expected)
    {
        var sut = new FloatingDuration( hours, minutes, seconds, milliseconds, microseconds, ticks );
        sut.MinutesInHour.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTotalMicrosecondsData ) )]
    public void TotalMicroseconds_ShouldReturnCorrectResult(int microseconds, int ticks, decimal expected)
    {
        var sut = new FloatingDuration( 0, 0, 0, 0, microseconds, ticks );
        sut.TotalMicroseconds.Should().BeApproximately( expected, 0.0000000000000001m );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTotalMillisecondsData ) )]
    public void TotalMilliseconds_ShouldReturnCorrectResult(int milliseconds, int microseconds, int ticks, decimal expected)
    {
        var sut = new FloatingDuration( 0, 0, 0, milliseconds, microseconds, ticks );
        sut.TotalMilliseconds.Should().BeApproximately( expected, 0.0000000000000001m );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTotalSecondsData ) )]
    public void TotalSeconds_ShouldReturnCorrectResult(int seconds, int milliseconds, int microseconds, int ticks, decimal expected)
    {
        var sut = new FloatingDuration( 0, 0, seconds, milliseconds, microseconds, ticks );
        sut.TotalSeconds.Should().BeApproximately( expected, 0.0000000000000001m );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTotalMinutesData ) )]
    public void TotalMinutes_ShouldReturnCorrectResult(
        int minutes,
        int seconds,
        int milliseconds,
        int microseconds,
        int ticks,
        decimal expected)
    {
        var sut = new FloatingDuration( 0, minutes, seconds, milliseconds, microseconds, ticks );
        sut.TotalMinutes.Should().BeApproximately( expected, 0.0000000000000001m );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTotalHoursData ) )]
    public void TotalHours_ShouldReturnCorrectResult(
        int hours,
        int minutes,
        int seconds,
        int milliseconds,
        int microseconds,
        int ticks,
        decimal expected)
    {
        var sut = new FloatingDuration( hours, minutes, seconds, milliseconds, microseconds, ticks );
        sut.TotalHours.Should().BeApproximately( expected, 0.0000000000000001m );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTicksData ) )]
    public void FromTicks_ShouldReturnCorrectResult(long ticks)
    {
        var sut = FloatingDuration.FromTicks( ticks );
        sut.Ticks.Should().Be( ticks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetFromMicrosecondsData ) )]
    public void FromMicroseconds_ShouldReturnCorrectResult(long microseconds, long expectedTicks)
    {
        var sut = FloatingDuration.FromMicroseconds( microseconds );
        sut.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetFromMillisecondsData ) )]
    public void FromMilliseconds_ShouldReturnCorrectResult(long milliseconds, long expectedTicks)
    {
        var sut = FloatingDuration.FromMilliseconds( milliseconds );
        sut.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetFromSecondsData ) )]
    public void FromSeconds_ShouldReturnCorrectResult(long seconds, long expectedTicks)
    {
        var sut = FloatingDuration.FromSeconds( seconds );
        sut.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetFromMinutesData ) )]
    public void FromMinutes_ShouldReturnCorrectResult(long minutes, long expectedTicks)
    {
        var sut = FloatingDuration.FromMinutes( minutes );
        sut.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetFromHoursData ) )]
    public void FromHours_ShouldReturnCorrectResult(long hours, long expectedTicks)
    {
        var sut = FloatingDuration.FromHours( hours );
        sut.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(long ticks, string expected)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var ticks = Fixture.Create<decimal>();
        var sut = new FloatingDuration( ticks );

        var result = sut.GetHashCode();

        result.Should().Be( ticks.GetHashCode() );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
    {
        var a = new FloatingDuration( ticks1 );
        var b = new FloatingDuration( ticks2 );

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(long ticks1, long ticks2, int expectedSign)
    {
        var a = new FloatingDuration( ticks1 );
        var b = new FloatingDuration( ticks2 );

        var result = a.CompareTo( b );

        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetNegateData ) )]
    public void Negate_ShouldReturnCorrectResult(long ticks, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.Negate();
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetAbsData ) )]
    public void Abs_ShouldReturnCorrectResult(long ticks, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.Abs();
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetAddTicksData ) )]
    public void Add_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks1 );
        var other = new FloatingDuration( ticks2 );

        var result = sut.Add( other );

        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetAddTicksData ) )]
    public void AddTicks_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks1 );
        var result = sut.AddTicks( ticks2 );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetAddMicrosecondsData ) )]
    public void AddMicroseconds_ShouldReturnCorrectResult(long ticks, long microseconds, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.AddMicroseconds( microseconds );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetAddMillisecondsData ) )]
    public void AddMilliseconds_ShouldReturnCorrectResult(long ticks, long milliseconds, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.AddMilliseconds( milliseconds );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetAddSecondsData ) )]
    public void AddSeconds_ShouldReturnCorrectResult(long ticks, long seconds, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.AddSeconds( seconds );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetAddMinutesData ) )]
    public void AddMinutes_ShouldReturnCorrectResult(long ticks, long minutes, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.AddMinutes( minutes );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetAddHoursData ) )]
    public void AddHours_ShouldReturnCorrectResult(long ticks, long hours, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.AddHours( hours );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSubtractTicksData ) )]
    public void Subtract_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks1 );
        var other = new FloatingDuration( ticks2 );

        var result = sut.Subtract( other );

        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSubtractTicksData ) )]
    public void SubtractTicks_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks1 );
        var result = sut.SubtractTicks( ticks2 );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSubtractMicrosecondsData ) )]
    public void SubtractMicroseconds_ShouldReturnCorrectResult(long ticks, long microseconds, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SubtractMicroseconds( microseconds );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSubtractMillisecondsData ) )]
    public void SubtractMilliseconds_ShouldReturnCorrectResult(long ticks, long milliseconds, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SubtractMilliseconds( milliseconds );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSubtractSecondsData ) )]
    public void SubtractSeconds_ShouldReturnCorrectResult(long ticks, long seconds, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SubtractSeconds( seconds );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSubtractMinutesData ) )]
    public void SubtractMinutes_ShouldReturnCorrectResult(long ticks, long minutes, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SubtractMinutes( minutes );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSubtractHoursData ) )]
    public void SubtractHours_ShouldReturnCorrectResult(long ticks, long hours, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SubtractHours( hours );
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetMultiplyData ) )]
    public void Multiply_ShouldReturnCorrectResult(long ticks, decimal multiplier, decimal expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.Multiply( multiplier );
        result.Ticks.Should().BeApproximately( expectedTicks, 0.0000000000000001m );
    }

    [Fact]
    public void Divide_ShouldThrowDivideByZeroException_WhenDivisorIsZero()
    {
        var sut = FloatingDuration.FromTicks( Fixture.Create<decimal>() );
        var action = Lambda.Of( () => sut.Divide( 0 ) );
        action.Should().ThrowExactly<DivideByZeroException>();
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetDivideData ) )]
    public void Divide_ShouldReturnCorrectResult(long ticks, decimal divisor, decimal expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.Divide( divisor );
        result.Ticks.Should().BeApproximately( expectedTicks, 0.0000000000000001m );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTrimToTickData ) )]
    public void TrimToTick_ShouldReturnCorrectResult(decimal ticks, decimal expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.TrimToTick();
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTrimToMicrosecondData ) )]
    public void TrimToMicrosecond_ShouldReturnCorrectResult(long ticks, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.TrimToMicrosecond();
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTrimToMillisecondData ) )]
    public void TrimToMillisecond_ShouldReturnCorrectResult(long ticks, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.TrimToMillisecond();
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTrimToSecondData ) )]
    public void TrimToSecond_ShouldReturnCorrectResult(long ticks, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.TrimToSecond();
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTrimToMinuteData ) )]
    public void TrimToMinute_ShouldReturnCorrectResult(long ticks, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.TrimToMinute();
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTrimToHourData ) )]
    public void TrimToHour_ShouldReturnCorrectResult(long ticks, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.TrimToHour();
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetTicksInMicrosecondThrowData ) )]
    public void SetTicksInMicrosecond_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(long ticks, int value)
    {
        var sut = new FloatingDuration( ticks );
        var action = Lambda.Of( () => sut.SetTicksInMicrosecond( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetTicksInMicrosecondData ) )]
    public void SetTicksInMicrosecond_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SetTicksInMicrosecond( value );

        using ( new AssertionScope() )
        {
            result.TicksInMicrosecond.Should().Be( value );
            result.Ticks.Should().Be( expectedTicks );
        }
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetMicrosecondsInMillisecondThrowData ) )]
    public void SetMicrosecondsInMillisecond_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(long ticks, int value)
    {
        var sut = new FloatingDuration( ticks );
        var action = Lambda.Of( () => sut.SetMicrosecondsInMillisecond( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetMicrosecondsInMillisecondData ) )]
    public void SetMicrosecondsInMillisecond_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SetMicrosecondsInMillisecond( value );

        using ( new AssertionScope() )
        {
            result.MicrosecondsInMillisecond.Should().Be( value );
            result.Ticks.Should().Be( expectedTicks );
        }
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetMillisecondsInSecondThrowData ) )]
    public void SetMillisecondsInSecond_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(long ticks, int value)
    {
        var sut = new FloatingDuration( ticks );
        var action = Lambda.Of( () => sut.SetMillisecondsInSecond( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetMillisecondsInSecondData ) )]
    public void SetMillisecondsInSecond_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SetMillisecondsInSecond( value );

        using ( new AssertionScope() )
        {
            result.MillisecondsInSecond.Should().Be( value );
            result.Ticks.Should().Be( expectedTicks );
        }
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetSecondsInMinuteThrowData ) )]
    public void SetSecondsInMinute_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(long ticks, int value)
    {
        var sut = new FloatingDuration( ticks );
        var action = Lambda.Of( () => sut.SetSecondsInMinute( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetSecondsInMinuteData ) )]
    public void SetSecondsInMinute_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SetSecondsInMinute( value );

        using ( new AssertionScope() )
        {
            result.SecondsInMinute.Should().Be( value );
            result.Ticks.Should().Be( expectedTicks );
        }
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetMinutesInHourThrowData ) )]
    public void SetMinutesInHour_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(long ticks, int value)
    {
        var sut = new FloatingDuration( ticks );
        var action = Lambda.Of( () => sut.SetMinutesInHour( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetMinutesInHourData ) )]
    public void SetMinutesInHour_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SetMinutesInHour( value );

        using ( new AssertionScope() )
        {
            result.MinutesInHour.Should().Be( value );
            result.Ticks.Should().Be( expectedTicks );
        }
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetHoursThrowData ) )]
    public void SetHours_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(long ticks, int value)
    {
        var sut = new FloatingDuration( ticks );
        var action = Lambda.Of( () => sut.SetHours( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSetHoursData ) )]
    public void SetHours_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut.SetHours( value );

        using ( new AssertionScope() )
        {
            result.FullHours.Should().Be( value );
            result.Ticks.Should().Be( expectedTicks );
        }
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTicksData ) )]
    public void TimeSpanConversionOperator_ShouldReturnTimeSpanWithCorrectTicks(decimal ticks)
    {
        var expected = Math.Round( ticks, MidpointRounding.AwayFromZero );
        var sut = new FloatingDuration( ticks );
        var result = ( TimeSpan )sut;
        result.Ticks.Should().Be( ( long )expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetTicksData ) )]
    public void DurationConversionOperator_ShouldReturnDurationWithCorrectTicks(decimal ticks)
    {
        var expected = Math.Round( ticks, MidpointRounding.AwayFromZero );
        var sut = new FloatingDuration( ticks );
        var result = ( Duration )sut;
        result.Ticks.Should().Be( ( long )expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetNegateData ) )]
    public void NegateOperator_ShouldReturnCorrectResult(long ticks, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = -sut;
        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetAddTicksData ) )]
    public void AddOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks1 );
        var other = new FloatingDuration( ticks2 );

        var result = sut + other;

        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetSubtractTicksData ) )]
    public void SubtractOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
    {
        var sut = new FloatingDuration( ticks1 );
        var other = new FloatingDuration( ticks2 );

        var result = sut - other;

        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetMultiplyData ) )]
    public void MultiplyOperator_ShouldReturnCorrectResult(long ticks, decimal multiplier, decimal expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut * multiplier;
        result.Ticks.Should().BeApproximately( expectedTicks, 0.0000000000000001m );
    }

    [Fact]
    public void DivideOperator_ShouldThrowDivideByZeroException_WhenDivisorIsZero()
    {
        var sut = FloatingDuration.FromTicks( Fixture.Create<decimal>() );
        var action = Lambda.Of( () => sut / 0 );
        action.Should().ThrowExactly<DivideByZeroException>();
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetDivideData ) )]
    public void DivideOperator_ShouldReturnCorrectResult(long ticks, decimal divisor, decimal expectedTicks)
    {
        var sut = new FloatingDuration( ticks );
        var result = sut / divisor;
        result.Ticks.Should().BeApproximately( expectedTicks, 0.0000000000000001m );
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
    public void MultiplyOperator_ForPercentRight_ShouldReturnCorrectResult(long ticks, int right, long expectedTicks)
    {
        var a = FloatingDuration.FromTicks( ticks );
        var b = Percent.Normalize( right );

        var result = a * b;

        result.Ticks.Should().Be( expectedTicks );
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
    public void MultiplyOperator_ForPercentLeft_ShouldReturnCorrectResult(long ticks, int right, long expectedTicks)
    {
        var a = FloatingDuration.FromTicks( ticks );
        var b = Percent.Normalize( right );

        var result = b * a;

        result.Ticks.Should().Be( expectedTicks );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
    {
        var a = new FloatingDuration( ticks1 );
        var b = new FloatingDuration( ticks2 );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
    {
        var a = new FloatingDuration( ticks1 );
        var b = new FloatingDuration( ticks2 );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetGreaterThanComparisonData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
    {
        var a = new FloatingDuration( ticks1 );
        var b = new FloatingDuration( ticks2 );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetLessThanOrEqualToComparisonData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
    {
        var a = new FloatingDuration( ticks1 );
        var b = new FloatingDuration( ticks2 );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetLessThanComparisonData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
    {
        var a = new FloatingDuration( ticks1 );
        var b = new FloatingDuration( ticks2 );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( FloatingDurationTestsData.GetGreaterThanOrEqualToComparisonData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
    {
        var a = new FloatingDuration( ticks1 );
        var b = new FloatingDuration( ticks2 );

        var result = a >= b;

        result.Should().Be( expected );
    }
}
