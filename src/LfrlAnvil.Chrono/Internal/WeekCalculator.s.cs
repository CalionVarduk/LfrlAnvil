using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Extensions;

namespace LfrlAnvil.Chrono.Internal;

internal static class WeekCalculator
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static int GetDayOfJanuaryInFirstWeekOfYear(DayOfWeek weekStart)
    {
        return weekStart == DayOfWeek.Monday ? 4 : 1;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DateTime GetDayInFirstWeekOfYear(int year, DayOfWeek weekStart)
    {
        var dayOfJanuary = GetDayOfJanuaryInFirstWeekOfYear( weekStart );
        return new DateTime( year, ( int )IsoMonthOfYear.January, dayOfJanuary );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static int GetDifferenceInWeeks(DateTime start, DateTime end)
    {
        return (end - start).Days / ChronoConstants.DaysPerWeek + 1;
    }

    [Pure]
    internal static int GetWeekCountInYear(int year, DayOfWeek weekStart)
    {
        var dayInFirstWeekOfYear = GetDayInFirstWeekOfYear( year, weekStart );
        var startOfFirstWeekInYear = dayInFirstWeekOfYear.GetStartOfWeek( weekStart );

        var dayInFirstWeekOfNextYear = dayInFirstWeekOfYear.AddMonths( ChronoConstants.MonthsPerYear );
        var startOfFirstWeekInNextYear = dayInFirstWeekOfNextYear.GetStartOfWeek( weekStart );

        var result = GetDifferenceInWeeks( startOfFirstWeekInYear, startOfFirstWeekInNextYear );
        return result - 1;
    }

    [Pure]
    internal static int GetWeekCountInMonth(DateTime firstDayOfMonth, DateTime lastDayOfMonth, DayOfWeek weekStart)
    {
        var startOfFirstWeekInMonth = firstDayOfMonth.GetStartOfWeek( weekStart );
        var startOfLastWeekInMonth = lastDayOfMonth.GetStartOfWeek( weekStart );

        var result = GetDifferenceInWeeks( startOfFirstWeekInMonth, startOfLastWeekInMonth );
        return result;
    }

    [Pure]
    internal static int GetYearInWeekFormat(DateTime startOfWeek)
    {
        startOfWeek = startOfWeek.Date;
        var year = startOfWeek.Year;
        var weekStart = startOfWeek.DayOfWeek;

        var dayInFirstWeekOfNextYear = GetDayInFirstWeekOfYear( year + 1, weekStart );
        var startOfFirstWeekInNextYear = dayInFirstWeekOfNextYear.GetStartOfWeek( weekStart );

        return startOfFirstWeekInNextYear == startOfWeek ? year + 1 : year;
    }

    [Pure]
    internal static int GetWeekOfYear(DateTime startOfWeek)
    {
        startOfWeek = startOfWeek.Date;
        var year = startOfWeek.Year;
        var weekStart = startOfWeek.DayOfWeek;

        var dayInFirstWeekOfYear = GetDayInFirstWeekOfYear( year, weekStart );

        var dayInFirstWeekOfNextYear = dayInFirstWeekOfYear.AddMonths( ChronoConstants.MonthsPerYear );
        var startOfFirstWeekInNextYear = dayInFirstWeekOfNextYear.GetStartOfWeek( weekStart );

        if ( startOfFirstWeekInNextYear == startOfWeek )
            return 1;

        var startOfFirstWeekInYear = dayInFirstWeekOfYear.GetStartOfWeek( weekStart );
        var weekValue = GetDifferenceInWeeks( startOfFirstWeekInYear, startOfWeek );

        return weekValue;
    }

    [Pure]
    internal static (int Year, int Week) GetYearAndWeekOfYear(DateTime startOfWeek)
    {
        startOfWeek = startOfWeek.Date;
        var year = startOfWeek.Year;
        var weekStart = startOfWeek.DayOfWeek;

        var dayInFirstWeekOfYear = GetDayInFirstWeekOfYear( year, weekStart );

        var dayInFirstWeekOfNextYear = dayInFirstWeekOfYear.AddMonths( ChronoConstants.MonthsPerYear );
        var startOfFirstWeekInNextYear = dayInFirstWeekOfNextYear.GetStartOfWeek( weekStart );

        if ( startOfFirstWeekInNextYear == startOfWeek )
            return (year + 1, 1);

        var startOfFirstWeekInYear = dayInFirstWeekOfYear.GetStartOfWeek( weekStart );
        var weekValue = GetDifferenceInWeeks( startOfFirstWeekInYear, startOfWeek );

        return (year, weekValue);
    }
}
