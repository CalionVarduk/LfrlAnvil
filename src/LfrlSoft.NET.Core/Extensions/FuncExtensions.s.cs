using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlSoft.NET.Core.Collections.Internal;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class FuncExtensions
    {
        [Pure]
        public static Lazy<T> ToLazy<T>(this Func<T> source)
        {
            return new Lazy<T>( source );
        }

        [Pure]
        public static IEnumerable<T> Memoize<T>(this Func<IEnumerable<T>> source)
        {
            return new MemoizedEnumerable<T>( source() );
        }
    }
}
