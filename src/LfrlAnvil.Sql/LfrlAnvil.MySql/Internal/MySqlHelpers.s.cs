using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.MySql.Internal;

internal static class MySqlHelpers
{
    private const int StackallocThreshold = 64;
    private const char TextDelimiter = '\'';
    private const char BlobMarker = 'X';
    private static readonly string EmptyTextLiteral = $"{TextDelimiter}{TextDelimiter}";
    private static readonly string EmptyBlobLiteral = $"{BlobMarker}{EmptyTextLiteral}";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T CastOrThrow<T>(object obj)
    {
        if ( obj is T t )
            return t;

        ExceptionThrower.Throw( new SqlObjectCastException( MySqlDialect.Instance, typeof( T ), obj.GetType() ) );
        return default!;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(bool value)
    {
        return value ? "1" : "0";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(ulong value)
    {
        return value.ToString( CultureInfo.InvariantCulture );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(long value)
    {
        return value.ToString( CultureInfo.InvariantCulture );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(double value)
    {
        var result = value.ToString( "G17", CultureInfo.InvariantCulture );
        return IsFloatingPoint( result ) ? result : $"{result}.0";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(float value)
    {
        var result = value.ToString( "G9", CultureInfo.InvariantCulture );
        return IsFloatingPoint( result ) ? result : $"{result}.0";
    }

    [Pure]
    public static string GetDbLiteral(string value)
    {
        var delimiterIndex = value.IndexOf( TextDelimiter );
        if ( delimiterIndex == -1 )
            return value.Length == 0 ? EmptyTextLiteral : $"{TextDelimiter}{value}{TextDelimiter}";

        var delimiterCount = GetDelimiterCount( value.AsSpan( delimiterIndex + 1 ) ) + 1;

        var length = checked( value.Length + delimiterCount + 2 );
        var data = length <= StackallocThreshold ? stackalloc char[length] : new char[length];
        data[0] = TextDelimiter;

        var startIndex = 0;
        var buffer = data.Slice( 1, data.Length - 2 );

        do
        {
            var segment = value.AsSpan( startIndex, delimiterIndex - startIndex );
            segment.CopyTo( buffer );
            buffer[segment.Length] = TextDelimiter;
            buffer[segment.Length + 1] = TextDelimiter;
            buffer = buffer.Slice( segment.Length + 2 );

            startIndex = delimiterIndex + 1;
            delimiterIndex = value.IndexOf( TextDelimiter, startIndex );
        }
        while ( delimiterIndex != -1 );

        value.AsSpan( startIndex ).CopyTo( buffer );
        data[^1] = TextDelimiter;
        return new string( data );

        [Pure]
        static int GetDelimiterCount(ReadOnlySpan<char> text)
        {
            var count = 0;
            for ( var i = 0; i < text.Length; ++i )
            {
                if ( text[i] == TextDelimiter )
                    ++count;
            }

            return count;
        }
    }

    [Pure]
    public static string GetDbLiteral(ReadOnlySpan<byte> value)
    {
        if ( value.Length == 0 )
            return EmptyBlobLiteral;

        var length = checked( (value.Length << 1) + 3 );
        var data = length <= StackallocThreshold ? stackalloc char[length] : new char[length];
        data[0] = BlobMarker;
        data[1] = TextDelimiter;
        var index = 2;

        for ( var i = 0; i < value.Length; ++i )
        {
            var b = value[i];
            data[index++] = ToHexChar( b >> 4 );
            data[index++] = ToHexChar( b & 0xF );
        }

        data[^1] = TextDelimiter;
        return new string( data );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static char ToHexChar(int value)
        {
            Assume.IsInRange( value, 0, 15 );
            return (char)(value < 10 ? '0' + value : 'A' + value - 10);
        }
    }

    [Pure]
    private static bool IsFloatingPoint(string value)
    {
        foreach ( var c in value )
        {
            if ( c == '.' || char.ToLower( c ) == 'e' )
                return true;
        }

        return false;
    }
}
