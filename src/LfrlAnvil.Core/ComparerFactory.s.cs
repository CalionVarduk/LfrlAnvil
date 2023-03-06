using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

public static class ComparerFactory<T>
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IComparer<T> CreateBy<TValue>(Func<T?, TValue?> selector)
    {
        return CreateBy( selector, Comparer<TValue>.Default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IComparer<T> CreateBy<TValue>(Func<T?, TValue?> selector, IComparer<TValue> valueComparer)
    {
        return Comparer<T>.Create( (a, b) => valueComparer.Compare( selector( a ), selector( b ) ) );
    }
}
