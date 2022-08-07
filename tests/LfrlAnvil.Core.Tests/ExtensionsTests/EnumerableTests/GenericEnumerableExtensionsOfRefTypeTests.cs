using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.ExtensionsTests.EnumerableTests;

public abstract class GenericEnumerableExtensionsOfRefTypeTests<T> : GenericEnumerableExtensionsTests<T>
    where T : class
{
    [Fact]
    public void WhereNotNull_ShouldFilterOutNullElements()
    {
        var expected = Fixture.CreateMany<T>().ToList();
        var sut = expected.Append( Fixture.CreateDefault<T>() );

        var result = sut.WhereNotNull();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void WhereNotNull_ShouldReturnEnumerableEquivalentToSourceWhenNoNullElementsExist()
    {
        var sut = Fixture.CreateMany<T>().ToList();
        var result = sut.WhereNotNull();
        result.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void WhereNotNull_ShouldFilterOutNullElements_WithExplicitComparer()
    {
        var expected = Fixture.CreateMany<T>().ToList();
        var sut = expected.Append( Fixture.CreateDefault<T>() );

        var result = sut.WhereNotNull( EqualityComparer<T>.Default );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void WhereNotNull_ShouldReturnEnumerableEquivalentToSourceWhenNoNullElementsExist_WithExplicitComparer()
    {
        var sut = Fixture.CreateMany<T>().ToList();
        var result = sut.WhereNotNull( EqualityComparer<T>.Default );
        result.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ContainsNull_ShouldReturnTrueWhenSourceContainsNullElement()
    {
        var sut = Fixture.CreateMany<T>().Append( Fixture.CreateDefault<T>() );
        var result = sut.ContainsNull();
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsNull_ShouldReturnFalseWhenSourceContainsNullElement()
    {
        var sut = Fixture.CreateMany<T>();
        var result = sut.ContainsNull();
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsNull_ShouldReturnTrueWhenSourceContainsNullElement_WithExplicitComparer()
    {
        var sut = Fixture.CreateMany<T>().Append( Fixture.CreateDefault<T>() );
        var result = sut.ContainsNull( EqualityComparer<T?>.Default );
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsNull_ShouldReturnFalseWhenSourceContainsNullElement_WithExplicitComparer()
    {
        var sut = Fixture.CreateMany<T>();
        var result = sut.ContainsNull( EqualityComparer<T?>.Default );
        result.Should().BeFalse();
    }
}
