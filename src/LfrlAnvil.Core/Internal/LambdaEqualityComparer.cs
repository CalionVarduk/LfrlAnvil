using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Internal;

internal sealed class LambdaEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T?, T?, bool> _equals;
    private readonly Func<T, int> _getHashCode;

    internal LambdaEqualityComparer(Func<T?, T?, bool> equals)
    {
        _equals = equals;
        _getHashCode = Generic<T>.GetHashCode;
    }

    internal LambdaEqualityComparer(Func<T?, T?, bool> equals, Func<T, int> getHashCode)
    {
        _equals = equals;
        _getHashCode = getHashCode;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(T? x, T? y)
    {
        return _equals( x, y );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int GetHashCode(T obj)
    {
        return _getHashCode( obj );
    }
}
