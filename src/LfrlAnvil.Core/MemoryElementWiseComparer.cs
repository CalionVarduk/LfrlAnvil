using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

public sealed class MemoryElementWiseComparer<T> : IEqualityComparer<ReadOnlyMemory<T>>
{
    [Pure]
    public bool Equals(ReadOnlyMemory<T> x, ReadOnlyMemory<T> y)
    {
        if ( x.Length != y.Length )
            return false;

        var xSpan = x.Span;
        var ySpan = y.Span;

        for ( var i = 0; i < xSpan.Length; ++i )
        {
            if ( ! Generic<T>.AreEqual( xSpan[i], ySpan[i] ) )
                return false;
        }

        return true;
    }

    [Pure]
    public int GetHashCode(ReadOnlyMemory<T> obj)
    {
        var result = Hash.Default;
        foreach ( var o in obj.Span )
            result = result.Add( o );

        return result.Value;
    }
}
