using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Exceptions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

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
    public int Year => Value.Year;
    public IsoMonthOfYear Month => Value.GetMonthOfYear();
    public int DayOfMonth => Value.Day;
    public int DayOfYear => Value.DayOfYear;
    public IsoDayOfWeek DayOfWeek => Value.GetDayOfWeek();
    public TimeZoneInfo TimeZone => _timeZone ?? TimeZoneInfo.Utc;
    public TimeOfDay TimeOfDay => new TimeOfDay( Value.TimeOfDay );
    public Duration UtcOffset => new Duration( Value.Ticks - DateTime.UnixEpoch.Ticks - Timestamp.UnixEpochTicks );
    public bool IsUtc => ReferenceEquals( TimeZone, TimeZoneInfo.Utc );
    public bool IsLocal => ReferenceEquals( TimeZone, TimeZoneInfo.Local );
    public bool IsInDaylightSavingTime => UtcOffset.Ticks != TimeZone.BaseUtcOffset.Ticks;
    public bool IsAmbiguous => TimeZone.IsAmbiguousTime( Value );

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime Create(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var result = TryCreate( dateTime, timeZone );
        if ( result is null )
            throw new InvalidZonedDateTimeException( dateTime, timeZone );

        return result.Value;
    }

    [Pure]
    public static ZonedDateTime? TryCreate(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var kind = timeZone.GetDateTimeKind();
        dateTime = DateTime.SpecifyKind( dateTime, kind );

        if ( timeZone.IsInvalidTime( dateTime ) )
            return null;

        var result = CreateUnsafe( dateTime, timeZone );
        return result;
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
        var valueText = TextFormatting.StringifyDateTime( Value );
        var utcOffsetText = TextFormatting.StringifyOffset( UtcOffset );
        return $"{valueText} {utcOffsetText} ({TimeZone.Id})";
    }

    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default.Add( Timestamp ).Add( TimeZone.Id ).Value;
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is ZonedDateTime dt && Equals( dt );
    }

    [Pure]
    public bool Equals(ZonedDateTime other)
    {
        return Timestamp.Equals( other.Timestamp ) && string.Equals( TimeZone.Id, other.TimeZone.Id, StringComparison.Ordinal );
    }

    [Pure]
    public int CompareTo(object? obj)
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
        if ( ReferenceEquals( TimeZone, targetTimeZone ) )
            return this;

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

    [Pure]
    public ZonedDateTime Add(Period value)
    {
        var dateTime = Value.Add( value );

        var result = Create( dateTime, TimeZone );
        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result );
        return result;
    }

    [Pure]
    public ZonedDateTime? TryAdd(Period value)
    {
        var dateTime = Value.Add( value );

        var result = TryCreate( dateTime, TimeZone );
        if ( result is null )
            return null;

        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result.Value );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime Subtract(Duration value)
    {
        return Add( -value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime Subtract(Period value)
    {
        return Add( -value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime? TrySubtract(Period value)
    {
        return TryAdd( -value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration GetDurationOffset(ZonedDateTime start)
    {
        return Timestamp.Subtract( start.Timestamp );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetPeriodOffset(ZonedDateTime start, PeriodUnits units)
    {
        return Value.GetPeriodOffset( start.Value, units );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetGreedyPeriodOffset(ZonedDateTime start, PeriodUnits units)
    {
        return Value.GetGreedyPeriodOffset( start.Value, units );
    }

    [Pure]
    public ZonedDateTime SetYear(int year)
    {
        var value = Value;
        var timeZone = TimeZone;
        var dateTime = value.SetYear( year );

        var invalidity = timeZone.GetContainingInvalidityRange( dateTime );
        if ( invalidity is not null )
        {
            dateTime = invalidity.Value.Min.AddTicks( -1 );
            if ( dateTime.Year != year )
                dateTime = invalidity.Value.Max.AddTicks( 1 );

            return CreateUnsafe( DateTime.SpecifyKind( dateTime, value.Kind ), timeZone );
        }

        var result = CreateUnsafe( dateTime, timeZone );
        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result );
        return result;
    }

    [Pure]
    public ZonedDateTime SetMonth(IsoMonthOfYear month)
    {
        var value = Value;
        var timeZone = TimeZone;
        var dateTime = value.SetMonth( month );

        var invalidity = timeZone.GetContainingInvalidityRange( dateTime );
        if ( invalidity is not null )
        {
            dateTime = invalidity.Value.Min.AddTicks( -1 );
            if ( dateTime.Month != (int)month )
                dateTime = invalidity.Value.Max.AddTicks( 1 );

            return CreateUnsafe( DateTime.SpecifyKind( dateTime, value.Kind ), timeZone );
        }

        var result = CreateUnsafe( dateTime, timeZone );
        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result );
        return result;
    }

    [Pure]
    public ZonedDateTime SetDayOfMonth(int day)
    {
        var value = Value;
        var timeZone = TimeZone;
        var dateTime = value.SetDayOfMonth( day );

        var invalidity = timeZone.GetContainingInvalidityRange( dateTime );
        if ( invalidity is not null )
        {
            dateTime = invalidity.Value.Min.AddTicks( -1 );
            if ( dateTime.Day != day )
                dateTime = invalidity.Value.Max.AddTicks( 1 );

            return CreateUnsafe( DateTime.SpecifyKind( dateTime, value.Kind ), timeZone );
        }

        var result = CreateUnsafe( dateTime, timeZone );
        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result );
        return result;
    }

    [Pure]
    public ZonedDateTime SetDayOfYear(int day)
    {
        var value = Value;
        var timeZone = TimeZone;
        var dateTime = value.SetDayOfYear( day );

        var invalidity = timeZone.GetContainingInvalidityRange( dateTime );
        if ( invalidity is not null )
        {
            dateTime = invalidity.Value.Min.AddTicks( -1 );
            if ( dateTime.DayOfYear != day )
                dateTime = invalidity.Value.Max.AddTicks( 1 );

            return CreateUnsafe( DateTime.SpecifyKind( dateTime, value.Kind ), timeZone );
        }

        var result = CreateUnsafe( dateTime, timeZone );
        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result );
        return result;
    }

    [Pure]
    public ZonedDateTime SetTimeOfDay(TimeOfDay timeOfDay)
    {
        var dateTime = Value.SetTimeOfDay( timeOfDay );
        var result = Create( dateTime, TimeZone );
        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result );
        return result;
    }

    [Pure]
    public ZonedDateTime? TrySetTimeOfDay(TimeOfDay timeOfDay)
    {
        var dateTime = Value.SetTimeOfDay( timeOfDay );
        var result = TryCreate( dateTime, TimeZone );
        if ( result is null )
            return null;

        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result.Value );
        return result;
    }

    [Pure]
    public ZonedDateTime? GetOppositeAmbiguousDateTime()
    {
        if ( ! IsAmbiguous )
            return null;

        var value = Value;
        var timeZone = TimeZone;
        var activeAdjustmentRule = timeZone.GetActiveAdjustmentRule( value );
        var daylightDelta = new Duration( activeAdjustmentRule!.DaylightDelta );

        return IsInDaylightSavingTime
            ? new ZonedDateTime( Timestamp.Add( daylightDelta ), value, timeZone )
            : new ZonedDateTime( Timestamp.Subtract( daylightDelta ), value, timeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetDay()
    {
        return ZonedDay.Create( this );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek GetWeek(IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return ZonedWeek.Create( this, weekStart );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetMonth()
    {
        return ZonedMonth.Create( this );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear GetYear()
    {
        return ZonedYear.Create( this );
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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime operator +(ZonedDateTime a, Period b)
    {
        return a.Add( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime operator -(ZonedDateTime a, Duration b)
    {
        return a.Subtract( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime operator -(ZonedDateTime a, Period b)
    {
        return a.Subtract( b );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ZonedDateTime CreateUnsafe(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc( dateTime, timeZone );
        var timestamp = new Timestamp( utcDateTime );
        return new ZonedDateTime( timestamp, dateTime, timeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ZonedDateTime CorrelatePotentialAmbiguityWithDaylightSavingTime(
        ZonedDateTime target)
    {
        var ambiguousResult = target.GetOppositeAmbiguousDateTime();
        if ( ambiguousResult is null )
            return target;

        return IsInDaylightSavingTime ? ambiguousResult.Value : target;
    }
}
