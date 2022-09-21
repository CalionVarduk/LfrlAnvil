using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

public static class Chain
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Chain<T> Create<T>(T value)
    {
        return new Chain<T>( value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Chain<T> Create<T>(IEnumerable<T> values)
    {
        return new Chain<T>( values );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Chain<T> Create<T>(Chain<T> other)
    {
        return new Chain<T>( other );
    }

    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Chain<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
