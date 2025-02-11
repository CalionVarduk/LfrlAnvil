using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.ListSlimTests;

public class ListSlimTests : TestsBase
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
        var sut = ListSlim<string>.Create( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.Capacity.TestEquals( expectedCapacity ),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Create_ShouldCopyElementsFromMaterializedSource()
    {
        var source = new[] { "x1", "x2", "x3", "x4", "x5" };
        var sut = ListSlim<string>.Create( source );

        Assertion.All(
                sut.Count.TestEquals( 5 ),
                sut.IsEmpty.TestFalse(),
                sut.Capacity.TestEquals( 8 ),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3", "x4", "x5" ] ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldCopyElementsFromNonMaterializedSource()
    {
        var source = new[] { "x1", "x2", "x3", "x4", "x5", "x6", "x7" }.Where( (_, i) => i > 0 && i < 6 );
        var sut = ListSlim<string>.Create( source, minCapacity: 16 );

        Assertion.All(
                sut.Count.TestEquals( 5 ),
                sut.IsEmpty.TestFalse(),
                sut.Capacity.TestEquals( 16 ),
                sut.AsSpan().TestSequence( [ "x2", "x3", "x4", "x5", "x6" ] ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddItemToEmptyList()
    {
        var sut = ListSlim<string>.Create();

        sut.Add( "foo" );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "foo" ] ),
                sut.First().TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_BelowCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3" ] ),
                sut.First().TestEquals( "x1" ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_UpToCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3", "x4" ] ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_ExceedingCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Add( "x6" );

        Assertion.All(
                sut.Count.TestEquals( 6 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3", "x4", "x5", "x6" ] ) )
            .Go();
    }

    [Fact]
    public void AddRange_ShouldDoNothing_WhenItemsAreEmpty()
    {
        var sut = ListSlim<string>.Create();

        sut.AddRange( ReadOnlySpan<string>.Empty );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void AddRange_ShouldAddItemsToEmptyList()
    {
        var sut = ListSlim<string>.Create();

        sut.AddRange( new[] { "x1", "x2" } );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2" ] ) )
            .Go();
    }

    [Fact]
    public void AddRange_ShouldAddItemsSequentiallyToEmptyList_BelowCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1" } );
        sut.AddRange( new[] { "x2", "x3" } );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3" ] ) )
            .Go();
    }

    [Fact]
    public void AddRange_ShouldAddItemsSequentiallyToEmptyList_UpToCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1" } );
        sut.AddRange( new[] { "x2", "x3", "x4" } );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3", "x4" ] ) )
            .Go();
    }

    [Fact]
    public void AddRange_ShouldAddItemsSequentiallyToEmptyList_ExceedingCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1" } );
        sut.AddRange( new[] { "x2", "x3", "x4", "x5", "x6" } );

        Assertion.All(
                sut.Count.TestEquals( 6 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3", "x4", "x5", "x6" ] ) )
            .Go();
    }

    [Fact]
    public void InsertAt_ShouldAddItemToEmptyList()
    {
        var sut = ListSlim<string>.Create();

        sut.InsertAt( 0, "foo" );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "foo" ] ) )
            .Go();
    }

    [Fact]
    public void InsertAt_ShouldAddItemToList_BelowCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2" } );

        sut.InsertAt( 1, "x3" );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x3", "x2" ] ) )
            .Go();
    }

    [Fact]
    public void InsertAt_ShouldAddItemToList_UpToCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3" } );
        sut.InsertAt( 3, "x4" );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3", "x4" ] ) )
            .Go();
    }

    [Fact]
    public void InsertAt_ShouldAddItemToList_ExceedingCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4", "x5" } );
        sut.InsertAt( 2, "x6" );

        Assertion.All(
                sut.Count.TestEquals( 6 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x6", "x3", "x4", "x5" ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 4 )]
    public void InsertAt_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut.InsertAt( index, "x4" ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void InsertRangeAt_ShouldDoNothing_WhenItemsAreEmpty()
    {
        var sut = ListSlim<string>.Create();

        sut.InsertRangeAt( 0, ReadOnlySpan<string>.Empty );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void InsertRangeAt_ShouldAddItemsToEmptyList()
    {
        var sut = ListSlim<string>.Create();

        sut.InsertRangeAt( 0, new[] { "x1", "x2" } );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2" ] ) )
            .Go();
    }

    [Fact]
    public void InsertRangeAt_ShouldAddItemsToList_BelowCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 8 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4", "x5" } );
        sut.InsertRangeAt( 2, new[] { "x6", "x7" } );

        Assertion.All(
                sut.Count.TestEquals( 7 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x6", "x7", "x3", "x4", "x5" ] ) )
            .Go();
    }

    [Fact]
    public void InsertRangeAt_ShouldAddItemsToList_UpToCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 8 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4", "x5" } );
        sut.InsertRangeAt( 5, new[] { "x6", "x7", "x8" } );

        Assertion.All(
                sut.Count.TestEquals( 8 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8" ] ) )
            .Go();
    }

    [Fact]
    public void InsertRangeAt_ShouldAddItemsSequentiallyToEmptyList_ExceedingCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 8 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4", "x5" } );
        sut.InsertRangeAt( 3, new[] { "x6", "x7", "x8", "x9" } );

        Assertion.All(
                sut.Count.TestEquals( 9 ),
                sut.Capacity.TestEquals( 16 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3", "x6", "x7", "x8", "x9", "x4", "x5" ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 4 )]
    public void InsertRangeAt_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut.InsertRangeAt( index, ReadOnlySpan<string>.Empty ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void RemoveLast_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create();

        var result = sut.RemoveLast();

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void RemoveLast_ShouldRemoveOnlyItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.Add( "foo" );

        var result = sut.RemoveLast();

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void RemoveLast_ShouldRemoveLastItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var result = sut.RemoveLast();

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2" ] ) )
            .Go();
    }

    [Fact]
    public void RemoveLastRange_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create();

        var result = sut.RemoveLastRange( 1 );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void RemoveLastRange_ShouldDoNothing_WhenCountIsLessThanOrEqualToZero(int count)
    {
        var sut = ListSlim<string>.Create();
        sut.Add( "foo" );

        var result = sut.RemoveLastRange( count );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "foo" ] ) )
            .Go();
    }

    [Theory]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void RemoveLastRange_ShouldRemoveAllItemsFromList(int count)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var result = sut.RemoveLastRange( count );

        Assertion.All(
                result.TestEquals( 3 ),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void RemoveLastRange_ShouldRemoveLastItemsFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var result = sut.RemoveLastRange( 2 );

        Assertion.All(
                result.TestEquals( 2 ),
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1" ] ) )
            .Go();
    }

    [Fact]
    public void RemoveAt_ShouldRemoveOnlyItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.Add( "foo" );

        sut.RemoveAt( 0 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void RemoveAt_ShouldRemoveFirstItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        sut.RemoveAt( 0 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x2", "x3" ] ) )
            .Go();
    }

    [Fact]
    public void RemoveAt_ShouldRemoveMiddleItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.RemoveAt( 2 );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x4" ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void RemoveAt_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut.RemoveAt( index ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void RemoveRangeAt_ShouldRemoveOnlyItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.Add( "foo" );

        sut.RemoveRangeAt( 0, 1 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void RemoveRangeAt_ShouldRemoveFirstItemsFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        sut.RemoveRangeAt( 0, 2 );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x3" ] ) )
            .Go();
    }

    [Fact]
    public void RemoveRangeAt_ShouldRemoveMiddleItemsFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8" } );

        sut.RemoveRangeAt( 2, 3 );

        Assertion.All(
                sut.Count.TestEquals( 5 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x6", "x7", "x8" ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void RemoveRangeAt_ShouldDoNothing_WhenCountIsLessThanOne(int count)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        sut.RemoveRangeAt( 1, count );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3" ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 1, 3 )]
    [InlineData( 2, 2 )]
    [InlineData( 3, 0 )]
    public void RemoveRangeAt_ShouldThrowArgumentOutOfRangeException_WhenIndexOrCountAreOutOfRange(int index, int count)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut.RemoveRangeAt( index, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create();

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems_AtFullCapacity()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenListIsEmptyAndMinCapacityIsLessThanOne(int minCapacity)
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenListIsEmptyAndNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
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
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "foo" );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "foo" ] ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenListIsEmptyAndNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 16 );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 16 );
        sut.AddRange( new[] { "x1", "x2" } );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2" ] ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 16 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3", "x4" ] ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenListIsEmptyAndNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestTrue(),
                sut.AsSpan().TestEmpty() )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2" } );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2" ] ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_AtFullCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.AsSpan().TestSequence( [ "x1", "x2", "x3", "x4" ] ) )
            .Go();
    }

    [Fact]
    public void AsMemory_ShouldReturnEmpty_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        var result = sut.AsMemory();
        result.TestEmpty().Go();
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectResult()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut.AsMemory();

        result.TestSequence( [ "x1", "x2", "x3", "x4" ] ).Go();
    }

    [Fact]
    public void AsSpan_ShouldReturnEmpty_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        var result = sut.AsSpan();
        result.TestEmpty().Go();
    }

    [Fact]
    public void AsSpan_ShouldReturnCorrectResult()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut.AsSpan();

        result.TestSequence( [ "x1", "x2", "x3", "x4" ] ).Go();
    }

    [Theory]
    [InlineData( 0, "x1" )]
    [InlineData( 1, "x2" )]
    [InlineData( 2, "x3" )]
    [InlineData( 3, "x4" )]
    public void Indexer_ShouldReturnCorrectItem(int index, string expected)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut[index];

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut[index] );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmpty_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );

        var result = new List<string>();
        foreach ( var e in sut ) result.Add( e );

        result.TestEmpty().Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = new List<string>();
        foreach ( var e in sut ) result.Add( e );

        result.TestSequence( [ "x1", "x2", "x3", "x4" ] ).Go();
    }
}
