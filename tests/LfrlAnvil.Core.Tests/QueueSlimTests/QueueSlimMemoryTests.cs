using System.Collections.Generic;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.QueueSlimTests;

public class QueueSlimMemoryTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnCorrectResult()
    {
        var sut = QueueSlimMemory<string>.Empty;

        Assertion.All(
                sut.First.TestEmpty(),
                sut.Second.TestEmpty(),
                sut.Length.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void From_ShouldCreateCorrectResult()
    {
        var items = new[] { "x1", "x2", "x3" };

        var sut = QueueSlimMemory<string>.From( items );

        Assertion.All(
                sut.First.TestSequence( items ),
                sut.Second.TestEmpty(),
                sut.Length.TestEquals( items.Length ) )
            .Go();
    }

    [Fact]
    public void Slice_WithLength_ShouldReturnCorrectResult()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        var sut = queue.AsMemory();

        var result = sut.Slice( 1, 2 );

        Assertion.All(
                result.First.TestSequence( [ "x2", "x3" ] ),
                result.Second.TestEmpty(),
                result.Length.TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void Slice_WithLength_ShouldReturnCorrectResult_WhenQueueIsWrapped()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.Dequeue();
        queue.Enqueue( "x5" );
        var sut = queue.AsMemory();

        var result = sut.Slice( 1, 2 );

        Assertion.All(
                result.First.TestSequence( [ "x3", "x4" ] ),
                result.Second.TestEmpty(),
                result.Length.TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void Slice_WithLength_ShouldReturnCorrectResult_Wrapped()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.DequeueRange( 2 );
        queue.EnqueueRange( new[] { "x5", "x6" } );
        var sut = queue.AsMemory();

        var result = sut.Slice( 1, 3 );

        Assertion.All(
                result.First.TestSequence( [ "x4" ] ),
                result.Second.TestSequence( [ "x5", "x6" ] ),
                result.Length.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public void Slice_WithLength_ShouldReturnCorrectResult_FromSecondRange()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.DequeueRange( 3 );
        queue.EnqueueRange( new[] { "x5", "x6", "x7" } );
        var sut = queue.AsMemory();

        var result = sut.Slice( 2, 2 );

        Assertion.All(
                result.First.TestSequence( [ "x6", "x7" ] ),
                result.Second.TestEmpty(),
                result.Length.TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void Slice_ShouldReturnCorrectResult()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.DequeueRange( 2 );
        queue.EnqueueRange( new[] { "x5", "x6" } );
        var sut = queue.AsMemory();

        var result = sut.Slice( 1 );

        Assertion.All(
                result.First.TestSequence( [ "x4" ] ),
                result.Second.TestSequence( [ "x5", "x6" ] ),
                result.Length.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public void CopyTo_ShouldDoNothing_WhenMemoryIsEmpty()
    {
        var sut = QueueSlimMemory<string>.Empty;
        var target = new[] { "foo" };

        sut.CopyTo( target );

        target.TestSequence( [ "foo" ] ).Go();
    }

    [Fact]
    public void CopyTo_ShouldCopyFirstAndSecondToBuffer()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.Dequeue();
        queue.Enqueue( "x5" );
        var sut = queue.AsMemory();
        var target = new[] { "a", "b", "c", "d", "e" };

        sut.CopyTo( target );

        target.TestSequence( [ "x2", "x3", "x4", "x5", "e" ] ).Go();
    }

    [Theory]
    [InlineData( 0, "x2" )]
    [InlineData( 1, "x3" )]
    [InlineData( 2, "x4" )]
    [InlineData( 3, "x5" )]
    public void Indexer_ShouldReturnCorrectIte(int index, string expected)
    {
        var queue = QueueSlim<string>.Create();
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.Dequeue();
        queue.Enqueue( "x5" );
        var sut = queue.AsMemory();

        var result = sut[index];

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var queue = QueueSlim<string>.Create();
        queue.EnqueueRange( new[] { "x1", "x2", "x3" } );
        queue.Dequeue();
        var sut = queue.AsMemory();

        var action = Lambda.Of( () => sut[index] );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmpty_WhenQueueIsEmpty()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 ).AsMemory();

        var result = new List<string>();
        foreach ( var e in sut ) result.Add( e );

        result.TestEmpty().Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        var sut = queue.AsMemory();

        var result = new List<string>();
        foreach ( var e in sut ) result.Add( e );

        result.TestSequence( [ "x1", "x2", "x3", "x4" ] ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult_AfterDequeue()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3" } );
        queue.Dequeue();
        var sut = queue.AsMemory();

        var result = new List<string>();
        foreach ( var e in sut ) result.Add( e );

        result.TestSequence( [ "x2", "x3" ] ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult_Wrapped()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.DequeueRange( 2 );
        queue.Enqueue( "x5" );
        var sut = queue.AsMemory();

        var result = new List<string>();
        foreach ( var e in sut ) result.Add( e );

        result.TestSequence( [ "x3", "x4", "x5" ] ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult_WrappedAtFullCapacity()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.DequeueRange( 3 );
        queue.EnqueueRange( new[] { "x5", "x6", "x7" } );
        var sut = queue.AsMemory();

        var result = new List<string>();
        foreach ( var e in sut ) result.Add( e );

        result.TestSequence( [ "x4", "x5", "x6", "x7" ] ).Go();
    }
}
