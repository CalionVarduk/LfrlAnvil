using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Internal;

internal sealed class InvertedComparer<T> : IComparer<T>
{
    internal readonly IComparer<T> BaseComparer;

    internal InvertedComparer(IComparer<T> baseComparer)
    {
        BaseComparer = baseComparer;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Compare(T? x, T? y)
    {
        var result = BaseComparer.Compare( x, y );
        return -Math.Sign( result );
    }
}
