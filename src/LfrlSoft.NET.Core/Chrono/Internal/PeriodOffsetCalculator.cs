using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono.Internal
{
    internal static class PeriodOffsetCalculator
    {
        [Pure]
        internal static Period GetGreedyPeriodOffset(DateTime start, DateTime end, PeriodUnits units)
        {
            var years = 0;
            var months = 0;
            var weeks = 0;
            var days = 0;
            var hours = 0;
            var minutes = 0L;
            var seconds = 0L;
            var milliseconds = 0L;
            var ticks = 0L;

            var yearMonthEndOffset = new YearMonthOffsetState( end );

            if ( IncludesUnit( units, PeriodUnits.Years ) )
            {
                if ( IncludesUnit( units, PeriodUnits.Months ) )
                    (years, months) = yearMonthEndOffset.HandleYearsAndMonthsGreedyOffset( start );
                else
                    years = yearMonthEndOffset.HandleYearsGreedyOffset( start );
            }
            else if ( IncludesUnit( units, PeriodUnits.Months ) )
                months = yearMonthEndOffset.HandleMonthsGreedyOffset( start );

            var startOffset = new FixedUnitOffsetState( start );
            var endOffset = new FixedUnitOffsetState( yearMonthEndOffset.DateTime );

            if ( IncludesUnit( units, PeriodUnits.Weeks ) )
            {
                if ( IncludesUnit( units, PeriodUnits.Days ) )
                    (weeks, days) = endOffset.HandleWeeksAndDaysGreedyOffset( startOffset.Current );
                else
                    weeks = endOffset.HandleWeeksGreedyOffset( startOffset.Current );
            }
            else if ( IncludesUnit( units, PeriodUnits.Days ) )
                days = endOffset.HandleDaysGreedyOffset( startOffset.Current );

            startOffset.MoveHours();
            endOffset.MoveHours();

            if ( IncludesUnit( units, PeriodUnits.Hours ) )
                hours = endOffset.HandleHoursGreedyOffset( startOffset.Current );

            startOffset.MoveMinutes();
            endOffset.MoveMinutes();

            if ( IncludesUnit( units, PeriodUnits.Minutes ) )
                minutes = endOffset.HandleMinutesGreedyOffset( startOffset.Current );

            startOffset.MoveSeconds();
            endOffset.MoveSeconds();

            if ( IncludesUnit( units, PeriodUnits.Seconds ) )
                seconds = endOffset.HandleSecondsGreedyOffset( startOffset.Current );

            startOffset.MoveMilliseconds();
            endOffset.MoveMilliseconds();

            if ( IncludesUnit( units, PeriodUnits.Milliseconds ) )
                milliseconds = endOffset.HandleMillisecondsGreedyOffset( startOffset.Current );

            startOffset.MoveTicks();
            endOffset.MoveTicks();

            if ( IncludesUnit( units, PeriodUnits.Ticks ) )
                ticks = endOffset.HandleTicksOffset( startOffset.Current );

            return new Period( years, months, weeks, days, hours, minutes, seconds, milliseconds, ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Period GetPeriodOffset(DateTime start, DateTime end, PeriodUnits units)
        {
            var years = 0;
            var months = 0;
            var weeks = 0;
            var days = 0;
            var hours = 0;
            var minutes = 0L;
            var seconds = 0L;
            var milliseconds = 0L;
            var ticks = 0L;

            var yearMonthEndOffset = new YearMonthOffsetState( end );

            if ( IncludesUnit( units, PeriodUnits.Years ) )
            {
                if ( IncludesUnit( units, PeriodUnits.Months ) )
                    (years, months) = yearMonthEndOffset.HandleYearsAndMonthsOffset( start );
                else
                    years = yearMonthEndOffset.HandleYearsOffset( start );
            }
            else if ( IncludesUnit( units, PeriodUnits.Months ) )
                months = yearMonthEndOffset.HandleMonthsOffset( start );

            var startOffset = new FixedUnitOffsetState( start );
            var endOffset = new FixedUnitOffsetState( yearMonthEndOffset.DateTime );

            if ( IncludesUnit( units, PeriodUnits.Weeks ) )
            {
                if ( IncludesUnit( units, PeriodUnits.Days ) )
                    (weeks, days) = endOffset.HandleWeeksAndDaysOffset( startOffset );
                else
                    weeks = endOffset.HandleWeeksOffset( startOffset );
            }
            else if ( IncludesUnit( units, PeriodUnits.Days ) )
                days = endOffset.HandleDaysOffset( startOffset );

            startOffset.MoveHours();
            endOffset.MoveHours();

            if ( IncludesUnit( units, PeriodUnits.Hours ) )
                hours = endOffset.HandleHoursOffset( startOffset );

            startOffset.MoveMinutes();
            endOffset.MoveMinutes();

            if ( IncludesUnit( units, PeriodUnits.Minutes ) )
                minutes = endOffset.HandleMinutesOffset( startOffset );

            startOffset.MoveSeconds();
            endOffset.MoveSeconds();

            if ( IncludesUnit( units, PeriodUnits.Seconds ) )
                seconds = endOffset.HandleSecondsOffset( startOffset );

            startOffset.MoveMilliseconds();
            endOffset.MoveMilliseconds();

            if ( IncludesUnit( units, PeriodUnits.Milliseconds ) )
                milliseconds = endOffset.HandleMillisecondsOffset( startOffset );

            startOffset.MoveTicks();
            endOffset.MoveTicks();

            if ( IncludesUnit( units, PeriodUnits.Ticks ) )
                ticks = endOffset.HandleTicksOffset( startOffset.Current );

            return new Period( years, months, weeks, days, hours, minutes, seconds, milliseconds, ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static bool IncludesUnit(PeriodUnits source, PeriodUnits unit)
        {
            return (source & unit) != 0;
        }

        private struct YearMonthOffsetState
        {
            public DateTime DateTime;

            public YearMonthOffsetState(DateTime dateTime)
            {
                DateTime = dateTime;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int HandleYearsGreedyOffset(DateTime start)
            {
                var offsetInYears = DateTime.Year - start.Year;
                if ( offsetInYears != 0 )
                    DateTime = DateTime.AddMonths( -offsetInYears * Constants.MonthsPerYear );

                return offsetInYears;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int HandleYearsOffset(DateTime start)
            {
                var offsetInYears = HandleYearsGreedyOffset( start );
                return TryCompensateForGreedyOffset( start, Constants.MonthsPerYear ) ? offsetInYears - 1 : offsetInYears;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int HandleMonthsGreedyOffset(DateTime start)
            {
                var offsetInYears = DateTime.Year - start.Year;
                var offsetInMonths = DateTime.Month - start.Month;

                var fullOffsetInMonths = offsetInYears * Constants.MonthsPerYear + offsetInMonths;
                if ( fullOffsetInMonths != 0 )
                    DateTime = DateTime.AddMonths( -fullOffsetInMonths );

                return fullOffsetInMonths;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int HandleMonthsOffset(DateTime start)
            {
                var offsetInMonths = HandleMonthsGreedyOffset( start );
                return TryCompensateForGreedyOffset( start, 1 ) ? offsetInMonths - 1 : offsetInMonths;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public (int Years, int Months) HandleYearsAndMonthsGreedyOffset(DateTime start)
            {
                var offsetInYears = DateTime.Year - start.Year;
                var offsetInMonths = DateTime.Month - start.Month;

                var fullOffsetInMonths = offsetInYears * Constants.MonthsPerYear + offsetInMonths;
                if ( fullOffsetInMonths != 0 )
                    DateTime = DateTime.AddMonths( -fullOffsetInMonths );

                return (Years: offsetInYears, Months: offsetInMonths);
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public (int Years, int Months) HandleYearsAndMonthsOffset(DateTime start)
            {
                var (offsetInYears, offsetInMonths) = HandleYearsAndMonthsGreedyOffset( start );

                if ( ! TryCompensateForGreedyOffset( start, 1 ) )
                {
                    return offsetInMonths >= 0
                        ? (Years: offsetInYears, Months: offsetInMonths)
                        : (Years: offsetInYears - 1, Months: offsetInMonths + Constants.MonthsPerYear);
                }

                return offsetInMonths > 0
                    ? (Years: offsetInYears, Months: offsetInMonths - 1)
                    : (Years: offsetInYears - 1, Months: offsetInMonths + Constants.MonthsPerYear - 1);
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private bool TryCompensateForGreedyOffset(DateTime start, int compensationInMonths)
            {
                if ( DateTime >= start )
                    return false;

                DateTime = DateTime.AddMonths( compensationInMonths );
                return true;
            }
        }

        private struct FixedUnitOffsetState
        {
            public Duration Current;
            public Duration RemainingTimeOfDay;

            public FixedUnitOffsetState(DateTime dateTime)
            {
                Current = new Duration( dateTime.Date.Ticks );
                RemainingTimeOfDay = new Duration( dateTime.TimeOfDay );
            }

            public Duration FullValue => Current.Add( RemainingTimeOfDay );

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public void MoveHours()
            {
                Move( RemainingTimeOfDay.TrimToHour() );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public void MoveMinutes()
            {
                Move( RemainingTimeOfDay.TrimToMinute() );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public void MoveSeconds()
            {
                Move( RemainingTimeOfDay.TrimToSecond() );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public void MoveMilliseconds()
            {
                Move( RemainingTimeOfDay.TrimToMillisecond() );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public void MoveTicks()
            {
                Move( RemainingTimeOfDay );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int HandleWeeksGreedyOffset(Duration currentStart)
            {
                var duration = Current - currentStart;
                var offsetInWeeks = (int)(duration.Ticks / Constants.TicksPerWeek);

                if ( offsetInWeeks != 0 )
                    Current = Current.SubtractTicks( offsetInWeeks * Constants.TicksPerWeek );

                return offsetInWeeks;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int HandleWeeksOffset(FixedUnitOffsetState start)
            {
                var offsetInWeeks = HandleWeeksGreedyOffset( start.Current );
                return TryCompensateForGreedyOffset( start, Constants.TicksPerWeek ) ? offsetInWeeks - 1 : offsetInWeeks;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int HandleDaysGreedyOffset(Duration currentStart)
            {
                var duration = Current - currentStart;
                if ( duration == Duration.Zero )
                    return 0;

                var offsetInDays = (int)(duration.Ticks / Constants.TicksPerDay);
                Current = Current.Subtract( duration );
                return offsetInDays;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int HandleDaysOffset(FixedUnitOffsetState start)
            {
                var offsetInDays = HandleDaysGreedyOffset( start.Current );
                return TryCompensateForGreedyOffset( start, Constants.TicksPerDay ) ? offsetInDays - 1 : offsetInDays;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public (int Weeks, int Days) HandleWeeksAndDaysGreedyOffset(Duration currentStart)
            {
                var duration = Current - currentStart;
                if ( duration == Duration.Zero )
                    return (Weeks: 0, Days: 0);

                var fullOffsetInDays = (int)(duration.Ticks / Constants.TicksPerDay);
                var offsetInWeeks = fullOffsetInDays / Constants.DaysPerWeek;
                var offsetInDays = fullOffsetInDays - offsetInWeeks * Constants.DaysPerWeek;

                Current = Current.Subtract( duration );
                return (Weeks: offsetInWeeks, Days: offsetInDays);
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public (int Weeks, int Days) HandleWeeksAndDaysOffset(FixedUnitOffsetState start)
            {
                var (offsetInWeeks, offsetInDays) = HandleWeeksAndDaysGreedyOffset( start.Current );
                if ( ! TryCompensateForGreedyOffset( start, Constants.TicksPerDay ) )
                    return (Weeks: offsetInWeeks, Days: offsetInDays);

                return offsetInDays > 0
                    ? (Weeks: offsetInWeeks, Days: offsetInDays - 1)
                    : (Weeks: offsetInWeeks - 1, Days: offsetInDays + Constants.DaysPerWeek - 1);
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int HandleHoursGreedyOffset(Duration currentStart)
            {
                var duration = Current - currentStart;
                if ( duration == Duration.Zero )
                    return 0;

                Current = Current.Subtract( duration );
                return (int)duration.FullHours;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int HandleHoursOffset(FixedUnitOffsetState start)
            {
                var offsetInHours = HandleHoursGreedyOffset( start.Current );
                return TryCompensateForGreedyOffset( start, Constants.TicksPerHour ) ? offsetInHours - 1 : offsetInHours;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public long HandleMinutesGreedyOffset(Duration currentStart)
            {
                var duration = Current - currentStart;
                if ( duration == Duration.Zero )
                    return 0;

                Current = Current.Subtract( duration );
                return duration.FullMinutes;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public long HandleMinutesOffset(FixedUnitOffsetState start)
            {
                var offsetInMinutes = HandleMinutesGreedyOffset( start.Current );
                return TryCompensateForGreedyOffset( start, Constants.TicksPerMinute ) ? offsetInMinutes - 1 : offsetInMinutes;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public long HandleSecondsGreedyOffset(Duration currentStart)
            {
                var duration = Current - currentStart;
                if ( duration == Duration.Zero )
                    return 0;

                Current = Current.Subtract( duration );
                return duration.FullSeconds;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public long HandleSecondsOffset(FixedUnitOffsetState start)
            {
                var offsetInSeconds = HandleSecondsGreedyOffset( start.Current );
                return TryCompensateForGreedyOffset( start, Constants.TicksPerSecond ) ? offsetInSeconds - 1 : offsetInSeconds;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public long HandleMillisecondsGreedyOffset(Duration currentStart)
            {
                var duration = Current - currentStart;
                if ( duration == Duration.Zero )
                    return 0;

                Current = Current.Subtract( duration );
                return duration.FullMilliseconds;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public long HandleMillisecondsOffset(FixedUnitOffsetState start)
            {
                var offsetInMilliseconds = HandleMillisecondsGreedyOffset( start.Current );
                return TryCompensateForGreedyOffset( start, Constants.TicksPerMillisecond )
                    ? offsetInMilliseconds - 1
                    : offsetInMilliseconds;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public long HandleTicksOffset(Duration currentStart)
            {
                var duration = Current - currentStart;
                Current = currentStart;
                return duration.Ticks;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private void Move(Duration offset)
            {
                Current = Current.Add( offset );
                RemainingTimeOfDay = RemainingTimeOfDay.Subtract( offset );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private bool TryCompensateForGreedyOffset(FixedUnitOffsetState start, long compensationInTicks)
            {
                if ( FullValue >= start.FullValue )
                    return false;

                Current = Current.AddTicks( compensationInTicks );
                return true;
            }
        }
    }
}
