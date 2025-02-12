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

        Assertion.All(
                result.Hour.TestEquals( 0 ),
                result.Minute.TestEquals( 0 ),
                result.Second.TestEquals( 0 ),
                result.Millisecond.TestEquals( 0 ),
                result.Microsecond.TestEquals( 0 ),
                result.Tick.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Mid_ShouldReturnNoon()
    {
        var result = TimeOfDay.Mid;

        Assertion.All(
                result.Hour.TestEquals( 12 ),
                result.Minute.TestEquals( 0 ),
                result.Second.TestEquals( 0 ),
                result.Millisecond.TestEquals( 0 ),
                result.Microsecond.TestEquals( 0 ),
                result.Tick.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void End_ShouldReturnOneTickBeforeMidnight()
    {
        var result = TimeOfDay.End;

        Assertion.All(
                result.Hour.TestEquals( 23 ),
                result.Minute.TestEquals( 59 ),
                result.Second.TestEquals( 59 ),
                result.Millisecond.TestEquals( 999 ),
                result.Microsecond.TestEquals( 999 ),
                result.Tick.TestEquals( 9 ) )
            .Go();
    }

    [Fact]
    public void Default_ShouldReturnMidnight()
    {
        var sut = default( TimeOfDay );

        Assertion.All(
                sut.Hour.TestEquals( 0 ),
                sut.Minute.TestEquals( 0 ),
                sut.Second.TestEquals( 0 ),
                sut.Millisecond.TestEquals( 0 ),
                sut.Microsecond.TestEquals( 0 ),
                sut.Tick.TestEquals( 0 ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithTickPrecisionData ) )]
    public void Ctor_WithTickPrecision_ShouldReturnCorrectResult(
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond,
        int tick)
    {
        var sut = new TimeOfDay( hour, minute, second, millisecond, microsecond, tick );

        Assertion.All(
                sut.Hour.TestEquals( hour ),
                sut.Minute.TestEquals( minute ),
                sut.Second.TestEquals( second ),
                sut.Millisecond.TestEquals( millisecond ),
                sut.Microsecond.TestEquals( microsecond ),
                sut.Tick.TestEquals( tick ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithTickPrecisionThrowData ) )]
    public void Ctor_WithTickPrecision_ShouldThrowArgumentOutOfRangeException_WhenParamsAreInvalid(
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond,
        int tick)
    {
        var action = Lambda.Of( () => new TimeOfDay( hour, minute, second, millisecond, microsecond, tick ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithTimeSpanData ) )]
    public void Ctor_WithTimeSpan_ShouldReturnCorrectResult(long ticks)
    {
        var timeSpan = TimeSpan.FromTicks( ticks );
        var sut = new TimeOfDay( timeSpan );

        Assertion.All(
                sut.Hour.TestEquals( timeSpan.Hours ),
                sut.Minute.TestEquals( timeSpan.Minutes ),
                sut.Second.TestEquals( timeSpan.Seconds ),
                sut.Millisecond.TestEquals( timeSpan.Milliseconds ),
                sut.Microsecond.TestEquals( timeSpan.Microseconds ),
                sut.Tick.TestEquals( ( int )(timeSpan.Ticks % ChronoConstants.TicksPerMicrosecond) ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCtorWithTimeSpanThrowData ) )]
    public void Ctor_WithTimeSpan_ShouldThrowArgumentOutOfRangeException_WhenParamsAreInvalid(long ticks)
    {
        var timeSpan = TimeSpan.FromTicks( ticks );
        var action = Lambda.Of( () => new TimeOfDay( timeSpan ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(long ticks, string expected)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var result = sut.ToString();
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetGetHashCodeData ) )]
    public void GetHashCode_ShouldReturnCorrectResult(long ticks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var result = sut.GetHashCode();
        result.TestEquals( ticks.GetHashCode() ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(long ticks1, long ticks2, int expectedSign)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a.CompareTo( b );

        Math.Sign( result ).TestEquals( expectedSign ).Go();
    }

    [Fact]
    public void Invert_ShouldDoNothing_ForMidnight()
    {
        var sut = TimeOfDay.Start;
        var result = sut.Invert();
        result.TestEquals( sut ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetInvertData ) )]
    public void Invert_ShouldReturnCorrectResult_ForNonMidnight(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.Invert();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSubtractData ) )]
    public void Subtract_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var other = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = sut.Subtract( other );

        result.Ticks.TestEquals( expectedTicks ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetTrimToMicrosecondData ) )]
    public void TrimToMicrosecond_ShouldResetTicksToZero(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.TrimToMicrosecond();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetTrimToMillisecondData ) )]
    public void TrimToMillisecond_ShouldResetTicksToZero(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.TrimToMillisecond();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetTrimToSecondData ) )]
    public void TrimToSecond_ShouldResetMillisecondsAndTicksToZero(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.TrimToSecond();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetTrimToMinuteData ) )]
    public void TrimToMinute_ShouldResetSecondsAndMillisecondsAndTicksToZero(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.TrimToMinute();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetTrimToHourData ) )]
    public void TrimToHour_ShouldResetMinutesAndSecondsAndMillisecondsAndTicksToZero(long ticks, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.TrimToHour();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetTickThrowData ) )]
    public void SetTick_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetTick( value ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetTickData ) )]
    public void SetTick_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetTick( value );

        Assertion.All(
                result.Tick.TestEquals( value ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetMicrosecondThrowData ) )]
    public void SetMicrosecond_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetMicrosecond( value ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetMicrosecondData ) )]
    public void SetMicrosecond_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetMicrosecond( value );

        Assertion.All(
                result.Microsecond.TestEquals( value ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetMillisecondThrowData ) )]
    public void SetMillisecond_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetMillisecond( value ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetMillisecondData ) )]
    public void SetMillisecond_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetMillisecond( value );

        Assertion.All(
                result.Millisecond.TestEquals( value ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetSecondThrowData ) )]
    public void SetSecond_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetSecond( value ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetSecondData ) )]
    public void SetSecond_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetSecond( value );

        Assertion.All(
                result.Second.TestEquals( value ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetMinuteThrowData ) )]
    public void SetMinute_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetMinute( value ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetMinuteData ) )]
    public void SetMinute_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetMinute( value );

        Assertion.All(
                result.Minute.TestEquals( value ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetHourThrowData ) )]
    public void SetHour_ShouldThrowArgumentOutOfRangeException_WhenValueIsInvalid(int value)
    {
        var sut = TimeOfDay.Start;
        var action = Lambda.Of( () => sut.SetHour( value ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSetHourData ) )]
    public void SetHour_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var expected = new TimeOfDay( TimeSpan.FromTicks( expectedTicks ) );

        var result = sut.SetHour( value );

        Assertion.All(
                result.Hour.TestEquals( value ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetConversionOperatorData ) )]
    public void TimeSpanConversionOperator_ShouldReturnCorrectResult(long ticks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var result = ( TimeSpan )sut;
        result.Ticks.TestEquals( ticks ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetConversionOperatorData ) )]
    public void DurationConversionOperator_ShouldReturnCorrectResult(long ticks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks ) );
        var result = ( Duration )sut;
        result.Ticks.TestEquals( ticks ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetSubtractData ) )]
    public void SubtractOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
    {
        var sut = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var other = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = sut - other;

        result.Ticks.TestEquals( expectedTicks ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a != b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetGreaterThanComparisonData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a > b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetLessThanOrEqualToComparisonData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a <= b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetLessThanComparisonData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a < b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeOfDayTestsData.GetGreaterThanOrEqualToComparisonData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int ticks1, int ticks2, bool expected)
    {
        var a = new TimeOfDay( TimeSpan.FromTicks( ticks1 ) );
        var b = new TimeOfDay( TimeSpan.FromTicks( ticks2 ) );

        var result = a >= b;

        result.TestEquals( expected ).Go();
    }
}
