using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.ExtensionsTests.EnumerableTests;

public class EnumerableExtensionsOfCharTests : TestsBase
{
    [Fact]
    public void ToMemory_ShouldReturnStringAsMemory_WhenUnderlyingCollectionIsOfStringType()
    {
        var sut = Fixture.Create<string>();
        var result = sut.ToMemory();
        result.Should().Be( sut.AsMemory() );
    }

    [Fact]
    public void ToMemory_ShouldReturnArrayAsMemory_WhenUnderlyingCollectionIsOfArrayType()
    {
        var sut = Fixture.CreateMany<char>( count: 10 ).ToArray();
        var result = sut.ToMemory();
        result.Should().Be( sut.AsMemory() );
    }

    [Fact]
    public void ToMemory_ShouldCreateNewArrayAndReturnItAsMemory_WhenUnderlyingCollectionIsNotEmptyAndNotOfArrayType()
    {
        var sut = Fixture.CreateMany<char>( count: 10 ).ToList();
        var result = sut.ToMemory();
        result.ToArray().Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ToMemory_ShouldReturnEmptyMemory_WhenUnderlyingCollectionIsEmpty()
    {
        var sut = Enumerable.Empty<char>();
        var result = sut.ToMemory();
        result.Should().Be( ReadOnlyMemory<char>.Empty );
    }
}
