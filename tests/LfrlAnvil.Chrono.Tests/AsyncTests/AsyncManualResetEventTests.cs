using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Chrono.Tests.AsyncTests;

public class AsyncManualResetEventTests : TestsBase
{
    [Fact]
    public void SourceGetResetEvent_ShouldReturnNewEvent()
    {
        using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();
        sut.Owner.TestRefEquals( source ).Go();
    }

    [Fact]
    public async Task SourceGetResetEvent_ShouldReturnDisposedEvent_WhenSourceIsDisposed()
    {
        var source = ValueTaskDelaySource.Start();
        await source.DisposeAsync();
        var sut = source.GetResetEvent();

        var result = await sut.WaitAsync();

        result.TestEquals( AsyncManualResetEventResult.Disposed ).Go();
    }

    [Fact]
    public async Task WaitAsync_ShouldReturnTaskWhichCompletesAccordingToScheduledEvent()
    {
        var timestamps = new TimestampProvider();
        await using var source = ValueTaskDelaySource.Start( timestamps );
        var sut = source.GetResetEvent();
        var delay = Duration.FromMilliseconds( 15 );
        var minTimestamp = timestamps.GetNow() + delay;

        var result = await sut.WaitAsync( delay );

        Assertion.All(
                timestamps.GetNow().TestGreaterThanOrEqualTo( minTimestamp ),
                result.TestEquals( AsyncManualResetEventResult.TimedOut ) )
            .Go();
    }

