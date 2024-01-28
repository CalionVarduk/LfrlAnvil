using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

public static class ReadOnlyArray
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReadOnlyArray<T> Create<T>(T[] source)
    {
        return new ReadOnlyArray<T>( source );
    }

    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( ReadOnlyArray<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
