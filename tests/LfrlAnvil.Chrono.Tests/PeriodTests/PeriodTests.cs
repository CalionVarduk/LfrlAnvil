﻿using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Chrono.Tests.PeriodTests;

[TestClass( typeof( PeriodTestsData ) )]
public class PeriodTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnEmptyPeriod()
    {
        var sut = default( Period );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.None );
        }
    }

    [Fact]
    public void Empty_ShouldReturnEmptyPeriod()
    {
        var sut = Period.Empty;

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.None );
        }
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

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( years );
            sut.Months.Should().Be( months );
            sut.Weeks.Should().Be( weeks );
            sut.Days.Should().Be( days );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( expectedUnits );
        }
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

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( hours );
            sut.Minutes.Should().Be( minutes );
            sut.Seconds.Should().Be( seconds );
            sut.Milliseconds.Should().Be( milliseconds );
            sut.Microseconds.Should().Be( microseconds );
            sut.Ticks.Should().Be( ticks );
            sut.ActiveUnits.Should().Be( expectedUnits );
        }
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

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( date.Years );
            sut.Months.Should().Be( date.Months );
            sut.Weeks.Should().Be( date.Weeks );
            sut.Days.Should().Be( date.Days );
            sut.Hours.Should().Be( time.Hours );
            sut.Minutes.Should().Be( time.Minutes );
            sut.Seconds.Should().Be( time.Seconds );
            sut.Milliseconds.Should().Be( time.Milliseconds );
            sut.Microseconds.Should().Be( time.Microseconds );
            sut.Ticks.Should().Be( time.Ticks );
            sut.ActiveUnits.Should().Be( expectedUnits );
        }
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

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( expectedDays );
            sut.Hours.Should().Be( expectedHours );
            sut.Minutes.Should().Be( expectedMinutes );
            sut.Seconds.Should().Be( expectedSeconds );
            sut.Milliseconds.Should().Be( expectedMilliseconds );
            sut.Microseconds.Should().Be( expectedMicroseconds );
            sut.Ticks.Should().Be( expectedTicks );
            sut.ActiveUnits.Should().Be( expectedUnits );
        }
    }

    [Fact]
    public void FromTicks_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromTicks( value );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( value );
            sut.ActiveUnits.Should().Be( PeriodUnits.Ticks );
        }
    }

    [Fact]
    public void FromMicroseconds_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromMicroseconds( value );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( value );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.Microseconds );
        }
    }

    [Fact]
    public void FromMilliseconds_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromMilliseconds( value );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( value );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.Milliseconds );
        }
    }

    [Fact]
    public void FromSeconds_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromSeconds( value );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( value );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.Seconds );
        }
    }

    [Fact]
    public void FromMinutes_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromMinutes( value );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( value );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.Minutes );
        }
    }

    [Fact]
    public void FromHours_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromHours( value );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( value );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.Hours );
        }
    }

    [Fact]
    public void FromDays_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromDays( value );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( value );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.Days );
        }
    }

    [Fact]
    public void FromWeeks_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromWeeks( value );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( value );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.Weeks );
        }
    }

    [Fact]
    public void FromMonths_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromMonths( value );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( 0 );
            sut.Months.Should().Be( value );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.Months );
        }
    }

    [Fact]
    public void FromYears_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Period.FromYears( value );

        using ( new AssertionScope() )
        {
            sut.Years.Should().Be( value );
            sut.Months.Should().Be( 0 );
            sut.Weeks.Should().Be( 0 );
            sut.Days.Should().Be( 0 );
            sut.Hours.Should().Be( 0 );
            sut.Minutes.Should().Be( 0 );
            sut.Seconds.Should().Be( 0 );
            sut.Milliseconds.Should().Be( 0 );
            sut.Microseconds.Should().Be( 0 );
            sut.Ticks.Should().Be( 0 );
            sut.ActiveUnits.Should().Be( PeriodUnits.Years );
        }
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

        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var expected = Hash.Default
            .Add( sut.Years )
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

        result.Should().Be( expected );
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

        result.Should().Be( expected );
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

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( expectedDate.Years );
            result.Months.Should().Be( expectedDate.Months );
            result.Weeks.Should().Be( expectedDate.Weeks );
            result.Days.Should().Be( expectedDate.Days );
            result.Hours.Should().Be( expectedTime.Hours );
            result.Minutes.Should().Be( expectedTime.Minutes );
            result.Seconds.Should().Be( expectedTime.Seconds );
            result.Milliseconds.Should().Be( expectedTime.Milliseconds );
            result.Microseconds.Should().Be( expectedTime.Microseconds );
            result.Ticks.Should().Be( expectedTime.Ticks );
        }
    }

    [Fact]
    public void AddTicks_ShouldEquivalentToSetTicksWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetTicks( sut.Ticks + value );

        var result = sut.AddTicks( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void AddMicroseconds_ShouldBeEquivalentToSetMicrosecondsWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMicroseconds( sut.Microseconds + value );

        var result = sut.AddMicroseconds( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void AddMilliseconds_ShouldBeEquivalentToSetMillisecondsWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMilliseconds( sut.Milliseconds + value );

        var result = sut.AddMilliseconds( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void AddSeconds_ShouldBeEquivalentToSetSecondsWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetSeconds( sut.Seconds + value );

        var result = sut.AddSeconds( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void AddMinutes_ShouldBeEquivalentToSetMinutesWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMinutes( sut.Minutes + value );

        var result = sut.AddMinutes( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void AddHours_ShouldBeEquivalentToSetHoursWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetHours( sut.Hours + value );

        var result = sut.AddHours( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void AddDays_ShouldBeEquivalentToSetDaysWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetDays( sut.Days + value );

        var result = sut.AddDays( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void AddWeeks_ShouldBeEquivalentToSetWeeksWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetWeeks( sut.Weeks + value );

        var result = sut.AddWeeks( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void AddMonths_ShouldBeEquivalentToSetMonthsWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMonths( sut.Months + value );

        var result = sut.AddMonths( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void AddYears_ShouldBeEquivalentToSetYearsWithSum()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetYears( sut.Years + value );

        var result = sut.AddYears( value );

        result.Should().BeEquivalentTo( expected );
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

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( expectedDate.Years );
            result.Months.Should().Be( expectedDate.Months );
            result.Weeks.Should().Be( expectedDate.Weeks );
            result.Days.Should().Be( expectedDate.Days );
            result.Hours.Should().Be( expectedTime.Hours );
            result.Minutes.Should().Be( expectedTime.Minutes );
            result.Seconds.Should().Be( expectedTime.Seconds );
            result.Milliseconds.Should().Be( expectedTime.Milliseconds );
            result.Microseconds.Should().Be( expectedTime.Microseconds );
            result.Ticks.Should().Be( expectedTime.Ticks );
        }
    }

    [Fact]
    public void SubtractTicks_ShouldBeEquivalentToSetTicksWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetTicks( sut.Ticks - value );

        var result = sut.SubtractTicks( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractMicroseconds_ShouldBeEquivalentToSetMicrosecondsWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMicroseconds( sut.Microseconds - value );

        var result = sut.SubtractMicroseconds( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractMilliseconds_ShouldBeEquivalentToSetMillisecondsWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMilliseconds( sut.Milliseconds - value );

        var result = sut.SubtractMilliseconds( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractSeconds_ShouldBeEquivalentToSetSecondsWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetSeconds( sut.Seconds - value );

        var result = sut.SubtractSeconds( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractMinutes_ShouldBeEquivalentToSetMinutesWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMinutes( sut.Minutes - value );

        var result = sut.SubtractMinutes( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractHours_ShouldBeEquivalentToSetHoursWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetHours( sut.Hours - value );

        var result = sut.SubtractHours( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractDays_ShouldBeEquivalentToSetDaysWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetDays( sut.Days - value );

        var result = sut.SubtractDays( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractWeeks_ShouldBeEquivalentToSetWeeksWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetWeeks( sut.Weeks - value );

        var result = sut.SubtractWeeks( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractMonths_ShouldBeEquivalentToSetMonthsWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetMonths( sut.Months - value );

        var result = sut.SubtractMonths( value );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractYears_ShouldBeEquivalentToSetYearsWithDifference()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();
        var expected = sut.SetYears( sut.Years - value );

        var result = sut.SubtractYears( value );

        result.Should().BeEquivalentTo( expected );
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

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( expectedDate.Years );
            result.Months.Should().Be( expectedDate.Months );
            result.Weeks.Should().Be( expectedDate.Weeks );
            result.Days.Should().Be( expectedDate.Days );
            result.Hours.Should().Be( expectedTime.Hours );
            result.Minutes.Should().Be( expectedTime.Minutes );
            result.Seconds.Should().Be( expectedTime.Seconds );
            result.Milliseconds.Should().Be( expectedTime.Milliseconds );
            result.Microseconds.Should().Be( expectedTime.Microseconds );
            result.Ticks.Should().Be( expectedTime.Ticks );
        }
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

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( years );
            result.Months.Should().Be( months );
            result.Weeks.Should().Be( weeks );
            result.Days.Should().Be( days );
            result.Hours.Should().Be( sut.Hours );
            result.Minutes.Should().Be( sut.Minutes );
            result.Seconds.Should().Be( sut.Seconds );
            result.Milliseconds.Should().Be( sut.Milliseconds );
            result.Microseconds.Should().Be( sut.Microseconds );
            result.Ticks.Should().Be( sut.Ticks );
        }
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

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( sut.Years );
            result.Months.Should().Be( sut.Months );
            result.Weeks.Should().Be( sut.Weeks );
            result.Days.Should().Be( sut.Days );
            result.Hours.Should().Be( hours );
            result.Minutes.Should().Be( minutes );
            result.Seconds.Should().Be( seconds );
            result.Milliseconds.Should().Be( milliseconds );
            result.Microseconds.Should().Be( microseconds );
            result.Ticks.Should().Be( ticks );
        }
    }

    [Fact]
    public void SetTicks_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetTicks( value );

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( sut.Years );
            result.Months.Should().Be( sut.Months );
            result.Weeks.Should().Be( sut.Weeks );
            result.Days.Should().Be( sut.Days );
            result.Hours.Should().Be( sut.Hours );
            result.Minutes.Should().Be( sut.Minutes );
            result.Seconds.Should().Be( sut.Seconds );
            result.Milliseconds.Should().Be( sut.Milliseconds );
            result.Microseconds.Should().Be( sut.Microseconds );
            result.Ticks.Should().Be( value );
        }
    }

    [Fact]
    public void SetMilliseconds_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetMilliseconds( value );

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( sut.Years );
            result.Months.Should().Be( sut.Months );
            result.Weeks.Should().Be( sut.Weeks );
            result.Days.Should().Be( sut.Days );
            result.Hours.Should().Be( sut.Hours );
            result.Minutes.Should().Be( sut.Minutes );
            result.Seconds.Should().Be( sut.Seconds );
            result.Milliseconds.Should().Be( value );
            result.Microseconds.Should().Be( sut.Microseconds );
            result.Ticks.Should().Be( sut.Ticks );
        }
    }

    [Fact]
    public void SetSeconds_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetSeconds( value );

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( sut.Years );
            result.Months.Should().Be( sut.Months );
            result.Weeks.Should().Be( sut.Weeks );
            result.Days.Should().Be( sut.Days );
            result.Hours.Should().Be( sut.Hours );
            result.Minutes.Should().Be( sut.Minutes );
            result.Seconds.Should().Be( value );
            result.Milliseconds.Should().Be( sut.Milliseconds );
            result.Microseconds.Should().Be( sut.Microseconds );
            result.Ticks.Should().Be( sut.Ticks );
        }
    }

    [Fact]
    public void SetMinutes_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetMinutes( value );

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( sut.Years );
            result.Months.Should().Be( sut.Months );
            result.Weeks.Should().Be( sut.Weeks );
            result.Days.Should().Be( sut.Days );
            result.Hours.Should().Be( sut.Hours );
            result.Minutes.Should().Be( value );
            result.Seconds.Should().Be( sut.Seconds );
            result.Milliseconds.Should().Be( sut.Milliseconds );
            result.Microseconds.Should().Be( sut.Microseconds );
            result.Ticks.Should().Be( sut.Ticks );
        }
    }

    [Fact]
    public void SetHours_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetHours( value );

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( sut.Years );
            result.Months.Should().Be( sut.Months );
            result.Weeks.Should().Be( sut.Weeks );
            result.Days.Should().Be( sut.Days );
            result.Hours.Should().Be( value );
            result.Minutes.Should().Be( sut.Minutes );
            result.Seconds.Should().Be( sut.Seconds );
            result.Milliseconds.Should().Be( sut.Milliseconds );
            result.Microseconds.Should().Be( sut.Microseconds );
            result.Ticks.Should().Be( sut.Ticks );
        }
    }

    [Fact]
    public void SetDays_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetDays( value );

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( sut.Years );
            result.Months.Should().Be( sut.Months );
            result.Weeks.Should().Be( sut.Weeks );
            result.Days.Should().Be( value );
            result.Hours.Should().Be( sut.Hours );
            result.Minutes.Should().Be( sut.Minutes );
            result.Seconds.Should().Be( sut.Seconds );
            result.Milliseconds.Should().Be( sut.Milliseconds );
            result.Microseconds.Should().Be( sut.Microseconds );
            result.Ticks.Should().Be( sut.Ticks );
        }
    }

    [Fact]
    public void SetWeeks_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetWeeks( value );

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( sut.Years );
            result.Months.Should().Be( sut.Months );
            result.Weeks.Should().Be( value );
            result.Days.Should().Be( sut.Days );
            result.Hours.Should().Be( sut.Hours );
            result.Minutes.Should().Be( sut.Minutes );
            result.Seconds.Should().Be( sut.Seconds );
            result.Milliseconds.Should().Be( sut.Milliseconds );
            result.Microseconds.Should().Be( sut.Microseconds );
            result.Ticks.Should().Be( sut.Ticks );
        }
    }

    [Fact]
    public void SetMonths_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetMonths( value );

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( sut.Years );
            result.Months.Should().Be( value );
            result.Weeks.Should().Be( sut.Weeks );
            result.Days.Should().Be( sut.Days );
            result.Hours.Should().Be( sut.Hours );
            result.Minutes.Should().Be( sut.Minutes );
            result.Seconds.Should().Be( sut.Seconds );
            result.Milliseconds.Should().Be( sut.Milliseconds );
            result.Microseconds.Should().Be( sut.Microseconds );
            result.Ticks.Should().Be( sut.Ticks );
        }
    }

    [Fact]
    public void SetYears_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var value = Fixture.Create<int>();

        var result = sut.SetYears( value );

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( value );
            result.Months.Should().Be( sut.Months );
            result.Weeks.Should().Be( sut.Weeks );
            result.Days.Should().Be( sut.Days );
            result.Hours.Should().Be( sut.Hours );
            result.Minutes.Should().Be( sut.Minutes );
            result.Seconds.Should().Be( sut.Seconds );
            result.Milliseconds.Should().Be( sut.Milliseconds );
            result.Microseconds.Should().Be( sut.Microseconds );
            result.Ticks.Should().Be( sut.Ticks );
        }
    }

    [Fact]
    public void Negate_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();

        var result = sut.Negate();

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( -sut.Years );
            result.Months.Should().Be( -sut.Months );
            result.Weeks.Should().Be( -sut.Weeks );
            result.Days.Should().Be( -sut.Days );
            result.Hours.Should().Be( -sut.Hours );
            result.Minutes.Should().Be( -sut.Minutes );
            result.Seconds.Should().Be( -sut.Seconds );
            result.Milliseconds.Should().Be( -sut.Milliseconds );
            result.Microseconds.Should().Be( -sut.Microseconds );
            result.Ticks.Should().Be( -sut.Ticks );
        }
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

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( expectedDate.Years );
            result.Months.Should().Be( expectedDate.Months );
            result.Weeks.Should().Be( expectedDate.Weeks );
            result.Days.Should().Be( expectedDate.Days );
            result.Hours.Should().Be( expectedTime.Hours );
            result.Minutes.Should().Be( expectedTime.Minutes );
            result.Seconds.Should().Be( expectedTime.Seconds );
            result.Milliseconds.Should().Be( expectedTime.Milliseconds );
            result.Microseconds.Should().Be( expectedTime.Microseconds );
            result.Ticks.Should().Be( expectedTime.Ticks );
        }
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

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( expectedDate.Years );
            result.Months.Should().Be( expectedDate.Months );
            result.Weeks.Should().Be( expectedDate.Weeks );
            result.Days.Should().Be( expectedDate.Days );
            result.Hours.Should().Be( expectedTime.Hours );
            result.Minutes.Should().Be( expectedTime.Minutes );
            result.Seconds.Should().Be( expectedTime.Seconds );
            result.Milliseconds.Should().Be( expectedTime.Milliseconds );
            result.Microseconds.Should().Be( expectedTime.Microseconds );
            result.Ticks.Should().Be( expectedTime.Ticks );
        }
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

        using ( new AssertionScope() )
        {
            result.Years.Should().Be( expectedDate.Years );
            result.Months.Should().Be( expectedDate.Months );
            result.Weeks.Should().Be( expectedDate.Weeks );
            result.Days.Should().Be( expectedDate.Days );
            result.Hours.Should().Be( expectedTime.Hours );
            result.Minutes.Should().Be( expectedTime.Minutes );
            result.Seconds.Should().Be( expectedTime.Seconds );
            result.Milliseconds.Should().Be( expectedTime.Milliseconds );
            result.Microseconds.Should().Be( expectedTime.Microseconds );
            result.Ticks.Should().Be( expectedTime.Ticks );
        }
    }

    [Fact]
    public void NegateOperator_ShouldReturnCorrectResult()
    {
        var sut = CreatePeriod();
        var expected = sut.Negate();

        var result = -sut;

        result.Should().Be( expected );
    }

    [Fact]
    public void AddOperator_ShouldReturnCorrectResult()
    {
        var a = CreatePeriod();
        var b = CreatePeriod();
        var expected = a.Add( b );

        var result = a + b;

        result.Should().Be( expected );
    }

    [Fact]
    public void SubtractOperator_ShouldReturnCorrectResult()
    {
        var a = CreatePeriod();
        var b = CreatePeriod();
        var expected = a.Subtract( b );

        var result = a - b;

        result.Should().Be( expected );
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

        result.Should().Be( expected );
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

        result.Should().Be( expected );
    }

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
