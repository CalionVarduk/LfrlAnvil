using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Collections.Internal;

namespace LfrlSoft.NET.Core.Collections
{
    public static class EqualityComparerFactory<T>
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEqualityComparer<T> CreateDefault()
        {
            return EqualityComparer<T>.Default;
        }

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
}
