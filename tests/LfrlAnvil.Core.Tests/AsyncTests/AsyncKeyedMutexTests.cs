using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class AsyncKeyedMutexTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateNonAcquiredMutex()
    {
        var keyComparer = StringComparer.InvariantCultureIgnoreCase;
        var sut = new AsyncKeyedMutex<string>( keyComparer );
        Assertion.All( sut.KeyComparer.TestRefEquals( keyComparer ), sut.ActiveKeys.TestEmpty() ).Go();
    }

    [Fact]
    public async Task EnterAsync_ShouldReturnImmediately_ForFirstKeyParticipant()
    {
        var sut = new AsyncKeyedMutex<string>();
        var token1 = await sut.EnterAsync( "foo" );
        var token2 = await sut.EnterAsync( "bar" );

        Assertion.All(
                token1.Mutex.TestRefEquals( sut ),
                token1.Key.TestEquals( "foo" ),
                token2.Mutex.TestRefEquals( sut ),
                token2.Key.TestEquals( "bar" ),
                sut.ActiveKeys.TestSetEqual( [ "foo", "bar" ] ),
                sut.Participants( "foo" ).TestEquals( 1 ),
                sut.Participants( "bar" ).TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void EnterAsync_ShouldThrowOperationCanceledException_WhenFirstParticipantCancellationTokenIsCancelled()
    {
        var sut = new AsyncKeyedMutex<string>();
        var action = Lambda.Of( async () => await sut.EnterAsync( "foo", new CancellationToken( canceled: true ) ) );

        action.Test( exc => Assertion.All(
                exc.TestType().AssignableTo<OperationCanceledException>(),
                sut.ActiveKeys.TestEmpty(),
                sut.Participants( "foo" ).TestEquals( 0 ) ) )
            .Go();
    }

    [Fact]
    public async Task EnterAsync_ShouldQueueUpParticipantCorrectly()
    {
        var sut = new AsyncKeyedMutex<string>();
        var token = await sut.EnterAsync( "foo" );

        _ = Task.Run( async () => _ = await sut.EnterAsync( "foo" ) );
        _ = Task.Run( async () => _ = await sut.EnterAsync( "foo" ) );
        await Task.Delay( 15 );

        Assertion.All(
                token.Mutex.TestRefEquals( sut ),
                token.Key.TestEquals( "foo" ),
                sut.ActiveKeys.TestSequence( [ "foo" ] ),
                sut.Participants( "foo" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public async Task EnterAsync_ShouldThrowOperationCanceledException_WhenParticipantCancels()
    {
        var source = new CancellationTokenSource();
        var sut = new AsyncKeyedMutex<string>();
        var token = await sut.EnterAsync( "foo" );

        var action = Lambda.Of( async () =>
        {
            source.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );
            var result = sut.EnterAsync( "foo", source.Token );
            _ = sut.EnterAsync( "foo" );
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
                token.Key.TestEquals( "foo" ),
                sut.ActiveKeys.TestSequence( [ "foo" ] ),
                sut.Participants( "foo" ).TestEquals( 2 ) ) )
            .Go();
    }

    [Fact]
    public void TryEnter_ShouldReturnEnteredToken_ForFirstParticipant()
    {
        var sut = new AsyncKeyedMutex<string>();

        var token = sut.TryEnter( "foo", out var entered );

        Assertion.All(
                token.Mutex.TestRefEquals( sut ),
                token.Key.TestEquals( "foo" ),
                sut.ActiveKeys.TestSequence( [ "foo" ] ),
                sut.Participants( "foo" ).TestEquals( 1 ),
                entered.TestTrue() )
            .Go();
    }

    [Fact]
    public void TryEnter_ShouldReturnNotEnteredToken_ForNotFirstParticipant()
    {
        var sut = new AsyncKeyedMutex<string>();
        _ = sut.TryEnter( "foo", out _ );

        var token = sut.TryEnter( "foo", out var entered );

        Assertion.All(
                token.Mutex.TestNull(),
                token.Key.TestNull(),
                sut.ActiveKeys.TestSequence( [ "foo" ] ),
                sut.Participants( "foo" ).TestEquals( 1 ),
                entered.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task LockDispose_ShouldReleaseLock_WithoutWaiters()
    {
        var sut = new AsyncKeyedMutex<string>();
        var token = await sut.EnterAsync( "foo" );

        token.Dispose();

        Assertion.All( sut.ActiveKeys.TestEmpty(), sut.Participants( "foo" ).TestEquals( 0 ) ).Go();
    }

    [Fact]
    public async Task LockDispose_ShouldNotifyNextWaiter()
    {
        var sut = new AsyncKeyedMutex<string>();
        var first = await sut.EnterAsync( "foo" );
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            first.Dispose();
        } );

        var token = await sut.EnterAsync( "foo" );
        _ = sut.EnterAsync( "foo" );

        Assertion.All(
                token.Mutex.TestRefEquals( sut ),
                token.Key.TestEquals( "foo" ),
                sut.ActiveKeys.TestSequence( [ "foo" ] ),
                sut.Participants( "foo" ).TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void LockDispose_ShouldNotThrow_ForDefault()
    {
        var token = default( AsyncKeyedMutexToken<string> );
        var action = Lambda.Of( () => token.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task LockDispose_ShouldDoNothing_WhenCalledOnDisposedToken()
    {
        var sut = new AsyncKeyedMutex<string>();
        var first = await sut.EnterAsync( "foo" );
        _ = Task.Run( async () =>
        {
            await Task.Delay( 15 );
            first.Dispose();
        } );

        _ = await sut.EnterAsync( "foo" );
        var action = Lambda.Of( () => first.Dispose() );

        action.Test( exc => Assertion.All(
                exc.TestNull(),
                sut.ActiveKeys.TestSequence( [ "foo" ] ),
                sut.Participants( "foo" ).TestEquals( 1 ) ) )
            .Go();
    }

    [Fact]
    public void TrimExcess_ShouldNotImpactParticipants()
    {
        var sut = new AsyncKeyedMutex<string>();
        _ = sut.EnterAsync( "foo" );
        _ = sut.EnterAsync( "foo" );
        _ = sut.EnterAsync( "bar" );

        sut.TrimExcess();

        Assertion.All(
                sut.ActiveKeys.TestSetEqual( [ "foo", "bar" ] ),
                sut.Participants( "foo" ).TestEquals( 2 ),
                sut.Participants( "bar" ).TestEquals( 1 ) )
            .Go();
    }
}
