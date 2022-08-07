using System.Linq;
using LfrlAnvil.Collections.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Collections.Tests.InternalTests;

public abstract class GenericCollectionCopyingTests<T> : TestsBase
{
    [Fact]
    public void CopyTo_ShouldCopyAllItemsToArrayAtValidIndex()
    {
        var allItems = Fixture.CreateDistinctCollection<T>( 4 );
        var items = allItems.Take( 3 ).ToList();
        var arrayItem = allItems[^1];

        var arr = new[]
        {
            arrayItem,
            arrayItem,
            arrayItem,
            arrayItem,
            arrayItem
        };

        CollectionCopying.CopyTo( items, arr, 1 );

        arr.Should().BeSequentiallyEqualTo( arrayItem, items[0], items[1], items[2], arrayItem );
    }

    [Fact]
    public void CopyTo_ShouldCopyItemsToArrayAtValidIndexAndNotExceedArrayLength()
    {
        var allItems = Fixture.CreateDistinctCollection<T>( 4 );
        var items = allItems.Take( 3 ).ToList();
        var arrayItem = allItems[^1];

        var arr = new[]
        {
            arrayItem,
            arrayItem,
            arrayItem
        };

        CollectionCopying.CopyTo( items, arr, 1 );

        arr.Should().BeSequentiallyEqualTo( arrayItem, items[0], items[1] );
    }

    [Fact]
    public void CopyTo_ShouldCopyItemsToArrayWithValidOffset_WhenIndexIsNegative()
    {
        var allItems = Fixture.CreateDistinctCollection<T>( 4 );
        var items = allItems.Take( 3 ).ToList();
        var arrayItem = allItems[^1];

        var arr = new[]
        {
            arrayItem,
            arrayItem,
            arrayItem
        };

        CollectionCopying.CopyTo( items, arr, -1 );

        arr.Should().BeSequentiallyEqualTo( items[1], items[2], arrayItem );
    }

    [Fact]
    public void CopyTo_ShouldCopyItemsToArrayWithValidOffset_WhenIndexIsNegativeAndArrayIsTooSmall()
    {
        var allItems = Fixture.CreateDistinctCollection<T>( 6 );
        var items = allItems.Take( 5 ).ToList();
        var arrayItem = allItems[^1];

        var arr = new[]
        {
            arrayItem,
            arrayItem,
            arrayItem
        };

        CollectionCopying.CopyTo( items, arr, -1 );

        arr.Should().BeSequentiallyEqualTo( items[1], items[2], items[3] );
    }

    [Theory]
    [InlineData( 3 )]
    [InlineData( -3 )]
    public void CopyTo_ShouldDoNothing_WhenIndexIsOutOfBounds(int arrayIndex)
    {
        var allItems = Fixture.CreateDistinctCollection<T>( 4 );
        var items = allItems.Take( 3 ).ToList();
        var arrayItem = allItems[^1];

        var arr = new[]
        {
            arrayItem,
            arrayItem,
            arrayItem
        };

        CollectionCopying.CopyTo( items, arr, arrayIndex );

        arr.Should().BeSequentiallyEqualTo( arrayItem, arrayItem, arrayItem );
    }
}
