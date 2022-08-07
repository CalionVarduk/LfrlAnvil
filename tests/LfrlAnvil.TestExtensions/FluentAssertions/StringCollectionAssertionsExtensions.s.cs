using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Collections;

namespace LfrlAnvil.TestExtensions.FluentAssertions;

public static class StringCollectionAssertionsExtensions
{
    public static AndConstraint<StringCollectionAssertions> BeSequentiallyEqualTo(
        this StringCollectionAssertions source,
        params string?[] expected)
    {
        return source.HaveSameCount( expected ).And.ContainInOrder( expected );
    }

    public static AndConstraint<StringCollectionAssertions> BeSequentiallyEqualTo(
        this StringCollectionAssertions source,
        IEnumerable<string>? expected,
        string because = "",
        params object[] becauseArgs)
    {
        return source.ContainInOrder( expected, because, becauseArgs ).And.HaveSameCount( expected, because, becauseArgs );
    }
}