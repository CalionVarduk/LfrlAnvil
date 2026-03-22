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
        Assertion.All( sut.Participants.TestEquals( 0 ) ).Go();
    }

    [Fact]
    public async Task EnterAsync_ShouldReturnImmediately_ForFirstParticipant()
    {
        var sut = new AsyncMutex();
        var token = await sut.EnterAsync();
        Assertion.All( token.Mutex.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public void EnterAsync_ShouldThrowOperationCanceledException_WhenFirstParticipantCancellationTokenIsCancelled()
    {
        var sut = new AsyncMutex();
        var action = Lambda.Of( async () => await sut.EnterAsync( new CancellationToken( canceled: true ) ) );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                sut.Participants.TestEquals( 0 ) ) )
            .Go();
    }

    [Fact]
    public async Task EnterAsync_ShouldQueueUpParticipantCorrectly()
    {
        var sut = new AsyncMutex();
        var token = await sut.EnterAsync();

        _ = Task.Run( async () => _ = await sut.EnterAsync() );
        _ = Task.Run( async () => _ = await sut.EnterAsync() );
        await Task.Delay( 15 );

        Assertion.All( token.Mutex.TestRefEquals( sut ), sut.Participants.TestEquals( 3 ) ).Go();
    }

    [Fact]
    public async Task EnterAsync_ShouldThrowOperationCanceledException_WhenParticipantCancels()
    {
        var source = new CancellationTokenSource();
        var sut = new AsyncMutex();
        var token = await sut.EnterAsync();

        var action = Lambda.Of( async () =>
        {
            source.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );
            var result = sut.EnterAsync( source.Token );
            _ = sut.EnterAsync();
            try
            {
                await result;
            }
            finally
            {
                await Task.Delay( 15 );
            }
        } );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                token.Mutex.TestRefEquals( sut ),
                sut.Participants.TestEquals( 2 ) ) )
            .Go();
    }

    [Fact]
    public void TryEnter_ShouldReturnEnteredToken_ForFirstParticipant()
    {
        var sut = new AsyncMutex();
        var token = sut.TryEnter( out var entered );
        Assertion.All( token.Mutex.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public void TryEnter_ShouldReturnNotEnteredToken_ForNotFirstParticipant()
    {
        var sut = new AsyncMutex();
        _ = sut.TryEnter( out _ );

        var token = sut.TryEnter( out var entered );

        Assertion.All( token.Mutex.TestNull(), sut.Participants.TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public async Task LockDispose_ShouldReleaseLock_WithoutWaiters()
    {
        var sut = new AsyncMutex();
        var token = await sut.EnterAsync();

        token.Dispose();

        sut.Participants.TestEquals( 0 ).Go();
    }

    [Fact]
    public async Task LockDispose_ShouldNotifyNextWaiter()
    {
        var sut = new AsyncMutex();
        var first = await sut.EnterAsync();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            first.Dispose();
        } );

        var token = await sut.EnterAsync();
        _ = sut.EnterAsync();

        Assertion.All( token.Mutex.TestRefEquals( sut ), sut.Participants.TestEquals( 2 ) ).Go();
    }

    [Fact]
    public void LockDispose_ShouldNotThrow_ForDefault()
    {
        var token = default( AsyncMutexToken );
        var action = Lambda.Of( () => token.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task LockDispose_ShouldDoNothing_WhenCalledOnDisposedToken()
    {
        var sut = new AsyncMutex();
        var first = await sut.EnterAsync();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            first.Dispose();
        } );

        _ = await sut.EnterAsync();
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestNull(), sut.Participants.TestEquals( 1 ) ) ).Go();
    }

    [Fact]
    public void TrimExcess_ShouldNotImpactParticipants()
    {
        var sut = new AsyncMutex();
        _ = sut.EnterAsync();
        _ = sut.EnterAsync();
        _ = sut.EnterAsync();

        sut.TrimExcess();

        sut.Participants.TestEquals( 3 ).Go();
    }
}
