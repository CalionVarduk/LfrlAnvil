using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal;

public static class SqliteHelpers
{
    public const string MemoryDataSource = ":memory:";
    public static readonly SqlSchemaObjectName DefaultVersionHistoryName = SqlSchemaObjectName.Create( SqlHelpers.VersionHistoryName );

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
    public static string DbGetCurrentDate()
    {
        return DateTime.Now.ToString( "yyyy-MM-dd", CultureInfo.InvariantCulture );
    }

    [Pure]
    public static string DbGetCurrentTime()
    {
        return DateTime.Now.ToString( "HH:mm:ss.fffffff", CultureInfo.InvariantCulture );
    }

    [Pure]
    public static string DbGetCurrentDateTime()
    {
        return DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture );
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
}
