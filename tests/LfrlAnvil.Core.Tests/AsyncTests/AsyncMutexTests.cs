using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class AsyncMutexTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateNonAcquiredMutex()
    {
        var sut = new AsyncMutex();
        Assertion.All( sut.Waiters.TestEquals( 0 ), sut.IsAcquired.TestFalse() ).Go();
    }

    [Fact]
    public async Task EnterAsync_ShouldReturnImmediately_ForFirstConsumer()
    {
        var sut = new AsyncMutex();
        var token = await sut.EnterAsync();
        Assertion.All( token.Mutex.TestRefEquals( sut ), sut.Waiters.TestEquals( 0 ), sut.IsAcquired.TestTrue() ).Go();
    }

    [Fact]
    public void EnterAsync_ShouldThrowOperationCanceledException_WhenFirstConsumerCancellationTokenIsCancelled()
    {
        var sut = new AsyncMutex();
        var action = Lambda.Of( async () => await sut.EnterAsync( new CancellationToken( canceled: true ) ) );
        action.Test(
                exc => Assertion.All(
                    exc.TestType().AssignableTo<OperationCanceledException>(),
                    sut.Waiters.TestEquals( 0 ),
                    sut.IsAcquired.TestFalse() ) )
            .Go();
    }

    [Fact]
    public async Task EnterAsync_ShouldQueueUpWaitersCorrectly()
    {
        var sut = new AsyncMutex();
        var token = await sut.EnterAsync();

        _ = Task.Run( async () => _ = await sut.EnterAsync() );
        _ = Task.Run( async () => _ = await sut.EnterAsync() );
        await Task.Delay( 15 );

        Assertion.All( token.Mutex.TestRefEquals( sut ), sut.Waiters.TestEquals( 2 ), sut.IsAcquired.TestTrue() ).Go();
    }

    [Fact]
    public async Task EnterAsync_ShouldThrowOperationCanceledException_WhenWaiterCancels()
    {
        var source = new CancellationTokenSource();
        var sut = new AsyncMutex();
        var token = await sut.EnterAsync();

        var action = Lambda.Of(
            async () =>
            {
                source.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );
                var result = sut.EnterAsync( source.Token );
                _ = sut.EnterAsync();
                await result;
            } );

        var assertion = action.Test( exc => exc.TestType().AssignableTo<OperationCanceledException>() ).Invoke();
        Assertion.All( assertion, token.Mutex.TestRefEquals( sut ), sut.Waiters.TestEquals( 1 ), sut.IsAcquired.TestTrue() ).Go();
    }

    [Fact]
    public async Task TokenDispose_ShouldReleaseLock_WithoutWaiters()
    {
        var sut = new AsyncMutex();
        using ( await sut.EnterAsync() )
            ;

        Assertion.All( sut.Waiters.TestEquals( 0 ), sut.IsAcquired.TestFalse() ).Go();
    }

    [Fact]
    public async Task TokenDispose_ShouldNotifyNextWaiter()
    {
        var sut = new AsyncMutex();
        var first = await sut.EnterAsync();
        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                first.Dispose();
            } );

        var token = await sut.EnterAsync();
        _ = sut.EnterAsync();

        Assertion.All( token.Mutex.TestRefEquals( sut ), sut.Waiters.TestEquals( 1 ), sut.IsAcquired.TestTrue() ).Go();
    }

    [Fact]
    public void TrimExcess_ShouldNotImpactWaiters()
    {
        var sut = new AsyncMutex();
        _ = sut.EnterAsync();
        _ = sut.EnterAsync();
        _ = sut.EnterAsync();

        sut.TrimExcess();

        Assertion.All( sut.Waiters.TestEquals( 2 ), sut.IsAcquired.TestTrue() ).Go();
    }
}
