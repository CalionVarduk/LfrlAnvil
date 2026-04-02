using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.ToAsyncEnumerableTests;

public class ToAsyncEnumerableTests : TestsBase
{
    [Fact]
    public void ToAsyncEnumerableExtension_ShouldCreateNewEnumerable()
    {
        var source = new EventPublisher<int>();
        _ = source.ToAsyncEnumerable();
        source.HasSubscribers.TestFalse().Go();
    }

    [Fact]
    public void ToAsyncEnumerableExtension_ShouldThrowArgumentOutOfRangeException_WhenMaxBufferSizeIsLessThanZero()
    {
        var source = new EventPublisher<int>();
        var action = Lambda.Of( () => source.ToAsyncEnumerable( maxBufferSize: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public async Task ToAsyncEnumerableExtension_FollowedByForeach_ShouldConsumeEvents()
    {
        var source = new EventPublisher<int>();
        var enumerable = source.ToAsyncEnumerable();
        source.Publish( 0 );

        var task = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            source.Publish( 1 );
            await Task.Delay( 15 );
            source.Publish( 2 );
            await Task.Delay( 15 );
            source.Publish( 3 );
            await Task.Delay( 15 );
            source.Dispose();
        } );

        var events = new List<AsyncEnumerableEvent<int>>();
        await foreach ( var e in enumerable )
            events.Add( e );

        await task;

        events.TestSequence(
            [
                AsyncEnumerableEvent<int>.Create( 1 ),
                AsyncEnumerableEvent<int>.Create( 2 ),
                AsyncEnumerableEvent<int>.Create( 3 ),
                AsyncEnumerableEvent<int>.CreateDisposal( DisposalSource.EventSource )
            ] )
            .Go();
    }

    [Fact]
    public async Task ToAsyncEnumerableExtension_FollowedByForeach_ShouldConsumeEvents_UntilSubscriberDisposes()
    {
        var source = new EventPublisher<int>();
        var enumerable = source.ToAsyncEnumerable();
        source.Publish( 0 );

        var task = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            source.Publish( 1 );
            await Task.Delay( 15 );
            source.Publish( 2 );
            await Task.Delay( 15 );
            source.Publish( 3 );
            await Task.Delay( 15 );
            source.Subscribers.FirstOrDefault()?.Dispose();
        } );

        var events = new List<AsyncEnumerableEvent<int>>();
        await foreach ( var e in enumerable )
            events.Add( e );

        await task;

