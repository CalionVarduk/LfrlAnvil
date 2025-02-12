using System.Diagnostics.Contracts;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Chrono.Tests.PeriodTests;

[TestClass( typeof( PeriodTestsData ) )]
public class PeriodTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnEmptyPeriod()
    {
        var sut = default( Period );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.None ) )
            .Go();
    }

    [Fact]
    public void Empty_ShouldReturnEmptyPeriod()
    {
        var sut = Period.Empty;

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.None ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetCtorWithDateData ) )]
    public void Ctor_WithDate_ShouldReturnCorrectResult(
        int years,
        int months,
        int weeks,
        int days,
        PeriodUnits expectedUnits)
    {
        var sut = new Period( years, months, weeks, days );

        Assertion.All(
                sut.Years.TestEquals( years ),
                sut.Months.TestEquals( months ),
                sut.Weeks.TestEquals( weeks ),
                sut.Days.TestEquals( days ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( expectedUnits ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetCtorWithTimeData ) )]
    public void Ctor_WithTime_ShouldReturnCorrectResult(
        int hours,
        int minutes,
        int seconds,
        int milliseconds,
        int microseconds,
        int ticks,
        PeriodUnits expectedUnits)
    {
        var sut = new Period( hours, minutes, seconds, milliseconds, microseconds, ticks );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( hours ),
                sut.Minutes.TestEquals( minutes ),
                sut.Seconds.TestEquals( seconds ),
                sut.Milliseconds.TestEquals( milliseconds ),
                sut.Microseconds.TestEquals( microseconds ),
                sut.Ticks.TestEquals( ticks ),
                sut.ActiveUnits.TestEquals( expectedUnits ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetCtorWithFullData ) )]
    public void Ctor_Full_ShouldReturnCorrectResult(
        (int Years, int Months, int Weeks, int Days) date,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time,
        PeriodUnits expectedUnits)
    {
        var sut = new Period(
            date.Years,
            date.Months,
            date.Weeks,
            date.Days,
            time.Hours,
            time.Minutes,
            time.Seconds,
            time.Milliseconds,
            time.Microseconds,
            time.Ticks );

        Assertion.All(
                sut.Years.TestEquals( date.Years ),
                sut.Months.TestEquals( date.Months ),
                sut.Weeks.TestEquals( date.Weeks ),
                sut.Days.TestEquals( date.Days ),
                sut.Hours.TestEquals( time.Hours ),
                sut.Minutes.TestEquals( time.Minutes ),
                sut.Seconds.TestEquals( time.Seconds ),
                sut.Milliseconds.TestEquals( time.Milliseconds ),
                sut.Microseconds.TestEquals( time.Microseconds ),
                sut.Ticks.TestEquals( time.Ticks ),
                sut.ActiveUnits.TestEquals( expectedUnits ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetCtorWithTimeSpanData ) )]
    public void Ctor_WithTimeSpan_ShouldReturnCorrectResult(
        TimeSpan timeSpan,
        int expectedDays,
        int expectedHours,
        int expectedMinutes,
        int expectedSeconds,
        int expectedMilliseconds,
        int expectedMicroseconds,
        int expectedTicks,
        PeriodUnits expectedUnits)
    {
        var sut = new Period( timeSpan );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( expectedDays ),
                sut.Hours.TestEquals( expectedHours ),
                sut.Minutes.TestEquals( expectedMinutes ),
                sut.Seconds.TestEquals( expectedSeconds ),
                sut.Milliseconds.TestEquals( expectedMilliseconds ),
                sut.Microseconds.TestEquals( expectedMicroseconds ),
                sut.Ticks.TestEquals( expectedTicks ),
                sut.ActiveUnits.TestEquals( expectedUnits ) )
            .Go();
    }

    [Fact]
    public void FromTicks_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromTicks( value );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( value ),
                sut.ActiveUnits.TestEquals( PeriodUnits.Ticks ) )
            .Go();
    }

    [Fact]
    public void FromMicroseconds_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromMicroseconds( value );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( value ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.Microseconds ) )
            .Go();
    }

    [Fact]
    public void FromMilliseconds_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromMilliseconds( value );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( value ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.Milliseconds ) )
            .Go();
    }

    [Fact]
    public void FromSeconds_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromSeconds( value );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( value ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.Seconds ) )
            .Go();
    }

    [Fact]
    public void FromMinutes_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromMinutes( value );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( value ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.Minutes ) )
            .Go();
    }

    [Fact]
    public void FromHours_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromHours( value );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( value ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.Hours ) )
            .Go();
    }

    [Fact]
    public void FromDays_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromDays( value );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( value ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.Days ) )
            .Go();
    }

    [Fact]
    public void FromWeeks_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromWeeks( value );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( value ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.Weeks ) )
            .Go();
    }

    [Fact]
    public void FromMonths_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromMonths( value );

        Assertion.All(
                sut.Years.TestEquals( 0 ),
                sut.Months.TestEquals( value ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.Months ) )
            .Go();
    }

    [Fact]
    public void FromYears_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromYears( value );

        Assertion.All(
                sut.Years.TestEquals( value ),
                sut.Months.TestEquals( 0 ),
                sut.Weeks.TestEquals( 0 ),
                sut.Days.TestEquals( 0 ),
                sut.Hours.TestEquals( 0 ),
                sut.Minutes.TestEquals( 0 ),
                sut.Seconds.TestEquals( 0 ),
                sut.Milliseconds.TestEquals( 0 ),
                sut.Microseconds.TestEquals( 0 ),
                sut.Ticks.TestEquals( 0 ),
                sut.ActiveUnits.TestEquals( PeriodUnits.Years ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(
        (int Years, int Months, int Weeks, int Days) date,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time,
        string expected)
    {
        var sut = new Period(
            date.Years,
            date.Months,
            date.Weeks,
            date.Days,
            time.Hours,
            time.Minutes,
            time.Seconds,
            time.Milliseconds,
            time.Microseconds,
            time.Ticks );

        var result = sut.ToString();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var expected = Hash.Default.Add( sut.Years )
            .Add( sut.Months )
            .Add( sut.Weeks )
            .Add( sut.Days )
            .Add( sut.Hours )
            .Add( sut.Minutes )
            .Add( sut.Seconds )
            .Add( sut.Milliseconds )
            .Add( sut.Microseconds )
            .Add( sut.Ticks )
            .Value;

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(
        (int Years, int Months, int Weeks, int Days) date1,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time1,
        (int Years, int Months, int Weeks, int Days) date2,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time2,
        bool expected)
    {
        var a = new Period(
            date1.Years,
            date1.Months,
            date1.Weeks,
            date1.Days,
            time1.Hours,
            time1.Minutes,
            time1.Seconds,
            time1.Milliseconds,
            time1.Microseconds,
            time1.Ticks );

        var b = new Period(
            date2.Years,
            date2.Months,
            date2.Weeks,
            date2.Days,
            time2.Hours,
            time2.Minutes,
            time2.Seconds,
            time2.Milliseconds,
            time2.Microseconds,
            time2.Ticks );

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetAddData ) )]
    public void Add_ShouldReturnCorrectResult(
        (int Years, int Months, int Weeks, int Days) date1,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time1,
        (int Years, int Months, int Weeks, int Days) date2,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time2,
        (int Years, int Months, int Weeks, int Days) expectedDate,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) expectedTime)
    {
        var sut = new Period(
            date1.Years,
            date1.Months,
            date1.Weeks,
            date1.Days,
            time1.Hours,
            time1.Minutes,
            time1.Seconds,
            time1.Milliseconds,
            time1.Microseconds,
            time1.Ticks );

        var other = new Period(
            date2.Years,
            date2.Months,
            date2.Weeks,
            date2.Days,
            time2.Hours,
            time2.Minutes,
            time2.Seconds,
            time2.Milliseconds,
            time2.Microseconds,
            time2.Ticks );

        var result = sut.Add( other );

        Assertion.All(
                result.Years.TestEquals( expectedDate.Years ),
                result.Months.TestEquals( expectedDate.Months ),
                result.Weeks.TestEquals( expectedDate.Weeks ),
                result.Days.TestEquals( expectedDate.Days ),
                result.Hours.TestEquals( expectedTime.Hours ),
                result.Minutes.TestEquals( expectedTime.Minutes ),
                result.Seconds.TestEquals( expectedTime.Seconds ),
                result.Milliseconds.TestEquals( expectedTime.Milliseconds ),
                result.Microseconds.TestEquals( expectedTime.Microseconds ),
                result.Ticks.TestEquals( expectedTime.Ticks ) )
            .Go();
    }

    [Fact]
    public void AddTicks_ShouldEquivalentToSetTicksWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetTicks( sut.Ticks + value );

        var result = sut.AddTicks( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void AddMicroseconds_ShouldBeEquivalentToSetMicrosecondsWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMicroseconds( sut.Microseconds + value );

        var result = sut.AddMicroseconds( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void AddMilliseconds_ShouldBeEquivalentToSetMillisecondsWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMilliseconds( sut.Milliseconds + value );

        var result = sut.AddMilliseconds( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void AddSeconds_ShouldBeEquivalentToSetSecondsWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetSeconds( sut.Seconds + value );

        var result = sut.AddSeconds( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void AddMinutes_ShouldBeEquivalentToSetMinutesWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMinutes( sut.Minutes + value );

        var result = sut.AddMinutes( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void AddHours_ShouldBeEquivalentToSetHoursWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetHours( sut.Hours + value );

        var result = sut.AddHours( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void AddDays_ShouldBeEquivalentToSetDaysWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetDays( sut.Days + value );

        var result = sut.AddDays( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void AddWeeks_ShouldBeEquivalentToSetWeeksWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetWeeks( sut.Weeks + value );

        var result = sut.AddWeeks( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void AddMonths_ShouldBeEquivalentToSetMonthsWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMonths( sut.Months + value );

        var result = sut.AddMonths( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void AddYears_ShouldBeEquivalentToSetYearsWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetYears( sut.Years + value );

        var result = sut.AddYears( value );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetSubtractData ) )]
    public void Subtract_ShouldReturnCorrectResult(
        (int Years, int Months, int Weeks, int Days) date1,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time1,
        (int Years, int Months, int Weeks, int Days) date2,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time2,
        (int Years, int Months, int Weeks, int Days) expectedDate,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) expectedTime)
    {
        var sut = new Period(
            date1.Years,
            date1.Months,
            date1.Weeks,
            date1.Days,
            time1.Hours,
            time1.Minutes,
            time1.Seconds,
            time1.Milliseconds,
            time1.Microseconds,
            time1.Ticks );

        var other = new Period(
            date2.Years,
            date2.Months,
            date2.Weeks,
            date2.Days,
            time2.Hours,
            time2.Minutes,
            time2.Seconds,
            time2.Milliseconds,
            time2.Microseconds,
            time2.Ticks );

        var result = sut.Subtract( other );

        Assertion.All(
                result.Years.TestEquals( expectedDate.Years ),
                result.Months.TestEquals( expectedDate.Months ),
                result.Weeks.TestEquals( expectedDate.Weeks ),
                result.Days.TestEquals( expectedDate.Days ),
                result.Hours.TestEquals( expectedTime.Hours ),
                result.Minutes.TestEquals( expectedTime.Minutes ),
                result.Seconds.TestEquals( expectedTime.Seconds ),
                result.Milliseconds.TestEquals( expectedTime.Milliseconds ),
                result.Microseconds.TestEquals( expectedTime.Microseconds ),
                result.Ticks.TestEquals( expectedTime.Ticks ) )
            .Go();
    }

    [Fact]
    public void SubtractTicks_ShouldBeEquivalentToSetTicksWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetTicks( sut.Ticks - value );

        var result = sut.SubtractTicks( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractMicroseconds_ShouldBeEquivalentToSetMicrosecondsWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMicroseconds( sut.Microseconds - value );

        var result = sut.SubtractMicroseconds( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractMilliseconds_ShouldBeEquivalentToSetMillisecondsWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMilliseconds( sut.Milliseconds - value );

        var result = sut.SubtractMilliseconds( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractSeconds_ShouldBeEquivalentToSetSecondsWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetSeconds( sut.Seconds - value );

        var result = sut.SubtractSeconds( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractMinutes_ShouldBeEquivalentToSetMinutesWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMinutes( sut.Minutes - value );

        var result = sut.SubtractMinutes( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractHours_ShouldBeEquivalentToSetHoursWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetHours( sut.Hours - value );

        var result = sut.SubtractHours( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractDays_ShouldBeEquivalentToSetDaysWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetDays( sut.Days - value );

        var result = sut.SubtractDays( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractWeeks_ShouldBeEquivalentToSetWeeksWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetWeeks( sut.Weeks - value );

        var result = sut.SubtractWeeks( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractMonths_ShouldBeEquivalentToSetMonthsWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMonths( sut.Months - value );

        var result = sut.SubtractMonths( value );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractYears_ShouldBeEquivalentToSetYearsWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetYears( sut.Years - value );

        var result = sut.SubtractYears( value );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetSetData ) )]
    public void Set_ShouldReturnCorrectResult(
        (int Years, int Months, int Weeks, int Days) date1,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time1,
        (int Years, int Months, int Weeks, int Days) date2,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time2,
        PeriodUnits units,
        (int Years, int Months, int Weeks, int Days) expectedDate,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) expectedTime)
    {
        var sut = new Period(
            date1.Years,
            date1.Months,
            date1.Weeks,
            date1.Days,
            time1.Hours,
            time1.Minutes,
            time1.Seconds,
            time1.Milliseconds,
            time1.Microseconds,
            time1.Ticks );

        var other = new Period(
            date2.Years,
            date2.Months,
            date2.Weeks,
            date2.Days,
            time2.Hours,
            time2.Minutes,
            time2.Seconds,
            time2.Milliseconds,
            time2.Microseconds,
            time2.Ticks );

        var result = sut.Set( other, units );

        Assertion.All(
                result.Years.TestEquals( expectedDate.Years ),
                result.Months.TestEquals( expectedDate.Months ),
                result.Weeks.TestEquals( expectedDate.Weeks ),
                result.Days.TestEquals( expectedDate.Days ),
                result.Hours.TestEquals( expectedTime.Hours ),
                result.Minutes.TestEquals( expectedTime.Minutes ),
                result.Seconds.TestEquals( expectedTime.Seconds ),
                result.Milliseconds.TestEquals( expectedTime.Milliseconds ),
                result.Microseconds.TestEquals( expectedTime.Microseconds ),
                result.Ticks.TestEquals( expectedTime.Ticks ) )
            .Go();
    }

    [Fact]
    public void SetDate_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var years = Fixture.Create<int>();
        var months = Fixture.Create<int>();
        var weeks = Fixture.Create<int>();
        var days = Fixture.Create<int>();

        var result = sut.SetDate( years, months, weeks, days );

        Assertion.All(
                result.Years.TestEquals( years ),
                result.Months.TestEquals( months ),
                result.Weeks.TestEquals( weeks ),
                result.Days.TestEquals( days ),
                result.Hours.TestEquals( sut.Hours ),
                result.Minutes.TestEquals( sut.Minutes ),
                result.Seconds.TestEquals( sut.Seconds ),
                result.Milliseconds.TestEquals( sut.Milliseconds ),
                result.Microseconds.TestEquals( sut.Microseconds ),
                result.Ticks.TestEquals( sut.Ticks ) )
            .Go();
    }

    [Fact]
    public void SetTime_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var hours = Fixture.Create<int>();
        var minutes = Fixture.Create<int>();
        var seconds = Fixture.Create<int>();
        var milliseconds = Fixture.Create<int>();
        var microseconds = Fixture.Create<int>();
        var ticks = Fixture.Create<int>();

        var result = sut.SetTime( hours, minutes, seconds, milliseconds, microseconds, ticks );

        Assertion.All(
                result.Years.TestEquals( sut.Years ),
                result.Months.TestEquals( sut.Months ),
                result.Weeks.TestEquals( sut.Weeks ),
                result.Days.TestEquals( sut.Days ),
                result.Hours.TestEquals( hours ),
                result.Minutes.TestEquals( minutes ),
                result.Seconds.TestEquals( seconds ),
                result.Milliseconds.TestEquals( milliseconds ),
                result.Microseconds.TestEquals( microseconds ),
                result.Ticks.TestEquals( ticks ) )
            .Go();
    }

    [Fact]
    public void SetTicks_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetTicks( value );

        Assertion.All(
                result.Years.TestEquals( sut.Years ),
                result.Months.TestEquals( sut.Months ),
                result.Weeks.TestEquals( sut.Weeks ),
                result.Days.TestEquals( sut.Days ),
                result.Hours.TestEquals( sut.Hours ),
                result.Minutes.TestEquals( sut.Minutes ),
                result.Seconds.TestEquals( sut.Seconds ),
                result.Milliseconds.TestEquals( sut.Milliseconds ),
                result.Microseconds.TestEquals( sut.Microseconds ),
                result.Ticks.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void SetMilliseconds_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetMilliseconds( value );

        Assertion.All(
                result.Years.TestEquals( sut.Years ),
                result.Months.TestEquals( sut.Months ),
                result.Weeks.TestEquals( sut.Weeks ),
                result.Days.TestEquals( sut.Days ),
                result.Hours.TestEquals( sut.Hours ),
                result.Minutes.TestEquals( sut.Minutes ),
                result.Seconds.TestEquals( sut.Seconds ),
                result.Milliseconds.TestEquals( value ),
                result.Microseconds.TestEquals( sut.Microseconds ),
                result.Ticks.TestEquals( sut.Ticks ) )
            .Go();
    }

    [Fact]
    public void SetSeconds_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetSeconds( value );

        Assertion.All(
                result.Years.TestEquals( sut.Years ),
                result.Months.TestEquals( sut.Months ),
                result.Weeks.TestEquals( sut.Weeks ),
                result.Days.TestEquals( sut.Days ),
                result.Hours.TestEquals( sut.Hours ),
                result.Minutes.TestEquals( sut.Minutes ),
                result.Seconds.TestEquals( value ),
                result.Milliseconds.TestEquals( sut.Milliseconds ),
                result.Microseconds.TestEquals( sut.Microseconds ),
                result.Ticks.TestEquals( sut.Ticks ) )
            .Go();
    }

    [Fact]
    public void SetMinutes_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetMinutes( value );

        Assertion.All(
                result.Years.TestEquals( sut.Years ),
                result.Months.TestEquals( sut.Months ),
                result.Weeks.TestEquals( sut.Weeks ),
                result.Days.TestEquals( sut.Days ),
                result.Hours.TestEquals( sut.Hours ),
                result.Minutes.TestEquals( value ),
                result.Seconds.TestEquals( sut.Seconds ),
                result.Milliseconds.TestEquals( sut.Milliseconds ),
                result.Microseconds.TestEquals( sut.Microseconds ),
                result.Ticks.TestEquals( sut.Ticks ) )
            .Go();
    }

    [Fact]
    public void SetHours_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetHours( value );

        Assertion.All(
                result.Years.TestEquals( sut.Years ),
                result.Months.TestEquals( sut.Months ),
                result.Weeks.TestEquals( sut.Weeks ),
                result.Days.TestEquals( sut.Days ),
                result.Hours.TestEquals( value ),
                result.Minutes.TestEquals( sut.Minutes ),
                result.Seconds.TestEquals( sut.Seconds ),
                result.Milliseconds.TestEquals( sut.Milliseconds ),
                result.Microseconds.TestEquals( sut.Microseconds ),
                result.Ticks.TestEquals( sut.Ticks ) )
            .Go();
    }

    [Fact]
    public void SetDays_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetDays( value );

        Assertion.All(
                result.Years.TestEquals( sut.Years ),
                result.Months.TestEquals( sut.Months ),
                result.Weeks.TestEquals( sut.Weeks ),
                result.Days.TestEquals( value ),
                result.Hours.TestEquals( sut.Hours ),
                result.Minutes.TestEquals( sut.Minutes ),
                result.Seconds.TestEquals( sut.Seconds ),
                result.Milliseconds.TestEquals( sut.Milliseconds ),
                result.Microseconds.TestEquals( sut.Microseconds ),
                result.Ticks.TestEquals( sut.Ticks ) )
            .Go();
    }

    [Fact]
    public void SetWeeks_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetWeeks( value );

        Assertion.All(
                result.Years.TestEquals( sut.Years ),
                result.Months.TestEquals( sut.Months ),
                result.Weeks.TestEquals( value ),
                result.Days.TestEquals( sut.Days ),
                result.Hours.TestEquals( sut.Hours ),
                result.Minutes.TestEquals( sut.Minutes ),
                result.Seconds.TestEquals( sut.Seconds ),
                result.Milliseconds.TestEquals( sut.Milliseconds ),
                result.Microseconds.TestEquals( sut.Microseconds ),
                result.Ticks.TestEquals( sut.Ticks ) )
            .Go();
    }

    [Fact]
    public void SetMonths_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetMonths( value );

        Assertion.All(
                result.Years.TestEquals( sut.Years ),
                result.Months.TestEquals( value ),
                result.Weeks.TestEquals( sut.Weeks ),
                result.Days.TestEquals( sut.Days ),
                result.Hours.TestEquals( sut.Hours ),
                result.Minutes.TestEquals( sut.Minutes ),
                result.Seconds.TestEquals( sut.Seconds ),
                result.Milliseconds.TestEquals( sut.Milliseconds ),
                result.Microseconds.TestEquals( sut.Microseconds ),
                result.Ticks.TestEquals( sut.Ticks ) )
            .Go();
    }

    [Fact]
    public void SetYears_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetYears( value );

        Assertion.All(
                result.Years.TestEquals( value ),
                result.Months.TestEquals( sut.Months ),
                result.Weeks.TestEquals( sut.Weeks ),
                result.Days.TestEquals( sut.Days ),
                result.Hours.TestEquals( sut.Hours ),
                result.Minutes.TestEquals( sut.Minutes ),
                result.Seconds.TestEquals( sut.Seconds ),
                result.Milliseconds.TestEquals( sut.Milliseconds ),
                result.Microseconds.TestEquals( sut.Microseconds ),
                result.Ticks.TestEquals( sut.Ticks ) )
            .Go();
    }

    [Fact]
    public void Negate_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();

        var result = sut.Negate();

        Assertion.All(
                result.Years.TestEquals( -sut.Years ),
                result.Months.TestEquals( -sut.Months ),
                result.Weeks.TestEquals( -sut.Weeks ),
                result.Days.TestEquals( -sut.Days ),
                result.Hours.TestEquals( -sut.Hours ),
                result.Minutes.TestEquals( -sut.Minutes ),
                result.Seconds.TestEquals( -sut.Seconds ),
                result.Milliseconds.TestEquals( -sut.Milliseconds ),
                result.Microseconds.TestEquals( -sut.Microseconds ),
                result.Ticks.TestEquals( -sut.Ticks ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetAbsData ) )]
    public void Abs_ShouldReturnCorrectResult(
        (int Years, int Months, int Weeks, int Days) date,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time,
        (int Years, int Months, int Weeks, int Days) expectedDate,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) expectedTime)
    {
        var sut = new Period(
            date.Years,
            date.Months,
            date.Weeks,
            date.Days,
            time.Hours,
            time.Minutes,
            time.Seconds,
            time.Milliseconds,
            time.Microseconds,
            time.Ticks );

        var result = sut.Abs();

        Assertion.All(
                result.Years.TestEquals( expectedDate.Years ),
                result.Months.TestEquals( expectedDate.Months ),
                result.Weeks.TestEquals( expectedDate.Weeks ),
                result.Days.TestEquals( expectedDate.Days ),
                result.Hours.TestEquals( expectedTime.Hours ),
                result.Minutes.TestEquals( expectedTime.Minutes ),
                result.Seconds.TestEquals( expectedTime.Seconds ),
                result.Milliseconds.TestEquals( expectedTime.Milliseconds ),
                result.Microseconds.TestEquals( expectedTime.Microseconds ),
                result.Ticks.TestEquals( expectedTime.Ticks ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetSkipData ) )]
    public void Skip_ShouldResetProvidedUnitsToZero(
        (int Years, int Months, int Weeks, int Days) date,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time,
        PeriodUnits units,
        (int Years, int Months, int Weeks, int Days) expectedDate,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) expectedTime)
    {
        var sut = new Period(
            date.Years,
            date.Months,
            date.Weeks,
            date.Days,
            time.Hours,
            time.Minutes,
            time.Seconds,
            time.Milliseconds,
            time.Microseconds,
            time.Ticks );

        var result = sut.Skip( units );

        Assertion.All(
                result.Years.TestEquals( expectedDate.Years ),
                result.Months.TestEquals( expectedDate.Months ),
                result.Weeks.TestEquals( expectedDate.Weeks ),
                result.Days.TestEquals( expectedDate.Days ),
                result.Hours.TestEquals( expectedTime.Hours ),
                result.Minutes.TestEquals( expectedTime.Minutes ),
                result.Seconds.TestEquals( expectedTime.Seconds ),
                result.Milliseconds.TestEquals( expectedTime.Milliseconds ),
                result.Microseconds.TestEquals( expectedTime.Microseconds ),
                result.Ticks.TestEquals( expectedTime.Ticks ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetTakeData ) )]
    public void Take_ShouldOnlyReturnProvidedUnits(
        (int Years, int Months, int Weeks, int Days) date,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time,
        PeriodUnits units,
        (int Years, int Months, int Weeks, int Days) expectedDate,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) expectedTime)
    {
        var sut = new Period(
            date.Years,
            date.Months,
            date.Weeks,
            date.Days,
            time.Hours,
            time.Minutes,
            time.Seconds,
            time.Milliseconds,
            time.Microseconds,
            time.Ticks );

        var result = sut.Take( units );

        Assertion.All(
                result.Years.TestEquals( expectedDate.Years ),
                result.Months.TestEquals( expectedDate.Months ),
                result.Weeks.TestEquals( expectedDate.Weeks ),
                result.Days.TestEquals( expectedDate.Days ),
                result.Hours.TestEquals( expectedTime.Hours ),
                result.Minutes.TestEquals( expectedTime.Minutes ),
                result.Seconds.TestEquals( expectedTime.Seconds ),
                result.Milliseconds.TestEquals( expectedTime.Milliseconds ),
                result.Microseconds.TestEquals( expectedTime.Microseconds ),
                result.Ticks.TestEquals( expectedTime.Ticks ) )
            .Go();
    }

    [Fact]
    public void NegateOperator_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var expected = sut.Negate();

        var result = -sut;

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void AddOperator_ShouldReturnCorrectResult()
    {
        var a = CreatePeriod();
        var b = CreatePeriod();
        var expected = a.Add( b );

        var result = a + b;

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractOperator_ShouldReturnCorrectResult()
    {
        var a = CreatePeriod();
        var b = CreatePeriod();
        var expected = a.Subtract( b );

        var result = a - b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(
        (int Years, int Months, int Weeks, int Days) date1,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time1,
        (int Years, int Months, int Weeks, int Days) date2,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time2,
        bool expected)
    {
        var a = new Period(
            date1.Years,
            date1.Months,
            date1.Weeks,
            date1.Days,
            time1.Hours,
            time1.Minutes,
            time1.Seconds,
            time1.Milliseconds,
            time1.Microseconds,
            time1.Ticks );

        var b = new Period(
            date2.Years,
            date2.Months,
            date2.Weeks,
            date2.Days,
            time2.Hours,
            time2.Minutes,
            time2.Seconds,
            time2.Milliseconds,
            time2.Microseconds,
            time2.Ticks );

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( PeriodTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(
        (int Years, int Months, int Weeks, int Days) date1,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time1,
        (int Years, int Months, int Weeks, int Days) date2,
        (int Hours, int Minutes, int Seconds, int Milliseconds, int Microseconds, int Ticks) time2,
        bool expected)
    {
        var a = new Period(
            date1.Years,
            date1.Months,
            date1.Weeks,
            date1.Days,
            time1.Hours,
            time1.Minutes,
            time1.Seconds,
            time1.Milliseconds,
            time1.Microseconds,
            time1.Ticks );

        var b = new Period(
            date2.Years,
            date2.Months,
            date2.Weeks,
            date2.Days,
            time2.Hours,
            time2.Minutes,
            time2.Seconds,
            time2.Milliseconds,
            time2.Microseconds,
            time2.Ticks );

        var result = a != b;

        result.TestEquals( expected ).Go();
    }

    [Pure]
    private Period CreatePeriod()
    {
        var years = Fixture.Create<int>();
        var months = Fixture.Create<int>();
        var weeks = Fixture.Create<int>();
        var days = Fixture.Create<int>();
        var hours = Fixture.Create<int>();
        var minutes = Fixture.Create<int>();
        var seconds = Fixture.Create<int>();
        var milliseconds = Fixture.Create<int>();
        var microseconds = Fixture.Create<int>();
        var ticks = Fixture.Create<int>();
        return new Period( years, months, weeks, days, hours, minutes, seconds, milliseconds, microseconds, ticks );
    }
}
