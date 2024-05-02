using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Exceptions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a <see cref="DateTime"/> with time zone.
/// </summary>
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

    /// <summary>
    /// <see cref="Chrono.Timestamp"/> equivalent to this date time.
    /// </summary>
    public Timestamp Timestamp { get; }

    /// <summary>
    /// Underlying <see cref="DateTime"/> value.
    /// </summary>
    public DateTime Value => _value ?? Timestamp.UtcValue;

    /// <summary>
    /// Year component.
    /// </summary>
    public int Year => Value.Year;

    /// <summary>
    /// Month component.
    /// </summary>
    public IsoMonthOfYear Month => Value.GetMonthOfYear();

    /// <summary>
    /// Day of month component.
    /// </summary>
    public int DayOfMonth => Value.Day;

    /// <summary>
    /// Day of year component.
    /// </summary>
    public int DayOfYear => Value.DayOfYear;

    /// <summary>
    /// Day of week component.
    /// </summary>
    public IsoDayOfWeek DayOfWeek => Value.GetDayOfWeek();

    /// <summary>
    /// Time zone of this date time.
    /// </summary>
    public TimeZoneInfo TimeZone => _timeZone ?? TimeZoneInfo.Utc;

    /// <summary>
    /// <see cref="TimeOfDay"/> component.
    /// </summary>
    public TimeOfDay TimeOfDay => new TimeOfDay( Value.TimeOfDay );

    /// <summary>
    /// Calculates the UTC offset of this date time.
    /// </summary>
    public Duration UtcOffset => new Duration( Value.Ticks - DateTime.UnixEpoch.Ticks - Timestamp.UnixEpochTicks );

    /// <summary>
    /// Checks whether or not the <see cref="TimeZone"/> is UTC.
    /// </summary>
    public bool IsUtc => ReferenceEquals( TimeZone, TimeZoneInfo.Utc );

    /// <summary>
    /// Checks whether or not the <see cref="TimeZone"/> is local.
    /// </summary>
    public bool IsLocal => ReferenceEquals( TimeZone, TimeZoneInfo.Local );

    /// <summary>
    /// Checks whether or not this date time is in daylight saving time.
    /// </summary>
    public bool IsInDaylightSavingTime => UtcOffset.Ticks != TimeZone.BaseUtcOffset.Ticks;

    /// <summary>
    /// Checks whether or not this date time is ambiguous.
    /// </summary>
    public bool IsAmbiguous => TimeZone.IsAmbiguousTime( Value );

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance.
    /// </summary>
    /// <param name="dateTime">Underlying date time.</param>
    /// <param name="timeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="InvalidZonedDateTimeException">
    /// When <paramref name="dateTime"/> is not valid in the given <paramref name="timeZone"/>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime Create(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var result = TryCreate( dateTime, timeZone );
        if ( result is null )
            ExceptionThrower.Throw( new InvalidZonedDateTimeException( dateTime, timeZone ) );

        return result.Value;
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedDateTime"/> instance.
    /// </summary>
    /// <param name="dateTime">Underlying date time.</param>
    /// <param name="timeZone">Target time zone.</param>
    /// <returns>
    /// New <see cref="ZonedDateTime"/> instance
    /// or null when <paramref name="dateTime"/> is not valid in the given <paramref name="timeZone"/>.
    /// </returns>
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

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="timestamp">Underlying timestamp.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime CreateUtc(Timestamp timestamp)
    {
        return new ZonedDateTime( timestamp, timestamp.UtcValue, TimeZoneInfo.Utc );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="utcDateTime">Underlying date time.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime CreateUtc(DateTime utcDateTime)
    {
        var timestamp = new Timestamp( utcDateTime );
        return CreateUtc( timestamp );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance in <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    /// <param name="localDateTime">Underlying date time.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="InvalidZonedDateTimeException">
    /// When <paramref name="localDateTime"/> is not valid in <see cref="TimeZoneInfo.Local"/> time zone.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime CreateLocal(DateTime localDateTime)
    {
        return Create( localDateTime, TimeZoneInfo.Local );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="ZonedDateTime"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var valueText = TextFormatting.StringifyDateTime( Value );
        var utcOffsetText = TextFormatting.StringifyOffset( UtcOffset );
        return $"{valueText} {utcOffsetText} ({TimeZone.Id})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default.Add( Timestamp ).Add( TimeZone.Id ).Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is ZonedDateTime dt && Equals( dt );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(ZonedDateTime other)
    {
        return Timestamp.Equals( other.Timestamp ) && string.Equals( TimeZone.Id, other.TimeZone.Id, StringComparison.Ordinal );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is ZonedDateTime dt ? CompareTo( dt ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(ZonedDateTime other)
    {
        var result = Timestamp.CompareTo( other.Timestamp );
        if ( result != 0 )
            return result;

        result = UtcOffset.CompareTo( other.UtcOffset );
        return result != 0 ? result : string.Compare( TimeZone.Id, other.TimeZone.Id, StringComparison.Ordinal );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> in the <paramref name="targetTimeZone"/> from this instance.
    /// </summary>
    /// <param name="targetTimeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime ToTimeZone(TimeZoneInfo targetTimeZone)
    {
        if ( ReferenceEquals( TimeZone, targetTimeZone ) )
            return this;

        var dateTime = TimeZoneInfo.ConvertTimeFromUtc( Timestamp.UtcValue, targetTimeZone );
        return new ZonedDateTime( Timestamp, dateTime, targetTimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> in <see cref="TimeZoneInfo.Utc"/> time zone from this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime ToUtcTimeZone()
    {
        return ToTimeZone( TimeZoneInfo.Utc );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> in <see cref="TimeZoneInfo.Local"/> time zone from this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime ToLocalTimeZone()
    {
        return ToTimeZone( TimeZoneInfo.Local );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by adding <paramref name="value"/> to this instance.
    /// </summary>
    /// <param name="value"><see cref="Duration"/> to add.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime Add(Duration value)
    {
        var timestamp = Timestamp.Add( value );
        var dateTime = TimeZoneInfo.ConvertTimeFromUtc( timestamp.UtcValue, TimeZone );
        return new ZonedDateTime( timestamp, dateTime, TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by adding <paramref name="value"/> to this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to add.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="InvalidZonedDateTimeException">When result is not valid in this instance's <see cref="TimeZone"/>.</exception>
    [Pure]
    public ZonedDateTime Add(Period value)
    {
        var dateTime = Value.Add( value );

        var result = Create( dateTime, TimeZone );
        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result );
        return result;
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedDateTime"/> instance by adding <paramref name="value"/> to this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to add.</param>
    /// <returns>
    /// New <see cref="ZonedDateTime"/> instance or null when result is not valid in this instance's <see cref="TimeZone"/>.
    /// </returns>
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

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by subtracting <paramref name="value"/> from this instance.
    /// </summary>
    /// <param name="value"><see cref="Duration"/> to subtract.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime Subtract(Duration value)
    {
        return Add( -value );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by subtracting <paramref name="value"/> from this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to subtract.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="InvalidZonedDateTimeException">When result is not valid in this instance's <see cref="TimeZone"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime Subtract(Period value)
    {
        return Add( -value );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedDateTime"/> instance by subtracting <paramref name="value"/> from this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to subtract.</param>
    /// <returns>
    /// New <see cref="ZonedDateTime"/> instance or null when result is not valid in this instance's <see cref="TimeZone"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime? TrySubtract(Period value)
    {
        return TryAdd( -value );
    }

    /// <summary>
    /// Calculates a difference in <see cref="Duration"/> between this instance and the <paramref name="start"/> instance,
    /// where this instance is treated as the end of the range.
    /// </summary>
    /// <param name="start">Instance to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration GetDurationOffset(ZonedDateTime start)
    {
        return Timestamp.Subtract( start.Timestamp );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between this instance and
    /// the <paramref name="start"/> instance, where this instance is treated as the end of the range,
    /// using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="start">Start date time.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetPeriodOffset(ZonedDateTime start, PeriodUnits units)
    {
        return Value.GetPeriodOffset( start.Value, units );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between this instance and
    /// the <paramref name="start"/> instance, where this instance is treated as the end of the range,
    /// using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="start">Start date time.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    /// <remarks>Greedy <see cref="Period"/> may contain components with negative values.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetGreedyPeriodOffset(ZonedDateTime start, PeriodUnits units)
    {
        return Value.GetGreedyPeriodOffset( start.Value, units );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by setting the <see cref="ZonedDateTime.Year"/> component in this instance.
    /// </summary>
    /// <param name="year">Year to set.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    /// <remarks>
    /// Result may end up with modified components other than the year,
    /// if it lands in the range of invalid values of this instance's <see cref="TimeZone"/>.
    /// </remarks>
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

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by setting the <see cref="ZonedDateTime.Month"/> component in this instance.
    /// </summary>
    /// <param name="month">Month to set.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <remarks>
    /// Result may end up with modified components other than the month,
    /// if it lands in the range of invalid values of this instance's <see cref="TimeZone"/>.
    /// </remarks>
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
            if ( dateTime.Month != ( int )month )
                dateTime = invalidity.Value.Max.AddTicks( 1 );

            return CreateUnsafe( DateTime.SpecifyKind( dateTime, value.Kind ), timeZone );
        }

        var result = CreateUnsafe( dateTime, timeZone );
        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result );
        return result;
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by setting the <see cref="ZonedDateTime.DayOfMonth"/> component in this instance.
    /// </summary>
    /// <param name="day">Day of month to set.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="day"/> is not valid for the current month.</exception>
    /// <remarks>
    /// Result may end up with modified components other than the day of month,
    /// if it lands in the range of invalid values of this instance's <see cref="TimeZone"/>.
    /// </remarks>
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

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by setting the <see cref="ZonedDateTime.DayOfYear"/> component in this instance.
    /// </summary>
    /// <param name="day">Day of year to set.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="day"/> is not valid for the current year.</exception>
    /// <remarks>
    /// Result may end up with modified components other than the day of year,
    /// if it lands in the range of invalid values of this instance's <see cref="TimeZone"/>.
    /// </remarks>
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

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by setting the <see cref="ZonedDateTime.TimeOfDay"/> component in this instance.
    /// </summary>
    /// <param name="timeOfDay">Time of day to set.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="InvalidZonedDateTimeException">When result is not valid in this instance's <see cref="TimeZone"/>.</exception>
    [Pure]
    public ZonedDateTime SetTimeOfDay(TimeOfDay timeOfDay)
    {
        var dateTime = Value.SetTimeOfDay( timeOfDay );
        var result = Create( dateTime, TimeZone );
        result = CorrelatePotentialAmbiguityWithDaylightSavingTime( result );
        return result;
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedDateTime"/> instance by setting the <see cref="ZonedDateTime.TimeOfDay"/>
    /// component in this instance.
    /// </summary>
    /// <param name="timeOfDay">Time of day to set.</param>
    /// <returns>
    /// New <see cref="ZonedDateTime"/> instance or null when result is not valid in this instance's <see cref="TimeZone"/>.
    /// </returns>
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

    /// <summary>
    /// Attempts to create a new <see cref="ZonedDateTime"/> instance by calculating the other version of an ambiguous value,
    /// if this instance represents an ambiguous date time in its <see cref="TimeZone"/>.
    /// </summary>
    /// <returns>New <see cref="ZonedDateTime"/> instance or null when this instance is not ambiguous.</returns>
    [Pure]
    public ZonedDateTime? GetOppositeAmbiguousDateTime()
    {
        if ( ! IsAmbiguous )
            return null;

        var value = Value;
        var timeZone = TimeZone;
        var activeAdjustmentRule = timeZone.GetActiveAdjustmentRule( value );
        Assume.IsNotNull( activeAdjustmentRule );
        var daylightDelta = new Duration( activeAdjustmentRule.DaylightDelta );

        return IsInDaylightSavingTime
            ? new ZonedDateTime( Timestamp.Add( daylightDelta ), value, timeZone )
            : new ZonedDateTime( Timestamp.Subtract( daylightDelta ), value, timeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that contains this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetDay()
    {
        return ZonedDay.Create( this );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance that contains this instance.
    /// </summary>
    /// <param name="weekStart">First day of the week.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek GetWeek(IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return ZonedWeek.Create( this, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that contains this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetMonth()
    {
        return ZonedMonth.Create( this );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance that contains this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear GetYear()
    {
        return ZonedYear.Create( this );
    }

    /// <summary>
    /// Coverts the provided <paramref name="source"/> to <see cref="DateTime"/>.
    /// </summary>
    /// <param name="source">Value to convert.</param>
    /// <returns><see cref="Value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator DateTime(ZonedDateTime source)
    {
        return source.Value;
    }

    /// <summary>
    /// Coverts the provided <paramref name="source"/> to <see cref="Chrono.Timestamp"/>.
    /// </summary>
    /// <param name="source">Value to convert.</param>
    /// <returns><see cref="Timestamp"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator Timestamp(ZonedDateTime source)
    {
        return source.Timestamp;
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime operator +(ZonedDateTime a, Duration b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="InvalidZonedDateTimeException">When result is not valid in the given <see cref="TimeZone"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime operator +(ZonedDateTime a, Period b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime operator -(ZonedDateTime a, Duration b)
    {
        return a.Subtract( b );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="InvalidZonedDateTimeException">When result is not valid in the given <see cref="TimeZone"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime operator -(ZonedDateTime a, Period b)
    {
        return a.Subtract( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(ZonedDateTime a, ZonedDateTime b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(ZonedDateTime a, ZonedDateTime b)
    {
        return ! a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(ZonedDateTime a, ZonedDateTime b)
    {
        return a.CompareTo( b ) > 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(ZonedDateTime a, ZonedDateTime b)
    {
        return a.CompareTo( b ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(ZonedDateTime a, ZonedDateTime b)
    {
        return a.CompareTo( b ) < 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
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
