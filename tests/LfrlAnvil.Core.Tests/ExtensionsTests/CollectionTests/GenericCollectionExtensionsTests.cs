using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.Memory;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.ExtensionsTests.CollectionTests;

public abstract class GenericCollectionExtensionsTests<T> : TestsBase
{
    [Fact]
    public void EmptyIfNull_ShouldReturnSource_WhenSourceIsNotNull()
    {
        var sut = Fixture.CreateMany<T>( count: 3 ).ToList();
        var result = sut.EmptyIfNull();
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void EmptyIfNull_ShouldReturnEmptyArray_WhenSourceIsNull()
    {
        IReadOnlyCollection<T>? sut = null;
        var result = sut.EmptyIfNull();
        result.Should().BeSameAs( Array.Empty<T>() );
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueWhenSourceIsNull()
    {
        IReadOnlyCollection<T>? sut = null;
        var result = sut.IsNullOrEmpty();
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueWhenSourceHasNoElements()
    {
        var sut = Array.Empty<T>();
        var result = sut.IsNullOrEmpty();
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 3 )]
    public void IsNullOrEmpty_ShouldReturnFalseWhenSourceHasSomeElements(int count)
    {
        var sut = Fixture.CreateMany<T>( count ).ToList();
        var result = sut.IsNullOrEmpty();
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEmpty_ShouldReturnTrueWhenSourceHasNoElements()
    {
        var sut = Array.Empty<T>();
        var result = sut.IsEmpty();
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 3 )]
    public void IsEmpty_ShouldReturnFalseWhenSourceHasSomeElements(int count)
    {
        var sut = Fixture.CreateMany<T>( count ).ToList();
        var result = sut.IsEmpty();
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 0, -1, true )]
    [InlineData( 0, 0, true )]
    [InlineData( 0, 1, false )]
    [InlineData( 1, -1, true )]
    [InlineData( 1, 0, true )]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, false )]
    [InlineData( 3, -1, true )]
    [InlineData( 3, 0, true )]
    [InlineData( 3, 1, true )]
    [InlineData( 3, 2, true )]
    [InlineData( 3, 3, true )]
    [InlineData( 3, 4, false )]
    public void ContainsAtLeast_ShouldReturnCorrectResult(int sourceCount, int minCount, bool expected)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList();
        var result = sut.ContainsAtLeast( minCount );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, -1, false )]
    [InlineData( 0, 0, true )]
    [InlineData( 0, 1, true )]
    [InlineData( 1, -1, false )]
    [InlineData( 1, 0, false )]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, true )]
    [InlineData( 3, -1, false )]
    [InlineData( 3, 0, false )]
    [InlineData( 3, 1, false )]
    [InlineData( 3, 2, false )]
    [InlineData( 3, 3, true )]
    [InlineData( 3, 4, true )]
    public void ContainsAtMost_ShouldReturnCorrectResult(int sourceCount, int maxCount, bool expected)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList();
        var result = sut.ContainsAtMost( maxCount );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 3 )]
    public void ContainsBetween_ShouldReturnFalseWhenMaxCountIsLessThanMinCount(int count)
    {
        var (max, min) = Fixture.CreateDistinctSortedCollection<int>( 2 );
        var sut = Fixture.CreateMany<T>( count ).ToList();

        var result = sut.ContainsBetween( min, max );

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 0, -1, false )]
    [InlineData( 0, 0, true )]
    [InlineData( 0, 1, true )]
    [InlineData( 1, -1, false )]
    [InlineData( 1, 0, false )]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, true )]
    [InlineData( 3, -1, false )]
    [InlineData( 3, 0, false )]
    [InlineData( 3, 1, false )]
    [InlineData( 3, 2, false )]
    [InlineData( 3, 3, true )]
    [InlineData( 3, 4, true )]
    public void ContainsBetween_ShouldReturnCorrectResultWhenMinCountIsZero(int count, int maxCount, bool expected)
    {
        var sut = Fixture.CreateMany<T>( count ).ToList();
        var result = sut.ContainsBetween( 0, maxCount );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, -1, false )]
    [InlineData( 0, 0, true )]
    [InlineData( 0, 1, true )]
    [InlineData( 1, -1, false )]
    [InlineData( 1, 0, false )]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, true )]
    [InlineData( 3, -1, false )]
    [InlineData( 3, 0, false )]
    [InlineData( 3, 1, false )]
    [InlineData( 3, 2, false )]
    [InlineData( 3, 3, true )]
    [InlineData( 3, 4, true )]
    public void ContainsBetween_ShouldReturnCorrectResultWhenMinCountIsNegative(int count, int maxCount, bool expected)
    {
        var minCount = Fixture.CreateNegativeInt32();
        var sut = Fixture.CreateMany<T>( count ).ToList();

        var result = sut.ContainsBetween( minCount, maxCount );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 0, 2 )]
    [InlineData( 1, 2 )]
    [InlineData( 1, 3 )]
    [InlineData( 3, 4 )]
    [InlineData( 3, 5 )]
    public void ContainsBetween_ShouldReturnFalseWhenSourceCountIsLessThanMinCount(int count, int minCount)
    {
        var sut = Fixture.CreateMany<T>( count ).ToList();
        var result = sut.ContainsBetween( minCount, minCount + 1 );
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 3, 2 )]
    [InlineData( 4, 3 )]
    [InlineData( 4, 2 )]
    [InlineData( 5, 4 )]
    [InlineData( 5, 3 )]
    public void ContainsBetween_ShouldReturnFalseWhenSourceCountIsGreaterThanMaxCount(int count, int maxCount)
    {
        var sut = Fixture.CreateMany<T>( count ).ToList();
        var result = sut.ContainsBetween( maxCount - 1, maxCount );
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 1, 1, 1 )]
    [InlineData( 1, 1, 2 )]
    [InlineData( 1, 1, 3 )]
    [InlineData( 3, 1, 3 )]
    [InlineData( 3, 1, 4 )]
    [InlineData( 3, 1, 5 )]
    [InlineData( 3, 2, 3 )]
    [InlineData( 3, 2, 4 )]
    [InlineData( 3, 2, 5 )]
    [InlineData( 3, 3, 3 )]
    [InlineData( 3, 3, 4 )]
    [InlineData( 3, 3, 5 )]
    public void ContainsBetween_ShouldReturnTrueWhenSourceCountIsBetweenMinAndMaxCount(int sourceCount, int minCount, int maxCount)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList();
        var result = sut.ContainsBetween( minCount, maxCount );
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 3 )]
    public void ContainsExactly_ShouldReturnFalseWhenCountIsNegative(int sourceCount)
    {
        var count = Fixture.CreateNegativeInt32();
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList();

        var result = sut.ContainsExactly( count );

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 0, 0, true )]
    [InlineData( 0, 1, false )]
    [InlineData( 1, 0, false )]
    [InlineData( 1, 1, true )]
    [InlineData( 1, 2, false )]
    [InlineData( 3, 2, false )]
    [InlineData( 3, 3, true )]
    [InlineData( 3, 4, false )]
    public void ContainsExactly_ShouldReturnCorrectResultWhenCountIsNotNegative(int sourceCount, int count, bool expected)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList();
        var result = sut.ContainsExactly( count );
        result.Should().Be( expected );
    }

    [Fact]
    public void CopyTo_ShouldCopyElementsFromSourceToSpan_WhenSourceAndSpanHaveTheSameLength()
    {
        var pool = new MemorySequencePool<T>( 8 );
        var span = pool.Rent( 3 );
        var sut = Fixture.CreateMany<T>( count: 3 ).ToList();

        sut.CopyTo( span );

        span.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void CopyTo_ShouldCopyElementsFromSourceToSpan_WhenSpanIsLargerThanSource()
    {
        var elements = Fixture.CreateDistinctCollection<T>( count: 4 );
        var pool = new MemorySequencePool<T>( 8 );
        var span = pool.Rent( 4 );
        span[^1] = elements[^1];
        var sut = elements.Take( 3 ).ToList();

        sut.CopyTo( span );

        span.Should().BeSequentiallyEqualTo( elements );
    }

    [Fact]
    public void CopyTo_ShouldThrowArgumentException_WhenSourceIsLargerThanSpan()
    {
        var pool = new MemorySequencePool<T>( 8 );
        var span = pool.Rent( 2 );
        var sut = Fixture.CreateMany<T>( count: 3 ).ToList();

        var action = Lambda.Of( () => sut.CopyTo( span ) );

        action.Should().ThrowExactly<ArgumentException>();
    }
}
