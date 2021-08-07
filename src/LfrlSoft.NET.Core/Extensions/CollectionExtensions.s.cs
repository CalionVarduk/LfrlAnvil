using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class CollectionExtensions
    {
        [Pure]
        public static bool IsNullOrEmpty<T>(this IReadOnlyCollection<T>? source)
        {
            return source is null || source.IsEmpty();
        }

        [Pure]
        public static bool IsEmpty<T>(this IReadOnlyCollection<T> source)
        {
            return source.Count == 0;
        }

        [Pure]
        public static bool ContainsAtLeast<T>(this IReadOnlyCollection<T> source, int count)
        {
            return source.Count >= count;
        }

        [Pure]
        public static bool ContainsAtMost<T>(this IReadOnlyCollection<T> source, int count)
        {
            return source.Count <= count;
        }

        [Pure]
        public static bool ContainsBetween<T>(this IReadOnlyCollection<T> source, int minCount, int maxCount)
        {
            return source.Count >= minCount && source.Count <= maxCount;
        }

        [Pure]
        public static bool ContainsExactly<T>(this IReadOnlyCollection<T> source, int count)
        {
            return source.Count == count;
        }
    }
}
