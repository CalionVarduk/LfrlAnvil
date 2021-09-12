using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class BoundsExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> AsEnumerable<T>(this Bounds<T> source)
            where T : IComparable<T>
        {
            yield return source.Min;
            yield return source.Max;
        }
    }
}
