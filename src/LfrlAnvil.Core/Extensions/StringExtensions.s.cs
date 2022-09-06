using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

public static class StringExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringSlice AsSlice(this string source)
    {
        return new StringSlice( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringSlice AsSlice(this string source, int startIndex)
    {
        return new StringSlice( source, startIndex );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringSlice AsSlice(this string source, int startIndex, int length)
    {
        return new StringSlice( source, startIndex, length );
    }
}
