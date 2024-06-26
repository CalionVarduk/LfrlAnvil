﻿// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Internal;

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
        var microseconds = 0L;
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

        startOffset.MoveMicroseconds();
        endOffset.MoveMicroseconds();

        if ( IncludesUnit( units, PeriodUnits.Microseconds ) )
            microseconds = endOffset.HandleMicrosecondsGreedyOffset( startOffset.Current );

        startOffset.MoveTicks();
        endOffset.MoveTicks();

        if ( IncludesUnit( units, PeriodUnits.Ticks ) )
            ticks = endOffset.HandleTicksOffset( startOffset.Current );

        return new Period( years, months, weeks, days, hours, minutes, seconds, milliseconds, microseconds, ticks );
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
        var microseconds = 0L;
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

        startOffset.MoveMicroseconds();
        endOffset.MoveMicroseconds();

        if ( IncludesUnit( units, PeriodUnits.Microseconds ) )
            microseconds = endOffset.HandleMicrosecondsOffset( startOffset );

        startOffset.MoveTicks();
        endOffset.MoveTicks();

        if ( IncludesUnit( units, PeriodUnits.Ticks ) )
            ticks = endOffset.HandleTicksOffset( startOffset.Current );

        return new Period( years, months, weeks, days, hours, minutes, seconds, milliseconds, microseconds, ticks );
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
                SubtractMonths( offsetInYears * ChronoConstants.MonthsPerYear );

            return offsetInYears;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int HandleYearsOffset(DateTime start)
        {
            var offsetInYears = HandleYearsGreedyOffset( start );
            return TryCompensateForGreedyOffset( start, ChronoConstants.MonthsPerYear ) ? offsetInYears - 1 : offsetInYears;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int HandleMonthsGreedyOffset(DateTime start)
        {
            var offsetInYears = DateTime.Year - start.Year;
            var offsetInMonths = DateTime.Month - start.Month;

            var fullOffsetInMonths = offsetInYears * ChronoConstants.MonthsPerYear + offsetInMonths;
            if ( fullOffsetInMonths != 0 )
                SubtractMonths( fullOffsetInMonths );

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

            var fullOffsetInMonths = offsetInYears * ChronoConstants.MonthsPerYear + offsetInMonths;
            if ( fullOffsetInMonths != 0 )
                SubtractMonths( fullOffsetInMonths );

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
                    : (Years: offsetInYears - 1, Months: offsetInMonths + ChronoConstants.MonthsPerYear);
            }

            return offsetInMonths > 0
                ? (Years: offsetInYears, Months: offsetInMonths - 1)
                : (Years: offsetInYears - 1, Months: offsetInMonths + ChronoConstants.MonthsPerYear - 1);
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private bool TryCompensateForGreedyOffset(DateTime start, int compensationInMonths)
        {
            if ( DateTime >= start )
                return false;

            DateTime = DateTime.AddMonths( compensationInMonths );
            return true;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void SubtractMonths(int months)
        {
            var originalDay = DateTime.Day;
            DateTime = DateTime.AddMonths( -months );
            var currentDay = DateTime.Day;
            if ( originalDay == currentDay )
                return;

            Assume.IsGreaterThan( originalDay, currentDay );
            DateTime = DateTime.AddDays( originalDay - currentDay );
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
        public void MoveMicroseconds()
        {
            Move( RemainingTimeOfDay.TrimToMicrosecond() );
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
            var offsetInWeeks = ( int )(duration.Ticks / ChronoConstants.TicksPerStandardWeek);

            if ( offsetInWeeks != 0 )
                Current = Current.SubtractTicks( offsetInWeeks * ChronoConstants.TicksPerStandardWeek );

            return offsetInWeeks;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int HandleWeeksOffset(FixedUnitOffsetState start)
        {
            var offsetInWeeks = HandleWeeksGreedyOffset( start.Current );
            return TryCompensateForGreedyOffset( start, ChronoConstants.TicksPerStandardWeek ) ? offsetInWeeks - 1 : offsetInWeeks;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int HandleDaysGreedyOffset(Duration currentStart)
        {
            var duration = Current - currentStart;
            if ( duration == Duration.Zero )
                return 0;

            var offsetInDays = ( int )(duration.Ticks / ChronoConstants.TicksPerStandardDay);
            Current = Current.Subtract( duration );
            return offsetInDays;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int HandleDaysOffset(FixedUnitOffsetState start)
        {
            var offsetInDays = HandleDaysGreedyOffset( start.Current );
            return TryCompensateForGreedyOffset( start, ChronoConstants.TicksPerStandardDay ) ? offsetInDays - 1 : offsetInDays;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public (int Weeks, int Days) HandleWeeksAndDaysGreedyOffset(Duration currentStart)
        {
            var duration = Current - currentStart;
            if ( duration == Duration.Zero )
                return (Weeks: 0, Days: 0);

            var fullOffsetInDays = ( int )(duration.Ticks / ChronoConstants.TicksPerStandardDay);
            var offsetInWeeks = fullOffsetInDays / ChronoConstants.DaysPerWeek;
            var offsetInDays = fullOffsetInDays - offsetInWeeks * ChronoConstants.DaysPerWeek;

            Current = Current.Subtract( duration );
            return (Weeks: offsetInWeeks, Days: offsetInDays);
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public (int Weeks, int Days) HandleWeeksAndDaysOffset(FixedUnitOffsetState start)
        {
            var (offsetInWeeks, offsetInDays) = HandleWeeksAndDaysGreedyOffset( start.Current );
            if ( ! TryCompensateForGreedyOffset( start, ChronoConstants.TicksPerStandardDay ) )
                return (Weeks: offsetInWeeks, Days: offsetInDays);

            return offsetInDays > 0
                ? (Weeks: offsetInWeeks, Days: offsetInDays - 1)
                : (Weeks: offsetInWeeks - 1, Days: offsetInDays + ChronoConstants.DaysPerWeek - 1);
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int HandleHoursGreedyOffset(Duration currentStart)
        {
            var duration = Current - currentStart;
            if ( duration == Duration.Zero )
                return 0;

            Current = Current.Subtract( duration );
            return ( int )duration.FullHours;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int HandleHoursOffset(FixedUnitOffsetState start)
        {
            var offsetInHours = HandleHoursGreedyOffset( start.Current );
            return TryCompensateForGreedyOffset( start, ChronoConstants.TicksPerHour ) ? offsetInHours - 1 : offsetInHours;
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
            return TryCompensateForGreedyOffset( start, ChronoConstants.TicksPerMinute ) ? offsetInMinutes - 1 : offsetInMinutes;
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
            return TryCompensateForGreedyOffset( start, ChronoConstants.TicksPerSecond ) ? offsetInSeconds - 1 : offsetInSeconds;
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
            return TryCompensateForGreedyOffset( start, ChronoConstants.TicksPerMillisecond )
                ? offsetInMilliseconds - 1
                : offsetInMilliseconds;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public long HandleMicrosecondsGreedyOffset(Duration currentStart)
        {
            var duration = Current - currentStart;
            if ( duration == Duration.Zero )
                return 0;

            Current = Current.Subtract( duration );
            return duration.FullMicroseconds;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public long HandleMicrosecondsOffset(FixedUnitOffsetState start)
        {
            var offsetInMicroseconds = HandleMicrosecondsGreedyOffset( start.Current );
            return TryCompensateForGreedyOffset( start, ChronoConstants.TicksPerMicrosecond )
                ? offsetInMicroseconds - 1
                : offsetInMicroseconds;
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
