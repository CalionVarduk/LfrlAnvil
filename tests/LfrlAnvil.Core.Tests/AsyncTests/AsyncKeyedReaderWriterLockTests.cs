using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class AsyncKeyedReaderWriterLockTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateNonAcquiredMutex()
    {
        var keyComparer = StringComparer.InvariantCultureIgnoreCase;
        var sut = new AsyncKeyedReaderWriterLock<string>( keyComparer );
        Assertion.All( sut.KeyComparer.TestRefEquals( keyComparer ), sut.ActiveKeys.TestEmpty() ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldReturnImmediately_ForFirstParticipant()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = await sut.EnterReadAsync( "foo" );
        var token2 = await sut.EnterReadAsync( "bar" );

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token1.Key.TestEquals( "foo" ),
                token2.Lock.TestRefEquals( sut ),
                token2.Key.TestEquals( "bar" ),
                sut.ActiveKeys.TestSetEqual( [ "foo", "bar" ] ),
                sut.Participants( "foo" ).TestEquals( 1 ),
                sut.Participants( "bar" ).TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldReturnImmediately_ForParticipantsFollowingEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();

        var token1 = await sut.EnterReadAsync( "foo" );
        var token2 = await sut.EnterReadAsync( "foo" );
        var token3 = await sut.EnterReadAsync( "foo" );

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                token3.Lock.TestRefEquals( sut ),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldReturnImmediately_ForParticipantsFollowingEnteredUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();

        var token1 = await sut.EnterUpgradeableReadAsync( "foo" );
        var token2 = await sut.EnterReadAsync( "foo" );
        var token3 = await sut.EnterReadAsync( "foo" );

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                token3.Lock.TestRefEquals( sut ),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldBeBlocked_WhenEnteredWriteExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterWriteAsync( "foo" );

        var a = Task.Run( async () => _ = await sut.EnterReadAsync( "foo" ) );
        var b = Task.Run( async () => _ = await sut.EnterReadAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldBeBlocked_WhenEnteredUpgradingReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = await sut.EnterUpgradeableReadAsync( "foo" );
        var token2 = await sut.EnterReadAsync( "foo" );
        var a = Task.Run( async () => _ = await token1.UpgradeAsync() );
        await Delay();

        var b = Task.Run( async () => _ = await sut.EnterReadAsync( "foo" ) );
        var c = Task.Run( async () => _ = await sut.EnterReadAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                c.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldBeBlocked_WhenEnteredUpgradingReadIsLast()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = await sut.EnterReadAsync( "foo" );
        var token2 = await sut.EnterUpgradeableReadAsync( "foo" );
        var a = Task.Run( async () => _ = await token2.UpgradeAsync() );
        await Delay();

        var b = Task.Run( async () => _ = await sut.EnterReadAsync( "foo" ) );
        var c = Task.Run( async () => _ = await sut.EnterReadAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                c.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldBeBlocked_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = await sut.EnterUpgradeableReadAsync( "foo" );
        var token2 = await token1.UpgradeAsync();

        var a = Task.Run( async () => _ = await sut.EnterReadAsync( "foo" ) );
        var b = Task.Run( async () => _ = await sut.EnterReadAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldReturnImmediately_ForFirstParticipant()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = await sut.EnterWriteAsync( "foo" );
        var token2 = await sut.EnterWriteAsync( "bar" );

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token1.Key.TestEquals( "foo" ),
                token2.Lock.TestRefEquals( sut ),
                token2.Key.TestEquals( "bar" ),
                sut.ActiveKeys.TestSetEqual( [ "foo", "bar" ] ),
                sut.Participants( "foo" ).TestEquals( 1 ),
                sut.Participants( "bar" ).TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldBeBlocked_WhenEnteredWriteExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterWriteAsync( "foo" );

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync( "foo" ) );
        var b = Task.Run( async () => _ = await sut.EnterWriteAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldBeBlocked_WhenEnteredReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync( "foo" ) );
        var b = Task.Run( async () => _ = await sut.EnterWriteAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldBeBlocked_WhenEnteredUpgradeableReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync( "foo" ) );
        var b = Task.Run( async () => _ = await sut.EnterWriteAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldBeBlocked_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = await sut.EnterUpgradeableReadAsync( "foo" );
        var token2 = await token1.UpgradeAsync();

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync( "foo" ) );
        var b = Task.Run( async () => _ = await sut.EnterWriteAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldReturnImmediately_ForFirstParticipant()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = await sut.EnterUpgradeableReadAsync( "foo" );
        var token2 = await sut.EnterUpgradeableReadAsync( "bar" );

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token1.Key.TestEquals( "foo" ),
                token2.Lock.TestRefEquals( sut ),
                token2.Key.TestEquals( "bar" ),
                sut.ActiveKeys.TestSetEqual( [ "foo", "bar" ] ),
                sut.Participants( "foo" ).TestEquals( 1 ),
                sut.Participants( "bar" ).TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldReturnImmediately_ForParticipantFollowingEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();

        var token1 = await sut.EnterReadAsync( "foo" );
        var token2 = await sut.EnterReadAsync( "foo" );
        var token3 = await sut.EnterUpgradeableReadAsync( "foo" );

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                token3.Lock.TestRefEquals( sut ),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldBeBlocked_WhenEnteredUpgradeableReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();

        var token1 = await sut.EnterUpgradeableReadAsync( "foo" );
        var token2 = await sut.EnterReadAsync( "foo" );

        var a = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync( "foo" ) );
        var b = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldBeBlocked_WhenEnteredWriteExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterWriteAsync( "foo" );

        var a = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync( "foo" ) );
        var b = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldBeBlocked_WhenEnteredUpgradingReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = await sut.EnterUpgradeableReadAsync( "foo" );
        var token2 = await sut.EnterReadAsync( "foo" );
        var a = Task.Run( async () => _ = await token1.UpgradeAsync() );
        await Delay();

        var b = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync( "foo" ) );
        var c = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                c.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 4 ) )
            .Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldBeBlocked_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = await sut.EnterUpgradeableReadAsync( "foo" );
        var token2 = await token1.UpgradeAsync();

        var a = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync( "foo" ) );
        var b = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync( "foo" ) );
        await Delay();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                b.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldReturnImmediately_ForFirstParticipant()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source1 = await sut.EnterUpgradeableReadAsync( "foo" );
        var source2 = await sut.EnterUpgradeableReadAsync( "bar" );
        var token1 = await source1.UpgradeAsync();
        var token2 = await source2.UpgradeAsync();

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token1.Key.TestEquals( "foo" ),
                token2.Lock.TestRefEquals( sut ),
                token2.Key.TestEquals( "bar" ),
                sut.ActiveKeys.TestSetEqual( [ "foo", "bar" ] ),
                sut.Participants( "foo" ).TestEquals( 1 ),
                sut.Participants( "bar" ).TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldReturnImmediately_WhenIsFirstAndFollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );

        var a = Task.Run( async () => _ = await sut.EnterWriteAsync( "foo" ) );
        await Delay();

        var token = await source.UpgradeAsync();

        Assertion.All( token.Lock.TestRefEquals( sut ), a.IsCompleted.TestFalse(), sut.Participants( "foo" ).TestEquals( 2 ) ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldReturnImmediately_WhenIsFirstAndFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );

        var a = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync( "foo" ) );
        await Delay();

        var token = await source.UpgradeAsync();

        Assertion.All( token.Lock.TestRefEquals( sut ), a.IsCompleted.TestFalse(), sut.Participants( "foo" ).TestEquals( 2 ) ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldBeBlocked_WhenPrecedingEnteredReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );
        var source = await sut.EnterUpgradeableReadAsync( "foo" );

        var a = Task.Run( async () => _ = await source.UpgradeAsync() );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                source.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldBeBlocked_WhenFollowingEnteredReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );

        var a = Task.Run( async () => _ = await source.UpgradeAsync() );
        await Delay();

        Assertion.All(
                token.Lock.TestRefEquals( sut ),
                source.Lock.TestRefEquals( sut ),
                a.IsCompleted.TestFalse(),
                sut.Participants( "foo" ).TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void TokenUpgradeAsync_ShouldThrowInvalidOperationException_ForDefault()
    {
        var sut = default( AsyncKeyedReaderWriterLockUpgradeableReadToken<string> );
        var action = Lambda.Of( () => sut.UpgradeAsync() );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowInvalidOperationException_ForDisposedToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        source.Dispose();

        var action = Lambda.Of( () => source.UpgradeAsync() );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowInvalidOperationException_ForUpgradingToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var source = await sut.EnterUpgradeableReadAsync( "foo" );

        _ = Task.Run( async () => _ = await source.UpgradeAsync() );
        await Delay();

        var action = Lambda.Of( () => source.UpgradeAsync() );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowInvalidOperationException_ForUpgradedToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await source.UpgradeAsync();

        var action = Lambda.Of( () => source.UpgradeAsync() );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void EnterReadAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var action = Lambda.Of( async () => await sut.EnterReadAsync( "foo", new CancellationToken( canceled: true ) ) );
        TestCancellation( sut, action, 0 ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByNone()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );

        var action = Lambda.Of( async () => await sut.EnterReadAsync( "foo", GetFutureCancellation() ) );

        TestCancellation( sut, action, 1 ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );
        _ = sut.EnterReadAsync( "foo" ).AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        _ = token.UpgradeAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        _ = sut.EnterWriteAsync( "foo" ).AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );
        _ = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = token.UpgradeAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradedRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await token.UpgradeAsync();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public void EnterWriteAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var action = Lambda.Of( async () => await sut.EnterWriteAsync( "foo", new CancellationToken( canceled: true ) ) );
        TestCancellation( sut, action, 0 ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByNone()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );

        var action = Lambda.Of( async () => await sut.EnterWriteAsync( "foo", GetFutureCancellation() ) );

        TestCancellation( sut, action, 1 ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );
        _ = sut.EnterReadAsync( "foo" ).AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );

        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            var next2 = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 3 ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            var next2 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingReadThenUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            var next2 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            var next3 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1, next2, next3 ] );
        } );

        TestCancellation( sut, action, 6, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        _ = sut.EnterWriteAsync( "foo" ).AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );
        _ = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );

        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            var next2 = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 3 ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task
        EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = token.UpgradeAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterWriteAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradedRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await token.UpgradeAsync();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterWriteAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public void EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var action = Lambda.Of( async () => await sut.EnterUpgradeableReadAsync( "foo", new CancellationToken( canceled: true ) ) );
        TestCancellation( sut, action, 0 ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByNone()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );

        var action = Lambda.Of( async () => await sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() ) );

        TestCancellation( sut, action, 1 ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );
        _ = sut.EnterReadAsync( "foo" ).AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            var next2 = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 4 ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByPendingReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredRead_FollowedByBlockedPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        _ = token.UpgradeAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );
        _ = sut.EnterWriteAsync( "foo" ).AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterWriteAsync( "foo" );
        _ = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );

        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            var next2 = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 3 ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task
        EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradeableRead_FollowedByPendingReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = token.UpgradeAsync().AsTask();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 3, next ).Go();
    }

    [Fact]
    public async Task EnterUpgradeableReadAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenPrecededByEnteredUpgradedRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await token.UpgradeAsync();

        var next = PrepareNextTasks( 1 );
        var action = Lambda.Of( async () =>
        {
            var result = sut.EnterUpgradeableReadAsync( "foo", GetFutureCancellation() );
            next[0] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 2, next ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );

        var action = Lambda.Of( async () => await token.UpgradeAsync( new CancellationToken( canceled: true ) ) );

        TestCancellation( sut, action, 1 ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByNone()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );

        var action = Lambda.Of( async () => await token.UpgradeAsync( GetFutureCancellation() ) );

        TestCancellation( sut, action, 2 ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );

        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            var next2 = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 4 ).Go();
    }

    [Fact]
    public async Task
        TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByPendingReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByEnteredReadThenPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            var next2 = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrder( result, [ next1, next2 ] );
        } );

        TestCancellation( sut, action, 5 ).Go();
    }

    [Fact]
    public async Task
        TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByEnteredReadThenPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 6, next ).Go();
    }

    [Fact]
    public async Task
        TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByEnteredReadThenPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 5, next ).Go();
    }

    [Fact]
    public async Task
        TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByEnteredReadThenPendingReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            var next1 = sut.EnterReadAsync( "foo" ).AsTask();
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitInOrderThenDelay( result, [ next1 ] );
        } );

        TestCancellation( sut, action, 6, next ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            next[0] = sut.EnterWriteAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public async Task TokenUpgradeAsync_ShouldThrowOperationCanceledException_AfterCancellation_WhenFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );

        var next = PrepareNextTasks( 2 );
        var action = Lambda.Of( async () =>
        {
            var result = token.UpgradeAsync( GetFutureCancellation() );
            next[0] = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
            next[1] = sut.EnterReadAsync( "foo" ).AsTask();
            await WaitThenDelay( result );
        } );

        TestCancellation( sut, action, 4, next ).Go();
    }

    [Fact]
    public void TryEnterRead_ShouldReturnEnteredToken_ForFirstParticipant()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = sut.TryEnterRead( "foo", out var entered1 );
        var token2 = sut.TryEnterRead( "bar", out var entered2 );

        Assertion.All(
                entered1.TestTrue(),
                token1.Lock.TestRefEquals( sut ),
                token1.Key.TestEquals( "foo" ),
                entered2.TestTrue(),
                token2.Lock.TestRefEquals( sut ),
                token2.Key.TestEquals( "bar" ),
                sut.Participants( "foo" ).TestEquals( 1 ),
                sut.Participants( "bar" ).TestEquals( 1 ),
                sut.ActiveKeys.TestSetEqual( [ "foo", "bar" ] ) )
            .Go();
    }

    [Fact]
    public void TryEnterRead_ShouldReturnImmediately_ForParticipantsFollowingEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();

        var token1 = sut.TryEnterRead( "foo", out var entered1 );
        var token2 = sut.TryEnterRead( "foo", out var entered2 );
        var token3 = sut.TryEnterRead( "foo", out var entered3 );

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                token3.Lock.TestRefEquals( sut ),
                sut.Participants( "foo" ).TestEquals( 3 ),
                entered1.TestTrue(),
                entered2.TestTrue(),
                entered3.TestTrue() )
            .Go();
    }

    [Fact]
    public void TryEnterRead_ShouldReturnImmediately_ForParticipantsFollowingEnteredUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();

        var token1 = sut.TryEnterUpgradeableRead( "foo", out var entered1 );
        var token2 = sut.TryEnterRead( "foo", out var entered2 );
        var token3 = sut.TryEnterRead( "foo", out var entered3 );

        Assertion.All(
                token1.Lock.TestRefEquals( sut ),
                token2.Lock.TestRefEquals( sut ),
                token3.Lock.TestRefEquals( sut ),
                sut.Participants( "foo" ).TestEquals( 3 ),
                entered1.TestTrue(),
                entered2.TestTrue(),
                entered3.TestTrue() )
            .Go();
    }

    [Fact]
    public void TryEnterRead_ShouldReturnNotEnteredToken_WhenEnteredWriteExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = sut.TryEnterWrite( "foo", out _ );

        var token = sut.TryEnterRead( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public async Task TryEnterRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradingReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = sut.TryEnterUpgradeableRead( "foo", out _ );
        _ = sut.TryEnterRead( "foo", out _ );

        _ = Task.Run( async () => _ = await token1.UpgradeAsync() );
        await Delay();

        var token = sut.TryEnterRead( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public async Task TryEnterRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradingReadIsLast()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = sut.TryEnterRead( "foo", out _ );
        var token1 = sut.TryEnterUpgradeableRead( "foo", out _ );

        _ = Task.Run( async () => _ = await token1.UpgradeAsync() );
        await Delay();

        var token = sut.TryEnterRead( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = sut.TryEnterUpgradeableRead( "foo", out _ );
        _ = token1.TryUpgrade( out _ );

        var token = sut.TryEnterRead( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterWrite_ShouldReturnEnteredToken_ForFirstParticipant()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = sut.TryEnterWrite( "foo", out var entered1 );
        var token2 = sut.TryEnterWrite( "bar", out var entered2 );

        Assertion.All(
                entered1.TestTrue(),
                token1.Lock.TestRefEquals( sut ),
                token1.Key.TestEquals( "foo" ),
                entered2.TestTrue(),
                token2.Lock.TestRefEquals( sut ),
                token2.Key.TestEquals( "bar" ),
                sut.Participants( "foo" ).TestEquals( 1 ),
                sut.Participants( "bar" ).TestEquals( 1 ),
                sut.ActiveKeys.TestSetEqual( [ "foo", "bar" ] ) )
            .Go();
    }

    [Fact]
    public void TryEnterWrite_ShouldReturnNotEnteredToken_WhenEnteredWriteExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = sut.TryEnterWrite( "foo", out _ );

        var token = sut.TryEnterWrite( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterWrite_ShouldReturnNotEnteredToken_WhenEnteredReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = sut.TryEnterRead( "foo", out _ );

        var token = sut.TryEnterWrite( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterWrite_ShouldReturnNotEnteredToken_WhenEnteredUpgradeableReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = sut.TryEnterUpgradeableRead( "foo", out _ );

        var token = sut.TryEnterWrite( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterWrite_ShouldReturnNotEnteredToken_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = sut.TryEnterUpgradeableRead( "foo", out _ );
        _ = token1.TryUpgrade( out _ );

        var token = sut.TryEnterWrite( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterUpgradeableRead_ShouldReturnEnteredToken_ForFirstParticipant()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = sut.TryEnterUpgradeableRead( "foo", out var entered1 );
        var token2 = sut.TryEnterUpgradeableRead( "bar", out var entered2 );

        Assertion.All(
                entered1.TestTrue(),
                token1.Lock.TestRefEquals( sut ),
                token1.Key.TestEquals( "foo" ),
                entered2.TestTrue(),
                token2.Lock.TestRefEquals( sut ),
                token2.Key.TestEquals( "bar" ),
                sut.Participants( "foo" ).TestEquals( 1 ),
                sut.Participants( "bar" ).TestEquals( 1 ),
                sut.ActiveKeys.TestSetEqual( [ "foo", "bar" ] ) )
            .Go();
    }

    [Fact]
    public void TryEnterUpgradeableRead_ShouldReturnEnteredToken_ForParticipantFollowingEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = sut.TryEnterRead( "foo", out _ );
        _ = sut.TryEnterRead( "foo", out _ );

        var token = sut.TryEnterUpgradeableRead( "foo", out var entered );

        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants( "foo" ).TestEquals( 3 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public void TryEnterUpgradeableRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradeableReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = sut.TryEnterUpgradeableRead( "foo", out _ );
        _ = sut.TryEnterRead( "foo", out _ );

        var token = sut.TryEnterUpgradeableRead( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterUpgradeableRead_ShouldReturnNotEnteredToken_WhenEnteredWriteExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = sut.TryEnterWrite( "foo", out _ );

        var token = sut.TryEnterUpgradeableRead( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public async Task TryEnterUpgradeableRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradingReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = sut.TryEnterUpgradeableRead( "foo", out _ );
        _ = sut.TryEnterRead( "foo", out _ );

        _ = Task.Run( async () => _ = await token1.UpgradeAsync() );
        await Delay();

        var token = sut.TryEnterUpgradeableRead( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TryEnterUpgradeableRead_ShouldReturnNotEnteredToken_WhenEnteredUpgradedReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token1 = sut.TryEnterUpgradeableRead( "foo", out _ );
        token1.TryUpgrade( out _ );

        var token = sut.TryEnterUpgradeableRead( "foo", out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TokenTryUpgrade_ShouldReturnEnteredToken_ForFirstParticipant()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = sut.TryEnterUpgradeableRead( "foo", out _ );

        var token = source.TryUpgrade( out var entered );

        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants( "foo" ).TestEquals( 1 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public async Task TokenTryUpgrade_ShouldReturnEnteredToken_WhenIsFirstAndFollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = sut.TryEnterUpgradeableRead( "foo", out _ );

        _ = Task.Run( async () => _ = await sut.EnterWriteAsync( "foo" ) );
        await Delay();

        var token = source.TryUpgrade( out var entered );

        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants( "foo" ).TestEquals( 2 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public async Task TokenTryUpgrade_ShouldReturnEnteredToken_WhenIsFirstAndFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = sut.TryEnterUpgradeableRead( "foo", out _ );

        _ = Task.Run( async () => _ = await sut.EnterUpgradeableReadAsync( "foo" ) );
        await Delay();

        var token = source.TryUpgrade( out var entered );

        Assertion.All( token.Lock.TestRefEquals( sut ), sut.Participants( "foo" ).TestEquals( 2 ), entered.TestTrue() ).Go();
    }

    [Fact]
    public void TokenTryUpgrade_ShouldReturnNotEnteredToken_WhenPrecedingEnteredReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = sut.TryEnterRead( "foo", out _ );
        var source = sut.TryEnterUpgradeableRead( "foo", out _ );

        var token = source.TryUpgrade( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TokenTryUpgrade_ShouldReturnNotEnteredToken_WhenFollowingEnteredReadExists()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = sut.TryEnterUpgradeableRead( "foo", out _ );
        _ = sut.TryEnterRead( "foo", out _ );

        var token = source.TryUpgrade( out var entered );

        Assertion.All( token.Lock.TestNull(), sut.Participants( "foo" ).TestEquals( 2 ), entered.TestFalse() ).Go();
    }

    [Fact]
    public void TokenTryUpgrade_ShouldThrowInvalidOperationException_ForDefault()
    {
        var sut = default( AsyncKeyedReaderWriterLockUpgradeableReadToken<string> );
        var action = Lambda.Of( () => sut.TryUpgrade( out _ ) );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenTryUpgrade_ShouldThrowInvalidOperationException_ForDisposedToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        source.Dispose();

        var action = Lambda.Of( () => source.TryUpgrade( out _ ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenTryUpgrade_ShouldThrowInvalidOperationException_ForUpgradingToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var source = await sut.EnterUpgradeableReadAsync( "foo" );

        _ = Task.Run( async () => _ = await source.UpgradeAsync() );
        await Delay();

        var action = Lambda.Of( () => source.TryUpgrade( out _ ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task TokenTryUpgrade_ShouldThrowInvalidOperationException_ForUpgradedToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await source.UpgradeAsync();

        var action = Lambda.Of( () => source.TryUpgrade( out _ ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void ReadLockDispose_ShouldNotThrow_ForDefault()
    {
        var token = default( AsyncKeyedReaderWriterLockReadToken<string> );
        var action = Lambda.Of( () => token.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldDoNothing_WhenCalledOnDisposedToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var first = await sut.EnterReadAsync( "foo" );
        _ = Task.Run( async () =>
        {
            await Delay();
            first.Dispose();
        } );

        _ = await sut.EnterWriteAsync( "foo" );
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ) ) ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLast()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );

        token.Dispose();

        Assertion.All( sut.ActiveKeys.TestEmpty(), sut.Participants( "foo" ).TestEquals( 0 ) ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLastAndPrecededByEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );

        token.Dispose();

        sut.Participants( "foo" ).TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLastAndPrecededByEnteredUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );

        token.Dispose();

        sut.Participants( "foo" ).TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLastAndPrecededByEnteredUpgradingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync();

        token.Dispose();
        await upgradeTask;

        sut.Participants( "foo" ).TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLastAndPrecededByEnteredUpgradingReadThenRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), upgradeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenLastAndPrecededByEnteredReadThenUpgradingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), upgradeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        token.Dispose();

        sut.Participants( "foo" ).TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );
        var writeTask1 = sut.EnterWriteAsync( "foo" );
        var writeTask2 = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await writeTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), writeTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );
        _ = await sut.EnterUpgradeableReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync();

        token.Dispose();
        await upgradeTask;

        sut.Participants( "foo" ).TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradingReadThenEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync().AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), upgradeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradingReadThenPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync();
        var readTask = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradingReadThenPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync();
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredUpgradingReadThenPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterReadAsync( "foo" );
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync();
        var readTask = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync().AsTask();
        var readTask = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 3 ), upgradeTask.IsCompleted.TestFalse(), readTask.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 3 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadAndPrecededByEnteredUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var readTask = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadAndPrecededByEnteredUpgradingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync().AsTask();
        var readTask = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 3 ), upgradeTask.IsCompleted.TestFalse(), readTask.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        _ = await sut.EnterUpgradeableReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 3 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredUpgradingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync().AsTask();
        var readTask = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 3 ), upgradeTask.IsCompleted.TestFalse(), readTask.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenPrecededByEnteredUpgradingReadAndFollowedByEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync().AsTask();
        var readTask = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 3 ), upgradeTask.IsCompleted.TestFalse(), readTask.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenPrecededByEnteredUpgradingReadAndFollowedByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync();
        var readTask = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenPrecededByEnteredUpgradingReadAndFollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync();
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task ReadLockDispose_ShouldReleaseLock_WhenPrecededByEnteredUpgradingReadAndFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var other = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await sut.EnterReadAsync( "foo" );
        var upgradeTask = other.UpgradeAsync();
        var readTask = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await upgradeTask;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public void WriteLockDispose_ShouldNotThrow_ForDefault()
    {
        var token = default( AsyncKeyedReaderWriterLockWriteToken<string> );
        var action = Lambda.Of( () => token.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldDoNothing_WhenCalledOnDisposedToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var first = await sut.EnterWriteAsync( "foo" );
        _ = Task.Run( async () =>
        {
            await Delay();
            first.Dispose();
        } );

        _ = await sut.EnterReadAsync( "foo" );
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ) ) ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WhenLast()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterWriteAsync( "foo" );

        token.Dispose();

        Assertion.All( sut.ActiveKeys.TestEmpty(), sut.Participants( "foo" ).TestEquals( 0 ) ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WhenFollowedByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterWriteAsync( "foo" );
        var readTask1 = sut.EnterReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );

        token.Dispose();
        await readTask1;
        await readTask2;

        sut.Participants( "foo" ).TestEquals( 2 ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WhenFollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterWriteAsync( "foo" );
        var readTask1 = sut.EnterReadAsync( "foo" );
        var readTask2 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask3 = sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask4 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await readTask3;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 5 ), writeTask.IsCompleted.TestFalse(), readTask4.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WhenFollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterWriteAsync( "foo" );
        var writeTask1 = sut.EnterWriteAsync( "foo" );
        var writeTask2 = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await writeTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), writeTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task WriteLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterWriteAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask3 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 4 ), writeTask.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public void UpgradeableReadLockDispose_ShouldNotThrow_ForDefault()
    {
        var token = default( AsyncKeyedReaderWriterLockUpgradeableReadToken<string> );
        var action = Lambda.Of( () => token.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldDoNothing_WhenCalledOnDisposedToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var first = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = Task.Run( async () =>
        {
            await Delay();
            first.Dispose();
        } );

        _ = await sut.EnterWriteAsync( "foo" );
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestNull(), sut.Participants( "foo" ).TestEquals( 1 ) ) ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenLast()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );

        token.Dispose();

        Assertion.All( sut.ActiveKeys.TestEmpty(), sut.Participants( "foo" ).TestEquals( 0 ) ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        token.Dispose();

        sut.Participants( "foo" ).TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var readTask3 = sut.EnterReadAsync( "foo" );

        token.Dispose();
        await readTask1;
        await readTask2;
        await readTask3;

        sut.Participants( "foo" ).TestEquals( 4 ).Go();
    }

    [Fact]
    public async Task
        UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenPendingUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask3 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 5 ), writeTask.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task
        UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenPendingUpgradeableReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 3 ), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenPendingUpgradeableReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask2 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 5 ), writeTask.IsCompleted.TestFalse(), readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenWriteThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
        var readTask2 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All(
                sut.Participants( "foo" ).TestEquals( 4 ),
                writeTask.IsCompleted.TestFalse(),
                readTask1.IsCompleted.TestFalse(),
                readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByEnteredReadThenWriteThenRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask1 = sut.EnterReadAsync( "foo" ).AsTask();
        var readTask2 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All(
                sut.Participants( "foo" ).TestEquals( 4 ),
                writeTask.IsCompleted.TestFalse(),
                readTask1.IsCompleted.TestFalse(),
                readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var writeTask1 = sut.EnterWriteAsync( "foo" );
        var writeTask2 = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await writeTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), writeTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var readTask3 = sut.EnterReadAsync( "foo" );

        token.Dispose();
        await readTask1;
        await readTask2;
        await readTask3;

        sut.Participants( "foo" ).TestEquals( 3 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask3 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 4 ), writeTask.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingUpgradeableReadThenReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var readTask3 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
        var readTask4 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 4 ), readTask3.IsCompleted.TestFalse(), readTask4.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingUpgradeableReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFirstAndFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );

        token.Dispose();
        await readTask1;

        sut.Participants( "foo" ).TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );

        token.Dispose();

        sut.Participants( "foo" ).TestEquals( 2 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var readTask3 = sut.EnterReadAsync( "foo" );

        token.Dispose();
        await readTask1;
        await readTask2;
        await readTask3;

        sut.Participants( "foo" ).TestEquals( 5 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenPendingUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask3 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 6 ), writeTask.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenPendingUpgradeableReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 4 ), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenPendingUpgradeableReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask2 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 6 ), writeTask.IsCompleted.TestFalse(), readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenWriteThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
        var readTask2 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All(
                sut.Participants( "foo" ).TestEquals( 5 ),
                writeTask.IsCompleted.TestFalse(),
                readTask1.IsCompleted.TestFalse(),
                readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByEnteredReadThenWriteThenRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask1 = sut.EnterReadAsync( "foo" ).AsTask();
        var readTask2 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All(
                sut.Participants( "foo" ).TestEquals( 5 ),
                writeTask.IsCompleted.TestFalse(),
                readTask1.IsCompleted.TestFalse(),
                readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadThenRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var readTask3 = sut.EnterReadAsync( "foo" );

        token.Dispose();
        await readTask1;
        await readTask2;
        await readTask3;

        sut.Participants( "foo" ).TestEquals( 4 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadThenReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask3 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 5 ), writeTask.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadThenReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );
        var readTask3 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();
        var readTask4 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await readTask2;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 5 ), readTask3.IsCompleted.TestFalse(), readTask4.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask2 = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 3 ), readTask2.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var readTask1 = sut.EnterUpgradeableReadAsync( "foo" );

        token.Dispose();
        await readTask1;

        sut.Participants( "foo" ).TestEquals( 2 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldReleaseLock_WhenDoneAfterUpgradeAndDowngrade()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var upgraded = await token.UpgradeAsync();
        upgraded.Dispose();

        token.Dispose();

        sut.Participants( "foo" ).TestEquals( 0 ).Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldThrowInvalidOperationException_WhenCalledOnUpgradingToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = await sut.EnterReadAsync( "foo" );
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        var upgradeTask = token.UpgradeAsync().AsTask();

        var action = Lambda.Of( () => token.Dispose() );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<InvalidOperationException>(),
                sut.Participants( "foo" ).TestEquals( 2 ),
                upgradeTask.IsCompleted.TestFalse() ) )
            .Go();
    }

    [Fact]
    public async Task UpgradeableReadLockDispose_ShouldThrowInvalidOperationException_WhenCalledOnUpgradedToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var token = await sut.EnterUpgradeableReadAsync( "foo" );
        _ = await token.UpgradeAsync();

        var action = Lambda.Of( () => token.Dispose() );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<InvalidOperationException>(),
                sut.Participants( "foo" ).TestEquals( 1 ) ) )
            .Go();
    }

    [Fact]
    public void UpgradedReadLockDispose_ShouldNotThrow_ForDefault()
    {
        var token = default( AsyncKeyedReaderWriterLockUpgradedReadToken<string> );
        var action = Lambda.Of( () => token.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldThrowInvalidOperationException_WhenCalledOnDisposedToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        var first = await source.UpgradeAsync();
        _ = Task.Run( async () =>
        {
            await Delay();
            first.Dispose();
        } );

        _ = await sut.EnterReadAsync( "foo" );
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All( exc.TestType().Exact<InvalidOperationException>(), sut.Participants( "foo" ).TestEquals( 2 ) ) )
            .Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenLast()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await source.UpgradeAsync();

        token.Dispose();

        Assertion.All( sut.ActiveKeys.TestSetEqual( [ "foo" ] ), sut.Participants( "foo" ).TestEquals( 1 ) ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenFollowedByPendingRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await source.UpgradeAsync();
        var readTask1 = sut.EnterReadAsync( "foo" );
        var readTask2 = sut.EnterReadAsync( "foo" );

        token.Dispose();
        await readTask1;
        await readTask2;

        sut.Participants( "foo" ).TestEquals( 3 ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenFollowedByPendingReadThenUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await source.UpgradeAsync();
        var readTask1 = sut.EnterReadAsync( "foo" );
        var readTask2 = sut.EnterUpgradeableReadAsync( "foo" );
        var readTask3 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 4 ), readTask2.IsCompleted.TestFalse(), readTask3.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenFollowedByPendingReadThenWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await source.UpgradeAsync();
        var readTask1 = sut.EnterReadAsync( "foo" );
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();
        var readTask2 = sut.EnterReadAsync( "foo" ).AsTask();

        token.Dispose();
        await readTask1;
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 4 ), writeTask.IsCompleted.TestFalse(), readTask2.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenFollowedByPendingWrite()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await source.UpgradeAsync();
        var writeTask = sut.EnterWriteAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), writeTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradedReadLockDispose_ShouldDowngradeLock_WhenFollowedByPendingUpgradeableRead()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await source.UpgradeAsync();
        var readTask = sut.EnterUpgradeableReadAsync( "foo" ).AsTask();

        token.Dispose();
        await Delay();

        Assertion.All( sut.Participants( "foo" ).TestEquals( 2 ), readTask.IsCompleted.TestFalse() ).Go();
    }

    [Fact]
    public async Task UpgradedReadToken_GetReadToken_ShouldReturnBaseToken()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        var source = await sut.EnterUpgradeableReadAsync( "foo" );
        var token = await source.UpgradeAsync();

        var result = token.GetReadToken();

        result.TestEquals( source ).Go();
    }

    [Fact]
    public void TrimExcess_ShouldNotImpactParticipants()
    {
        var sut = new AsyncKeyedReaderWriterLock<string>();
        _ = sut.EnterReadAsync( "foo" );
        _ = sut.EnterWriteAsync( "foo" );
        _ = sut.EnterReadAsync( "bar" );

        sut.TrimExcess();

        Assertion.All(
                sut.ActiveKeys.TestSetEqual( [ "foo", "bar" ] ),
                sut.Participants( "foo" ).TestEquals( 2 ),
                sut.Participants( "bar" ).TestEquals( 1 ) )
            .Go();
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
        AsyncKeyedReaderWriterLock<string> sut,
        Func<Task> action,
        int expectedParticipants,
        Task?[]? pendingTasks = null)
    {
        return action.Test( GetCancellationAssertion( sut, expectedParticipants, pendingTasks ) );
    }

    [Pure]
    private static CallAssertion TestCancellation<T>(
        AsyncKeyedReaderWriterLock<string> sut,
        Func<Task<T>> action,
        int expectedParticipants,
        Task?[]? pendingTasks = null)
    {
        return action.Test( GetCancellationAssertion( sut, expectedParticipants, pendingTasks ) );
    }

    private static Func<Exception?, Assertion> GetCancellationAssertion(
        AsyncKeyedReaderWriterLock<string> sut,
        int expectedParticipants,
        Task?[]? pendingTasks)
    {
        pendingTasks ??= [ ];
        return exc => Assertion.All(
            exc.TestType().AssignableTo<OperationCanceledException>(),
            sut.Participants( "foo" ).TestEquals( expectedParticipants ),
            pendingTasks.TestAll( (e, _) => e.TestNotNull( t => t.IsCompleted.TestFalse() ) ) );
    }

    [Pure]
    private static Task?[] PrepareNextTasks(int count)
    {
        return new Task?[count];
    }
}
