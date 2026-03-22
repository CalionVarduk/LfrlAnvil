using System.Diagnostics.Contracts;
using System.Linq;
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
    public async Task EnterReadAsync_ShouldReturnImmediately_ForParticipantsFollowingEnteredUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();

        var token1 = await sut.EnterUpgradeableReadAsync();
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
    public async Task EnterReadAsync_ShouldBeBlocked_WhenEnteredWriteExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        var a = Task.Run( async () => _ = await sut.EnterReadAsync() );
        var b = Task.Run( async () => _ = await sut.EnterReadAsync() );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldBeBlocked_WhenEnteredUpgradingReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = await sut.EnterUpgradeableReadAsync();
        var token2 = await sut.EnterReadAsync();
        var a = Task.Run( async () => _ = await token1.UpgradeAsync() );
        await Delay();

        var b = Task.Run( async () => _ = await sut.EnterReadAsync() );
        var c = Task.Run( async () => _ = await sut.EnterReadAsync() );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                c.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldBeBlocked_WhenEnteredUpgradingReadIsLast()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = await sut.EnterReadAsync();
        var token2 = await sut.EnterUpgradeableReadAsync();
        var a = Task.Run( async () => _ = await token2.UpgradeAsync() );
        await Delay();

        var b = Task.Run( async () => _ = await sut.EnterReadAsync() );
        var c = Task.Run( async () => _ = await sut.EnterReadAsync() );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                c.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldBeBlocked_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = await sut.EnterUpgradeableReadAsync();
        var token2 = await token1.UpgradeAsync();

        var a = Task.Run( async () => _ = await sut.EnterReadAsync() );
        var b = Task.Run( async () => _ = await sut.EnterReadAsync() );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 3 ) )
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
    public async Task EnterWriteAsync_ShouldBeBlocked_WhenEnteredWriteExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        var b = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        await Delay();

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
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldBeBlocked_WhenEnteredUpgradeableReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        var b = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldBeBlocked_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = await sut.EnterUpgradeableReadAsync();
        var token2 = await token1.UpgradeAsync();

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        var b = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldReturnImmediately_ForFirstParticipant()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldReturnImmediately_ForParticipantFollowingEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();

        var token1 = await sut.EnterReadAsync();
        var token2 = await sut.EnterReadAsync();
        var token3 = await sut.EnterUpgradeableReadAsync();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                token3.Lock.TestRefEquals( sut ),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldBeBlocked_WhenEnteredUpgradeableReadExists()
    {
        var sut = new AsyncReaderWriterLock();

        var token1 = await sut.EnterUpgradeableReadAsync();
        var token2 = await sut.EnterReadAsync();

        var a = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync() );
        var b = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync() );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldBeBlocked_WhenEnteredWriteExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        var a = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync() );
        var b = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync() );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldBeBlocked_WhenEnteredUpgradingReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = await sut.EnterUpgradeableReadAsync();
        var token2 = await sut.EnterReadAsync();
        var a = Task.Run( async () => _ = await token1.UpgradeAsync() );
        await Delay();

        var b = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync() );
        var c = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync() );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                c.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldBeBlocked_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = await sut.EnterUpgradeableReadAsync();
        var token2 = await token1.UpgradeAsync();

        var a = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync() );
        var b = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync() );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldReturnImmediately_ForFirstParticipant()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        var token = await source.UpgradeAsync();
        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldReturnImmediately_WhenIsFirstAndFollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        await Delay();

        var token = await source.UpgradeAsync();

        Assertion.All( token.Lock.TestRefEquals( sut ), a.IsCompleted.TestFalse(), sut.Participants.TestEquals( 2 ) ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldReturnImmediately_WhenIsFirstAndFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();

        var a = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync() );
        await Delay();

        var token = await source.UpgradeAsync();

        Assertion.All( token.Lock.TestRefEquals( sut ), a.IsCompleted.TestFalse(), sut.Participants.TestEquals( 2 ) ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldBeBlocked_WhenPrecedingEnteredReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();
        var source = await sut.EnterUpgradeableReadAsync();

        var a = Task.Run( async () => _ = await source.UpgradeAsync() );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                source.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldBeBlocked_WhenFollowingEnteredReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        var token = await sut.EnterReadAsync();

        var a = Task.Run( async () => _ = await source.UpgradeAsync() );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                source.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                sut.Participants.TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void TokenUpgradeAsync_ShouldThrowInvalidOperationException_ForDefault()
    {
        var sut = default( AsyncReaderWriterLockUpgradeableReadToken );
        var action = Lambda.Of( () => sut.UpgradeAsync() );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowInvalidOperationException_ForDisposedToken()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        source.Dispose();

        var action = Lambda.Of( () => source.UpgradeAsync() );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowInvalidOperationException_ForUpgradingToken()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var source = await sut.EnterUpgradeableReadAsync();

        _ = Task.Run( async () => _ = await source.UpgradeAsync() );
        await Delay();

        var action = Lambda.Of( () => source.UpgradeAsync() );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowInvalidOperationException_ForUpgradedToken()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        _ = await source.UpgradeAsync();

        var action = Lambda.Of( () => source.UpgradeAsync() );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void EnterReadAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var sut = new AsyncReaderWriterLock();
        var action = Lambda.Of( async () => await sut.EnterReadAsync( new CancellationToken( canceled: true ) ) );
        TestCancellation( sut, action, 0 ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByNone()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();

        var action = Lambda.Of( async () => await sut.EnterReadAsync( GetFutureCancellation() ) );

        TestCancellation( sut, action, 1 ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();
        _ = sut.EnterReadAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        _ = token.UpgradeAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        _ = sut.EnterWriteAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();
        _ = sut.EnterUpgradeableReadAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = token.UpgradeAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradedRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await token.UpgradeAsync();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public void EnterWriteAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var sut = new AsyncReaderWriterLock();
        var action = Lambda.Of( async () => await sut.EnterWriteAsync( new CancellationToken( canceled: true ) ) );
        TestCancellation( sut, action, 0 ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByNone()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();

        var action = Lambda.Of( async () => await sut.EnterWriteAsync( GetFutureCancellation() ) );

        TestCancellation( sut, action, 1 ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();
        _ = sut.EnterReadAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();

        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            var next2 = sut.EnterReadAsync().AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 3 ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            var next1 = sut.EnterUpgradeableReadAsync().AsTask();
            var next2 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterWriteAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingReadThenUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            var next2 = sut.EnterUpgradeableReadAsync().AsTask();
            var next3 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterWriteAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1, next2, next3 ] );
        } );

        TestCancellation( sut, action, 6, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        _ = sut.EnterWriteAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();
        _ = sut.EnterUpgradeableReadAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();

        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            var next2 = sut.EnterReadAsync().AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 3 ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = token.UpgradeAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradedRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await token.UpgradeAsync();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public void EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var sut = new AsyncReaderWriterLock();
        var action = Lambda.Of( async () => await sut.EnterUpgradeableReadAsync( new CancellationToken( canceled: true ) ) );
        TestCancellation( sut, action, 0 ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByNone()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();

        var action = Lambda.Of( async () => await sut.EnterUpgradeableReadAsync( GetFutureCancellation() ) );

        TestCancellation( sut, action, 1 ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();
        _ = sut.EnterReadAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();

        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            var next2 = sut.EnterReadAsync().AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 4 ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterWriteAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByBlockedPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        _ = token.UpgradeAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();
        _ = sut.EnterWriteAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterWriteAsync();
        _ = sut.EnterUpgradeableReadAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();

        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            var next2 = sut.EnterReadAsync().AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 3 ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterWriteAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = token.UpgradeAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradedRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await token.UpgradeAsync();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( GetFutureCancellation() );
            next[0] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();

        var action = Lambda.Of( async () => await token.UpgradeAsync( new CancellationToken( canceled: true ) ) );

        TestCancellation( sut, action, 1 ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByNone()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();

        var action = Lambda.Of( async () => await token.UpgradeAsync( GetFutureCancellation() ) );

        TestCancellation( sut, action, 2 ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();

        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            var next2 = sut.EnterReadAsync().AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 4 ).Go();
    }

    [Fact]
    public async Task
        TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByPendingReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterWriteAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByEnteredReadThenPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        _ = await sut.EnterReadAsync();

        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            var next2 = sut.EnterReadAsync().AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 5 ).Go();
    }

    [Fact]
    public async Task
        TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByEnteredReadThenPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 6, next ).Go();
    }

    [Fact]
    public async Task
        TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByEnteredReadThenPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task
        TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByEnteredReadThenPendingReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        _ = await sut.EnterReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync().AsTask();
            next[0] = sut.EnterWriteAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 6, next ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync().AsTask();
            next[1] = sut.EnterReadAsync().AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 4, next ).Go();
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
    public void TryEnterRead_ShouldReturnImmediately_ForParticipantsFollowingEnteredUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();

        var token1 = sut.TryEnterUpgradeableRead( out var entered1 );
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
    public void TryEnterRead_ShouldReturnNotEnteredToken_WhenEnteredWriteExists()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterWrite( out _ );

        var token = sut.TryEnterRead( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public async Task TryEnterRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradingReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = sut.TryEnterUpgradeableRead( out _ );
        _ = sut.TryEnterRead( out _ );

        _ = Task.Run( async () => _ = await token1.UpgradeAsync() );
        await Delay();

        var token = sut.TryEnterRead( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public async Task TryEnterRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradingReadIsLast()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterRead( out _ );
        var token1 = sut.TryEnterUpgradeableRead( out _ );

        _ = Task.Run( async () => _ = await token1.UpgradeAsync() );
        await Delay();

        var token = sut.TryEnterRead( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = sut.TryEnterUpgradeableRead( out _ );
        _ = token1.TryUpgrade( out _ );

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
    public void TryEnterWrite_ShouldReturnNotEnteredToken_WhenEnteredWriteExists()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterWrite( out _ );

        var token = sut.TryEnterWrite( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterWrite_ShouldReturnNotEnteredToken_WhenEnteredReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterRead( out _ );

        var token = sut.TryEnterWrite( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterWrite_ShouldReturnNotEnteredToken_WhenEnteredUpgradeableReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterUpgradeableRead( out _ );

        var token = sut.TryEnterWrite( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterWrite_ShouldReturnNotEnteredToken_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = sut.TryEnterUpgradeableRead( out _ );
        _ = token1.TryUpgrade( out _ );

        var token = sut.TryEnterWrite( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterUpgradeableRead_ShouldReturnEnteredToken_ForFirstParticipant()
    {
        var sut = new AsyncReaderWriterLock();
        var token = sut.TryEnterUpgradeableRead( out var entered );
        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public void TryEnterUpgradeableRead_ShouldReturnEnteredToken_ForParticipantFollowingEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterRead( out _ );
        _ = sut.TryEnterRead( out _ );

        var token = sut.TryEnterUpgradeableRead( out var entered );

        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 3 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public void TryEnterUpgradeableRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradeableReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterUpgradeableRead( out _ );
        _ = sut.TryEnterRead( out _ );

        var token = sut.TryEnterUpgradeableRead( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterUpgradeableRead_ShouldReturnNotEnteredToken_WhenEnteredWriteExists()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterWrite( out _ );

        var token = sut.TryEnterUpgradeableRead( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public async Task TryEnterUpgradeableRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradingReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = sut.TryEnterUpgradeableRead( out _ );
        _ = sut.TryEnterRead( out _ );

        _ = Task.Run( async () => _ = await token1.UpgradeAsync() );
        await Delay();

        var token = sut.TryEnterUpgradeableRead( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterUpgradeableRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var token1 = sut.TryEnterUpgradeableRead( out _ );
        token1.TryUpgrade( out _ );

        var token = sut.TryEnterUpgradeableRead( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TokenTryUpgrade_ShouldReturnEnteredToken_ForFirstParticipant()
    {
        var sut = new AsyncReaderWriterLock();
        var source = sut.TryEnterUpgradeableRead( out _ );

        var token = source.TryUpgrade( out var entered );

        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 1 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public async Task TokenTryUpgrade_ShouldReturnEnteredToken_WhenIsFirstAndFollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var source = sut.TryEnterUpgradeableRead( out _ );

        _ = Task.Run( async () => _ = await sut.EnterWriteAsync() );
        await Delay();

        var token = source.TryUpgrade( out var entered );

        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 2 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public async Task TokenTryUpgrade_ShouldReturnEnteredToken_WhenIsFirstAndFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var source = sut.TryEnterUpgradeableRead( out _ );

        _ = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync() );
        await Delay();

        var token = source.TryUpgrade( out var entered );

        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants.TestEquals( 2 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public void TokenTryUpgrade_ShouldReturnNotEnteredToken_WhenPrecedingEnteredReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        _ = sut.TryEnterRead( out _ );
        var source = sut.TryEnterUpgradeableRead( out _ );

        var token = source.TryUpgrade( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TokenTryUpgrade_ShouldReturnNotEnteredToken_WhenFollowingEnteredReadExists()
    {
        var sut = new AsyncReaderWriterLock();
        var source = sut.TryEnterUpgradeableRead( out _ );
        _ = sut.TryEnterRead( out _ );

        var token = source.TryUpgrade( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants.TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TokenTryUpgrade_ShouldThrowInvalidOperationException_ForDefault()
    {
        var sut = default( AsyncReaderWriterLockUpgradeableReadToken );
        var action = Lambda.Of( () => sut.TryUpgrade( out _ ) );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenTryUpgrade_ShouldThrowInvalidOperationException_ForDisposedToken()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        source.Dispose();

        var action = Lambda.Of( () => source.TryUpgrade( out _ ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenTryUpgrade_ShouldThrowInvalidOperationException_ForUpgradingToken()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var source = await sut.EnterUpgradeableReadAsync();

        _ = Task.Run( async () => _ = await source.UpgradeAsync() );
        await Delay();

        var action = Lambda.Of( () => source.TryUpgrade( out _ ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenTryUpgrade_ShouldThrowInvalidOperationException_ForUpgradedToken()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        _ = await source.UpgradeAsync();

        var action = Lambda.Of( () => source.TryUpgrade( out _ ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
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
            await Delay();
            first.Dispose();
        } );

        _ = await sut.EnterWriteAsync();
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestNull(), sut.Participants.TestEquals( 1 ) ) ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLast()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();

        token.Dispose();

        sut.Participants.TestEquals( 0 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLastAndPrecededByEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterReadAsync();

        token.Dispose();

        sut.Participants.TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLastAndPrecededByEnteredUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();
        var token = await sut.EnterReadAsync();

        token.Dispose();

        sut.Participants.TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLastAndPrecededByEnteredUpgradingRead()
    {
        var sut = new AsyncReaderWriterLock();
        var other = await sut.EnterUpgradeableReadAsync();
        var token = await sut.EnterReadAsync();
        var upgradeTask = other.UpgradeAsync();

        token.Dispose();
        await upgradeTask;

        sut.Participants.TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLastAndPrecededByEnteredUpgradingReadThenRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var other = await sut.EnterUpgradeableReadAsync();
        var token = await sut.EnterReadAsync();
        var upgradeTask = other.UpgradeAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), upgradeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLastAndPrecededByEnteredReadThenUpgradingRead()
    {
        var sut = new AsyncReaderWriterLock();
        var other = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterReadAsync();
        var upgradeTask = other.UpgradeAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), upgradeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();
        _ = await sut.EnterReadAsync();

        token.Dispose();

        sut.Participants.TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();
        var writeTask1 = sut.EnterWriteAsync();
        var writeTask2 = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await writeTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), writeTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();
        _ = await sut.EnterUpgradeableReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradingRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();
        var other = await sut.EnterUpgradeableReadAsync();
        var upgradeTask = other.UpgradeAsync();

        token.Dispose();
        await upgradeTask;

        sut.Participants.TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradingReadThenEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();
        var other = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var upgradeTask = other.UpgradeAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), upgradeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradingReadThenPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();
        var other = await sut.EnterUpgradeableReadAsync();
        var upgradeTask = other.UpgradeAsync();
        var readTask = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradingReadThenPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();
        var other = await sut.EnterUpgradeableReadAsync();
        var upgradeTask = other.UpgradeAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradingReadThenPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterReadAsync();
        var other = await sut.EnterUpgradeableReadAsync();
        var upgradeTask = other.UpgradeAsync();
        var readTask = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        var other = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterReadAsync();
        var upgradeTask = other.UpgradeAsync().AsTask();
        var readTask = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 3 ), upgradeTask.IsCompleted.TestFalse(), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterReadAsync();
        _ = await sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 3 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadAndPrecededByEnteredUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterUpgradeableReadAsync();
        var token = await sut.EnterReadAsync();
        var readTask = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadAndPrecededByEnteredUpgradingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var other = await sut.EnterUpgradeableReadAsync();
        var token = await sut.EnterReadAsync();
        var upgradeTask = other.UpgradeAsync().AsTask();
        var readTask = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 3 ), upgradeTask.IsCompleted.TestFalse(), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterReadAsync();
        _ = await sut.EnterUpgradeableReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 3 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredUpgradingRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterReadAsync();
        var other = await sut.EnterUpgradeableReadAsync();
        var upgradeTask = other.UpgradeAsync().AsTask();
        var readTask = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 3 ), upgradeTask.IsCompleted.TestFalse(), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenPrecededByEnteredUpgradingReadAndFollowedByEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        var other = await sut.EnterUpgradeableReadAsync();
        var token = await sut.EnterReadAsync();
        _ = await sut.EnterReadAsync();
        var upgradeTask = other.UpgradeAsync().AsTask();
        var readTask = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 3 ), upgradeTask.IsCompleted.TestFalse(), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenPrecededByEnteredUpgradingReadAndFollowedByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        var other = await sut.EnterUpgradeableReadAsync();
        var token = await sut.EnterReadAsync();
        var upgradeTask = other.UpgradeAsync();
        var readTask = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenPrecededByEnteredUpgradingReadAndFollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var other = await sut.EnterUpgradeableReadAsync();
        var token = await sut.EnterReadAsync();
        var upgradeTask = other.UpgradeAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenPrecededByEnteredUpgradingReadAndFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var other = await sut.EnterUpgradeableReadAsync();
        var token = await sut.EnterReadAsync();
        var upgradeTask = other.UpgradeAsync();
        var readTask = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
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
            await Delay();
            first.Dispose();
        } );

        _ = await sut.EnterReadAsync();
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestNull(), sut.Participants.TestEquals( 1 ) ) ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WhenLast()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();

        token.Dispose();

        sut.Participants.TestEquals( 0 ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WhenFollowedByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();
        var readTask1 = sut.EnterReadAsync();
        var readTask2 = sut.EnterReadAsync();

        token.Dispose();
        await readTask1;
        await readTask2;

        sut.Participants.TestEquals( 2 ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WhenFollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();
        var readTask1 = sut.EnterReadAsync();
        var readTask2 = sut.EnterUpgradeableReadAsync();
        var readTask3 = sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask4 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await readTask3;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 5 ), writeTask.IsCompleted.TestFalse(), readTask4.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WhenFollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();
        var writeTask1 = sut.EnterWriteAsync();
        var writeTask2 = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await writeTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), writeTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterWriteAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask3 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 4 ), writeTask.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public void UpgradeableReadLockDispose_ShouldNotThrow_ForDefault()
    {
        var token = default( AsyncReaderWriterLockUpgradeableReadToken );
        var action = Lambda.Of( () => token.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldDoNothing_WhenCalledOnDisposedToken()
    {
        var sut = new AsyncReaderWriterLock();
        var first = await sut.EnterUpgradeableReadAsync();
        _ = Task.Run( async () =>
        {
            await Delay();
            first.Dispose();
        } );

        _ = await sut.EnterWriteAsync();
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestNull(), sut.Participants.TestEquals( 1 ) ) ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenLast()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();

        token.Dispose();

        sut.Participants.TestEquals( 0 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();

        token.Dispose();

        sut.Participants.TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var readTask3 = sut.EnterReadAsync();

        token.Dispose();
        await readTask1;
        await readTask2;
        await readTask3;

        sut.Participants.TestEquals( 4 ).Go();
    }

    [Fact]
    public async Task
        UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenPendingUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask3 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 5 ), writeTask.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task
        UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenPendingUpgradeableReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 3 ), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenPendingUpgradeableReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        _ = await sut.EnterReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask2 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 5 ), writeTask.IsCompleted.TestFalse(), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenWriteThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask1 = sut.EnterUpgradeableReadAsync().AsTask();
        var readTask2 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All(
                sut.Participants.TestEquals( 4 ),
                writeTask.IsCompleted.TestFalse(),
                readTask1.IsCompleted.TestFalse(),
                readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenWriteThenRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask1 = sut.EnterReadAsync().AsTask();
        var readTask2 = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All(
                sut.Participants.TestEquals( 4 ),
                writeTask.IsCompleted.TestFalse(),
                readTask1.IsCompleted.TestFalse(),
                readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        var writeTask1 = sut.EnterWriteAsync();
        var writeTask2 = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await writeTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), writeTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var readTask3 = sut.EnterReadAsync();

        token.Dispose();
        await readTask1;
        await readTask2;
        await readTask3;

        sut.Participants.TestEquals( 3 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask3 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 4 ), writeTask.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingUpgradeableReadThenReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var readTask3 = sut.EnterUpgradeableReadAsync().AsTask();
        var readTask4 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 4 ), readTask3.IsCompleted.TestFalse(), readTask4.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingUpgradeableReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();

        token.Dispose();
        await readTask1;

        sut.Participants.TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();

        token.Dispose();

        sut.Participants.TestEquals( 2 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var readTask3 = sut.EnterReadAsync();

        token.Dispose();
        await readTask1;
        await readTask2;
        await readTask3;

        sut.Participants.TestEquals( 5 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenPendingUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask3 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 6 ), writeTask.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenPendingUpgradeableReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 4 ), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenPendingUpgradeableReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        _ = await sut.EnterReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask2 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 6 ), writeTask.IsCompleted.TestFalse(), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenWriteThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask1 = sut.EnterUpgradeableReadAsync().AsTask();
        var readTask2 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All(
                sut.Participants.TestEquals( 5 ),
                writeTask.IsCompleted.TestFalse(),
                readTask1.IsCompleted.TestFalse(),
                readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenWriteThenRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask1 = sut.EnterReadAsync().AsTask();
        var readTask2 = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All(
                sut.Participants.TestEquals( 5 ),
                writeTask.IsCompleted.TestFalse(),
                readTask1.IsCompleted.TestFalse(),
                readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var readTask3 = sut.EnterReadAsync();

        token.Dispose();
        await readTask1;
        await readTask2;
        await readTask3;

        sut.Participants.TestEquals( 4 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask3 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 5 ), writeTask.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadThenReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterReadAsync();
        var readTask3 = sut.EnterUpgradeableReadAsync().AsTask();
        var readTask4 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 5 ), readTask3.IsCompleted.TestFalse(), readTask4.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();
        var readTask2 = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 3 ), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        var readTask1 = sut.EnterUpgradeableReadAsync();

        token.Dispose();
        await readTask1;

        sut.Participants.TestEquals( 2 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenDoneAfterUpgradeAndDowngrade()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        var upgraded = await token.UpgradeAsync();
        upgraded.Dispose();

        token.Dispose();

        sut.Participants.TestEquals( 0 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldThrowInvalidOperationException_WhenCalledOnUpgradingToken()
    {
        var sut = new AsyncReaderWriterLock();
        _ = await sut.EnterReadAsync();
        var token = await sut.EnterUpgradeableReadAsync();
        var upgradeTask = token.UpgradeAsync().AsTask();

        var action = Lambda.Of( () => token.Dispose() );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<InvalidOperationException>(),
                sut.Participants.TestEquals( 2 ),
                upgradeTask.IsCompleted.TestFalse() ) )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldThrowInvalidOperationException_WhenCalledOnUpgradedToken()
    {
        var sut = new AsyncReaderWriterLock();
        var token = await sut.EnterUpgradeableReadAsync();
        _ = await token.UpgradeAsync();

        var action = Lambda.Of( () => token.Dispose() );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<InvalidOperationException>(),
                sut.Participants.TestEquals( 1 ) ) )
            .Go();
    }

    [Fact]
    public void UpgradedReadLockDispose_ShouldNotThrow_ForDefault()
    {
        var token = default( AsyncReaderWriterLockUpgradedReadToken );
        var action = Lambda.Of( () => token.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldThrowInvalidOperationException_WhenCalledOnDisposedToken()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        var first = await source.UpgradeAsync();
        _ = Task.Run( async () =>
        {
            await Delay();
            first.Dispose();
        } );

        _ = await sut.EnterReadAsync();
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestType().Exact<InvalidOperationException>(), sut.Participants.TestEquals( 2 ) ) ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenLast()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        var token = await source.UpgradeAsync();

        token.Dispose();

        sut.Participants.TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenFollowedByPendingRead()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        var token = await source.UpgradeAsync();
        var readTask1 = sut.EnterReadAsync();
        var readTask2 = sut.EnterReadAsync();

        token.Dispose();
        await readTask1;
        await readTask2;

        sut.Participants.TestEquals( 3 ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenFollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        var token = await source.UpgradeAsync();
        var readTask1 = sut.EnterReadAsync();
        var readTask2 = sut.EnterUpgradeableReadAsync();
        var readTask3 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 4 ), readTask2.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenFollowedByPendingReadThenWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        var token = await source.UpgradeAsync();
        var readTask1 = sut.EnterReadAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();
        var readTask2 = sut.EnterReadAsync().AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 4 ), writeTask.IsCompleted.TestFalse(), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenFollowedByPendingWrite()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        var token = await source.UpgradeAsync();
        var writeTask = sut.EnterWriteAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        var token = await source.UpgradeAsync();
        var readTask = sut.EnterUpgradeableReadAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants.TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradedReadToken_GetReadToken_ShouldReturnBaseToken()
    {
        var sut = new AsyncReaderWriterLock();
        var source = await sut.EnterUpgradeableReadAsync();
        var token = await source.UpgradeAsync();

        var result = token.GetReadToken();

        result.TestEquals( source ).Go();
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

    [Pure]
    private static CancellationToken GetFutureCancellation(TimeSpan? delay = null)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter( delay ?? TimeSpan.FromMilliseconds( 15 ) );
        return cts.Token;
    }

    private static Task Delay(TimeSpan? delay = null)
    {
        return Task.Delay( delay ?? TimeSpan.FromMilliseconds( 15 ) );
    }

    private static async Task WaitInOrder(Task task, Task next)
    {
        try
        {
            await task.WaitAsync( TimeSpan.FromSeconds( 15 ) );
        }
        finally
        {
            await next.WaitAsync( TimeSpan.FromSeconds( 15 ) );
        }
    }

    private static Task WaitInOrder<T>(ValueTask<T> task, Task[] next)
    {
        return WaitInOrder( task.AsTask(), Task.WhenAll( next.Select( t => t.WaitAsync( TimeSpan.FromSeconds( 15 ) ) ) ) );
    }

    private static Task WaitThenDelay<T>(ValueTask<T> task, TimeSpan? delay = null)
    {
        return WaitInOrder( task.AsTask(), Delay( delay ) );
    }

    private static Task WaitInOrderThenDelay<T>(ValueTask<T> task, Task[] next, TimeSpan? delay = null)
    {
        return WaitInOrder( WaitInOrder( task, next ), Delay( delay ) );
    }

    [Pure]
    private static CallAssertion TestCancellation(
        AsyncReaderWriterLock sut,
        Func<Task> action,
        int expectedParticipants,
        Task?[]? pendingTasks = null)
    {
        return action.Test( GetCancellationAssertion( sut, expectedParticipants, pendingTasks ) );
    }

    [Pure]
    private static CallAssertion TestCancellation<T>(
        AsyncReaderWriterLock sut,
        Func<Task<T>> action,
        int expectedParticipants,
        Task?[]? pendingTasks = null)
    {
        return action.Test( GetCancellationAssertion( sut, expectedParticipants, pendingTasks ) );
    }

    private static Func<Exception?, Assertion> GetCancellationAssertion(
        AsyncReaderWriterLock sut,
        int expectedParticipants,
        Task?[]? pendingTasks)
    {
        pendingTasks ??= [ ];
        return exc => Assertion.All(
            exc.TestType().AssignableTo<OperationCanceledException>(),
            sut.Participants.TestEquals( expectedParticipants ),
            pendingTasks.TestAll( (e, _) => e.TestNotNull( t => t.IsCompleted.TestFalse() ) ) );
    }

    [Pure]
    private static Task?[] PrepareNextTasks(int count)
    {
        return new Task?[count];
    }
}
