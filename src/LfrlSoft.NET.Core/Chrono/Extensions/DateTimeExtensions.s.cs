using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono.Extensions
{
    public static class DateTimeExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IsoMonthOfYear GetMonthOfYear(this DateTime dt)
        {
            return (IsoMonthOfYear)dt.Month;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IsoDayOfWeek GetDayOfWeek(this DateTime dt)
        {
            return dt.DayOfWeek.ToIso();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime GetStartOfDay(this DateTime dt)
        {
            return dt.Date;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime GetEndOfDay(this DateTime dt)
        {
            return dt.Date.AddDays( 1 ).AddTicks( -1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime GetStartOfWeek(this DateTime dt, DayOfWeek weekStart)
        {
            var dayDelta = (int)dt.DayOfWeek - (int)weekStart;
            if ( dayDelta < 0 )
                dayDelta += Constants.DaysPerWeek;

            return (dayDelta > 0 ? dt.AddDays( -dayDelta ) : dt).GetStartOfDay();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime GetEndOfWeek(this DateTime dt, DayOfWeek weekStart)
        {
            var dayDelta = (int)dt.DayOfWeek - (int)weekStart;
            if ( dayDelta < 0 )
                dayDelta += Constants.DaysPerWeek;

            dayDelta = -dayDelta + Constants.DaysPerWeek - 1;

            return (dayDelta > 0 ? dt.AddDays( dayDelta ) : dt).GetEndOfDay();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime GetStartOfMonth(this DateTime dt)
        {
            return new DateTime( dt.Year, dt.Month, 1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime GetEndOfMonth(this DateTime dt)
        {
            var daysInMonth = DateTime.DaysInMonth( dt.Year, dt.Month );
            return new DateTime( dt.Year, dt.Month, daysInMonth ).GetEndOfDay();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime GetStartOfYear(this DateTime dt)
        {
            return new DateTime( dt.Year, 1, 1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime GetEndOfYear(this DateTime dt)
        {
            return new DateTime( dt.Year, 12, 31 ).GetEndOfDay();
        }
    }
}
