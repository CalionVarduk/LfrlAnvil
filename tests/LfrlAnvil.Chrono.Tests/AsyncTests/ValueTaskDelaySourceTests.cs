using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Chrono.Tests.AsyncTests;

public class ValueTaskDelaySourceTests : TestsBase
{
    [Fact]
    public async Task Schedule_ShouldReturnTaskWhichCompletesAccordingToScheduledDelay()
    {
        var timestamps = new TimestampProvider();
        await using var sut = ValueTaskDelaySource.Start( timestamps );
        var delay = Duration.FromMilliseconds( 15 );
        var minTimestamp = timestamps.GetNow() + delay;
        var task = sut.Schedule( delay );

        var result = await task;

        Assertion.All(
                task.Owner.TestRefEquals( sut ),
                timestamps.GetNow().TestGreaterThanOrEqualTo( minTimestamp ),
                result.TestEquals( ValueTaskDelayResult.Completed ) )
            .Go();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public async Task Schedule_ShouldReturnTaskWhichCompletesAccordingToScheduledDelay_WithConfigureAwait(bool continueOnCapturedContext)
    {
        var timestamps = new TimestampProvider();
        await using var sut = ValueTaskDelaySource.Start( timestamps );
        var delay = Duration.FromMilliseconds( 15 );
        var minTimestamp = timestamps.GetNow() + delay;
        var task = sut.Schedule( delay );

        var result = await task.ConfigureAwait( continueOnCapturedContext );

        Assertion.All(
                task.Owner.TestRefEquals( sut ),
                timestamps.GetNow().TestGreaterThanOrEqualTo( minTimestamp ),
                result.TestEquals( ValueTaskDelayResult.Completed ) )
            .Go();
    }

    [Fact]
    public async Task Schedule_ShouldReturnTaskWhichCompletesWithCancelledResult_WhenCancellationTokenIsImmediatelyCancelled()
    {
        await using var sut = ValueTaskDelaySource.Start( new TimestampProvider() );
        var result = await sut.Schedule( Duration.FromSeconds( 1 ), new CancellationToken( canceled: true ) );
        result.TestEquals( ValueTaskDelayResult.Cancelled ).Go();
    }

    [Fact]
    public async Task Schedule_ShouldReturnTaskWhichCompletesWithCancelledResult_WhenCancellationTokenIsCancelledBeforeDelayCompletion()
    {
        await using var sut = ValueTaskDelaySource.Start( new TimestampProvider() );
        var cancellationSource = new CancellationTokenSource();
        cancellationSource.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );

        var result = await sut.Schedule( Duration.FromSeconds( 1 ), cancellationSource.Token );

        result.TestEquals( ValueTaskDelayResult.Cancelled ).Go();
    }

