using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal;

public static class PostgreSqlHelpers
{
    public const string ByteaMarker = "\\x";
    public const string ByteaTypeCast = "::BYTEA";
    public const string EmptyByteaLiteral = $"'{ByteaMarker}'{ByteaTypeCast}";
    public const string UpsertExcludedRecordSetName = "EXCLUDED";

    public static readonly SqlSchemaObjectName DefaultVersionHistoryName =
        SqlSchemaObjectName.Create( "public", SqlHelpers.VersionHistoryName );

    [Pure]
    public static string GetDbLiteral(bool value)
    {
        return value ? "1::BOOLEAN" : "0::BOOLEAN";
    }

    [Pure]
    public static string GetDbLiteral(ReadOnlySpan<byte> value)
    {
        if ( value.Length == 0 )
            return EmptyByteaLiteral;

        var length = checked( (value.Length << 1) + ByteaMarker.Length + ByteaTypeCast.Length + 2 );
        var data = length <= SqlHelpers.StackallocThreshold ? stackalloc char[length] : new char[length];
        data[0] = SqlHelpers.TextDelimiter;
        data[1] = ByteaMarker[0];
        data[2] = ByteaMarker[1];
        var index = 3;

        for ( var i = 0; i < value.Length; ++i )
        {
            var b = value[i];
            data[index++] = ToHexChar( b >> 4 );
            data[index++] = ToHexChar( b & 0xF );
        }

        data[index++] = SqlHelpers.TextDelimiter;
        ByteaTypeCast.CopyTo( data.Slice( index ) );
        return new string( data );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static char ToHexChar(int value)
        {
            Assume.IsInRange( value, 0, 15 );
            return (char)(value < 10 ? '0' + value : 'A' + value - 10);
        }
    }
}
