using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Collections.Internal;
using LfrlSoft.NET.Core.Functional;
using Unsafe = LfrlSoft.NET.Core.Functional.Unsafe;

namespace LfrlSoft.NET.Core.Extensions
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
            return new MemoizedEnumerable<T>( source() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Unsafe<T> TryInvoke<T>(this Func<T> source)
        {
            return Unsafe.Try( source );
        }
    }
}
