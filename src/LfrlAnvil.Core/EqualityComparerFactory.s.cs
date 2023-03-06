using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

public static class EqualityComparerFactory<T>
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEqualityComparer<T> Create(Func<T?, T?, bool> equalityComparer)
    {
        return new LambdaEqualityComparer<T>( equalityComparer );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEqualityComparer<T> Create(Func<T?, T?, bool> equalityComparer, Func<T, int> hashCodeCalculator)
    {
        return new LambdaEqualityComparer<T>( equalityComparer, hashCodeCalculator );
    }
}
