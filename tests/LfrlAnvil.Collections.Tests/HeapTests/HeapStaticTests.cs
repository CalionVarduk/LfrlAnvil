using FluentAssertions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Collections.Tests.HeapTests;

public class HeapStaticTests : TestsBase
{
    [Theory]
    [InlineData( 1, 0 )]
    [InlineData( 2, 0 )]
    [InlineData( 3, 1 )]
    [InlineData( 4, 1 )]
    [InlineData( 5, 2 )]
    [InlineData( 6, 2 )]
    [InlineData( 7, 3 )]
    [InlineData( 8, 3 )]
    [InlineData( 9, 4 )]
    [InlineData( 10, 4 )]
    public void GetParentIndex_ShouldReturnCorrectResult(int childIndex, int expected)
    {
        var result = Heap.GetParentIndex( childIndex );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 1, 3 )]
    [InlineData( 2, 5 )]
    [InlineData( 3, 7 )]
    [InlineData( 4, 9 )]
    [InlineData( 5, 11 )]
    [InlineData( 6, 13 )]
    [InlineData( 7, 15 )]
    [InlineData( 8, 17 )]
    [InlineData( 9, 19 )]
    [InlineData( 10, 21 )]
    public void GetLeftChildIndex_ShouldReturnCorrectResult(int parentIndex, int expected)
    {
        var result = Heap.GetLeftChildIndex( parentIndex );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, 2 )]
    [InlineData( 1, 4 )]
    [InlineData( 2, 6 )]
    [InlineData( 3, 8 )]
    [InlineData( 4, 10 )]
    [InlineData( 5, 12 )]
    [InlineData( 6, 14 )]
    [InlineData( 7, 16 )]
    [InlineData( 8, 18 )]
    [InlineData( 9, 20 )]
    [InlineData( 10, 22 )]
    public void GetRightChildIndex_ShouldReturnCorrectResult(int parentIndex, int expected)
    {
        var result = Heap.GetRightChildIndex( parentIndex );

        result.Should().Be( expected );
    }
}
