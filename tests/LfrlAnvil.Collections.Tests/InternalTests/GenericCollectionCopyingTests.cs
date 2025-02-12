using System.Linq;
using LfrlAnvil.Collections.Internal;

namespace LfrlAnvil.Collections.Tests.InternalTests;

public abstract class GenericCollectionCopyingTests<T> : TestsBase
{
    [Fact]
    public void CopyTo_ShouldCopyAllItemsToArrayAtValidIndex()
    {
        var allItems = Fixture.CreateManyDistinct<T>( count: 4 );
        var items = allItems.Take( 3 ).ToList();
        var arrayItem = allItems[^1];

        var arr = new[] { arrayItem, arrayItem, arrayItem, arrayItem, arrayItem };

        CollectionCopying.CopyTo( items, arr, 1 );

        arr.TestSequence( [ arrayItem, items[0], items[1], items[2], arrayItem ] ).Go();
    }

    [Fact]
    public void CopyTo_ShouldCopyItemsToArrayAtValidIndexAndNotExceedArrayLength()
    {
        var allItems = Fixture.CreateManyDistinct<T>( count: 4 );
        var items = allItems.Take( 3 ).ToList();
        var arrayItem = allItems[^1];

        var arr = new[] { arrayItem, arrayItem, arrayItem };

        CollectionCopying.CopyTo( items, arr, 1 );

        arr.TestSequence( [ arrayItem, items[0], items[1] ] ).Go();
    }

    [Fact]
    public void CopyTo_ShouldCopyItemsToArrayWithValidOffset_WhenIndexIsNegative()
    {
        var allItems = Fixture.CreateManyDistinct<T>( count: 4 );
        var items = allItems.Take( 3 ).ToList();
        var arrayItem = allItems[^1];

        var arr = new[] { arrayItem, arrayItem, arrayItem };

        CollectionCopying.CopyTo( items, arr, -1 );

        arr.TestSequence( [ items[1], items[2], arrayItem ] ).Go();
    }

    [Fact]
    public void CopyTo_ShouldCopyItemsToArrayWithValidOffset_WhenIndexIsNegativeAndArrayIsTooSmall()
    {
        var allItems = Fixture.CreateManyDistinct<T>( count: 6 );
        var items = allItems.Take( 5 ).ToList();
        var arrayItem = allItems[^1];

        var arr = new[] { arrayItem, arrayItem, arrayItem };

        CollectionCopying.CopyTo( items, arr, -1 );

        arr.TestSequence( [ items[1], items[2], items[3] ] ).Go();
    }

    [Theory]
    [InlineData( 3 )]
    [InlineData( -3 )]
    public void CopyTo_ShouldDoNothing_WhenIndexIsOutOfBounds(int arrayIndex)
    {
        var allItems = Fixture.CreateManyDistinct<T>( count: 4 );
        var items = allItems.Take( 3 ).ToList();
        var arrayItem = allItems[^1];

        var arr = new[] { arrayItem, arrayItem, arrayItem };

        CollectionCopying.CopyTo( items, arr, arrayIndex );

        arr.TestSequence( [ arrayItem, arrayItem, arrayItem ] ).Go();
    }
}
