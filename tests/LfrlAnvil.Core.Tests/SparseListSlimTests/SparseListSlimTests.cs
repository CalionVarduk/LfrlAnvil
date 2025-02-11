using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.SparseListSlimTests;

public class SparseListSlimTests : TestsBase
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
        var sut = SparseListSlim<string>.Create( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.Capacity.TestEquals( expectedCapacity ),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddItemToEmptyList()
    {
        var sut = SparseListSlim<string>.Create();

        var result = sut.Add( "foo" );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "foo" ),
                AssertLast( sut, 0, "foo" ),
                AssertEnumerator( sut, (0, "foo") ),
                AssertSequenceEnumerator( sut, (0, "foo") ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_BelowCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.Add( "x1" ),
            sut.Add( "x2" ),
            sut.Add( "x3" )
        };

        Assertion.All(
                result.TestSequence( [ 0, 1, 2 ] ),
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_UpToCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.Add( "x1" ),
            sut.Add( "x2" ),
            sut.Add( "x3" ),
            sut.Add( "x4" )
        };

        Assertion.All(
                result.TestSequence( [ 0, 1, 2, 3 ] ),
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_ExceedingCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.Add( "x1" ),
            sut.Add( "x2" ),
            sut.Add( "x3" ),
            sut.Add( "x4" ),
            sut.Add( "x5" ),
            sut.Add( "x6" )
        };

        Assertion.All(
                result.TestSequence( [ 0, 1, 2, 3, 4, 5 ] ),
                sut.Count.TestEquals( 6 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 5, "x6" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4"), (4, "x5"), (5, "x6") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4"), (4, "x5"), (5, "x6") ) )
            .Go();
    }

    [Fact]
    public void AddDefault_ShouldAddItemWithDefaultValue()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );

        ref var result = ref sut.AddDefault( out var index );
        result = "x3";

        Assertion.All(
                index.TestEquals( 2 ),
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddItemsToEmptyListAtCorrectPositions_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Remove( 0 );
        sut.Remove( 1 );
        sut.Remove( 2 );

        var result = new List<int>
        {
            sut.Add( "x4" ),
            sut.Add( "x5" ),
            sut.Add( "x6" ),
            sut.Add( "x7" ),
            sut.Add( "x8" )
        };

        Assertion.All(
                result.TestSequence( [ 2, 1, 0, 3, 4 ] ),
                sut.Count.TestEquals( 5 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 2, "x4" ),
                AssertLast( sut, 4, "x8" ),
                AssertEnumerator( sut, (2, "x4"), (1, "x5"), (0, "x6"), (3, "x7"), (4, "x8") ),
                AssertSequenceEnumerator( sut, (0, "x6"), (1, "x5"), (2, "x4"), (3, "x7"), (4, "x8") ) )
            .Go();
    }

    [Theory]
    [InlineData( 0, "x1" )]
    [InlineData( 1, "x2" )]
    [InlineData( 2, "x3" )]
    [InlineData( 3, "x4" )]
    public void GetRefOrAddDefault_ShouldReturnExistingValue_WhenIndexIsOccupied(int index, string expected)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        ref var result = ref sut.GetRefOrAddDefault( index, out var exists )!;

        Assertion.All(
                exists.TestTrue(),
                result.TestEquals( expected ),
                sut.Count.TestEquals( 4 ) )
            .Go();
    }

    [Theory]
    [InlineData( 0, 4 )]
    [InlineData( 2, 4 )]
    [InlineData( 3, 4 )]
    [InlineData( 4, 8 )]
    [InlineData( 10, 16 )]
    public void GetRefOrAddDefault_ShouldAddItemToEmptyList_WhenIndexIsNotOccupied(int index, int expectedCapacity)
    {
        var sut = SparseListSlim<string>.Create();

        ref var result = ref sut.GetRefOrAddDefault( index, out var exists )!;

        Assertion.All(
                exists.TestFalse(),
                result.TestNull(),
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( expectedCapacity ),
                AssertFirst( sut, index, result ),
                AssertLast( sut, index, result ),
                AssertEnumerator( sut, (index, result) ),
                AssertSequenceEnumerator( sut, (index, result) ) )
            .Go();
    }

    [Theory]
    [InlineData( 0, 4 )]
    [InlineData( 1, 4 )]
    [InlineData( 2, 4 )]
    [InlineData( 4, 8 )]
    public void GetRefOrAddDefault_ShouldAddItemToEmptyList_WhenIndexIsNotOccupied_AfterRemoval(int index, int expectedCapacity)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Remove( 2 );
        sut.Remove( 1 );
        sut.Remove( 0 );

        ref var result = ref sut.GetRefOrAddDefault( index, out var exists )!;

        Assertion.All(
                exists.TestFalse(),
                result.TestNull(),
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( expectedCapacity ),
                AssertFirst( sut, index, result ),
                AssertLast( sut, index, result ),
                AssertEnumerator( sut, (index, result) ),
                AssertSequenceEnumerator( sut, (index, result) ) )
            .Go();
    }

    [Fact]
    public void GetRefOrAddDefault_ShouldAddItemsToNonEmptyList_WhenIndexIsNotOccupied_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 0 );
        sut.Remove( 3 );
        sut.Remove( 2 );

        ref var value = ref sut.GetRefOrAddDefault( 6, out _ );
        value = "x6";
        value = ref sut.GetRefOrAddDefault( 0, out _ );
        value = "x7";
        value = ref sut.GetRefOrAddDefault( 3, out _ );
        value = "x8";
        value = ref sut.GetRefOrAddDefault( 2, out _ );
        value = "x9";
        value = ref sut.GetRefOrAddDefault( 5, out _ );
        value = "x10";

        Assertion.All(
                sut.Count.TestEquals( 7 ),
                sut.Capacity.TestEquals( 8 ),
                AssertFirst( sut, 1, "x2" ),
                AssertLast( sut, 5, "x10" ),
                AssertEnumerator( sut, (1, "x2"), (4, "x5"), (6, "x6"), (0, "x7"), (3, "x8"), (2, "x9"), (5, "x10") ),
                AssertSequenceEnumerator( sut, (0, "x7"), (1, "x2"), (2, "x9"), (3, "x8"), (4, "x5"), (5, "x10"), (6, "x6") ) )
            .Go();
    }

    [Fact]
    public void GetRefOrAddDefault_ShouldAddItemsToList_WhenIndexIsNotOccupied_FollowedByItemsAddition()
    {
        var sut = SparseListSlim<string>.Create();
        sut.TryAdd( 3, "x1" );

        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );

        Assertion.All(
                sut.Count.TestEquals( 5 ),
                sut.Capacity.TestEquals( 8 ),
                AssertFirst( sut, 3, "x1" ),
                AssertLast( sut, 4, "x5" ),
                AssertEnumerator( sut, (3, "x1"), (0, "x2"), (1, "x3"), (2, "x4"), (4, "x5") ),
                AssertSequenceEnumerator( sut, (0, "x2"), (1, "x3"), (2, "x4"), (3, "x1"), (4, "x5") ) )
            .Go();
    }

    [Fact]
    public void GetRefOrAddDefault_ShouldAddItemToNonEmptyList_WhenIndexIsNotOccupiedAndEqualToOne()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );

        ref var result = ref sut.GetRefOrAddDefault( 1, out _ )!;
        result = "x2";

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void GetRefOrAddDefault_ShouldAddItemToEmptyList_WhenIndexIsNotOccupiedAndEqualToOne()
    {
        var sut = SparseListSlim<string>.Create();

        ref var result = ref sut.GetRefOrAddDefault( 1, out _ )!;
        result = "x2";
        sut.Add( "x3" );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 1, "x2" ),
                AssertLast( sut, 0, "x3" ),
                AssertEnumerator( sut, (1, "x2"), (0, "x3") ),
                AssertSequenceEnumerator( sut, (0, "x3"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void GetRefOrAddDefault_ShouldThrowArgumentOutOfRangeException_WhenIndexIsLessThanZero()
    {
        var sut = SparseListSlim<string>.Create();
        var action = Lambda.Of( () => _ = sut.GetRefOrAddDefault( -1, out _ ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void TryAdd_ShouldAddItem_WhenIndexIsNotOccupied()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Remove( 2 );

        var result = sut.TryAdd( 2, "x5" );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 2, "x5" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (3, "x4"), (2, "x5") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x5"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldDoNothing_WhenIndexIsOccupied()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        var result = sut.TryAdd( 2, "x5" );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ) )
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
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.Remove( index );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 8 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (2, "x3"), (3, "x4") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveFirstItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 0 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 1, "x2" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (1, "x2"), (2, "x3") ),
                AssertSequenceEnumerator( sut, (1, "x2"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveLastItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 2 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveMiddleItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 1 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (0, "x1"), (2, "x3") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveOnlyItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );

        var result = sut.Remove( 0 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
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
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.Remove( index, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull(),
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 8 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (2, "x3"), (3, "x4") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveFirstItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 0, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( "x1" ),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 1, "x2" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (1, "x2"), (2, "x3") ),
                AssertSequenceEnumerator( sut, (1, "x2"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveLastItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 2, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( "x3" ),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveMiddleItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 1, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( "x2" ),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 2, "x3" ),
                AssertEnumerator( sut, (0, "x1"), (2, "x3") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (2, "x3") ) )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveOnlyItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );

        var result = sut.Remove( 0, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( "x1" ),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
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
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
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
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
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
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
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
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 16 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenListIsEmpty_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Remove( 0 );
        sut.Remove( 1 );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 16 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems_WhenListIsNotEmpty()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenListIsEmptyAndMinCapacityIsLessThanOne(int minCapacity)
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenListIsEmptyAndMinCapacityIsLessThanOne_AfterRemoval(int minCapacity)
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "foo" );
        sut.Remove( 0 );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenListIsEmptyAndNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
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
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "foo" );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "foo" ),
                AssertLast( sut, 0, "foo" ),
                AssertEnumerator( sut, (0, "foo") ),
                AssertSequenceEnumerator( sut, (0, "foo") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenListIsEmptyAndNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenListIsEmptyAndNewCapacityIsLessThanCurrentCapacity_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Remove( 0 );
        sut.Remove( 1 );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Remove( 3 );
        sut.Remove( 2 );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Add( "x6" );
        sut.Remove( 5 );
        sut.Remove( 4 );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldDoNothing_WhenMaxOccupiedIndexDoesNotAllowToReduceCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.TryAdd( 10, "x2" );
        sut.Add( "x3" );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 16 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x3" ),
                AssertEnumerator( sut, (0, "x1"), (10, "x2"), (1, "x3") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x3"), (10, "x2") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenMaxOccupiedIndexLimitsCapacityReduction()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.TryAdd( 6, "x2" );
        sut.Add( "x3" );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x3" ),
                AssertEnumerator( sut, (0, "x1"), (6, "x2"), (1, "x3") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x3"), (6, "x2") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenMaxOccupiedIndexLimitsCapacityReduction_FollowedByItemsAddition()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Add( "x6" );
        sut.Add( "x7" );
        sut.Remove( 1 );
        sut.Remove( 3 );
        sut.Remove( 5 );
        sut.Remove( 4 );

        sut.ResetCapacity( minCapacity: 4 );

        sut.Add( "x8" );
        sut.Add( "x9" );
        sut.Add( "x10" );
        sut.Add( "x11" );
        sut.Add( "x12" );

        Assertion.All(
                sut.Count.TestEquals( 8 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 7, "x12" ),
                AssertEnumerator( sut, (0, "x1"), (2, "x3"), (6, "x7"), (1, "x8"), (3, "x9"), (4, "x10"), (5, "x11"), (7, "x12") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x8"), (2, "x3"), (3, "x9"), (4, "x10"), (5, "x11"), (6, "x7"), (7, "x12") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenListIsEmptyAndNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestTrue(),
                sut.First.TestNull(),
                sut.Last.TestNull(),
                AssertEnumerator( sut ),
                AssertSequenceEnumerator( sut ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "x1" );
        sut.Add( "x2" );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 1, "x2" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_AtFullCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 0, "x1" ),
                AssertLast( sut, 3, "x4" ),
                AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ),
                AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_FollowedByItemsAddition()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Remove( 2 );
        sut.Remove( 1 );
        sut.Remove( 0 );

        sut.ResetCapacity( minCapacity: 16 );

        sut.Add( "x5" );
        sut.Add( "x6" );
        sut.Add( "x7" );
        sut.Add( "x8" );
        sut.Add( "x9" );

        Assertion.All(
                sut.Count.TestEquals( 6 ),
                sut.Capacity.TestEquals( 16 ),
                sut.IsEmpty.TestFalse(),
                AssertFirst( sut, 3, "x4" ),
                AssertLast( sut, 5, "x9" ),
                AssertEnumerator( sut, (3, "x4"), (0, "x5"), (1, "x6"), (2, "x7"), (4, "x8"), (5, "x9") ),
                AssertSequenceEnumerator( sut, (0, "x5"), (1, "x6"), (2, "x7"), (3, "x4"), (4, "x8"), (5, "x9") ) )
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
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
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
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        ref var result = ref sut[index];

        Assertion.All(
                Unsafe.IsNullRef( ref result ).TestFalse(),
                Unsafe.IsNullRef( ref result ) ? Assertion.All() : result.TestEquals( expected ) )
            .Go();
    }

    [Pure]
    private static Assertion AssertEnumerator<T>(SparseListSlim<T> source, params (int, T)[] expected)
    {
        var i = 0;
        var result = new KeyValuePair<int, T>[source.Count];
        foreach ( var e in source )
            result[i++] = e;

        return result.TestSequence( expected.Select( static e => KeyValuePair.Create( e.Item1, e.Item2 ) ) );
    }

    [Pure]
    private static Assertion AssertSequenceEnumerator<T>(SparseListSlim<T> source, params (int, T)[] expected)
    {
        var i = 0;
        var result = new KeyValuePair<int, T>[source.Count];
        foreach ( var e in source.Sequential )
            result[i++] = e;

        return result.TestSequence( expected.Select( static e => KeyValuePair.Create( e.Item1, e.Item2 ) ) );
    }

    [Pure]
    private static Assertion AssertFirst<T>(SparseListSlim<T> source, int index, T value)
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
    private static Assertion AssertLast<T>(SparseListSlim<T> source, int index, T value)
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
