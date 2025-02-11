using System.Collections.Generic;
using LfrlAnvil.Extensions;

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

        result.TestSequence( [ 1, 2, 3 ] ).Go();
    }

    [Fact]
    public void GetEnumerator_ForMemory_ShouldBeEquivalentToSpanEnumerator()
    {
        var sut = new[] { 1, 2, 3 }.AsMemory();

        var result = new List<int>();
        foreach ( var e in sut )
            result.Add( e );

        result.TestSequence( [ 1, 2, 3 ] ).Go();
    }
}
