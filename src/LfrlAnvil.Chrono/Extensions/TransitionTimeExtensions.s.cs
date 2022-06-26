using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions;

public static class TransitionTimeExtensions
{
    [Pure]
    public static DateTime ToDateTime(this TimeZoneInfo.TransitionTime transitionTime, int year)
    {
        return transitionTime.IsFixedDateRule
            ? FixedTimeToDateTime( transitionTime, year )
            : FloatingTimeToDateTime( transitionTime, year );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DateTime FixedTimeToDateTime(TimeZoneInfo.TransitionTime transitionTime, int year)
    {
        var daysInMonth = DateTime.DaysInMonth( year, transitionTime.Month );
        var day = Math.Min( transitionTime.Day, daysInMonth );
        var result = new DateTime( year, transitionTime.Month, day ) + transitionTime.TimeOfDay.TimeOfDay;
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DateTime FloatingTimeToDateTime(TimeZoneInfo.TransitionTime transitionTime, int year)
    {
        return transitionTime.Week <= 4
            ? FloatingTimeFromStartOfMonthToDateTime( transitionTime, year )
            : FloatingTimeFromEndOfMonthToDateTime( transitionTime, year );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DateTime FloatingTimeFromStartOfMonthToDateTime(TimeZoneInfo.TransitionTime transitionTime, int year)
    {
        var result = new DateTime( year, transitionTime.Month, 1 ) + transitionTime.TimeOfDay.TimeOfDay;

        var daysToAdd = (int)transitionTime.DayOfWeek - (int)result.DayOfWeek;
        if ( daysToAdd < 0 )
            daysToAdd += ChronoConstants.DaysPerWeek;

        daysToAdd += ChronoConstants.DaysPerWeek * (transitionTime.Week - 1);

        if ( daysToAdd > 0 )
            result = result.AddDays( daysToAdd );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DateTime FloatingTimeFromEndOfMonthToDateTime(TimeZoneInfo.TransitionTime transitionTime, int year)
    {
        var daysInMonth = DateTime.DaysInMonth( year, transitionTime.Month );
        var result = new DateTime( year, transitionTime.Month, daysInMonth ) + transitionTime.TimeOfDay.TimeOfDay;

        var dayDelta = (int)result.DayOfWeek - (int)transitionTime.DayOfWeek;
        if ( dayDelta < 0 )
            dayDelta += ChronoConstants.DaysPerWeek;

        if ( dayDelta > 0 )
            result = result.AddDays( -dayDelta );

        return result;
    }
}