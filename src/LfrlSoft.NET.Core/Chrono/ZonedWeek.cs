using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.Core.Chrono.Internal;

namespace LfrlSoft.NET.Core.Chrono
{
    public readonly struct ZonedWeek : IEquatable<ZonedWeek>, IComparable<ZonedWeek>, IComparable
    {
        private readonly ZonedDateTime? _end;
        private readonly Duration? _duration;

        private ZonedWeek(ZonedDateTime start, ZonedDateTime end, Duration duration)
        {
            Start = start;
            _end = end;
            _duration = duration;
        }

        public ZonedDateTime Start { get; }
        public ZonedDateTime End => _end ?? ZonedDateTime.CreateUtc( DateTime.UnixEpoch.AddTicks( Constants.TicksPerWeek - 1 ) );
        public int Year => WeekCalculator.GetYearInWeekFormat( Start.Value );
        public int WeekOfYear => WeekCalculator.GetWeekOfYear( Start.Value );
        public TimeZoneInfo TimeZone => Start.TimeZone;
        public Duration Duration => _duration ?? Duration.FromTicks( Constants.TicksPerWeek );
        public bool IsUtc => Start.IsUtc;
        public bool IsLocal => Start.IsLocal;

        [Pure]
        public static ZonedWeek Create(DateTime dateTime, TimeZoneInfo timeZone, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            Ensure.IsInRange( (int)weekStart, (int)IsoDayOfWeek.Monday, (int)IsoDayOfWeek.Sunday, nameof( weekStart ) );

            var bclWeekStart = weekStart.ToBcl();
            var kind = timeZone.GetDateTimeKind();
            dateTime = DateTime.SpecifyKind( dateTime, kind );

            var (start, startDurationOffset) = dateTime.GetStartOfWeek( bclWeekStart ).CreateIntervalStart( timeZone );
            var (end, endDurationOffset) = dateTime.GetEndOfWeek( bclWeekStart ).CreateIntervalEnd( timeZone );
            var duration = end.GetDurationOffset( start ).Add( startDurationOffset ).Add( endDurationOffset ).AddTicks( 1 );

            return new ZonedWeek( start, end, duration );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek Create(ZonedDateTime dateTime, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            return Create( dateTime.Value, dateTime.TimeZone, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek Create(ZonedDay day, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            return Create( day.Start.Value, day.TimeZone, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek Create(int year, int weekOfYear, TimeZoneInfo timeZone, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            Ensure.IsInRange( (int)weekStart, (int)IsoDayOfWeek.Monday, (int)IsoDayOfWeek.Sunday, nameof( weekStart ) );

            var bclWeekStart = weekStart.ToBcl();
            var maxWeekOfYear = WeekCalculator.GetWeekCountInYear( year, bclWeekStart );
            Ensure.IsInRange( weekOfYear, 1, maxWeekOfYear, nameof( weekOfYear ) );

            return CreateUnsafe( year, weekOfYear, timeZone, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek? TryCreate(int year, int weekOfYear, TimeZoneInfo timeZone, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            Ensure.IsInRange( (int)weekStart, (int)IsoDayOfWeek.Monday, (int)IsoDayOfWeek.Sunday, nameof( weekStart ) );

            var bclWeekStart = weekStart.ToBcl();
            var maxWeekOfYear = WeekCalculator.GetWeekCountInYear( year, bclWeekStart );
            if ( weekOfYear <= 0 || weekOfYear > maxWeekOfYear )
                return null;

            return CreateUnsafe( year, weekOfYear, timeZone, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek CreateUtc(Timestamp timestamp, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            return CreateUtc( timestamp.UtcValue, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek CreateUtc(DateTime utcDateTime, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            Ensure.IsInRange( (int)weekStart, (int)IsoDayOfWeek.Monday, (int)IsoDayOfWeek.Sunday, nameof( weekStart ) );

            var bclStartDay = weekStart.ToBcl();
            var start = ZonedDateTime.CreateUtc( utcDateTime.GetStartOfWeek( bclStartDay ) );
            var end = ZonedDateTime.CreateUtc( utcDateTime.GetEndOfWeek( bclStartDay ) );

            return new ZonedWeek( start, end, Duration.FromTicks( Constants.TicksPerWeek ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek CreateUtc(int year, int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            return Create( year, weekOfYear, TimeZoneInfo.Utc, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek? TryCreateUtc(int year, int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            return TryCreate( year, weekOfYear, TimeZoneInfo.Utc, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek CreateLocal(DateTime localDateTime, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            return Create( localDateTime, TimeZoneInfo.Local, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek CreateLocal(int year, int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            return Create( year, weekOfYear, TimeZoneInfo.Local, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek? TryCreateLocal(int year, int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
        {
            return TryCreate( year, weekOfYear, TimeZoneInfo.Local, weekStart );
        }

        [Pure]
        public override string ToString()
        {
            var start = Start;
            var end = End;
            var startUtcOffset = start.UtcOffset;
            var endUtcOffset = end.UtcOffset;
            var (year, weekOfYear) = WeekCalculator.GetYearAndWeekOfYear( start.Value );

            var dateText = TextFormatting.StringifyYearAndWeek( year, weekOfYear );
            var startEndDayText = TextFormatting.StringifyWeekStartAndEndDay( start.DayOfWeek, end.DayOfWeek );
            var utcOffsetText = TextFormatting.StringifyOffset( start.UtcOffset );

            if ( startUtcOffset == endUtcOffset )
                return $"{dateText} ({startEndDayText}) {utcOffsetText} ({TimeZone.Id})";

            var endUtcOffsetText = TextFormatting.StringifyOffset( endUtcOffset );
            return $"{dateText} ({startEndDayText}) {utcOffsetText} {endUtcOffsetText} ({TimeZone.Id})";
        }

        [Pure]
        public override int GetHashCode()
        {
            return Hash.Default.Add( Start.Timestamp ).Add( End.Timestamp ).Add( TimeZone.Id ).Value;
        }

        [Pure]
        public override bool Equals(object obj)
        {
            return obj is ZonedWeek d && Equals( d );
        }

        [Pure]
        public bool Equals(ZonedWeek other)
        {
            return Start.Equals( other.Start );
        }

        [Pure]
        public int CompareTo(object obj)
        {
            return obj is ZonedWeek d ? CompareTo( d ) : 1;
        }

        [Pure]
        public int CompareTo(ZonedWeek other)
        {
            return Start.CompareTo( other.Start );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek ToTimeZone(TimeZoneInfo targetTimeZone)
        {
            return Create( Start.Value, targetTimeZone, Start.DayOfWeek );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek ToUtcTimeZone()
        {
            return CreateUtc( Start.Value, Start.DayOfWeek );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek ToLocalTimeZone()
        {
            return ToTimeZone( TimeZoneInfo.Local );
        }

        [Pure]
        public bool Contains(ZonedDateTime dateTime)
        {
            var start = Start;
            var startDate = start.Value.Date;
            var endDate = End.Value.Date;
            var convertedDate = dateTime.ToTimeZone( start.TimeZone ).Value.Date;
            return startDate <= convertedDate && endDate >= convertedDate;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains(ZonedDay day)
        {
            return Contains( day.Start ) && (ReferenceEquals( TimeZone, day.TimeZone ) || Contains( day.End ));
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek GetNext()
        {
            return AddWeeks( 1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek GetPrevious()
        {
            return AddWeeks( -1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek AddWeeks(int weeks)
        {
            var start = Start;
            var value = start.Value.AddTicks( weeks * Constants.TicksPerWeek );
            return Create( value, start.TimeZone, start.DayOfWeek );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek Add(Period value)
        {
            var start = Start;
            var dateTime = start.Value.Add( value );
            return Create( dateTime, start.TimeZone, start.DayOfWeek );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek SubtractWeeks(int weeks)
        {
            return AddWeeks( -weeks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek Subtract(Period value)
        {
            return Add( -value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period GetPeriodOffset(ZonedWeek start, PeriodUnits units)
        {
            return Start.GetPeriodOffset( start.Start, units );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period GetGreedyPeriodOffset(ZonedWeek start, PeriodUnits units)
        {
            return Start.GetGreedyPeriodOffset( start.Start, units );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek SetYear(int year)
        {
            var start = Start;
            var weekStart = start.DayOfWeek;
            var weekCount = WeekCalculator.GetWeekCountInYear( year, weekStart.ToBcl() );
            return Create( year, Math.Min( WeekOfYear, weekCount ), start.TimeZone, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek SetWeekOfYear(int week)
        {
            var start = Start;
            return Create( Year, week, start.TimeZone, start.DayOfWeek );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek? TrySetWeekOfYear(int week)
        {
            var start = Start;
            return TryCreate( Year, week, start.TimeZone, start.DayOfWeek );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek SetWeekStart(IsoDayOfWeek weekStart)
        {
            var start = Start;
            var (year, weekOfYear) = WeekCalculator.GetYearAndWeekOfYear( start.Value );
            return Create( year, weekOfYear, start.TimeZone, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedWeek? TrySetWeekStart(IsoDayOfWeek weekStart)
        {
            var start = Start;
            var (year, weekOfYear) = WeekCalculator.GetYearAndWeekOfYear( start.Value );
            return TryCreate( year, weekOfYear, start.TimeZone, weekStart );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay GetDayOfWeek(IsoDayOfWeek day)
        {
            Ensure.IsInRange( (int)day, (int)IsoDayOfWeek.Monday, (int)IsoDayOfWeek.Sunday, nameof( day ) );

            var start = Start;

            var offsetInDays = (int)day - (int)start.DayOfWeek;
            if ( offsetInDays < 0 )
                offsetInDays += Constants.DaysPerWeek;

            var dayValue = start.Value.AddTicks( Constants.TicksPerDay * offsetInDays );
            return ZonedDay.Create( dayValue, start.TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay GetMonday()
        {
            return GetDayOfWeek( IsoDayOfWeek.Monday );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay GetTuesday()
        {
            return GetDayOfWeek( IsoDayOfWeek.Tuesday );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay GetWednesday()
        {
            return GetDayOfWeek( IsoDayOfWeek.Wednesday );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay GetThursday()
        {
            return GetDayOfWeek( IsoDayOfWeek.Thursday );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay GetFriday()
        {
            return GetDayOfWeek( IsoDayOfWeek.Friday );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay GetSaturday()
        {
            return GetDayOfWeek( IsoDayOfWeek.Saturday );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay GetSunday()
        {
            return GetDayOfWeek( IsoDayOfWeek.Sunday );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedYear GetYear()
        {
            return ZonedYear.Create( Year, TimeZone );
        }

        [Pure]
        public IEnumerable<ZonedDay> GetAllDays()
        {
            var start = Start;
            var timeZone = start.TimeZone;

            for ( var dayOffset = 0; dayOffset < Constants.DaysPerWeek; ++dayOffset )
                yield return ZonedDay.Create( start.Value.AddTicks( Constants.TicksPerDay * dayOffset ), timeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek operator +(ZonedWeek a, Period b)
        {
            return a.Add( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedWeek operator -(ZonedWeek a, Period b)
        {
            return a.Subtract( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator ==(ZonedWeek a, ZonedWeek b)
        {
            return a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator !=(ZonedWeek a, ZonedWeek b)
        {
            return ! a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >(ZonedWeek a, ZonedWeek b)
        {
            return a.CompareTo( b ) > 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <=(ZonedWeek a, ZonedWeek b)
        {
            return a.CompareTo( b ) <= 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <(ZonedWeek a, ZonedWeek b)
        {
            return a.CompareTo( b ) < 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >=(ZonedWeek a, ZonedWeek b)
        {
            return a.CompareTo( b ) >= 0;
        }

        [Pure]
        private static ZonedWeek CreateUnsafe(int year, int weekOfYear, TimeZoneInfo timeZone, IsoDayOfWeek weekStart)
        {
            var bclWeekStart = weekStart.ToBcl();
            var dayInFirstWeekOfYear = WeekCalculator.GetDayInFirstWeekOfYear( year, bclWeekStart );
            var startOfFirstWeekOfYear = dayInFirstWeekOfYear.GetStartOfWeek( bclWeekStart );
            var startOfTargetWeek = startOfFirstWeekOfYear.AddTicks( Constants.TicksPerWeek * (weekOfYear - 1) );
            return Create( startOfTargetWeek, timeZone, weekStart );
        }
    }
}