    [Fact]
    public async Task WaitAsync_ShouldReturnAlreadyAwaited_WhenEventIsScheduled()
    {
        await using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();
        var task = sut.WaitAsync( Duration.FromMilliseconds( 15 ) );

        var result = await sut.WaitAsync( Duration.Zero );
        await task;

        result.TestEquals( AsyncManualResetEventResult.AlreadyAwaited ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public async Task WaitAsync_ShouldReturnImmediately_WhenDelayIsLessThanOrEqualToZero(int delayTicks)
    {
        await using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();

        var result = await sut.WaitAsync( Duration.FromTicks( delayTicks ) );

        result.TestEquals( AsyncManualResetEventResult.TimedOut ).Go();
    }

    [Fact]
    public async Task WaitAsync_ShouldWaitUntilSignaled_WhenNoDelayIsProvided()
    {
        await using var source = ValueTaskDelaySource.Start();
        var sut = Ref.Create( source.GetResetEvent() );
        var cancellationSource = new CancellationTokenSource();
        cancellationSource.Token.Register( () => sut.Value.Set() );
        cancellationSource.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );

        var result = await sut.Value.WaitAsync();

        result.TestEquals( AsyncManualResetEventResult.Signaled ).Go();
    }

    [Fact]
    public async Task WaitAsync_ForManyEvents_ShouldHandleMultipleDifferentDelaysCorrectly()
    {
        var completionOrder = new CompletionOrder();
        await using var sut = ValueTaskDelaySource.Start();

        var delay1 = sut.GetResetEvent().WaitAsync( Duration.FromMilliseconds( 90 ) );
        var delay2 = sut.GetResetEvent().WaitAsync( Duration.FromMilliseconds( 30 ) );
        var delay3 = sut.GetResetEvent().WaitAsync( Duration.FromMilliseconds( 60 ) );
        var delay4 = sut.GetResetEvent().WaitAsync( Duration.FromMilliseconds( 150 ) );
        var delay5 = sut.GetResetEvent().WaitAsync( Duration.FromMilliseconds( 120 ) );

        var tasks = new[] { delay1, delay2, delay3, delay4, delay5 }.Select(
                d =>
                    Task.Factory.StartNew(
                        async () =>
                        {
                            await d;
                            return completionOrder.Next();
                        } ) )
            .ToArray();

        var result = await Task.WhenAll( tasks );

        result.Select( t => t.Result ).TestSequence( [ 3, 1, 2, 5, 4 ] ).Go();
    }

    [Fact]
    public async Task WaitAsync_ForManyEvents_ShouldHandleMultipleDifferentDelaysCorrectly_WithSignaling()
    {
        var completionOrder = new CompletionOrder();
        await using var sut = ValueTaskDelaySource.Start();
        var signaled1 = Ref.Create( sut.GetResetEvent() );
        var signaled2 = Ref.Create( sut.GetResetEvent() );
        var cancellationSource1 = new CancellationTokenSource();
        var cancellationSource2 = new CancellationTokenSource();
        cancellationSource1.Token.Register( () => signaled1.Value.Set() );
        cancellationSource2.Token.Register( () => signaled2.Value.Set() );
        cancellationSource1.CancelAfter( TimeSpan.FromMilliseconds( 30 ) );
        cancellationSource2.CancelAfter( TimeSpan.FromMilliseconds( 60 ) );

        var delay1 = sut.GetResetEvent().WaitAsync( Duration.FromMilliseconds( 90 ) );
        var delay2 = signaled1.Value.WaitAsync( Duration.FromMilliseconds( 150 ) );
        var delay3 = sut.GetResetEvent().WaitAsync( Duration.FromMilliseconds( 120 ) );
        var delay4 = sut.GetResetEvent().WaitAsync( Duration.FromMilliseconds( 180 ) );
        var delay5 = sut.GetResetEvent().WaitAsync( Duration.FromMilliseconds( 210 ) );
        var delay6 = sut.GetResetEvent().WaitAsync( Duration.FromMilliseconds( 240 ) );
        var delay7 = signaled2.Value.WaitAsync( Duration.FromMilliseconds( 270 ) );

        var tasks = new[] { delay1, delay2, delay3, delay4, delay5, delay6, delay7 }.Select(
                d =>
                    Task.Factory.StartNew(
                        async () =>
                        {
                            var result = await d;
                            return (completionOrder.Next(), result);
                        } ) )
            .ToArray();

        var result = await Task.WhenAll( tasks );

        result.Select( t => t.Result )
            .TestSequence(
            [
                (3, AsyncManualResetEventResult.TimedOut),
                (1, AsyncManualResetEventResult.Signaled),
                (4, AsyncManualResetEventResult.TimedOut),
                (5, AsyncManualResetEventResult.TimedOut),
                (6, AsyncManualResetEventResult.TimedOut),
                (7, AsyncManualResetEventResult.TimedOut),
                (2, AsyncManualResetEventResult.Signaled)
            ] )
            .Go();
    }

    [Fact]
    public async Task Set_ShouldReturnTrueAndImmediatelyFinishScheduledEvent()
    {
        var timestamps = new TimestampProvider();
        await using var source = ValueTaskDelaySource.Start( timestamps );
        var sut = source.GetResetEvent();
        var delay = Duration.FromSeconds( 1 );
        var task = sut.WaitAsync( delay );
        var endTimestamp = timestamps.GetNow() + delay;

        var result = sut.Set();
        var taskResult = await task;

        Assertion.All(
                result.TestTrue(),
                taskResult.TestEquals( AsyncManualResetEventResult.Signaled ),
                timestamps.GetNow().TestLessThan( endTimestamp ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( null )]
    public async Task Set_ShouldReturnTrueAndCauseNextWaitToFinishImmediately(int? delayInSeconds)
    {
        await using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();

        var result = sut.Set();
        var taskResult = await sut.WaitAsync( delayInSeconds is null ? null : Duration.FromSeconds( delayInSeconds.Value ) );

        Assertion.All( result.TestTrue(), taskResult.TestEquals( AsyncManualResetEventResult.Signaled ) ).Go();
    }

    [Fact]
    public void Set_ShouldReturnFalse_WhenEventIsSignaled()
    {
        using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent( signaled: true );

        var result = sut.Set();

        result.TestFalse().Go();
    }

    [Fact]
    public void Set_ShouldReturnFalse_WhenEventIsDisposed()
    {
        using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();
        sut.Dispose();

        var result = sut.Set();

        result.TestFalse().Go();
    }

    [Fact]
    public async Task Reset_ShouldReturnTrueAndCauseNextWaitToNotFinishImmediately_WhenEventIsSignaled()
    {
        var timestamps = new TimestampProvider();
        await using var source = ValueTaskDelaySource.Start( timestamps );
        var sut = source.GetResetEvent( signaled: true );
        var delay = Duration.FromMilliseconds( 15 );

        var result = sut.Reset();
        var minTimestamp = timestamps.GetNow() + delay;
        var taskResult = await sut.WaitAsync( delay );

        Assertion.All(
                result.TestTrue(),
                taskResult.TestEquals( AsyncManualResetEventResult.TimedOut ),
                timestamps.GetNow().TestGreaterThanOrEqualTo( minTimestamp ) )
            .Go();
    }

    [Fact]
    public void Reset_ShouldReturnFalse_WhenEventIsNotSignaled()
    {
        using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();

        var result = sut.Reset();

        result.TestFalse().Go();
    }

    [Fact]
    public async Task Reset_ShouldReturnFalse_WhenEventIsScheduled()
    {
        var timestamps = new TimestampProvider();
        await using var source = ValueTaskDelaySource.Start( timestamps );
        var sut = source.GetResetEvent();
        var delay = Duration.FromMilliseconds( 15 );
        var minTimestamp = timestamps.GetNow() + delay;
        var task = sut.WaitAsync( delay );

        var result = sut.Reset();
        var taskResult = await task;

        Assertion.All(
                result.TestFalse(),
                taskResult.TestEquals( AsyncManualResetEventResult.TimedOut ),
                timestamps.GetNow().TestGreaterThanOrEqualTo( minTimestamp ) )
            .Go();
    }

    [Fact]
    public void Reset_ShouldReturnFalse_WhenEventIsDisposed()
    {
        using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();
        sut.Dispose();

        var result = sut.Reset();

        result.TestFalse().Go();
    }

    [Fact]
    public async Task Dispose_ShouldCauseNextWaitToReturnDisposed_WhenEventIsNotScheduled()
    {
        await using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();
        sut.Dispose();

        var result = await sut.WaitAsync( Duration.Zero );

        result.TestEquals( AsyncManualResetEventResult.Disposed ).Go();
    }

    [Fact]
    public async Task Dispose_ShouldImmediatelyDisposeScheduledEvent()
    {
        var timestamps = new TimestampProvider();
        await using var source = ValueTaskDelaySource.Start( timestamps );
        var sut = source.GetResetEvent();
        var delay = Duration.FromSeconds( 1 );
        var task = sut.WaitAsync( delay );
        var endTimestamp = timestamps.GetNow() + delay;

        sut.Dispose();
        var result = await task;

        Assertion.All( result.TestEquals( AsyncManualResetEventResult.Disposed ), timestamps.GetNow().TestLessThan( endTimestamp ) ).Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenEventIsAlreadyDisposed()
    {
        using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenEventIsAlreadyDisposedAndUnderlyingNodeIsReused()
    {
        using var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();
        sut.Dispose();
        var other = source.GetResetEvent();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => Assertion.All( exc.TestNull(), other.Set().TestTrue() ) ).Go();
    }

    [Fact]
    public async Task SourceDispose_ShouldLockOutNonScheduledEventFromPerformingAnyAction()
    {
        var source = ValueTaskDelaySource.Start();
        var sut = source.GetResetEvent();
        await source.DisposeAsync();

        var setResult = sut.Set();
        var resetResult = sut.Reset();
        var waitResult = await sut.WaitAsync( Duration.Zero );

        Assertion.All( setResult.TestFalse(), resetResult.TestFalse(), waitResult.TestEquals( AsyncManualResetEventResult.Disposed ) ).Go();
    }

    [Fact]
    public async Task SourceDispose_ShouldCompleteAllPendingEventsWithDisposedResult()
    {
        var source = ValueTaskDelaySource.Start();

        var task1 = Task.Factory.StartNew( async () => await source.GetResetEvent().WaitAsync( Duration.FromSeconds( 1 ) ) );
        var task2 = Task.Factory.StartNew( async () => await source.GetResetEvent().WaitAsync( Duration.FromSeconds( 1 ) ) );
        var task3 = Task.Factory.StartNew( async () => await source.GetResetEvent().WaitAsync( Duration.FromSeconds( 1 ) ) );

        await Task.Delay( 15 );
        await source.DisposeAsync();
        var result = await Task.WhenAll( task1, task2, task3 );

        result.TestAll( (t, _) => t.Result.TestEquals( AsyncManualResetEventResult.Disposed ) ).Go();
    }
}
