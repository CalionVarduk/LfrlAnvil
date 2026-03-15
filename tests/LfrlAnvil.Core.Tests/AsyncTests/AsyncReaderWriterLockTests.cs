using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class AsyncReaderWriterLockTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateNonAcquiredMutex()
    {
        var sut = new AsyncReaderWriterLock();
        Assertion.All( sut.Participants.TestEquals( 0 ) ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldReturnImmediately_ForFirstParticipant()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();
        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldReturnImmediately_ForParticipantsFollowingEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();

        var token1 = await sut.EnterReadAsync();
        var token2 = await sut.EnterReadAsync();
        var token3 = await sut.EnterReadAsync();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                token3.Lock.TestRefEquals( sut ),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public void EnterReadAsync_ShouldThrowOperationCanceledException_WhenFirstParticipantCancellationTokenIsCancelled()
    {
        var sut = new AsyncReaderWriterLock();
        var action = Lambda.Of( async () => await sut.EnterReadAsync( new CancellationToken( canceled: true ) ) );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                sut.Participants.TestEquals( 0 ) ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldReturnImmediately_ForFirstParticipant()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();
        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public void EnterWriteAsync_ShouldThrowOperationCanceledException_WhenFirstParticipantCancellationTokenIsCancelled()
    {
        var sut = new AsyncReaderWriterLock();
        var action = Lambda.Of( async () => await sut.EnterWriteAsync( new CancellationToken( canceled: true ) ) );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                sut.Participants.TestEquals( 0 ) ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldBeBlocked_WhenEnteredWriteExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        var a = Task.Run( async () => _ = await sut.EnterReadAsync() );
        var b = Task.Run( async () => _ = await sut.EnterReadAsync() );
        await Task.Delay( 15 );

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldBeBlocked_WhenEnteredWriteExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        var b = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        await Task.Delay( 15 );

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldBeBlocked_WhenEnteredReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        var b = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        await Task.Delay( 15 );

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_WhenParticipantCancels_FollowedByRead()
    {
        var source = new CancellationTokenSource();
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        Task? next = null;
        var action = Lambda.Of( async () =>
        {
            source.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );
            var result = sut.EnterReadAsync( source.Token );
            next = sut.EnterReadAsync().AsTask();
            await result;
        } );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                token.Lock.TestRefEquals( sut ),
                sut.Participants.TestEquals( 2 ),
                next.TestNotNull( t => t.IsCompleted.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_WhenParticipantCancels_FollowedByWrite()
    {
        var source = new CancellationTokenSource();
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        Task? next = null;
        var action = Lambda.Of( async () =>
        {
            source.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );
            var result = sut.EnterReadAsync( source.Token );
            next = sut.EnterWriteAsync().AsTask();
            await result;
        } );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                token.Lock.TestRefEquals( sut ),
                sut.Participants.TestEquals( 2 ),
                next.TestNotNull( t => t.IsCompleted.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_WhenParticipantCancels_FollowedByRead()
    {
        var source = new CancellationTokenSource();
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        Task? next = null;
        var action = Lambda.Of( async () =>
        {
            source.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );
            var result = sut.EnterWriteAsync( source.Token );
            next = sut.EnterReadAsync().AsTask();
            await result;
        } );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                token.Lock.TestRefEquals( sut ),
                sut.Participants.TestEquals( 2 ),
                next.TestNotNull( t => t.IsCompleted.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_WhenParticipantCancels_FollowedByWrite()
    {
        var source = new CancellationTokenSource();
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        Task? next = null;
        var action = Lambda.Of( async () =>
        {
            source.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );
            var result = sut.EnterWriteAsync( source.Token );
            next = sut.EnterWriteAsync().AsTask();
            await result;
        } );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                token.Lock.TestRefEquals( sut ),
                sut.Participants.TestEquals( 2 ),
                next.TestNotNull( t => t.IsCompleted.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_WhenParticipantCancels_FollowedByWriteAndBlockedByRead()
    {
        var source = new CancellationTokenSource();
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();

        Task? next = null;
        var action = Lambda.Of( async () =>
        {
            source.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );
            var result = sut.EnterWriteAsync( source.Token );
            next = sut.EnterWriteAsync().AsTask();
            await result;
        } );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                token.Lock.TestRefEquals( sut ),
                sut.Participants.TestEquals( 2 ),
                next.TestNotNull( t => t.IsCompleted.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_WhenParticipantCancels_FollowedByReadAndBlockedByRead()
    {
        var source = new CancellationTokenSource();
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();

        Task? next1 = null;
        Task? next2 = null;
        var action = Lambda.Of( async () =>
        {
            source.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );
            var result = sut.EnterWriteAsync( source.Token );
            next1 = sut.EnterReadAsync().AsTask();
            next2 = sut.EnterReadAsync().AsTask();
            await result;
        } );

        await Task.WhenAll( next1 ?? Task.CompletedTask, next2 ?? Task.CompletedTask );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                token.Lock.TestRefEquals( sut ),
                sut.Participants.TestEquals( 3 ),
                next1.TestNotNull(),
                next2.TestNotNull() ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_WhenParticipantCancels_FollowedByReadThenWriteAndBlockedByRead()
    {
        var source = new CancellationTokenSource();
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();

        Task? next1 = null;
        Task? next2 = null;
        Task? next3 = null;
        Task? next4 = null;
        var action = Lambda.Of( async () =>
        {
            source.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );
            var result = sut.EnterWriteAsync( source.Token );
            next1 = sut.EnterReadAsync().AsTask();
            next2 = sut.EnterReadAsync().AsTask();
            next3 = sut.EnterWriteAsync().AsTask();
            next4 = sut.EnterReadAsync().AsTask();
            await result;
        } );

        await Task.WhenAll( next1 ?? Task.CompletedTask, next2 ?? Task.CompletedTask );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                token.Lock.TestRefEquals( sut ),
                sut.Participants.TestEquals( 5 ),
                next1.TestNotNull(),
                next2.TestNotNull(),
                next3.TestNotNull( t => t.IsCompleted.TestFalse() ),
                next4.TestNotNull( t => t.IsCompleted.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void TryEnterRead_ShouldReturnEnteredToken_ForFirstParticipant()
    {
        var sut = new AsyncReaderWriterLock();
        var token = sut.TryEnterRead( out var entered );
        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public void TryEnterRead_ShouldReturnImmediately_ForParticipantsFollowingEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();

        var token1 = sut.TryEnterRead( out var entered1 );
        var token2 = sut.TryEnterRead( out var entered2 );
        var token3 = sut.TryEnterRead( out var entered3 );

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                token3.Lock.TestRefEquals( sut ),
                sut.Participants.TestEquals( 3 ),
                entered1.TestTrue(),
                entered2.TestTrue(),
                entered3.TestTrue() )
            .Go();
    }

    [Fact]
    public void TryEnterRead_ShouldReturnNotEnteredToken_ForBlockedParticipant()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterWrite( out _ );

        var token = sut.TryEnterRead( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterWrite_ShouldReturnEnteredToken_ForFirstParticipant()
    {
        var sut = new AsyncReaderWriterLock();
        var token = sut.TryEnterWrite( out var entered );
        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public void TryEnterWrite_ShouldReturnNotEnteredToken_ForBlockedParticipant()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterRead( out _ );

        var token = sut.TryEnterWrite( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WithoutWaiters()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();

        token.Dispose();

        sut.Participants.TestEquals( 0 ).Go();
    }

    [Fact]
    public void ReadLockDispose_ShouldNotThrow_ForDefault()
    {
        var token = default( AsyncReaderWriterLockReadToken );
        var action = Lambda.Of( () => token.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldDoNothing_WhenCalledOnDisposedToken()
    {
        var sut = new AsyncReaderWriterLock();
        var first = await sut.EnterReadAsync();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            first.Dispose();
        } );

        _ = await sut.EnterWriteAsync();
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestNull(), sut.Participants.TestEquals( 1 ) ) ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WithoutWaiters()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        token.Dispose();

        sut.Participants.TestEquals( 0 ).Go();
    }

    [Fact]
    public void WriteLockDispose_ShouldNotThrow_ForDefault()
    {
        var token = default( AsyncReaderWriterLockWriteToken );
        var action = Lambda.Of( () => token.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldDoNothing_WhenCalledOnDisposedToken()
    {
        var sut = new AsyncReaderWriterLock();
        var first = await sut.EnterWriteAsync();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            first.Dispose();
        } );

        _ = await sut.EnterWriteAsync();
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestNull(), sut.Participants.TestEquals( 1 ) ) ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        var first = await sut.EnterReadAsync();
        var second = await sut.EnterReadAsync();

        first.Dispose();

        Assertion.All( second.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLockAndActivatePendingWrite_WhenFirstAndFollowedByWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var first = await sut.EnterReadAsync();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            first.Dispose();
        } );

        var second = await sut.EnterWriteAsync();

        Assertion.All( second.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenNotFirstAndFollowedByEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        var first = await sut.EnterReadAsync();
        var second = await sut.EnterReadAsync();
        var third = await sut.EnterReadAsync();

        second.Dispose();

        Assertion.All( first.Lock.TestRefEquals( sut ), third.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 2 ) ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenNotFirstAndFollowedByWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var first = await sut.EnterReadAsync();
        var second = await sut.EnterReadAsync();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            second.Dispose();
        } );

        var third = sut.EnterWriteAsync().AsTask();
        await Task.Delay( 30 );

        Assertion.All( first.Lock.TestRefEquals( sut ), third.IsCompleted.TestFalse(), sut.Participants.TestEquals( 2 ) ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLockAndActivatePendingWrite_WhenFollowedByWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var first = await sut.EnterWriteAsync();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            first.Dispose();
        } );

        var second = await sut.EnterWriteAsync();

        Assertion.All( second.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLockAndActivatePendingWrite_WhenFollowedByRead()
    {
        var sut = new AsyncReaderWriterLock();
        var first = await sut.EnterWriteAsync();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            first.Dispose();
        } );

        var second = await sut.EnterReadAsync();
        var third = await sut.EnterReadAsync();

        Assertion.All( second.Lock.TestRefEquals( sut ), third.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 2 ) ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLockAndActivatePendingWrite_WhenFollowedByReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var first = await sut.EnterWriteAsync();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            first.Dispose();
        } );

        var second = await sut.EnterReadAsync();
        var third = await sut.EnterReadAsync();
        var fourth = sut.EnterWriteAsync().AsTask();
        var fifth = sut.EnterReadAsync().AsTask();
        await Task.Delay( 15 );

        Assertion.All(
                second.Lock.TestRefEquals( sut ),
                third.Lock.TestRefEquals( sut ),
                fourth.IsCompleted.TestFalse(),
                fifth.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public void TrimExcess_ShouldNotImpactParticipants()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.EnterReadAsync();
        _ = sut.EnterWriteAsync();
        _ = sut.EnterReadAsync();

        sut.TrimExcess();

        sut.Participants.TestEquals( 3 ).Go();
    }
}
