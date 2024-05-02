﻿using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Internal;

internal static class TextFormatting
{
    internal const string SevenDigitFormat = "0000000";
    internal const string FourDigitFormat = "0000";
    internal const string TwoDigitFormat = "00";
    internal const char DateComponentSeparator = '-';
    internal const char TimeComponentSeparator = ':';
    internal const char TicksInSecondSeparator = '.';
    internal const char WeekSymbol = 'W';

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyYear(int year)
    {
        return year.ToString( FourDigitFormat );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyMonth(int month)
    {
        return month.ToString( TwoDigitFormat );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyDayOfMonth(int day)
    {
        return day.ToString( TwoDigitFormat );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyWeekOfYear(int week)
    {
        return $"{WeekSymbol}{week.ToString( TwoDigitFormat )}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyHour(int hour)
    {
        return hour.ToString( TwoDigitFormat );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyMinute(int minute)
    {
        return minute.ToString( TwoDigitFormat );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifySecond(int second)
    {
        return second.ToString( TwoDigitFormat );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyTicksInSecond(long ticksInSecond)
    {
        return ticksInSecond.ToString( SevenDigitFormat );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyYearAndMonth(DateTime date)
    {
        var year = StringifyYear( date.Year );
        var month = StringifyMonth( date.Month );
        return $"{year}{DateComponentSeparator}{month}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyYearAndWeek(int year, int week)
    {
        var yearText = StringifyYear( year );
        var weekText = StringifyWeekOfYear( week );
        return $"{yearText}{DateComponentSeparator}{weekText}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyWeekStartAndEndDay(IsoDayOfWeek start, IsoDayOfWeek end)
    {
        return $"{start}{DateComponentSeparator}{end}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyDate(DateTime date)
    {
        var year = StringifyYear( date.Year );
        var month = StringifyMonth( date.Month );
        var day = StringifyDayOfMonth( date.Day );
        return $"{year}{DateComponentSeparator}{month}{DateComponentSeparator}{day}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyTimeOfDay(TimeSpan time)
    {
        var hour = StringifyHour( time.Hours );
        var minute = StringifyMinute( time.Minutes );
        var second = StringifySecond( time.Seconds );
        var ticksInSecond = StringifyTicksInSecond(
            time.Milliseconds * ChronoConstants.TicksPerMillisecond + time.Ticks % ChronoConstants.TicksPerMillisecond );

        return $"{hour}{TimeComponentSeparator}{minute}{TimeComponentSeparator}{second}{TicksInSecondSeparator}{ticksInSecond}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyDateTime(DateTime dateTime)
    {
        return $"{StringifyDate( dateTime )} {StringifyTimeOfDay( dateTime.TimeOfDay )}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringifyOffset(Duration offset)
    {
        var sign = offset < Duration.Zero ? '-' : '+';
        var hour = StringifyHour( Math.Abs( ( int )offset.FullHours ) );
        var minute = StringifyMinute( offset.MinutesInHour );
        return $"{sign}{hour}{TimeComponentSeparator}{minute}";
    }
}
