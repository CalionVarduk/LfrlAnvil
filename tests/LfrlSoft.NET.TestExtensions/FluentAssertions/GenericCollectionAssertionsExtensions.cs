using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Collections;

namespace LfrlSoft.NET.TestExtensions.FluentAssertions
{
    public static class GenericCollectionAssertionsExtensions
    {
        public static AndConstraint<GenericCollectionAssertions<T>> BeSequentiallyEqualTo<T>(
            this GenericCollectionAssertions<T> source,
            params T?[] expected)
        {
            return source.HaveSameCount( expected ).And.ContainInOrder( expected );
        }

        public static AndConstraint<GenericCollectionAssertions<T>> BeSequentiallyEqualTo<T>(
            this GenericCollectionAssertions<T> source,
            IEnumerable<T?>? expected,
            string because = "",
            params object[] becauseArgs)
        {
            return source.ContainInOrder( expected, because, becauseArgs ).And.HaveSameCount( expected, because, becauseArgs );
        }
    }
}