        events.TestSequence(
            [
                AsyncEnumerableEvent<int>.Create( 1 ),
                AsyncEnumerableEvent<int>.Create( 2 ),
                AsyncEnumerableEvent<int>.Create( 3 ),
                AsyncEnumerableEvent<int>.CreateDisposal( DisposalSource.Subscriber )
            ] )
            .Go();
    }

    [Fact]
    public async Task ToAsyncEnumerableExtension_FollowedByForeach_ShouldConsumeEvents_UntilLoopBreak()
    {
        var source = new EventPublisher<int>();
        var enumerable = source.ToAsyncEnumerable();
        source.Publish( 0 );

        var task = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            source.Publish( 1 );
            await Task.Delay( 15 );
            source.Publish( 2 );
            await Task.Delay( 15 );
            source.Publish( 3 );
            await Task.Delay( 15 );
            source.Publish( 4 );
        } );

        var events = new List<AsyncEnumerableEvent<int>>();
        await foreach ( var e in enumerable )
        {
            events.Add( e );
            if ( events.Count == 3 )
                break;
        }

        await task;

        events.TestSequence(
            [
                AsyncEnumerableEvent<int>.Create( 1 ),
                AsyncEnumerableEvent<int>.Create( 2 ),
                AsyncEnumerableEvent<int>.Create( 3 )
            ] )
            .Go();
    }

    [Fact]
    public async Task ToAsyncEnumerableExtension_FollowedByForeach_ShouldConsumeEvents_WithoutLosingAny()
    {
        var completion = new SafeTaskCompletionSource();
        var source = new EventPublisher<int>();
        var enumerable = source.ToAsyncEnumerable();
        source.Publish( 0 );

        var task = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            source.Publish( 1 );
            source.Publish( 2 );
            source.Publish( 3 );
            source.Dispose();
            completion.Complete();
        } );

        var events = new List<AsyncEnumerableEvent<int>>();
        await foreach ( var e in enumerable )
        {
            events.Add( e );
            if ( events.Count == 1 )
                await completion.Task;
        }

        await task;

        events.TestSequence(
            [
                AsyncEnumerableEvent<int>.Create( 1 ),
                AsyncEnumerableEvent<int>.Create( 2 ),
                AsyncEnumerableEvent<int>.Create( 3 ),
                AsyncEnumerableEvent<int>.CreateDisposal( DisposalSource.EventSource )
            ] )
            .Go();
    }

    [Fact]
    public async Task ToAsyncEnumerableExtension_FollowedByForeach_ShouldConsumeEvents_WithBoundBuffer()
    {
        var completion = new SafeTaskCompletionSource();
        var source = new EventPublisher<int>();
        var enumerable = source.ToAsyncEnumerable( maxBufferSize: 2 );
        source.Publish( 0 );

        var task = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            source.Publish( 1 );
            source.Publish( 2 );
            source.Publish( 3 );
            source.Publish( 4 );
            source.Publish( 5 );
            source.Dispose();
            completion.Complete();
        } );

        var events = new List<AsyncEnumerableEvent<int>>();
        await foreach ( var e in enumerable )
        {
            events.Add( e );
            if ( events.Count == 1 )
            {
                await completion.Task;
                await Task.Delay( 15 );
            }
        }

        await task;

        events.TestSequence(
            [
                AsyncEnumerableEvent<int>.Create( 1 ),
                AsyncEnumerableEvent<int>.Create( 4 ),
                AsyncEnumerableEvent<int>.Create( 5 ),
                AsyncEnumerableEvent<int>.CreateDisposal( DisposalSource.EventSource )
            ] )
            .Go();
    }

    [Fact]
    public async Task ToAsyncEnumerableExtension_FollowedByForeach_ShouldConsumeEvents_WithBoundBufferAndDisposeLatestStrategy()
    {
        var completion = new SafeTaskCompletionSource();
        var source = new EventPublisher<int>();
        var enumerable = source.ToAsyncEnumerable( maxBufferSize: 2, discardLatest: true );
        source.Publish( 0 );

        var task = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            source.Publish( 1 );
            source.Publish( 2 );
            source.Publish( 3 );
            source.Publish( 4 );
            source.Publish( 5 );
            source.Dispose();
            completion.Complete();
        } );

        var events = new List<AsyncEnumerableEvent<int>>();
        await foreach ( var e in enumerable )
        {
            events.Add( e );
            if ( events.Count == 1 )
            {
                await completion.Task;
                await Task.Delay( 15 );
            }
        }

        await task;

        events.TestSequence(
            [
                AsyncEnumerableEvent<int>.Create( 1 ),
                AsyncEnumerableEvent<int>.Create( 2 ),
                AsyncEnumerableEvent<int>.Create( 3 ),
                AsyncEnumerableEvent<int>.CreateDisposal( DisposalSource.EventSource )
            ] )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public async Task ToAsyncEnumerableExtension_FollowedByForeach_ShouldConsumeEvents_WithDisabledBuffer(bool discardLatest)
    {
        var completion = new SafeTaskCompletionSource();
        var source = new EventPublisher<int>();
        var enumerable = source.ToAsyncEnumerable( maxBufferSize: 0, discardLatest: discardLatest );
        source.Publish( 0 );

        var task = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            source.Publish( 1 );
            source.Publish( 2 );
            source.Publish( 3 );
            source.Publish( 4 );
            source.Publish( 5 );
            source.Dispose();
            completion.Complete();
        } );

        var events = new List<AsyncEnumerableEvent<int>>();
        await foreach ( var e in enumerable )
        {
            events.Add( e );
            if ( events.Count == 1 )
            {
                await completion.Task;
                await Task.Delay( 15 );
            }
        }

        await task;

        events.TestSequence(
            [
                AsyncEnumerableEvent<int>.Create( 1 ),
                AsyncEnumerableEvent<int>.CreateDisposal( DisposalSource.EventSource )
            ] )
            .Go();
    }

    [Fact]
    public async Task ToAsyncEnumerableExtension_FollowedByForeach_ShouldEnd_WhenSourceIsDisposed()
    {
        var source = new EventPublisher<int>();
        source.Dispose();
        var enumerable = source.ToAsyncEnumerable();

        var events = new List<AsyncEnumerableEvent<int>>();
        await foreach ( var e in enumerable )
            events.Add( e );

        events.TestSequence( [ AsyncEnumerableEvent<int>.CreateDisposal( DisposalSource.EventSource ) ] ).Go();
    }

    [Fact]
    public void
        ToAsyncEnumerableExtension_FollowedByGetEnumerator_ShouldThrowOperationCanceledException_WhenCancellationIsRequestedImmediately()
    {
        var source = new EventPublisher<int>();
        var enumerable = source.ToAsyncEnumerable();

        var action = Lambda.Of( () => enumerable.GetAsyncEnumerator( new CancellationToken( true ) ) );

        action.Test( exc => exc.TestType().AssignableTo<OperationCanceledException>() ).Go();
    }

    [Fact]
    public void ToAsyncEnumerableExtension_FollowedByForeach_ShouldThrowOperationCanceledException_WhenCancellationIsRequested()
    {
        var cts = new CancellationTokenSource();
        var source = new EventPublisher<int>();
        var enumerable = source.ToAsyncEnumerable();
        source.Publish( 0 );

        var task = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            source.Publish( 1 );
            await Task.Delay( 15 );
            source.Publish( 2 );
            cts.Cancel();
            await Task.Delay( 15 );
            source.Publish( 3 );
        } );

        var events = new List<AsyncEnumerableEvent<int>>();
        var action = Lambda.Of( async () =>
        {
            try
            {
                await foreach ( var e in enumerable.WithCancellation( cts.Token ) )
                    events.Add( e );
            }
            finally
            {
                await task;
            }
        } );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                events.TestSequence(
                [
                    AsyncEnumerableEvent<int>.Create( 1 ),
                    AsyncEnumerableEvent<int>.Create( 2 )
                ] ) ) )
            .Go();
    }

    [Fact]
    public void
        ToAsyncEnumerableExtension_FollowedByForeach_ShouldThrowOperationCanceledException_WhenCancellationIsRequested_WithoutLosingEvents()
    {
        var completion = new SafeTaskCompletionSource();
        var cts = new CancellationTokenSource();
        var source = new EventPublisher<int>();
        var enumerable = source.ToAsyncEnumerable();
        source.Publish( 0 );

        var task = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            source.Publish( 1 );
            source.Publish( 2 );
            cts.Cancel();
            completion.Complete();
            await Task.Delay( 15 );
            source.Publish( 3 );
        } );

        var events = new List<AsyncEnumerableEvent<int>>();
        var action = Lambda.Of( async () =>
        {
            try
            {
                await foreach ( var e in enumerable.WithCancellation( cts.Token ) )
                {
                    events.Add( e );
                    if ( events.Count == 1 )
                        await completion.Task;
                }
            }
            finally
            {
                await task;
            }
        } );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                events.Count.TestInRange( 1, 2 ),
                events.TestContainsSequence( [ AsyncEnumerableEvent<int>.Create( 1 ) ] ) ) )
            .Go();
    }

    [Fact]
    public void Event_TryGetEvent_ShouldReturnTrue_WhenEventExists()
    {
        var sut = AsyncEnumerableEvent<int>.Create( 1 );
        var result = sut.TryGetEvent( out var outResult );
        Assertion.All( result.TestTrue(), outResult.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public void Event_TryGetEvent_ShouldReturnFalse_WhenEventDoesNotExist()
    {
        var sut = AsyncEnumerableEvent<int>.CreateDisposal( DisposalSource.EventSource );
        var result = sut.TryGetEvent( out var outResult );
        Assertion.All( result.TestFalse(), outResult.TestEquals( 0 ) ).Go();
    }

    [Fact]
    public void Event_TryGetEvent_WithDisposalSource_ShouldReturnTrue_WhenEventExists()
    {
        var sut = AsyncEnumerableEvent<int>.Create( 1 );
        var result = sut.TryGetEvent( out var outResult, out var disposalSource );
        Assertion.All( result.TestTrue(), outResult.TestEquals( 1 ), disposalSource.TestNull() ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.Subscriber )]
    [InlineData( DisposalSource.EventSource )]
    public void Event_TryGetEvent_WithDisposalSource_ShouldReturnFalse_WhenEventDoesNotExist(DisposalSource source)
    {
        var sut = AsyncEnumerableEvent<int>.CreateDisposal( source );
        var result = sut.TryGetEvent( out var outResult, out var disposalSource );
        Assertion.All( result.TestFalse(), outResult.TestEquals( 0 ), disposalSource.TestEquals( source ) ).Go();
    }

    [Fact]
    public void Event_ToString_ShouldReturnCorrectResultForEvent()
    {
        var sut = AsyncEnumerableEvent<int>.Create( 123 );
        var result = sut.ToString();
        result.TestEquals( "Event(123)" ).Go();
    }

    [Fact]
    public void Event_ToString_ShouldReturnCorrectResultForDisposal()
    {
        var sut = AsyncEnumerableEvent<int>.CreateDisposal( DisposalSource.EventSource );
        var result = sut.ToString();
        result.TestEquals( "Disposal(EventSource)" ).Go();
    }
}
