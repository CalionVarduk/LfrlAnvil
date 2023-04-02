using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

public static class StringExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringSegment AsSegment(this string source)
    {
        return new StringSegment( source );
    }

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
}
