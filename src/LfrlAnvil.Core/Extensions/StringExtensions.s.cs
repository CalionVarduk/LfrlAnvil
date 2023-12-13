using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

public static class StringExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringSegment AsSegment(this string source, int startIndex)
    {
        return new StringSegment( source, startIndex );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringSegment AsSegment(this string source, int startIndex, int length)
    {
        return new StringSegment( source, startIndex, length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string Reverse(this string source)
    {
        const int stackallocThreshold = 64;
        if ( source.Length <= 1 )
            return source;

        var buffer = source.Length > stackallocThreshold ? new char[source.Length] : stackalloc char[source.Length];
        var sourceSpan = source.AsSpan();

        do
        {
            var charLength = StringInfo.GetNextTextElementLength( sourceSpan );
            sourceSpan.Slice( 0, charLength ).CopyTo( buffer.Slice( sourceSpan.Length - charLength ) );
            sourceSpan = sourceSpan.Slice( charLength );
        }
        while ( sourceSpan.Length > 0 );

        return new string( buffer );
    }
}
