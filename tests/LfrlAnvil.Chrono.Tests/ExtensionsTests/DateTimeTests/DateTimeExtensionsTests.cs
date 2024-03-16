using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.DateTimeTests;

[TestClass( typeof( DateTimeExtensionsTestsData ) )]
public class DateTimeExtensionsTests : TestsBase
{
    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetMonthOfYearData ) )]
    public void GetMonthOfYear_ShouldReturnCorrectResult(int month, IsoMonthOfYear expected)
    {
        var value = new DateTime( 2021, month, 1 );
        var result = value.GetMonthOfYear();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetDayOfWeek_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<DateTime>();
        var result = value.GetDayOfWeek();
        result.Should().Be( value.DayOfWeek.ToIso() );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetStartOfDayData ) )]
    public void GetStartOfDay_ShouldReturnTargetWithNoTimeOfDay(DateTime value, DateTime expected)
    {
        var result = value.GetStartOfDay();
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetEndOfDayData ) )]
    public void GetEndOfDay_ShouldReturnTargetWithTimeOfDaySetToLastPossibleTick(DateTime value, DateTime expected)
    {
        var result = value.GetEndOfDay();
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetStartOfWeekData ) )]
    public void GetStartOfWeek_ShouldReturnStartOfFirstDayInWeek(
        DateTime value,
        DayOfWeek weekStart,
        DateTime expected)
    {
        var result = value.GetStartOfWeek( weekStart );
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetEndOfWeekData ) )]
    public void GetEndOfWeek_ShouldReturnEndOfLastDayInWeek(DateTime value, DayOfWeek weekStart, DateTime expected)
    {
        var result = value.GetEndOfWeek( weekStart );
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetStartOfMonthData ) )]
    public void GetStartOfMonth_ShouldReturnStartOfFirstDayInMonth(DateTime value, DateTime expected)
    {
        var result = value.GetStartOfMonth();
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetEndOfMonthData ) )]
    public void GetEndOfMonth_ShouldReturnEndOfLastDayInMonth(DateTime value, DateTime expected)
    {
        var result = value.GetEndOfMonth();
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetStartOfYearData ) )]
    public void GetStartOfYear_ShouldReturnStartOfFirstDayInYear(DateTime value, DateTime expected)
    {
        var result = value.GetStartOfYear();
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetEndOfYearData ) )]
    public void GetEndOfYear_ShouldReturnEndOfLastDayInYear(DateTime value, DateTime expected)
    {
        var result = value.GetEndOfYear();
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetAddData ) )]
    public void Add_ShouldReturnCorrectResult(DateTime value, Period period, DateTime expected)
    {
        var result = value.Add( period );
        result.Should().Be( expected );
    }

    [Fact]
    public void Subtract_ShouldReturnCorrectResult()
    {
        var sut = Fixture.Create<DateTime>();
        var periodToSubtract = new Period(
            years: Fixture.Create<sbyte>(),
            months: Fixture.Create<sbyte>(),
            weeks: Fixture.Create<short>(),
            days: Fixture.Create<short>(),
            hours: Fixture.Create<short>(),
            minutes: Fixture.Create<short>(),
            seconds: Fixture.Create<short>(),
            milliseconds: Fixture.Create<short>(),
            microseconds: Fixture.Create<short>(),
            ticks: Fixture.Create<short>() );

        var expected = sut.Add( -periodToSubtract );

        var result = sut.Subtract( periodToSubtract );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetSetYearData ) )]
    public void SetYear_ShouldReturnTargetWithChangedYear(DateTime value, int year, DateTime expected)
    {
        var result = value.SetYear( year );
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetSetYearThrowData ) )]
    public void SetYear_ShouldThrowArgumentOutOfRangeException_WhenYearIsInvalid(DateTime value, int year)
    {
        var action = Lambda.Of( () => value.SetYear( year ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetSetMonthData ) )]
    public void SetMonth_ShouldReturnTargetWithChangedMonth(DateTime value, IsoMonthOfYear month, DateTime expected)
    {
        var result = value.SetMonth( month );
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetSetDayOfMonthData ) )]
    public void SetDayOfMonth_ShouldReturnTargetWithChangedDayOfMonth(DateTime value, int day, DateTime expected)
    {
        var result = value.SetDayOfMonth( day );
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetSetDayOfMonthThrowData ) )]
    public void SetDayOfMonth_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(DateTime value, int day)
    {
        var action = Lambda.Of( () => value.SetDayOfMonth( day ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetSetDayOfYearData ) )]
    public void SetDayOfYear_ShouldReturnTargetWithChangedDayOfYear(DateTime value, int day, DateTime expected)
    {
        var result = value.SetDayOfYear( day );
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetSetDayOfYearThrowData ) )]
    public void SetDayOfYear_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(DateTime value, int day)
    {
        var action = Lambda.Of( () => value.SetDayOfYear( day ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetSetTimeOfDayData ) )]
    public void SetTimeOfDay_ShouldReturnTargetWithChangedTimeOfDay(
        DateTime value,
        TimeOfDay timeOfDay,
        DateTime expected)
    {
        var result = value.SetTimeOfDay( timeOfDay );
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetPeriodOffsetData ) )]
    public void GetPeriodOffset_ShouldReturnCorrectResult(
        DateTime end,
        DateTime start,
        PeriodUnits units,
        Period expected)
    {
        var result = end.GetPeriodOffset( start, units );
        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetPeriodOffsetData ) )]
    public void GetPeriodOffset_WithStartGreaterThanEnd_ShouldReturnCorrectResult(
        DateTime start,
        DateTime end,
        PeriodUnits units,
        Period expected)
    {
        var result = end.GetPeriodOffset( start, units );
        result.Should().BeEquivalentTo( -expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetGreedyPeriodOffsetData ) )]
    public void GetGreedyPeriodOffset_ShouldReturnCorrectResult(
        DateTime end,
        DateTime start,
        PeriodUnits units,
        Period expected)
    {
        var result = end.GetGreedyPeriodOffset( start, units );
        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( DateTimeExtensionsTestsData.GetGetGreedyPeriodOffsetData ) )]
    public void GetGreedyPeriodOffset_WithStartGreaterThanEnd_ShouldReturnCorrectResult(
        DateTime start,
        DateTime end,
        PeriodUnits units,
        Period expected)
    {
        var result = end.GetGreedyPeriodOffset( start, units );
        result.Should().BeEquivalentTo( -expected );
    }
}
