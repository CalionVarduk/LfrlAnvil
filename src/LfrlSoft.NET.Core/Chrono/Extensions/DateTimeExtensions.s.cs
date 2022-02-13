using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Chrono.Internal;

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
            return new DateTime( dt.Year, (int)IsoMonthOfYear.January, 1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime GetEndOfYear(this DateTime dt)
        {
            return new DateTime( dt.Year, (int)IsoMonthOfYear.December, Constants.DaysInDecember ).GetEndOfDay();
        }

        [Pure]
        public static DateTime Add(this DateTime dt, Period period)
        {
            var normalizedMonths = period.Years * Constants.MonthsPerYear + period.Months;

            var normalizedTicks =
                period.Weeks * Constants.DaysPerWeek * Constants.TicksPerDay +
                period.Days * Constants.TicksPerDay +
                period.Hours * Constants.TicksPerHour +
                period.Minutes * Constants.TicksPerMinute +
                period.Seconds * Constants.TicksPerSecond +
                period.Milliseconds * Constants.TicksPerMillisecond +
                period.Ticks;

            var result = dt
                .AddMonths( normalizedMonths )
                .AddTicks( normalizedTicks );

            return result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime Subtract(this DateTime dt, Period period)
        {
            return dt.Add( -period );
        }

        [Pure]
        public static DateTime SetYear(this DateTime dt, int year)
        {
            var daysInMonth = DateTime.DaysInMonth( year, dt.Month );

            return DateTime.SpecifyKind(
                new DateTime( year, dt.Month, Math.Min( dt.Day, daysInMonth ) ).Add( dt.TimeOfDay ),
                dt.Kind );
        }

        [Pure]
        public static DateTime SetMonth(this DateTime dt, IsoMonthOfYear month)
        {
            var daysInMonth = DateTime.DaysInMonth( dt.Year, (int)month );

            return DateTime.SpecifyKind(
                new DateTime( dt.Year, (int)month, Math.Min( dt.Day, daysInMonth ) ).Add( dt.TimeOfDay ),
                dt.Kind );
        }

        [Pure]
        public static DateTime SetDayOfMonth(this DateTime dt, int day)
        {
            return DateTime.SpecifyKind(
                new DateTime( dt.Year, dt.Month, day ).Add( dt.TimeOfDay ),
                dt.Kind );
        }

        [Pure]
        public static DateTime SetDayOfYear(this DateTime dt, int day)
        {
            var maxDay = DateTime.IsLeapYear( dt.Year ) ? Constants.DaysInLeapYear : Constants.DaysInYear;

            return DateTime.SpecifyKind(
                (day < 1 ? new DateTime( dt.Year, (int)IsoMonthOfYear.January, day ) :
                    day > maxDay ? new DateTime( dt.Year, (int)IsoMonthOfYear.December, day - maxDay + Constants.DaysInDecember ) :
                    dt.GetStartOfYear().AddDays( day - 1 ))
                .Add( dt.TimeOfDay ),
                dt.Kind );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DateTime SetTimeOfDay(this DateTime dt, TimeOfDay timeOfDay)
        {
            return dt.GetStartOfDay().Add( (TimeSpan)timeOfDay );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period GetPeriodOffset(this DateTime end, DateTime start, PeriodUnits units)
        {
            return end < start
                ? PeriodOffsetCalculator.GetPeriodOffset( end, start, units ).Negate()
                : PeriodOffsetCalculator.GetPeriodOffset( start, end, units );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period GetGreedyPeriodOffset(this DateTime end, DateTime start, PeriodUnits units)
        {
            return end < start
                ? PeriodOffsetCalculator.GetGreedyPeriodOffset( end, start, units ).Negate()
                : PeriodOffsetCalculator.GetGreedyPeriodOffset( start, end, units );
        }

        [Pure]
        internal static (ZonedDateTime DateTime, Duration DurationOffset) CreateIntervalStart(
            this DateTime minStartValue,
            TimeZoneInfo timeZone)
        {
            var startInvalidity = timeZone.GetContainingInvalidityRange( minStartValue );
            if ( startInvalidity is not null )
            {
                minStartValue = DateTime.SpecifyKind( startInvalidity.Value.Max.AddTicks( 1 ), minStartValue.Kind );
                return (ZonedDateTime.CreateUnsafe( minStartValue, timeZone ), Duration.Zero);
            }

            var result = ZonedDateTime.CreateUnsafe( minStartValue, timeZone );
            var ambiguousResult = result.GetOppositeAmbiguousDateTime();
            if ( ambiguousResult is null )
                return (result, Duration.Zero);

            var activeRule = timeZone.GetActiveAdjustmentRule( minStartValue )!;
            var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();

            var durationOffset = new Duration( transitionTime.TimeOfDay.TimeOfDay )
                .SubtractTicks( activeRule.DaylightDelta.Abs().Ticks );

            return (result.Timestamp < ambiguousResult.Value.Timestamp ? result : ambiguousResult.Value, durationOffset);
        }

        [Pure]
        internal static (ZonedDateTime DateTime, Duration DurationOffset) CreateIntervalEnd(
            this DateTime maxEndValue,
            TimeZoneInfo timeZone)
        {
            var startInvalidity = timeZone.GetContainingInvalidityRange( maxEndValue );
            if ( startInvalidity is not null )
            {
                maxEndValue = DateTime.SpecifyKind( startInvalidity.Value.Min.AddTicks( -1 ), maxEndValue.Kind );
                return (ZonedDateTime.CreateUnsafe( maxEndValue, timeZone ), Duration.Zero);
            }

            var result = ZonedDateTime.CreateUnsafe( maxEndValue, timeZone );
            var ambiguousResult = result.GetOppositeAmbiguousDateTime();
            if ( ambiguousResult is null )
                return (result, Duration.Zero);

            var activeRule = timeZone.GetActiveAdjustmentRule( maxEndValue )!;
            var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();

            var durationOffset = new Duration( -transitionTime.TimeOfDay.TimeOfDay );

            return (result.Timestamp > ambiguousResult.Value.Timestamp ? result : ambiguousResult.Value, durationOffset);
        }
    }
}
