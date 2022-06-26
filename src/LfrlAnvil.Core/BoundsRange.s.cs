using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

public static class BoundsRange
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BoundsRange<T> Create<T>(Bounds<T> value)
        where T : IComparable<T>
    {
        return new BoundsRange<T>( value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BoundsRange<T> Create<T>(IEnumerable<Bounds<T>> range)
        where T : IComparable<T>
    {
        return new BoundsRange<T>( range );
    }

    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( BoundsRange<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
