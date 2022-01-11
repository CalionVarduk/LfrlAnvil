using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Chrono.Exceptions;

namespace LfrlSoft.NET.Core.Chrono
{
    public readonly struct ZonedDateTime : IEquatable<ZonedDateTime>, IComparable<ZonedDateTime>, IComparable
    {
        private readonly DateTime? _value;
        private readonly TimeZoneInfo? _timeZone;

        private ZonedDateTime(Timestamp timestamp, DateTime value, TimeZoneInfo timeZone)
        {
            Timestamp = timestamp;
            _value = value;
            _timeZone = timeZone;
        }

        public Timestamp Timestamp { get; }
        public DateTime Value => _value ?? Timestamp.UtcValue;
        public TimeZoneInfo TimeZone => _timeZone ?? TimeZoneInfo.Utc;
        public TimeOfDay TimeOfDay => new TimeOfDay( Value.TimeOfDay );
        public Duration UtcOffset => new Duration( Value.Ticks - DateTime.UnixEpoch.Ticks - Timestamp.UnixEpochTicks );
        public bool IsUtc => ReferenceEquals( TimeZone, TimeZoneInfo.Utc );
        public bool IsLocal => ReferenceEquals( TimeZone, TimeZoneInfo.Local );
        public bool IsInDaylightSavingTime => UtcOffset.Ticks != TimeZone.BaseUtcOffset.Ticks;
        public bool IsAmbiguous => TimeZone.IsAmbiguousTime( Value );

        [Pure]
        public static ZonedDateTime Create(DateTime dateTime, TimeZoneInfo timeZone)
        {
            var kind = GetExpectedDateTimeKind( timeZone );
            dateTime = DateTime.SpecifyKind( dateTime, kind );

            if ( timeZone.IsInvalidTime( dateTime ) )
                throw new InvalidZonedDateTimeException( dateTime, timeZone );

            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc( dateTime, timeZone );
            var timestamp = new Timestamp( utcDateTime );

            return new ZonedDateTime( timestamp, dateTime, timeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDateTime CreateUtc(Timestamp timestamp)
        {
            return new ZonedDateTime( timestamp, timestamp.UtcValue, TimeZoneInfo.Utc );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDateTime CreateUtc(DateTime utcDateTime)
        {
            var timestamp = new Timestamp( utcDateTime );
            return CreateUtc( timestamp );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDateTime CreateLocal(DateTime localDateTime)
        {
            return Create( localDateTime, TimeZoneInfo.Local );
        }

        [Pure]
        public override string ToString()
        {
            var value = Value;
            var utcOffset = UtcOffset;
            var ticksInSecond = value.Millisecond * Constants.TicksPerMillisecond + value.Ticks % Constants.TicksPerMillisecond;

            var dateText = $"{value.Year:0000}-{value.Month:00}-{value.Day:00}";
            var timeText = $"{value.Hour:00}:{value.Minute:00}:{value.Second:00}.{ticksInSecond:0000000}";

            var utcOffsetSign = utcOffset < Duration.Zero ? '-' : '+';
            var utcOffsetText = $"{utcOffsetSign}{Math.Abs( utcOffset.FullHours ):00}:{utcOffset.MinutesInHour:00}";

            return $"{dateText} {timeText} {utcOffsetText} ({TimeZone.Id})";
        }

        [Pure]
        public override int GetHashCode()
        {
            return Hash.Default.Add( Timestamp ).Add( TimeZone.Id ).Value;
        }

        [Pure]
        public override bool Equals(object obj)
        {
            return obj is ZonedDateTime dt && Equals( dt );
        }

        [Pure]
        public bool Equals(ZonedDateTime other)
        {
            return Timestamp.Equals( other.Timestamp ) && TimeZone.Id.Equals( other.TimeZone.Id );
        }

        [Pure]
        public int CompareTo(object obj)
        {
            return obj is ZonedDateTime dt ? CompareTo( dt ) : 1;
        }

        [Pure]
        public int CompareTo(ZonedDateTime other)
        {
            var result = Timestamp.CompareTo( other.Timestamp );
            if ( result != 0 )
                return result;

            result = UtcOffset.CompareTo( other.UtcOffset );
            return result != 0 ? result : string.Compare( TimeZone.Id, other.TimeZone.Id, StringComparison.Ordinal );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime ToTimeZone(TimeZoneInfo targetTimeZone)
        {
            var dateTime = TimeZoneInfo.ConvertTimeFromUtc( Timestamp.UtcValue, targetTimeZone );
            return new ZonedDateTime( Timestamp, dateTime, targetTimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime ToUtcTimeZone()
        {
            return ToTimeZone( TimeZoneInfo.Utc );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime ToLocalTimeZone()
        {
            return ToTimeZone( TimeZoneInfo.Local );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime Add(Duration value)
        {
            var timestamp = Timestamp.Add( value );
            var dateTime = TimeZoneInfo.ConvertTimeFromUtc( timestamp.UtcValue, TimeZone );
            return new ZonedDateTime( timestamp, dateTime, TimeZone );
        }

        // TODO (LF): new method -> Add(Period value) & remove AddX below

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime AddYears(int value)
        {
            return Create( Value.AddYears( value ), TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime AddMonths(int value)
        {
            return Create( Value.AddMonths( value ), TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime AddWeeks(int value)
        {
            return AddDays( value * Constants.DaysPerWeek );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime AddDays(int value)
        {
            return Create( Value.AddDays( value ), TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime Subtract(Duration value)
        {
            return Add( -value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration GetDurationOffset(ZonedDateTime start)
        {
            return Timestamp.Subtract( start.Timestamp );
        }

        // TODO (LF): new method -> GetPeriodOffset(ZonedDateTime start)
        // TODO (LF): new method -> GetUnbalancedPeriodOffset(ZonedDateTime start)

        // TODO (LF): add methods that allow to set component (year, month, dayinmonth, dayinweek (via enum), hour, minute, second, ms, tickinms) + time of day

        [Pure]
        public ZonedDateTime GetOppositeAmbiguousDateTime()
        {
            if ( ! IsAmbiguous )
                return this;

            var value = Value;

            var activeAdjustmentRule = TimeZone
                .GetAdjustmentRules()
                .First(r => r.DateStart <= value && r.DateEnd >= value );

            var daylightDelta = new Duration( activeAdjustmentRule.DaylightDelta );

            return IsInDaylightSavingTime
                ? new ZonedDateTime( Timestamp.Add( daylightDelta ), Value, TimeZone )
                : new ZonedDateTime( Timestamp.Subtract( daylightDelta ), Value, TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static explicit operator DateTime(ZonedDateTime source)
        {
            return source.Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static explicit operator Timestamp(ZonedDateTime source)
        {
            return source.Timestamp;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDateTime operator +(ZonedDateTime a, Duration b)
        {
            return a.Add( b );
        }

        // TODO (LF): new operator -> Add(ZonedDateTime a, Period b)

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDateTime operator -(ZonedDateTime a, Duration b)
        {
            return a.Subtract( b );
        }

        // TODO (LF): new operator -> Subtract(ZonedDateTime a, Period b)

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator ==(ZonedDateTime a, ZonedDateTime b)
        {
            return a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator !=(ZonedDateTime a, ZonedDateTime b)
        {
            return ! a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >(ZonedDateTime a, ZonedDateTime b)
        {
            return a.CompareTo( b ) > 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <=(ZonedDateTime a, ZonedDateTime b)
        {
            return a.CompareTo( b ) <= 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <(ZonedDateTime a, ZonedDateTime b)
        {
            return a.CompareTo( b ) < 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >=(ZonedDateTime a, ZonedDateTime b)
        {
            return a.CompareTo( b ) >= 0;
        }

        private static DateTimeKind GetExpectedDateTimeKind(TimeZoneInfo timeZone)
        {
            if ( ReferenceEquals( timeZone, TimeZoneInfo.Utc ) )
                return DateTimeKind.Utc;

            if ( ReferenceEquals( timeZone, TimeZoneInfo.Local ) )
                return DateTimeKind.Local;

            return DateTimeKind.Unspecified;
        }
    }
}