    [Fact]
    public async Task
        Schedule_ShouldReturnTaskWhichCompletesAccordingToScheduledDelay_WhenCancellationTokenIsCancelledAfterDelayCompletion()
    {
        var completion = new TaskCompletionSource();
        await using var sut = ValueTaskDelaySource.Start( new TimestampProvider() );
        var cancellationSource = new CancellationTokenSource();
        cancellationSource.Token.Register( () => completion.SetResult() );
        cancellationSource.CancelAfter( TimeSpan.FromMilliseconds( 50 ) );

        var result = await sut.Schedule( Duration.FromMilliseconds( 15 ), cancellationSource.Token );
        await completion.Task;

        result.TestEquals( ValueTaskDelayResult.Completed ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public async Task Schedule_ShouldReturnTaskWhichCompletesImmediately_WhenDelayIsLessThanOrEqualToZero(int delayTicks)
    {
        await using var sut = ValueTaskDelaySource.Start( new TimestampProvider() );
        var result = await sut.Schedule( Duration.FromTicks( delayTicks ) );
        result.TestEquals( ValueTaskDelayResult.Completed ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public async Task
        Schedule_ShouldReturnTaskWhichCompletesWithCancelledResult_WhenDelayIsLessThanOrEqualToZeroAndCancellationTokenIsImmediatelyCancelled(
            int delayTicks)
    {
        await using var sut = ValueTaskDelaySource.Start( new TimestampProvider() );
        var result = await sut.Schedule( Duration.FromTicks( delayTicks ), new CancellationToken( canceled: true ) );
        result.TestEquals( ValueTaskDelayResult.Cancelled ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public async Task
        Schedule_ShouldReturnTaskWhichCompletesImmediately_WhenDelayIsLessThanOrEqualToZeroAndCancellationTokenIsCancelledAfterDelayCompletion(
            int delayTicks)
    {
        var completion = new TaskCompletionSource();
        await using var sut = ValueTaskDelaySource.Start( new TimestampProvider() );
        var cancellationSource = new CancellationTokenSource();
        cancellationSource.Token.Register( () => completion.SetResult() );
        cancellationSource.CancelAfter( TimeSpan.FromMilliseconds( 15 ) );

        var result = await sut.Schedule( Duration.FromTicks( delayTicks ), cancellationSource.Token );
        await completion.Task;

        result.TestEquals( ValueTaskDelayResult.Completed ).Go();
    }

    [Fact]
    public async Task Schedule_ShouldHandleMultipleDifferentDelaysCorrectly()
    {
        var completionOrder = new CompletionOrder();
        await using var sut = ValueTaskDelaySource.Start( new TimestampProvider() );

        var delay1 = sut.Schedule( Duration.FromMilliseconds( 90 ) );
        var delay2 = sut.Schedule( Duration.FromMilliseconds( 30 ) );
        var delay3 = sut.Schedule( Duration.FromMilliseconds( 60 ) );
        var delay4 = sut.Schedule( Duration.FromMilliseconds( 150 ) );
        var delay5 = sut.Schedule( Duration.FromMilliseconds( 120 ) );

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
    public async Task Schedule_ShouldHandleMultipleDifferentDelaysCorrectly_WithCancellations()
    {
        var completionOrder = new CompletionOrder();
        await using var sut = ValueTaskDelaySource.Start( new TimestampProvider() );
        var cancellationSource1 = new CancellationTokenSource();
        var cancellationSource2 = new CancellationTokenSource();
        cancellationSource1.CancelAfter( TimeSpan.FromMilliseconds( 30 ) );
        cancellationSource2.CancelAfter( TimeSpan.FromMilliseconds( 60 ) );

        var delay1 = sut.Schedule( Duration.FromMilliseconds( 90 ) );
        var delay2 = sut.Schedule( Duration.FromMilliseconds( 150 ), cancellationSource1.Token );
        var delay3 = sut.Schedule( Duration.FromMilliseconds( 120 ) );
        var delay4 = sut.Schedule( Duration.FromMilliseconds( 180 ) );
        var delay5 = sut.Schedule( Duration.FromMilliseconds( 210 ) );
        var delay6 = sut.Schedule( Duration.FromMilliseconds( 240 ) );
        var delay7 = sut.Schedule( Duration.FromMilliseconds( 270 ), cancellationSource2.Token );

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
                (3, ValueTaskDelayResult.Completed),
                (1, ValueTaskDelayResult.Cancelled),
                (4, ValueTaskDelayResult.Completed),
                (5, ValueTaskDelayResult.Completed),
                (6, ValueTaskDelayResult.Completed),
                (7, ValueTaskDelayResult.Completed),
                (2, ValueTaskDelayResult.Cancelled)
            ] )
            .Go();
    }

    [Fact]
    public async Task Schedule_ShouldReturnTaskWhichCompletesWithDisposedResult_WhenSourceIsDisposed()
    {
        var sut = ValueTaskDelaySource.Start( new TimestampProvider() );
        await sut.DisposeAsync();

        var result = await sut.Schedule( Duration.Zero );

        result.TestEquals( ValueTaskDelayResult.Disposed ).Go();
    }

    [Fact]
    public async Task ThrowingTimestampProvider_ShouldSilentlyDisposeSource()
    {
        var sut = ValueTaskDelaySource.Start( new ThrowingTimestampProvider() );
        await Task.Delay( 15 );

        var result = await sut.Schedule( Duration.Zero );

        result.TestEquals( ValueTaskDelayResult.Disposed ).Go();
    }

    [Fact]
    public async Task Dispose_ShouldCompleteAllPendingDelaysWithDisposesResult()
    {
        var sut = ValueTaskDelaySource.Start( new TimestampProvider() );

        var task1 = Task.Factory.StartNew( async () => await sut.Schedule( Duration.FromSeconds( 1 ) ) );
        var task2 = Task.Factory.StartNew( async () => await sut.Schedule( Duration.FromSeconds( 1 ) ) );
        var task3 = Task.Factory.StartNew( async () => await sut.Schedule( Duration.FromSeconds( 1 ) ) );

        await Task.Delay( 15 );
        await sut.DisposeAsync();
        var result = await Task.WhenAll( task1, task2, task3 );

        result.TestAll( (t, _) => t.Result.TestEquals( ValueTaskDelayResult.Disposed ) ).Go();
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenAlreadyDisposes()
    {
        var sut = ValueTaskDelaySource.Start( new TimestampProvider() );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
    }

    private sealed class ThrowingTimestampProvider : TimestampProviderBase
    {
        public override Timestamp GetNow()
        {
            throw new Exception( "foo" );
        }
    }

    private sealed class CompletionOrder
    {
        private int _current;

        public int Next()
        {
            return Interlocked.Increment( ref _current );
        }
    }
}
