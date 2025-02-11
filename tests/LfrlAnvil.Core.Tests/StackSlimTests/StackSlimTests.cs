using System.Collections.Generic;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.StackSlimTests;

public class StackSlimTests : TestsBase
{
    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, 0 )]
    [InlineData( 1, 4 )]
    [InlineData( 3, 4 )]
    [InlineData( 4, 4 )]
    [InlineData( 5, 8 )]
    [InlineData( 17, 32 )]
    public void Create_ShouldReturnEmptyStack(int minCapacity, int expectedCapacity)
    {
        var sut = StackSlim<string>.Create( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                sut.Capacity.TestEquals( expectedCapacity ) )
            .Go();
    }

    [Fact]
    public void Push_ShouldAddItemToEmptyStack()
    {
        var sut = StackSlim<string>.Create();

        sut.Push( "foo" );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void Push_ShouldAddItemsSequentiallyToEmptyStack_BelowCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.Push( "x1" );
        sut.Push( "x2" );
        sut.Push( "x3" );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x3" ) )
            .Go();
    }

    [Fact]
    public void Push_ShouldAddItemsSequentiallyToEmptyStack_UpToCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.Push( "x1" );
        sut.Push( "x2" );
        sut.Push( "x3" );
        sut.Push( "x4" );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x4" ) )
            .Go();
    }

    [Fact]
    public void Push_ShouldAddItemsSequentiallyToEmptyStack_ExceedingCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.Push( "x1" );
        sut.Push( "x2" );
        sut.Push( "x3" );
        sut.Push( "x4" );
        sut.Push( "x5" );
        sut.Push( "x6" );

        Assertion.All(
                sut.Count.TestEquals( 6 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x6" ) )
            .Go();
    }

    [Fact]
    public void PushRange_ShouldDoNothing_WhenItemsAreEmpty()
    {
        var sut = StackSlim<string>.Create();

        sut.PushRange( ReadOnlySpan<string>.Empty );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void PushRange_ShouldAddItemsToEmptyStack()
    {
        var sut = StackSlim<string>.Create();

        sut.PushRange( new[] { "x1", "x2" } );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x2" ) )
            .Go();
    }

    [Fact]
    public void PushRange_ShouldAddItemsSequentiallyToEmptyStack_BelowCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1" } );
        sut.PushRange( new[] { "x2", "x3" } );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x3" ) )
            .Go();
    }

    [Fact]
    public void PushRange_ShouldAddItemsSequentiallyToEmptyStack_UpToCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1" } );
        sut.PushRange( new[] { "x2", "x3", "x4" } );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x4" ) )
            .Go();
    }

    [Fact]
    public void PushRange_ShouldAddItemsSequentiallyToEmptyStack_ExceedingCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1" } );
        sut.PushRange( new[] { "x2", "x3", "x4", "x5", "x6" } );

        Assertion.All(
                sut.Count.TestEquals( 6 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x6" ) )
            .Go();
    }

    [Fact]
    public void Pop_ShouldDoNothing_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create();

        var result = sut.Pop();

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void Pop_ShouldRemoveOnlyItemFromStack()
    {
        var sut = StackSlim<string>.Create();
        sut.Push( "foo" );

        var result = sut.Pop();

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void Pop_ShouldRemoveTopItemFromStack()
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        var result = sut.Pop();

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x2" ) )
            .Go();
    }

    [Fact]
    public void TryPop_ShouldDoNothing_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create();

        var result = sut.TryPop( out var outResult );

        Assertion.All(
                outResult.TestNull(),
                result.TestFalse(),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void TryPop_ShouldRemoveOnlyItemFromStack()
    {
        var sut = StackSlim<string>.Create();
        sut.Push( "foo" );

        var result = sut.TryPop( out var outResult );

        Assertion.All(
                outResult.TestEquals( "foo" ),
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void TryPop_ShouldRemoveTopItemFromStack()
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        var result = sut.TryPop( out var outResult );

        Assertion.All(
                outResult.TestEquals( "x3" ),
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x2" ) )
            .Go();
    }

    [Fact]
    public void PopRange_ShouldDoNothing_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create();

        var result = sut.PopRange( 1 );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void PopRange_ShouldDoNothing_WhenCountIsLessThanOrEqualToZero(int count)
    {
        var sut = StackSlim<string>.Create();
        sut.Push( "foo" );

        var result = sut.PopRange( count );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void PopRange_ShouldRemoveAllItemsFromStack(int count)
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        var result = sut.PopRange( count );

        Assertion.All(
                result.TestEquals( 3 ),
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void PopRange_ShouldRemoveTopItemsFromStack()
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        var result = sut.PopRange( 2 );

        Assertion.All(
                result.TestEquals( 2 ),
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x1" ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create();

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems_AtFullCapacity()
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenStackIsEmptyAndMinCapacityIsLessThanOne(int minCapacity)
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 0 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenStackIsEmptyAndNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue() )
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
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.Push( "foo" );

        sut.ResetCapacity( minCapacity );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenStackIsEmptyAndNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 16 );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 16 );
        sut.PushRange( new[] { "x1", "x2" } );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x2" ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 16 );
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.ResetCapacity( minCapacity: 4 );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 4 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x4" ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenStackIsEmptyAndNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1", "x2" } );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x2" ) )
            .Go();
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_AtFullCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.ResetCapacity( minCapacity: 8 );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut.Capacity.TestEquals( 8 ),
                sut.IsEmpty.TestFalse(),
                sut.Top().TestEquals( "x4" ) )
            .Go();
    }

    [Fact]
    public void AsMemory_ShouldReturnEmpty_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        var result = sut.AsMemory();
        result.ToArray().TestEmpty().Go();
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectResult()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut.AsMemory();

        result.ToArray().TestSequence( [ "x4", "x3", "x2", "x1" ] ).Go();
    }

    [Fact]
    public void AsSpan_ShouldReturnEmpty_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        var result = sut.AsSpan();
        result.ToArray().TestEmpty().Go();
    }

    [Fact]
    public void AsSpan_ShouldReturnCorrectResult()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut.AsSpan();

        result.ToArray().TestSequence( [ "x4", "x3", "x2", "x1" ] ).Go();
    }

    [Theory]
    [InlineData( 0, "x4" )]
    [InlineData( 1, "x3" )]
    [InlineData( 2, "x2" )]
    [InlineData( 3, "x1" )]
    public void Indexer_ShouldReturnCorrectItem(int index, string expected)
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut[index];

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut[index] );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmpty_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );

        var result = new List<string>();
        foreach ( var e in sut ) result.Add( e );

        result.TestEmpty().Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = new List<string>();
        foreach ( var e in sut ) result.Add( e );

        result.TestSequence( [ "x4", "x3", "x2", "x1" ] ).Go();
    }
}
