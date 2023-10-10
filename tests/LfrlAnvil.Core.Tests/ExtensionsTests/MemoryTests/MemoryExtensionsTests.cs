using System.Collections.Generic;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.ExtensionsTests.MemoryTests;

public class MemoryExtensionsTests : TestsBase
{
    [Fact]
    public void GetEnumerator_ForReadOnlyMemory_ShouldBeEquivalentToSpanEnumerator()
    {
        ReadOnlyMemory<int> sut = new[] { 1, 2, 3 }.AsMemory();

        var result = new List<int>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( 1, 2, 3 );
    }

    [Fact]
    public void GetEnumerator_ForMemory_ShouldBeEquivalentToSpanEnumerator()
    {
        var sut = new[] { 1, 2, 3 }.AsMemory();

        var result = new List<int>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( 1, 2, 3 );
    }
}
