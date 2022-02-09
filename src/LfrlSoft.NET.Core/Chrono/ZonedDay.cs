using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Chrono.Extensions;

namespace LfrlSoft.NET.Core.Chrono
{
    public readonly struct ZonedDay : IEquatable<ZonedDay>, IComparable<ZonedDay>, IComparable
    {
        private readonly ZonedDateTime? _end;
        private readonly Duration? _duration;

        private ZonedDay(ZonedDateTime start, ZonedDateTime end, Duration duration)
        {
            Start = start;
            _end = end;
            _duration = duration;
        }

        public ZonedDateTime Start { get; }
        public ZonedDateTime End => _end ?? ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfDay() );
        public int Year => Start.Year;
        public IsoMonthOfYear Month => Start.Month;
        public int DayOfMonth => Start.DayOfMonth;
        public int DayOfYear => Start.DayOfYear;
        public IsoDayOfWeek DayOfWeek => Start.DayOfWeek;
        public TimeZoneInfo TimeZone => Start.TimeZone;
        public Duration Duration => _duration ?? Duration.FromHours( 24 );
        public bool IsUtc => Start.IsUtc;
        public bool IsLocal => Start.IsLocal;

        [Pure]
        public static ZonedDay Create(DateTime dateTime, TimeZoneInfo timeZone)
        {
            var kind = timeZone.GetDateTimeKind();
            dateTime = DateTime.SpecifyKind( dateTime, kind );

            var (start, startDurationOffset) = CreateStartDateTime( dateTime, timeZone );
            var (end, endDurationOffset) = CreateEndDateTime( dateTime, timeZone );
            var duration = end.GetDurationOffset( start ).Add( startDurationOffset ).Add( endDurationOffset ).AddTicks( 1 );

            return new ZonedDay( start, end, duration );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDay Create(ZonedDateTime dateTime)
        {
            return Create( dateTime.Value, dateTime.TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDay CreateUtc(Timestamp timestamp)
        {
            return CreateUtc( timestamp.UtcValue );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDay CreateUtc(DateTime utcDateTime)
        {
            var start = ZonedDateTime.CreateUtc( utcDateTime.GetStartOfDay() );
            var end = ZonedDateTime.CreateUtc( utcDateTime.GetEndOfDay() );
            return new ZonedDay( start, end, Duration.FromHours( 24 ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDay CreateLocal(DateTime localDateTime)
        {
            return Create( localDateTime, TimeZoneInfo.Local );
        }

        [Pure]
        public override string ToString()
        {
            var value = Start.Value;
            var startUtcOffset = Start.UtcOffset;
            var endUtcOffset = End.UtcOffset;

            var dateText = $"{value.Year:0000}-{value.Month:00}-{value.Day:00}";

            var startUtcOffsetSign = startUtcOffset < Duration.Zero ? '-' : '+';
            var utcOffsetText = $"{startUtcOffsetSign}{Math.Abs( startUtcOffset.FullHours ):00}:{startUtcOffset.MinutesInHour:00}";

            if ( startUtcOffset != endUtcOffset )
            {
                var endUtcOffsetSign = endUtcOffset < Duration.Zero ? '-' : '+';
                utcOffsetText += $" {endUtcOffsetSign}{Math.Abs( endUtcOffset.FullHours ):00}:{endUtcOffset.MinutesInHour:00}";
            }

            return $"{dateText} {utcOffsetText} ({TimeZone.Id})";
        }

        [Pure]
        public override int GetHashCode()
        {
            return Hash.Default.Add( Start.Timestamp ).Add( End.Timestamp ).Add( TimeZone.Id ).Value;
        }

        [Pure]
        public override bool Equals(object obj)
        {
            return obj is ZonedDay d && Equals( d );
        }

        [Pure]
        public bool Equals(ZonedDay other)
        {
            return Start.Equals( other.Start );
        }

        [Pure]
        public int CompareTo(object obj)
        {
            return obj is ZonedDay d ? CompareTo( d ) : 1;
        }

        [Pure]
        public int CompareTo(ZonedDay other)
        {
            return Start.CompareTo( other.Start );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay ToTimeZone(TimeZoneInfo targetTimeZone)
        {
            return Create( Start.Value, targetTimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay ToUtcTimeZone()
        {
            return CreateUtc( Start.Value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay ToLocalTimeZone()
        {
            return ToTimeZone( TimeZoneInfo.Local );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains(ZonedDateTime dateTime)
        {
            var start = Start;
            return start.Value.Date == dateTime.ToTimeZone( start.TimeZone ).Value.Date;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay GetNext()
        {
            return AddDays( 1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay GetPrevious()
        {
            return AddDays( -1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay AddDays(int days)
        {
            var start = Start;
            var value = start.Value.AddDays( days );
            return Create( value, start.TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay Add(Period value)
        {
            var start = Start;
            var dateTime = start.Value.Add( value );
            return Create( dateTime, start.TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay SubtractDays(int days)
        {
            return AddDays( -days );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay Subtract(Period value)
        {
            return Add( -value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period GetPeriodOffset(ZonedDay start, PeriodUnits units)
        {
            return Start.GetPeriodOffset( start.Start, units );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period GetGreedyPeriodOffset(ZonedDay start, PeriodUnits units)
        {
            return Start.GetGreedyPeriodOffset( start.Start, units );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay SetYear(int year)
        {
            var start = Start;
            var value = start.Value.SetYear( year );
            return Create( value, start.TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay SetMonth(IsoMonthOfYear month)
        {
            var start = Start;
            var value = start.Value.SetMonth( month );
            return Create( value, start.TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay SetDayOfMonth(int day)
        {
            var start = Start;
            var value = start.Value.SetDayOfMonth( day );
            return Create( value, start.TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDay SetDayOfYear(int day)
        {
            var start = Start;
            var value = start.Value.SetDayOfYear( day );
            return Create( value, start.TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime GetDateTime(TimeOfDay timeOfDay)
        {
            return Start.SetTimeOfDay( timeOfDay );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime? TryGetDateTime(TimeOfDay timeOfDay)
        {
            return Start.TrySetTimeOfDay( timeOfDay );
        }

        [Pure]
        public Bounds<DateTime>? GetIntersectingInvalidityRange()
        {
            var start = Start;
            var timeZone = start.TimeZone;
            var startValue = start.Value;

            var activeRule = timeZone.GetActiveAdjustmentRule( startValue );
            if ( activeRule is null || activeRule.DaylightDelta == TimeSpan.Zero )
                return null;

            var transitionTime = activeRule.GetTransitionTimeWithInvalidity();
            var transitionStart = transitionTime.ToDateTime( startValue.Year );
            if ( transitionStart > startValue.GetEndOfDay() )
                return null;

            var transitionEnd = transitionStart.AddTicks( Math.Abs( activeRule.DaylightDelta.Ticks ) - 1 );
            if ( transitionEnd < startValue.GetStartOfDay() )
                return null;

            // NOTE: sanity check for adjustment rules internally marked as [Start/End]DateMarkerFor[End/Beginning]OfYear
            if ( ! timeZone.IsInvalidTime( transitionStart ) )
                return null;

            return Bounds.Create( transitionStart, transitionEnd );
        }

        [Pure]
        public Bounds<DateTime>? GetIntersectingAmbiguityRange()
        {
            var start = Start;
            var timeZone = start.TimeZone;
            var startValue = start.Value;

            var activeRule = timeZone.GetActiveAdjustmentRule( startValue );
            if ( activeRule is null || activeRule.DaylightDelta == TimeSpan.Zero )
                return null;

            var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();
            var transitionEnd = transitionTime.ToDateTime( startValue.Year ).AddTicks( -1 );
            if ( transitionEnd < startValue.GetStartOfDay() )
                return null;

            var transitionStart = transitionEnd.AddTicks( -Math.Abs( activeRule.DaylightDelta.Ticks ) + 1 );
            if ( transitionStart > startValue.GetEndOfDay() )
                return null;

            // NOTE: sanity check for adjustment rules internally marked as [Start/End]DateMarkerFor[End/Beginning]OfYear
            if ( ! timeZone.IsAmbiguousTime( transitionStart ) )
                return null;

            return Bounds.Create( transitionStart, transitionEnd );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static explicit operator ZonedDateTime(ZonedDay source)
        {
            return source.Start;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static explicit operator DateTime(ZonedDay source)
        {
            return source.Start.Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDay operator +(ZonedDay a, Period b)
        {
            return a.Add( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDay operator -(ZonedDay a, Period b)
        {
            return a.Subtract( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator ==(ZonedDay a, ZonedDay b)
        {
            return a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator !=(ZonedDay a, ZonedDay b)
        {
            return ! a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >(ZonedDay a, ZonedDay b)
        {
            return a.CompareTo( b ) > 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <=(ZonedDay a, ZonedDay b)
        {
            return a.CompareTo( b ) <= 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <(ZonedDay a, ZonedDay b)
        {
            return a.CompareTo( b ) < 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >=(ZonedDay a, ZonedDay b)
        {
            return a.CompareTo( b ) >= 0;
        }

        [Pure]
        private static (ZonedDateTime DateTime, Duration DurationOffset) CreateStartDateTime(DateTime value, TimeZoneInfo timeZone)
        {
            value = value.GetStartOfDay();

            var startInvalidity = timeZone.GetContainingInvalidityRange( value );
            if ( startInvalidity is not null )
            {
                value = DateTime.SpecifyKind( startInvalidity.Value.Max.AddTicks( 1 ), value.Kind );
                return (ZonedDateTime.CreateUnsafe( value, timeZone ), Duration.Zero);
            }

            var result = ZonedDateTime.CreateUnsafe( value, timeZone );
            var ambiguousResult = result.GetOppositeAmbiguousDateTime();
            if ( ambiguousResult is null )
                return (result, Duration.Zero);

            var activeRule = timeZone.GetActiveAdjustmentRule( value )!;
            var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();

            var durationOffset = new Duration( transitionTime.TimeOfDay.TimeOfDay )
                .SubtractTicks( Math.Abs( activeRule.DaylightDelta.Ticks ) );

            return (result.Timestamp < ambiguousResult.Value.Timestamp ? result : ambiguousResult.Value, durationOffset);
        }

        [Pure]
        private static (ZonedDateTime DateTime, Duration DurationOffset) CreateEndDateTime(DateTime value, TimeZoneInfo timeZone)
        {
            value = value.GetEndOfDay();

            var startInvalidity = timeZone.GetContainingInvalidityRange( value );
            if ( startInvalidity is not null )
            {
                value = DateTime.SpecifyKind( startInvalidity.Value.Min.AddTicks( -1 ), value.Kind );
                return (ZonedDateTime.CreateUnsafe( value, timeZone ), Duration.Zero);
            }

            var result = ZonedDateTime.CreateUnsafe( value, timeZone );
            var ambiguousResult = result.GetOppositeAmbiguousDateTime();
            if ( ambiguousResult is null )
                return (result, Duration.Zero);

            var activeRule = timeZone.GetActiveAdjustmentRule( value )!;
            var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();

            var durationOffset = new Duration( -transitionTime.TimeOfDay.TimeOfDay );

            return (result.Timestamp > ambiguousResult.Value.Timestamp ? result : ambiguousResult.Value, durationOffset);
        }
    }
}
