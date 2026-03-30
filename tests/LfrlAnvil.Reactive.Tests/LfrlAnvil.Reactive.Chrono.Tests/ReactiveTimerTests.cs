using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Reactive.Chrono.Tests;

public class ReactiveTimerTests : TestsBase
{
    [Fact]
    public void Ctor_WithInterval_ShouldCreateCorrectTimer()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( interval );

        Assertion.All(
                sut.Interval.TestEquals( interval ),
                sut.Count.TestEquals( long.MaxValue ),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestFalse(),
                sut.Subscribers.TestEmpty(),
                sut.HasSubscribers.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_WithInterval_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanOneTick(long ticks)
    {
        var interval = Duration.FromTicks( ticks );
        var action = Lambda.Of( () => new ReactiveTimer( interval ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void Ctor_WithInterval_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsTooLarge(long ms)
    {
        var interval = Duration.FromMilliseconds( ms );
        var action = Lambda.Of( () => new ReactiveTimer( interval ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithIntervalAndCount_ShouldCreateCorrectTimer()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var count = Fixture.Create<int>( x => x > 0 );
        var sut = new ReactiveTimer( interval, count: count );

        Assertion.All(
                sut.Interval.TestEquals( interval ),
                sut.Count.TestEquals( count ),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestFalse(),
                sut.Subscribers.TestEmpty(),
                sut.HasSubscribers.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_WithIntervalAndCount_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(long count)
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var action = Lambda.Of( () => new ReactiveTimer( interval, count: count ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithIntervalAndSpinWaitDurationHint_ShouldCreateCorrectTimer()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var sut = new ReactiveTimer( interval, spinWaitDurationHint );

        Assertion.All(
                sut.Interval.TestEquals( interval ),
                sut.Count.TestEquals( long.MaxValue ),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestFalse(),
                sut.Subscribers.TestEmpty(),
                sut.HasSubscribers.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_WithIntervalAndSpinWaitDurationHint_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanOneTick(long ticks)
    {
        var interval = Duration.FromTicks( ticks );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );

        var action = Lambda.Of( () => new ReactiveTimer( interval, spinWaitDurationHint ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void Ctor_WithIntervalAndSpinWaitDurationHint_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsTooLarge(long ms)
    {
        var interval = Duration.FromMilliseconds( ms );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );

        var action = Lambda.Of( () => new ReactiveTimer( interval, spinWaitDurationHint ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithIntervalAndSpinWaitDurationHint_ShouldThrowArgumentOutOfRangeException_WhenSpinWaitDurationHintIsLessThanZero()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( -1 );

        var action = Lambda.Of( () => new ReactiveTimer( interval, spinWaitDurationHint ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithAllProperties_ShouldCreateCorrectTimer()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.Create<int>( x => x > 0 );
        var sut = new ReactiveTimer( interval, spinWaitDurationHint, count: count );

        Assertion.All(
                sut.Interval.TestEquals( interval ),
                sut.Count.TestEquals( count ),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestFalse(),
                sut.Subscribers.TestEmpty(),
                sut.HasSubscribers.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldStartTimer()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };

        var expectedEvent = new WithInterval<long>( 0, timestamps[1], interval );
        var actualEvents = new List<WithInterval<long>>();

        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( interval, timestampProvider, count: 1 );
        sut.Listen( listener );

        var result = sut.Start();
        await completion.Task;

        Assertion.All(
                result.TestTrue(),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public void Start_ReturnFalse_WhenTimerIsAlreadyRunning()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var sut = new ReactiveTimer( interval, timestampProvider, count: 1 );

        sut.Start();
        var result = sut.Start();

        result.TestFalse().Go();
    }

    [Fact]
    public void Start_ShouldThrowObjectDisposedException_WhenTimerIsDisposed()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( interval );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Start() );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public async Task Start_WithDelay_ShouldRunTimerOnTheCurrentThreadAndDelayTheFirstEvent()
    {
        var interval = Duration.FromTicks( 1 );
        var delay = Duration.FromTicks( 5 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + delay };

        var expectedEvent = new WithInterval<long>( 0, timestamps[1], delay );
        var actualEvents = new List<WithInterval<long>>();

        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( interval, timestampProvider, count: 1 );
        sut.Listen( listener );

        var result = sut.Start( delay );
        await completion.Task;

        Assertion.All(
                result.TestTrue(),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public void Start_WithDelay_ShouldReturnFalse_WhenTimerIsAlreadyRunning()
    {
        var interval = Duration.FromMilliseconds( 50 );
        var delay = Duration.FromTicks( 5 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var sut = new ReactiveTimer( interval, timestampProvider, count: 1 );

        sut.Start();
        var result = sut.Start( delay );

        result.TestFalse().Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Start_WithDelay_ShouldThrowArgumentOutOfRangeException_WhenDelayIsLessThanOne(long ticks)
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromTicks( ticks );
        var sut = new ReactiveTimer( interval );

        var action = Lambda.Of( () => sut.Start( delay ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void Start_WithDelay_ShouldThrowArgumentOutOfRangeException_WhenDelayIsTooLarge(long ms)
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromMilliseconds( ms );
        var sut = new ReactiveTimer( interval );

        var action = Lambda.Of( () => sut.Start( delay ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Start_WithDelay_ShouldReturnFalse_WhenTimerIsDisposed()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( interval );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Start( delay ) );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public async Task RunningTimer_ShouldPublishMultipleEventsInCorrectOrderAndDispose_WhenCountIsReached()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[]
        {
            Timestamp.Zero,
            Timestamp.Zero + interval,
            Timestamp.Zero + interval,
            Timestamp.Zero + interval * 2,
            Timestamp.Zero + interval * 2,
            Timestamp.Zero + interval * 3
        };

        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( interval, timestampProvider, count: eventTimestamps.Count );
        sut.Listen( listener );

        sut.Start();
        await completion.Task;

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public async Task RunningTimer_ShouldSkipFirstEvents_WhenAwaitingForFirstEventPreparationTakesTooLong()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[]
        {
            Timestamp.Zero, Timestamp.Zero + interval * 3, Timestamp.Zero + interval * 3, Timestamp.Zero + interval * 4
        };

        var expectedEvents = new[]
        {
            new WithInterval<long>( 2, Timestamp.Zero + interval * 3, interval * 3 ),
            new WithInterval<long>( 3, Timestamp.Zero + interval * 4, interval )
        };

        var actualEvents = new List<WithInterval<long>>();

        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( interval, timestampProvider, count: 4 );
        sut.Listen( listener );

        sut.Start();
        await completion.Task;

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public async Task RunningTimer_ShouldSkipLastEvents_WhenAwaitingForLastEventPreparationTakesTooLong()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval, Timestamp.Zero + interval, Timestamp.Zero + interval * 10 };

        var expectedEvents = new[]
        {
            new WithInterval<long>( 0, Timestamp.Zero + interval, interval ),
            new WithInterval<long>( 3, Timestamp.Zero + interval * 10, interval * 9 )
        };

        var actualEvents = new List<WithInterval<long>>();

        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( interval, timestampProvider, count: 4 );
        sut.Listen( listener );

        sut.Start();
        await completion.Task;

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public async Task RunningTimer_ShouldSkipIntermediateEvents_WhenAwaitingForTheirPreparationTakesTooLong()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[]
        {
            Timestamp.Zero,
            Timestamp.Zero + interval,
            Timestamp.Zero + interval,
            Timestamp.Zero + interval * 2,
            Timestamp.Zero + interval * 2,
            Timestamp.Zero + interval * 5,
            Timestamp.Zero + interval * 5,
            Timestamp.Zero + interval * 6
        };

        var expectedEvents = new[]
        {
            new WithInterval<long>( 0, Timestamp.Zero + interval, interval ),
            new WithInterval<long>( 1, Timestamp.Zero + interval * 2, interval ),
            new WithInterval<long>( 4, Timestamp.Zero + interval * 5, interval * 3 ),
            new WithInterval<long>( 5, Timestamp.Zero + interval * 6, interval )
        };

        var actualEvents = new List<WithInterval<long>>();

        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( interval, timestampProvider, count: 6 );
        sut.Listen( listener );

        sut.Start();
        await completion.Task;

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public async Task RunningTimer_ShouldSkipIntermediateEvents_WhenListenerReactionsTakeTooLong()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[]
        {
            Timestamp.Zero,
            Timestamp.Zero + interval,
            Timestamp.Zero + interval,
            Timestamp.Zero + interval * 2,
            Timestamp.Zero + interval * 4,
            Timestamp.Zero + interval * 5,
            Timestamp.Zero + interval * 6,
            Timestamp.Zero + interval * 7
        };

        var expectedEvents = new[]
        {
            new WithInterval<long>( 0, Timestamp.Zero + interval, interval ),
            new WithInterval<long>( 1, Timestamp.Zero + interval * 2, interval ),
            new WithInterval<long>( 4, Timestamp.Zero + interval * 5, interval * 3 ),
            new WithInterval<long>( 5, Timestamp.Zero + interval * 7, interval * 2 )
        };

        var actualEvents = new List<WithInterval<long>>();

        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( interval, timestampProvider, count: 6 );
        sut.Listen( listener );

        sut.Start();
        await completion.Task;

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public async Task RunningTimer_ShouldAttemptToSelfAlignToExpectedEventTimestamps()
    {
        var interval = Duration.FromTicks( 100 );
        var timestamps = new[]
        {
            Timestamp.Zero,
            Timestamp.Zero + interval * 1.1,
            Timestamp.Zero + interval * 1.2,
            Timestamp.Zero + interval * 2.0,
            Timestamp.Zero + interval * 2.5,
            Timestamp.Zero + interval * 3.4,
            Timestamp.Zero + interval * 3.9,
            Timestamp.Zero + interval * 4.1,
            Timestamp.Zero + interval * 4.2,
            Timestamp.Zero + interval * 5.3,
            Timestamp.Zero + interval * 5.7
        };

        var expectedEvents = new[]
        {
            new WithInterval<long>( 0, Timestamp.Zero + interval * 1.1, interval * 1.1 ),
            new WithInterval<long>( 1, Timestamp.Zero + interval * 2.0, interval * 0.9 ),
            new WithInterval<long>( 2, Timestamp.Zero + interval * 3.4, interval * 1.4 ),
            new WithInterval<long>( 3, Timestamp.Zero + interval * 4.1, interval * 0.7 ),
            new WithInterval<long>( 4, Timestamp.Zero + interval * 5.3, interval * 1.2 )
        };

        var actualEvents = new List<WithInterval<long>>();

        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( interval, timestampProvider, count: 5 );
        sut.Listen( listener );

        sut.Start();
        await completion.Task;

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public async Task RunningTimer_ShouldSpinWait_WhenNextEventTimestampIsLessThanTheMinExpectedTimestamp()
    {
        var interval = Duration.FromTicks( 100 );
        var timestamps = new[]
        {
            Timestamp.Zero, Timestamp.Zero + interval * 0.8, Timestamp.Zero + interval * 0.9, Timestamp.Zero + interval
        };

        var expectedEvent = new WithInterval<long>( 0, Timestamp.Zero + interval, interval );
        var actualEvents = new List<WithInterval<long>>();

        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( interval, timestampProvider, count: 1 );
        sut.Listen( listener );

        sut.Start();
        await completion.Task;

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public async Task RestartingStoppedTimer_ShouldContinueEventIndexesFromBefore()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[]
        {
            Timestamp.Zero,
            Timestamp.Zero + interval,
            Timestamp.Zero + interval,
            Timestamp.Zero + interval * 2,
            Timestamp.Zero + interval * 2,
            Timestamp.Zero + interval * 3,
            Timestamp.Zero + interval * 5,
            Timestamp.Zero + interval * 6,
            Timestamp.Zero + interval * 6,
            Timestamp.Zero + interval * 7
        };

        var expectedEvents = new[]
        {
            new WithInterval<long>( 0, Timestamp.Zero + interval, interval ),
            new WithInterval<long>( 1, Timestamp.Zero + interval * 2, interval ),
            new WithInterval<long>( 2, Timestamp.Zero + interval * 6, interval ),
            new WithInterval<long>( 3, Timestamp.Zero + interval * 7, interval )
        };

        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( interval, Duration.Zero, timestampProvider, count: 4 );

        var continuation = new SafeTaskCompletionSource();
        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>(
            e =>
            {
                actualEvents.Add( e );
                if ( e.Event == 1 )
                {
                    sut.Stop();
                    continuation.Complete();
                }
            },
            _ => completion.Complete() );

        sut.Listen( listener );

        sut.Start();

        await continuation.Task;
        var spinWait = new SpinWait();
        while ( sut.State != ReactiveTimerState.Idle )
            spinWait.SpinOnce();

        var result = sut.Start();
        await completion.Task;

        Assertion.All(
                result.TestTrue(),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldMarkTimerAsDisposed()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( interval );

        sut.Dispose();

        sut.IsDisposed.TestTrue().Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenTimerIsAlreadyDisposed()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( interval );
        sut.Dispose();

        sut.Dispose();

        sut.IsDisposed.TestTrue().Go();
    }

    [Fact]
    public async Task Dispose_ShouldStopRunningTimer()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var completion = new SafeTaskCompletionSource();
        var listener = Substitute.For<IEventListener<WithInterval<long>>>();
        listener.When( l => l.OnDispose( Arg.Any<DisposalSource>() ) ).Do( _ => completion.Complete() );

        var sut = new ReactiveTimer( interval );
        sut.Listen( listener );
        sut.Start();

        sut.Dispose();
        await completion.Task;

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<WithInterval<long>>() ) ) )
            .Go();
    }

    [Fact]
    public async Task Stop_ShouldStopTheTimer_WhenTimerHasAlreadyStartedWaitingForNextStep()
    {
        var interval = Duration.FromMilliseconds( 100 );
        var listener = Substitute.For<IEventListener<WithInterval<long>>>();
        var sut = new ReactiveTimer( interval );
        sut.Listen( listener );
        sut.Start();

        await Task.Delay( 15 );
        var result = sut.Stop();
        var state = sut.State;

        var spinWait = new SpinWait();
        while ( sut.State != ReactiveTimerState.Idle )
            spinWait.SpinOnce();

        Assertion.All(
                result.TestTrue(),
                state.TestNotEquals( ReactiveTimerState.Running ),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestFalse(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<WithInterval<long>>() ) ) )
            .Go();
    }

    [Fact]
    public async Task Stop_ShouldStopTheTimer_WhenTimerIsPreparingToWaitForTheNextStep()
    {
        var interval = Duration.FromTicks( 100 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval, Timestamp.Zero + interval };

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var expectedEvent = new WithInterval<long>( 0, Timestamp.Zero + interval, interval );
        var actualEvents = new List<WithInterval<long>>();

        var sut = new ReactiveTimer( interval, timestampProvider );

        var completion = new SafeTaskCompletionSource();
        var listener = EventListener.Create<WithInterval<long>>( e =>
        {
            actualEvents.Add( e );
            sut.Stop();
            completion.Complete();
        } );

        sut.Listen( listener );
        sut.Start();
        await completion.Task;

        Assertion.All(
                sut.State.TestNotEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestFalse(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public void Stop_ShouldDoNothing_WhenTimerIsNotRunning()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( interval );

        var result = sut.Stop();

        Assertion.All(
                result.TestFalse(),
                sut.State.TestEquals( ReactiveTimerState.Idle ) )
            .Go();
    }

    [Fact]
    public void Stop_ShouldReturnFalse_WhenTimerIsDisposed()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( interval );
        sut.Dispose();

        var result = sut.Stop();

        Assertion.All(
                result.TestFalse(),
                sut.State.TestEquals( ReactiveTimerState.Idle ) )
            .Go();
    }
}
