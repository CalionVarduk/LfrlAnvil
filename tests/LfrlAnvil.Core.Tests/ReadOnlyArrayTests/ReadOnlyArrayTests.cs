using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.ReadOnlyArrayTests;

public class ReadOnlyArrayTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnEmptyArray()
    {
        var sut = ReadOnlyArray<int>.Empty;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_ShouldReturnCorrectArray()
    {
        var source = Fixture.CreateMany<string>( count: 10 ).ToArray();
        var sut = ReadOnlyArray.Create( source );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( source.Length );
            sut.Should().BeSequentiallyEqualTo( source );
        }
    }

    [Theory]
    [InlineData( 0, "a" )]
    [InlineData( 1, "b" )]
    [InlineData( 2, "c" )]
    public void Indexer_Get_ShouldReturnCorrectElement(int index, string expected)
    {
        var source = new[] { "a", "b", "c" };
        var sut = new ReadOnlyArray<string>( source );

        var result = sut[index];

        result.Should().Be( expected );
    }

    [Fact]
    public void From_ShouldReturnCorrectArray()
    {
        var source = Fixture.CreateMany<string>( count: 10 ).ToArray();
        var sut = ReadOnlyArray<IEnumerable<char>>.From( ReadOnlyArray.Create( source ) );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( source.Length );
            sut.Should().BeSequentiallyEqualTo( source );
        }
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectResult()
    {
        var source = Fixture.CreateMany<string>( count: 10 ).ToArray();
        var sut = ReadOnlyArray.Create( source );

        var result = sut.AsMemory();

        result.ToArray().Should().BeSequentiallyEqualTo( source );
    }

    [Fact]
    public void AsSpan_ShouldReturnCorrectResult()
    {
        var source = Fixture.CreateMany<string>( count: 10 ).ToArray();
        var sut = ReadOnlyArray.Create( source );

        var result = sut.AsSpan();

        result.ToArray().Should().BeSequentiallyEqualTo( source );
    }

    [Fact]
    public void GetUnderlyingArray_ShouldReturnSourceArray()
    {
        var source = Fixture.CreateMany<string>().ToArray();
        var sut = ReadOnlyArray.Create( source );

        var result = sut.GetUnderlyingArray();

        result.Should().BeSameAs( source );
    }

    [Fact]
    public void ReadOnlyArrayConversionOperator_ShouldReturnCorrectArray()
    {
        var source = Fixture.CreateMany<string>( count: 10 ).ToArray();
        var sut = (ReadOnlyArray<string>)source;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( source.Length );
            sut.Should().BeSequentiallyEqualTo( source );
        }
    }
}
