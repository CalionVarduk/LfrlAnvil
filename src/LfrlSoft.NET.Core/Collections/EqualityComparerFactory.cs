using System;
using System.Collections.Generic;
using LfrlSoft.NET.Core.Collections.Internal;

namespace LfrlSoft.NET.Core.Collections
{
    public static class EqualityComparerFactory<T>
    {
        public static IEqualityComparer<T> CreateDefault()
        {
            return EqualityComparer<T>.Default;
        }

        public static IEqualityComparer<T> Create(Func<T, T, bool> equalityComparer)
        {
            return new LambdaEqualityComparer<T>( equalityComparer );
        }

        public static IEqualityComparer<T> Create(Func<T, T, bool> equalityComparer, Func<T, int> hashCodeCalculator)
        {
            return new LambdaEqualityComparer<T>( equalityComparer, hashCodeCalculator );
        }
    }
}
