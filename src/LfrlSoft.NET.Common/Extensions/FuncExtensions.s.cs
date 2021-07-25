using System;
using System.Collections.Generic;
using LfrlSoft.NET.Common.Collections.Internal;

namespace LfrlSoft.NET.Common.Extensions
{
    public static class FuncExtensions
    {
        public static Lazy<T> ToLazy<T>(this Func<T> source)
        {
            return new Lazy<T>( source );
        }

        public static IEnumerable<T> Memoize<T>(this Func<IEnumerable<T>> source)
        {
            return new MemoizedEnumeration<T>( source() );
        }
    }
}
