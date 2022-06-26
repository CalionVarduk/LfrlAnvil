using System;
using FluentAssertions;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.BoundsTests;

public abstract class GenericBoundsExtensionsTests<T> : TestsBase
    where T : IComparable<T>
{
    [Fact]
    public void AsEnumerable_ShouldReturnResultWithMinAndMax()
    {
        var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        var sut = Bounds.Create( min, max );

        var result = sut.AsEnumerable();

        result.Should().BeSequentiallyEqualTo( min, max );
    }
}
