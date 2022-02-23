using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions
{
    public static class FuncExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Lazy<T> ToLazy<T>(this Func<T> source)
        {
            return new Lazy<T>( source );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> Memoize<T>(this Func<IEnumerable<T>> source)
        {
            return source().Memoize();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Action IgnoreResult<T>(this Func<T> source)
        {
            return () =>
            {
                var _ = source();
            };
        }
    }
}
