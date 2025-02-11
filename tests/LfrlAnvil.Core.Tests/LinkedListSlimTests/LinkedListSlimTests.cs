using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Tests.LinkedListSlimTests;

public class LinkedListSlimTests : TestsBase
{
    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, 0 )]
    [InlineData( 1, 4 )]
    [InlineData( 3, 4 )]
    [InlineData( 4, 4 )]
    [InlineData( 5, 8 )]
    [InlineData( 17, 32 )]
    public void Create_ShouldReturnEmptyList(int minCapacity, int expectedCapacity)
    {
        var sut = LinkedListSlim<string>.Create( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.Capacity.TestEquals( expectedCapacity ),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void AddFirst_ShouldAddItemToEmptyList()
    {
        var sut = LinkedListSlim<string>.Create();

        var result = sut.AddFirst( "foo" );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "foo" ),
                AssertLast( sut, 0, "foo" ),
                AssertEnumerator( sut, (0, "foo") ) )
            .Go();
    }

    [Fact]
    public void AddFirst_ShouldAddItemsSequentiallyToEmptyListInReverseOrder_BelowCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.AddFirst( "x1" ),
            sut.AddFirst( "x2" ),
            sut.AddFirst( "x3" )
        };

        Assertion.All(
                result.TestSequence( [ 0, 1, 2 ] ),
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 2, "x3" ),
                AssertLast( sut, 0, "x1" ),
                AssertEnumerator( sut, (2, "x3"), (1, "x2"), (0, "x1") ) )
            .Go();
    }

    [Fact]
    public void AddFirst_ShouldAddItemsSequentiallyToEmptyListInReverseOrder_UpToCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.AddFirst( "x1" ),
            sut.AddFirst( "x2" ),
            sut.AddFirst( "x3" ),
            sut.AddFirst( "x4" )
        };

        Assertion.All(
                result.TestSequence( [ 0, 1, 2, 3 ] ),
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 3, "x4" ),
                AssertLast( sut, 0, "x1" ),
                AssertEnumerator( sut, (3, "x4"), (2, "x3"), (1, "x2"), (0, "x1") ) )
            .Go();
    }

    [Fact]
    public void AddFirst_ShouldAddItemsSequentiallyToEmptyListInReverseOrder_ExceedingCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.AddFirst( "x1" ),
            sut.AddFirst( "x2" ),
            sut.AddFirst( "x3" ),
            sut.AddFirst( "x4" ),
            sut.AddFirst( "x5" ),
            sut.AddFirst( "x6" )
        };

        Assertion.All(
                result.TestSequence( [ 0, 1, 2, 3, 4, 5 ] ),
                sut.Count.TestEquals( 6 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 5, "x6" ),
                AssertLast( sut, 0, "x1" ),
                AssertEnumerator( sut, (5, "x6"), (4, "x5"), (3, "x4"), (2, "x3"), (1, "x2"), (0, "x1") ) )
            .Go();
    }

    [Fact]
    public void AddLast_ShouldAddItemToEmptyList()
    {
        var sut = LinkedListSlim<string>.Create();

        var result = sut.AddLast( "foo" );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "foo" ),
                AssertLast( sut, 0, "foo" ),
                AssertEnumerator( sut, (0, "foo") ) )
            .Go();
    }

    [Fact]
    public void AddLast_ShouldAddItemsSequentiallyToEmptyList_BelowCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.AddLast( "x1" ),
            sut.AddLast( "x2" ),
            sut.AddLast( "x3" )
        };

        Assertion.All(
                result.TestSequence( [ 0, 1, 2 ] ),
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void AddLast_ShouldAddItemsSequentiallyToEmptyList_UpToCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.AddLast( "x1" ),
            sut.AddLast( "x2" ),
            sut.AddLast( "x3" ),
            sut.AddLast( "x4" )
        };

        Assertion.All(
                result.TestSequence( [ 0, 1, 2, 3 ] ),
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void AddLast_ShouldAddItemsSequentiallyToEmptyList_ExceedingCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.AddLast( "x1" ),
            sut.AddLast( "x2" ),
            sut.AddLast( "x3" ),
            sut.AddLast( "x4" ),
            sut.AddLast( "x5" ),
            sut.AddLast( "x6" )
        };

        Assertion.All(
                result.TestSequence( [ 0, 1, 2, 3, 4, 5 ] ),
                sut.Count.TestEquals( 6 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 5, "x6" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4"), (4, "x5"), (5, "x6") ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void AddBefore_ShouldDoNothing_WhenIndexIsOutOfRange(int index)
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );

        var result = sut.AddBefore( index, "foo" );

        Assertion.All(
                result.TestEquals( -1 ),
                sut.Count.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public void AddBefore_ShouldDoNothing_WhenListIsEmptyAndIndexIsNull()
    {
        var sut = LinkedListSlim<string>.Create();

        var result = sut.AddBefore( NullableIndex.NullValue, "foo" );

        Assertion.All(
                result.TestEquals( -1 ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void AddBefore_ShouldDoNothing_WhenIndexEqualsRemovedNodeIndex()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        var index = sut.AddLast( "x3" );
        sut.Remove( index );

        var result = sut.AddBefore( index, "foo" );

        Assertion.All(
                result.TestEquals( -1 ),
                sut.Count.TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void AddBefore_ShouldAddItemAsFirst_WhenIndexEqualsFirstNodeIndex()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        var index = sut.AddFirst( "x3" );

        var result = sut.AddBefore( index, "foo" );

        Assertion.All(
                result.TestEquals( 3 ),
                sut.Count.TestEquals( 4 ),
                AssertFirst( sut, 3, "foo" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (3, "foo"), (2, "x3"), (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void AddBefore_ShouldAddItemCorrectly_WhenIndexEqualsLastNodeIndex()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        var index = sut.AddLast( "x2" );
        sut.AddFirst( "x3" );

        var result = sut.AddBefore( index, "foo" );

        Assertion.All(
                result.TestEquals( 3 ),
                sut.Count.TestEquals( 4 ),
                AssertFirst( sut, 2, "x3" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (2, "x3"), (0, "x1"), (3, "foo"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void AddBefore_ShouldAddItemCorrectly_WhenIndexEqualsMiddleNodeIndex()
    {
        var sut = LinkedListSlim<string>.Create();
        var index = sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddFirst( "x3" );

        var result = sut.AddBefore( index, "foo" );

        Assertion.All(
                result.TestEquals( 3 ),
                sut.Count.TestEquals( 4 ),
                AssertFirst( sut, 2, "x3" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (2, "x3"), (3, "foo"), (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void AddAfter_ShouldDoNothing_WhenIndexIsOutOfRange(int index)
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );

        var result = sut.AddAfter( index, "foo" );

        Assertion.All(
                result.TestEquals( -1 ),
                sut.Count.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public void AddAfter_ShouldDoNothing_WhenListIsEmptyAndIndexIsNull()
    {
        var sut = LinkedListSlim<string>.Create();

        var result = sut.AddAfter( NullableIndex.NullValue, "foo" );

        Assertion.All(
                result.TestEquals( -1 ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void AddAfter_ShouldDoNothing_WhenIndexEqualsRemovedNodeIndex()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        var index = sut.AddLast( "x3" );
        sut.Remove( index );

        var result = sut.AddAfter( index, "foo" );

        Assertion.All(
                result.TestEquals( -1 ),
                sut.Count.TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void AddAfter_ShouldAddItemCorrectly_WhenIndexEqualsFirstNodeIndex()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        var index = sut.AddFirst( "x3" );

        var result = sut.AddAfter( index, "foo" );

        Assertion.All(
                result.TestEquals( 3 ),
                sut.Count.TestEquals( 4 ),
                AssertFirst( sut, 2, "x3" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (2, "x3"), (3, "foo"), (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void AddAfter_ShouldAddItemAsLast_WhenIndexEqualsLastNodeIndex()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        var index = sut.AddLast( "x2" );
        sut.AddFirst( "x3" );

        var result = sut.AddAfter( index, "foo" );

        Assertion.All(
                result.TestEquals( 3 ),
                sut.Count.TestEquals( 4 ),
                AssertFirst( sut, 2, "x3" ),
                AssertLast( sut, 3, "foo" ),
                AssertEnumerator( sut, (2, "x3"), (0, "x1"), (1, "x2"), (3, "foo") ) )
            .Go();
    }

    [Fact]
    public void AddAfter_ShouldAddItemCorrectly_WhenIndexEqualsMiddleNodeIndex()
    {
        var sut = LinkedListSlim<string>.Create();
        var index = sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddFirst( "x3" );

        var result = sut.AddAfter( index, "foo" );

        Assertion.All(
                result.TestEquals( 3 ),
                sut.Count.TestEquals( 4 ),
                AssertFirst( sut, 2, "x3" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (2, "x3"), (0, "x1"), (3, "foo"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddItemsToEmptyListAtCorrectPositions_AfterRemoval()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.Remove( 0 );
        sut.Remove( 1 );
        sut.Remove( 2 );

        var result = new List<int>
        {
            sut.AddLast( "x4" ),
            sut.AddFirst( "x5" ),
            sut.AddLast( "x6" ),
            sut.AddFirst( "x7" ),
            sut.AddLast( "x8" )
        };

        Assertion.All(
                result.TestSequence( [ 2, 1, 0, 3, 4 ] ),
                sut.Count.TestEquals( 5 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 3, "x7" ),
                AssertLast( sut, 4, "x8" ),
                AssertEnumerator( sut, (3, "x7"), (1, "x5"), (2, "x4"), (0, "x6"), (4, "x8") ) )
            .Go();
    }

    [Fact]
    public void RemoveFirst_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = LinkedListSlim<string>.Create();
        var result = sut.RemoveFirst();
        result.TestFalse().Go();
    }

    [Fact]
    public void RemoveFirst_ShouldRemoveFirstItem_WhenListOnlyContainsOneItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );

        var result = sut.RemoveFirst();

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void RemoveFirst_ShouldRemoveFirstItem_WhenListContainsMoreThanOneItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );

        var result = sut.RemoveFirst();

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                AssertFirst( sut, 1, "x2" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void RemoveFirst_WithRemoved_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = LinkedListSlim<string>.Create();

        var result = sut.RemoveFirst( out var removed );

        Assertion.All(
                result.TestFalse(),
                removed.TestNull() )
            .Go();
    }

    [Fact]
    public void RemoveFirst_WithRemoved_ShouldRemoveFirstItem_WhenListOnlyContainsOneItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );

        var result = sut.RemoveFirst( out var removed );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( "x1" ),
                sut.Count.TestEquals( 0 ),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void RemoveFirst_WithRemoved_ShouldRemoveFirstItem_WhenListContainsMoreThanOneItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );

        var result = sut.RemoveFirst( out var removed );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( "x1" ),
                sut.Count.TestEquals( 1 ),
                AssertFirst( sut, 1, "x2" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void RemoveLast_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = LinkedListSlim<string>.Create();
        var result = sut.RemoveLast();
        result.TestFalse().Go();
    }

    [Fact]
    public void RemoveLast_ShouldRemoveLastItem_WhenListOnlyContainsOneItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );

        var result = sut.RemoveLast();

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void RemoveLast_ShouldRemoveLastItem_WhenListContainsMoreThanOneItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );

        var result = sut.RemoveLast();

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 0, "x1" ),
                AssertEnumerator( sut, (0, "x1") ) )
            .Go();
    }

    [Fact]
    public void RemoveLast_WithRemoved_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = LinkedListSlim<string>.Create();

        var result = sut.RemoveLast( out var removed );

        Assertion.All(
                result.TestFalse(),
                removed.TestNull() )
            .Go();
    }

    [Fact]
    public void RemoveLast_WithRemoved_ShouldRemoveLastItem_WhenListOnlyContainsOneItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );

        var result = sut.RemoveLast( out var removed );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( "x1" ),
                sut.Count.TestEquals( 0 ),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void RemoveLast_WithRemoved_ShouldRemoveLastItem_WhenListContainsMoreThanOneItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );

        var result = sut.RemoveLast( out var removed );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( "x2" ),
                sut.Count.TestEquals( 1 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 0, "x1" ),
                AssertEnumerator( sut, (0, "x1") ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 8 )]
    [InlineData( 9 )]
    public void Remove_ShouldDoNothing_WhenIndexIsNotOccupied(int index)
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.Remove( index );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 8 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveFirstItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );

        var result = sut.Remove( 0 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 1, "x2" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (1, "x2"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveLastItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );

        var result = sut.Remove( 2 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveMiddleItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );

        var result = sut.Remove( 1 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (0, "x1"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveOnlyItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );

        var result = sut.Remove( 0 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 8 )]
    [InlineData( 9 )]
    public void Remove_WithRemoved_ShouldDoNothing_WhenIndexIsNotOccupied(int index)
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.Remove( index, out var removed );

        Assertion.All(
                result.TestFalse(),
                removed.TestNull(),
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 8 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveFirstItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );

        var result = sut.Remove( 0, out var removed );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( "x1" ),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 1, "x2" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (1, "x2"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveLastItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );

        var result = sut.Remove( 2, out var removed );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( "x3" ),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveMiddleItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );

        var result = sut.Remove( 1, out var removed );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( "x2" ),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (0, "x1"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveOnlyItem()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );

        var result = sut.Remove( 0, out var removed );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( "x1" ),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( -1, false )]
    [InlineData( 0, true )]
    [InlineData( 1, false )]
    [InlineData( 2, true )]
    [InlineData( 3, true )]
    [InlineData( 4, false )]
    [InlineData( 5, false )]
    [InlineData( 8, false )]
    [InlineData( 9, false )]
    public void Contains_ShouldReturnTrue_WhenIndexIsOccupied(int index, bool expected)
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.Contains( index );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 8 )]
    [InlineData( 9 )]
    public void GetNode_ShouldReturnNull_WhenIndexIsNotOccupied(int index)
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.GetNode( index );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( 0, "x1", null, 2, "[0]: x1" )]
    [InlineData( 2, "x3", 0, 3, "[2]: x3" )]
    [InlineData( 3, "x4", 2, null, "[3]: x4" )]
    public void GetNode_ShouldReturnNode_WhenIndexIsOccupied(
        int index,
        string expectedValue,
        int? expectedPrev,
        int? expectedNext,
        string expectedString)
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.GetNode( index );

        Assertion.All(
                result.TestNotNull(),
                result.TestIf()
                    .NotNull(
                        value => Assertion.All(
                            "result.Value",
                            value.Index.TestEquals( index ),
                            value.Value.TestEquals( expectedValue ),
                            (value.Prev?.Index).TestEquals( expectedPrev ),
                            (value.Next?.Index).TestEquals( expectedNext ),
                            value.ToString().TestEquals( expectedString ) ) ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 16 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenListIsEmpty_AfterRemoval()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.Remove( 0 );
        sut.Remove( 1 );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 16 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems_WhenListIsNotEmpty()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenListIsEmptyAndMinCapacityIsLessThanOne(int minCapacity)
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenListIsEmptyAndMinCapacityIsLessThanOne_AfterRemoval(int minCapacity)
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );
        sut.AddLast( "foo" );
        sut.Remove( 0 );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenListIsEmptyAndNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );
        sut.AddLast( "foo" );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "foo" ),
                AssertLast( sut, 0, "foo" ),
                AssertEnumerator( sut, (0, "foo") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenListIsEmptyAndNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenListIsEmptyAndNewCapacityIsLessThanCurrentCapacity_AfterRemoval()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.Remove( 0 );
        sut.Remove( 1 );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AfterRemoval()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.Remove( 3 );
        sut.Remove( 2 );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity_AfterRemoval()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.AddLast( "x6" );
        sut.Remove( 5 );
        sut.Remove( 4 );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldDoNothing_WhenMaxOccupiedIndexDoesNotAllowToReduceCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.AddLast( "x6" );
        sut.AddLast( "x7" );
        sut.AddLast( "x8" );
        sut.AddLast( "x9" );
        sut.AddLast( "x10" );
        sut.AddLast( "x11" );
        sut.Remove( 9 );
        sut.Remove( 2 );
        sut.Remove( 3 );
        sut.Remove( 4 );
        sut.Remove( 5 );
        sut.Remove( 6 );
        sut.Remove( 7 );
        sut.Remove( 8 );
        sut.Remove( 1 );
        sut.AddLast( "x12" );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 16 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x12" ),
                AssertEnumerator( sut, (0, "x1"), (10, "x11"), (1, "x12") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenMaxOccupiedIndexLimitsCapacityReduction()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.AddLast( "x6" );
        sut.AddLast( "x7" );
        sut.Remove( 5 );
        sut.Remove( 2 );
        sut.Remove( 3 );
        sut.Remove( 4 );
        sut.Remove( 1 );
        sut.AddLast( "x8" );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x8" ),
                AssertEnumerator( sut, (0, "x1"), (6, "x7"), (1, "x8") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenMaxOccupiedIndexLimitsCapacityReduction_FollowedByItemsAddition()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 16 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.AddLast( "x6" );
        sut.AddLast( "x7" );
        sut.Remove( 1 );
        sut.Remove( 3 );
        sut.Remove( 5 );
        sut.Remove( 4 );

        sut.ResetCapacity( minCapacity: 4 );

        sut.AddLast( "x8" );
        sut.AddLast( "x9" );
        sut.AddLast( "x10" );
        sut.AddLast( "x11" );
        sut.AddLast( "x12" );

        Assertion.All(
                sut.Count.TestEquals( 8 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 7, "x12" ),
                AssertEnumerator( sut, (0, "x1"), (2, "x3"), (6, "x7"), (1, "x8"), (3, "x9"), (4, "x10"), (5, "x11"), (7, "x12") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenListIsEmptyAndNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_AtFullCapacity()
    {
        var sut = LinkedListSlim<string>.Create( minCapacity: 4 );
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_FollowedByItemsAddition()
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.Remove( 2 );
        sut.Remove( 1 );
        sut.Remove( 0 );

        sut.ResetCapacity( minCapacity: 16 );

        sut.AddLast( "x5" );
        sut.AddLast( "x6" );
        sut.AddLast( "x7" );
        sut.AddLast( "x8" );
        sut.AddLast( "x9" );

        Assertion.All(
                sut.Count.TestEquals( 6 ),
                sut.Capacity.TestEquals( 16 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 3, "x4" ),
                AssertLast( sut, 5, "x9" ),
                AssertEnumerator( sut, (3, "x4"), (0, "x5"), (1, "x6"), (2, "x7"), (4, "x8"), (5, "x9") ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 8 )]
    [InlineData( 9 )]
    public void Indexer_ShouldReturnNull_WhenIndexIsNotOccupied(int index)
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        ref var result = ref sut[index];

        Unsafe.IsNullRef( ref result ).TestTrue().Go();
    }

    [Theory]
    [InlineData( 0, "x1" )]
    [InlineData( 2, "x3" )]
    [InlineData( 3, "x4" )]
    public void Indexer_ShouldReturnValue_WhenIndexIsOccupied(int index, string expected)
    {
        var sut = LinkedListSlim<string>.Create();
        sut.AddLast( "x1" );
        sut.AddLast( "x2" );
        sut.AddLast( "x3" );
        sut.AddLast( "x4" );
        sut.AddLast( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        ref var result = ref sut[index];

        Assertion.All(
                Unsafe.IsNullRef( ref result ).TestFalse(),
                Unsafe.IsNullRef( ref result ) ? Assertion.All() : result.TestEquals( expected ) )
            .Go();
    }

    [Pure]
    private static Assertion AssertEnumerator<T>(LinkedListSlim<T> source, params (int, T)[] expected)
    {
        var i = 0;
        var result = new KeyValuePair<int, T>[source.Count];
        foreach ( var e in source )
            result[i++] = e;

        return result.TestSequence( expected.Select( static e => KeyValuePair.Create( e.Item1, e.Item2 ) ) );
    }

    [Pure]
    private static Assertion AssertFirst<T>(LinkedListSlim<T> source, int index, T value)
    {
        return Assertion.All(
            "First",
            source.First.TestNotNull(),
            source.First.TestIf()
                .NotNull(
                    first => Assertion.All(
                        "First.Value",
                        first.Index.TestEquals( index ),
                        first.Value.TestEquals( value ),
                        first.Prev.TestNull() ) ) );
    }

    [Pure]
    private static Assertion AssertLast<T>(LinkedListSlim<T> source, int index, T value)
    {
        return Assertion.All(
            "Last",
            source.Last.TestNotNull(),
            source.Last.TestIf()
                .NotNull(
                    last => Assertion.All(
                        "Last.Value",
                        last.Index.TestEquals( index ),
                        last.Value.TestEquals( value ),
                        last.Next.TestNull() ) ) );
    }
}
