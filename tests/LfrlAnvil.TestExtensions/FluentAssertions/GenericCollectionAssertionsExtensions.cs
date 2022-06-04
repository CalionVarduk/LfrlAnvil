using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Collections;

namespace LfrlAnvil.TestExtensions.FluentAssertions
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
            IEnumerable<T>? expected,
            string because = "",
            params object[] becauseArgs)
        {
            return source.ContainInOrder( expected, because, becauseArgs ).And.HaveSameCount( expected, because, becauseArgs );
        }

        public static AndConstraint<GenericCollectionAssertions<T>> BeEmptyOrOnlyContain<T>(
            this SelfReferencingCollectionAssertions<T, GenericCollectionAssertions<T>> source,
            Expression<Func<T, bool>> predicate,
            string because = "",
            params object[] becauseArgs)
        {
            if ( source.Subject is null || source.Subject.Any() )
                return source.OnlyContain( predicate, because, becauseArgs );

            return source.BeEmpty( because, becauseArgs );
        }
    }
}
