using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Internal;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

public static class SqliteHelpers
{
    public const string MemoryDataSource = ":memory:";
    public const long TemporalYearUnit = 0;
    public const long TemporalMonthUnit = 1;
    public const long TemporalWeekOfYearUnit = 2;
    public const long TemporalHourUnit = 3;
    public const long TemporalMinuteUnit = 4;
    public const long TemporalSecondUnit = 5;
    public const long TemporalMillisecondUnit = 6;
    public const long TemporalMicrosecondUnit = 7;
    public const long TemporalNanosecondUnit = 8;
    public const long TemporalDayOfMonthUnit = 9;
    public const long TemporalDayOfYearUnit = 10;
    public const long TemporalDayOfWeekUnit = 11;
    public const string UpsertExcludedRecordSetName = "EXCLUDED";
    public static readonly SqlSchemaObjectName DefaultVersionHistoryName = SqlSchemaObjectName.Create( SqlHelpers.VersionHistoryName );

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConnectionStringEntry[] ExtractConnectionStringEntries(SqliteConnectionStringBuilder builder)
    {
        var i = 0;
        var result = new SqlConnectionStringEntry[builder.Count];
        foreach ( var e in builder )
        {
            var (key, value) = (KeyValuePair<string, object>)e;
            result[i++] = new SqlConnectionStringEntry( key, value, IsMutableConnectionStringKey( key ) );
        }

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExtendConnectionString(ReadOnlyArray<SqlConnectionStringEntry> entries, string options)
    {
        var builder = new SqliteConnectionStringBuilder( options );
        foreach ( var (key, value, isMutable) in entries )
        {
            if ( ! isMutable || ! builder.ShouldSerialize( key ) )
                builder.Add( key, value );
        }

        return builder.ToString();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsMutableConnectionStringKey(string key)
    {
        return ! key.Equals( "Data Source", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "DataSource", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "Filename", StringComparison.OrdinalIgnoreCase );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetFullName(string schemaName, string name)
    {
        return SqlHelpers.GetFullName( schemaName, name, separator: '_' );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetFullName(string schemaName, string recordSetName, string name)
    {
        return SqlHelpers.GetFullName( schemaName, recordSetName, name, firstSeparator: '_' );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static long GetDbTemporalUnit(SqlTemporalUnit unit)
    {
        Assume.IsDefined( unit );
        return unit switch
        {
            SqlTemporalUnit.Year => TemporalYearUnit,
            SqlTemporalUnit.Month => TemporalMonthUnit,
            SqlTemporalUnit.Week => TemporalWeekOfYearUnit,
            SqlTemporalUnit.Day => TemporalDayOfMonthUnit,
            SqlTemporalUnit.Hour => TemporalHourUnit,
            SqlTemporalUnit.Minute => TemporalMinuteUnit,
            SqlTemporalUnit.Second => TemporalSecondUnit,
            SqlTemporalUnit.Millisecond => TemporalMillisecondUnit,
            SqlTemporalUnit.Microsecond => TemporalMicrosecondUnit,
            _ => TemporalNanosecondUnit
        };
    }

    [Pure]
    public static string DbGetCurrentDate()
    {
        return DateTime.Now.ToString( SqlHelpers.DateFormat, CultureInfo.InvariantCulture );
    }

    [Pure]
    public static string DbGetCurrentTime()
    {
        return DateTime.Now.ToString( SqlHelpers.TimeFormat, CultureInfo.InvariantCulture );
    }

    [Pure]
    public static string DbGetCurrentDateTime()
    {
        return DateTime.Now.ToString( SqlHelpers.DateTimeFormat, CultureInfo.InvariantCulture );
    }

    [Pure]
    public static string DbGetCurrentUtcDateTime()
    {
        return DateTime.UtcNow.ToString( SqlHelpers.DateTimeFormat, CultureInfo.InvariantCulture );
    }

    [Pure]
    public static long DbGetCurrentTimestamp()
    {
        return DateTime.UtcNow.Ticks;
    }

    [Pure]
    public static byte[] DbNewGuid()
    {
        return Guid.NewGuid().ToByteArray();
    }

    [Pure]
    public static string? DbToLower(string? s)
    {
        return s?.ToLowerInvariant();
    }

    [Pure]
    public static string? DbToUpper(string? s)
    {
        return s?.ToUpperInvariant();
    }

    [Pure]
    public static long? DbInstrLast(string? s, string? v)
    {
        return s is not null && v is not null ? s.LastIndexOf( v, StringComparison.Ordinal ) + 1 : null;
    }

    [Pure]
    public static string? DbReverse(string? s)
    {
        return s?.Reverse();
    }

    [Pure]
    public static double? DbTrunc2(double? d, int? p)
    {
        return d is not null ? Math.Round( d.Value, p ?? 0, MidpointRounding.ToZero ) : null;
    }

    [Pure]
    public static string? DbTimeOfDay(string? s)
    {
        return TryParseDateTime( s )?.ToString( SqlHelpers.TimeFormat, CultureInfo.InvariantCulture );
    }

    [Pure]
    public static long? DbExtractTemporal(long? u, string? s)
    {
        if ( u is null )
            return null;

        return u switch
        {
            TemporalYearUnit => TryParseTime( s ) is null ? TryParseDateTime( s )?.Year : null,
            TemporalMonthUnit => TryParseTime( s ) is null ? TryParseDateTime( s )?.Month : null,
            TemporalWeekOfYearUnit => TryGetIsoWeekOfYear( TryParseTime( s ) is null ? TryParseDateTime( s ) : null ),
            TemporalDayOfYearUnit => TryParseTime( s ) is null ? TryParseDateTime( s )?.DayOfYear : null,
            TemporalDayOfMonthUnit => TryParseTime( s ) is null ? TryParseDateTime( s )?.Day : null,
            TemporalDayOfWeekUnit => TryParseTime( s ) is null ? (int?)TryParseDateTime( s )?.DayOfWeek : null,
            TemporalHourUnit => TryParseDateTime( s )?.Hour,
            TemporalMinuteUnit => TryParseDateTime( s )?.Minute,
            TemporalSecondUnit => TryParseDateTime( s )?.Second,
            TemporalMillisecondUnit => TryParseDateTime( s )?.Millisecond,
            TemporalMicrosecondUnit => TryParseDateTime( s )?.Ticks % TimeSpan.TicksPerSecond / (1000 / TimeSpan.NanosecondsPerTick),
            TemporalNanosecondUnit => TryParseDateTime( s )?.Ticks % TimeSpan.TicksPerSecond * TimeSpan.NanosecondsPerTick,
            _ => null
        };
    }

    [Pure]
    public static string? DbTemporalAdd(long? u, long? v, string? s)
    {
        if ( u is null || v is null )
            return null;

        return u switch
        {
            TemporalYearUnit => AddMonths( s, v.Value * 12 ),
            TemporalMonthUnit => AddMonths( s, v.Value ),
            TemporalWeekOfYearUnit => AddDays( s, v.Value * 7 ),
            TemporalDayOfMonthUnit => AddDays( s, v.Value ),
            TemporalHourUnit => AddTicks( s, v.Value * TimeSpan.TicksPerHour ),
            TemporalMinuteUnit => AddTicks( s, v.Value * TimeSpan.TicksPerMinute ),
            TemporalSecondUnit => AddTicks( s, v.Value * TimeSpan.TicksPerSecond ),
            TemporalMillisecondUnit => AddTicks( s, v.Value * TimeSpan.TicksPerMillisecond ),
            TemporalMicrosecondUnit => AddTicks( s, v.Value * TimeSpan.TicksPerMicrosecond ),
            TemporalNanosecondUnit => AddTicks( s, v.Value / TimeSpan.NanosecondsPerTick ),
            _ => null
        };
    }

    [Pure]
    public static long? DbTemporalDiff(long? u, string? l, string? r)
    {
        if ( u is null )
            return null;

        return u switch
        {
            TemporalYearUnit => MonthsDiff( l, r ) / 12,
            TemporalMonthUnit => MonthsDiff( l, r ),
            TemporalWeekOfYearUnit => DaysDiff( l, r ) / 7,
            TemporalDayOfMonthUnit => DaysDiff( l, r ),
            TemporalHourUnit => TicksDiff( l, r ) / TimeSpan.TicksPerHour,
            TemporalMinuteUnit => TicksDiff( l, r ) / TimeSpan.TicksPerMinute,
            TemporalSecondUnit => TicksDiff( l, r ) / TimeSpan.TicksPerSecond,
            TemporalMillisecondUnit => TicksDiff( l, r ) / TimeSpan.TicksPerMillisecond,
            TemporalMicrosecondUnit => TicksDiff( l, r ) / TimeSpan.TicksPerMicrosecond,
            TemporalNanosecondUnit => TicksDiff( l, r ) * TimeSpan.NanosecondsPerTick,
            _ => null
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string? AddMonths(string? s, long value)
    {
        if ( TryParseTime( s ) is not null )
            return null;

        var date = TryParseDate( s );
        if ( date is not null )
            return date.Value.AddMonths( (int)value ).ToString( SqlHelpers.DateFormat, CultureInfo.InvariantCulture );

        var dateTime = TryParseDateTime( s );
        return dateTime?.AddMonths( (int)value ).ToString( SqlHelpers.DateTimeFormat, CultureInfo.InvariantCulture );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string? AddDays(string? s, long value)
    {
        if ( TryParseTime( s ) is not null )
            return null;

        var date = TryParseDate( s );
        if ( date is not null )
            return date.Value.AddDays( (int)value ).ToString( SqlHelpers.DateFormat, CultureInfo.InvariantCulture );

        var dateTime = TryParseDateTime( s );
        return dateTime?.AddDays( value ).ToString( SqlHelpers.DateTimeFormat, CultureInfo.InvariantCulture );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string? AddTicks(string? s, long value)
    {
        var time = TryParseTime( s );
        if ( time is not null )
            return time.Value.Add( TimeSpan.FromTicks( value ) ).ToString( SqlHelpers.TimeFormat, CultureInfo.InvariantCulture );

        var dateTime = TryParseDateTime( s );
        return dateTime?.AddTicks( value ).ToString( SqlHelpers.DateTimeFormat, CultureInfo.InvariantCulture );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long? MonthsDiff(string? l, string? r)
    {
        if ( TryParseTime( l ) is not null || TryParseTime( r ) is not null )
            return null;

        var lDateTime = TryParseDateTime( l );
        if ( lDateTime is null )
            return null;

        var rDateTime = TryParseDateTime( r );
        if ( rDateTime is null )
            return null;

        var negate = false;
        if ( lDateTime.Value > rDateTime.Value )
        {
            negate = true;
            (lDateTime, rDateTime) = (rDateTime, lDateTime);
        }

        var yearDiff = rDateTime.Value.Year - lDateTime.Value.Year;
        var monthDiff = rDateTime.Value.Month - lDateTime.Value.Month;
        var result = yearDiff * 12 + monthDiff;

        var originalDay = rDateTime.Value.Day;
        rDateTime = rDateTime.Value.AddMonths( -result );
        var currentDay = rDateTime.Value.Day;
        if ( originalDay != currentDay )
            rDateTime = rDateTime.Value.AddDays( originalDay - currentDay );

        if ( rDateTime.Value < lDateTime.Value )
            --result;

        return negate ? -result : result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long? DaysDiff(string? l, string? r)
    {
        if ( TryParseTime( l ) is not null || TryParseTime( r ) is not null )
            return null;

        var lDateTime = TryParseDateTime( l );
        return lDateTime is null ? null : (TryParseDateTime( r ) - lDateTime.Value)?.Ticks / TimeSpan.TicksPerDay;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long? TicksDiff(string? l, string? r)
    {
        var lTime = TryParseTime( l );
        if ( lTime is not null )
        {
            var rTime = TryParseTime( r );
            if ( rTime is null )
                return null;

            return lTime.Value <= rTime.Value
                ? (rTime.Value - lTime.Value).Ticks
                : -(lTime.Value - rTime.Value).Ticks;
        }

        if ( TryParseTime( r ) is not null )
            return null;

        var lDateTime = TryParseDateTime( l );
        return lDateTime is null ? null : (TryParseDateTime( r ) - lDateTime.Value)?.Ticks;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DateTime? TryParseDateTime(string? s)
    {
        return DateTime.TryParse( s, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out var dt ) ? dt : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DateOnly? TryParseDate(string? s)
    {
        return DateOnly.TryParse( s, out var d ) ? d : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static TimeOnly? TryParseTime(string? s)
    {
        return TimeOnly.TryParse( s, out var t ) ? t : null;
    }

    private static int? TryGetIsoWeekOfYear(DateTime? dt)
    {
        return dt is null ? null : ISOWeek.GetWeekOfYear( dt.Value );
    }
}
