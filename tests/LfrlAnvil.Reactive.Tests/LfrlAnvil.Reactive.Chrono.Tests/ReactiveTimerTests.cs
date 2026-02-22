using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Reactive.Chrono.Tests;

public class ReactiveTimerTests : TestsBase
{
    [Fact]
    public void Ctor_WithTimestampProviderAndInterval_ShouldCreateCorrectTimer()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var sut = new ReactiveTimer( timestampProvider, interval );

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
    public void Ctor_WithTimestampProviderAndInterval_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanOneTick(long ticks)
    {
        var interval = Duration.FromTicks( ticks );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void Ctor_WithTimestampProviderAndInterval_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsTooLarge(long ms)
    {
        var interval = Duration.FromMilliseconds( ms );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithTimestampProviderAndIntervalAndCount_ShouldCreateCorrectTimer()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var count = Fixture.Create<int>( x => x > 0 );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var sut = new ReactiveTimer( timestampProvider, interval, count );

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
    public void Ctor_WithTimestampProviderAndIntervalAndCount_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanOneTick(
        long ticks)
    {
        var interval = Duration.FromTicks( ticks );
        var count = Fixture.Create<int>( x => x > 0 );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void Ctor_WithTimestampProviderAndIntervalAndCount_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsTooLarge(long ms)
    {
        var interval = Duration.FromMilliseconds( ms );
        var count = Fixture.Create<int>( x => x > 0 );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_WithTimestampProviderAndIntervalAndCount_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(long count)
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithTimestampProviderAndIntervalAndSpinWaitDurationHint_ShouldCreateCorrectTimer()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var sut = new ReactiveTimer( timestampProvider, interval, spinWaitDurationHint );

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
    public void
        Ctor_WithTimestampProviderAndIntervalAndSpinWaitDurationHint_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanOneTick(
            long ticks)
    {
        var interval = Duration.FromTicks( ticks );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval, spinWaitDurationHint ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void
        Ctor_WithTimestampProviderAndIntervalAndSpinWaitDurationHint_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsTooLarge(
            long ms)
    {
        var interval = Duration.FromMilliseconds( ms );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval, spinWaitDurationHint ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void
        Ctor_WithTimestampProviderAndIntervalAndSpinWaitDurationHint_ShouldThrowArgumentOutOfRangeException_WhenSpinWaitDurationHintIsLessThanZero()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( -1 );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval, spinWaitDurationHint ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithAllProperties_ShouldCreateCorrectTimer()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.Create<int>( x => x > 0 );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var sut = new ReactiveTimer( timestampProvider, interval, spinWaitDurationHint, count );

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
    public void Ctor_WithAllProperties_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanOneTick(long ticks)
    {
        var interval = Duration.FromTicks( ticks );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.Create<int>( x => x > 0 );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval, spinWaitDurationHint, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void Ctor_WithAllProperties_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsTooLarge(long ms)
    {
        var interval = Duration.FromMilliseconds( ms );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.Create<int>( x => x > 0 );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval, spinWaitDurationHint, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithAllProperties_ShouldThrowArgumentOutOfRangeException_WhenSpinWaitDurationHintIsLessThanZero()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( -1 );
        var count = Fixture.Create<int>( x => x > 0 );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval, spinWaitDurationHint, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_WithAllProperties_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(long count)
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new ReactiveTimer( timestampProvider, interval, spinWaitDurationHint, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Run_ShouldRunTimerOnTheCurrentThread()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };

        var expectedEvent = new WithInterval<long>( 0, timestamps[1], interval );
        var actualEvents = new List<WithInterval<long>>();

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );
        sut.Listen( listener );

        sut.Run();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public async Task Run_ThrowInvalidOperationException_WhenTimerIsAlreadyRunning()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );

        var task = Task.Factory.StartNew( () => sut.Run() );
        while ( sut.State != ReactiveTimerState.Running )
            ;

        var action = Lambda.Of( () => sut.Run() );
        var assertion = action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Invoke();
        await task;

        assertion.Go();
    }

    [Fact]
    public void Run_ShouldThrowObjectDisposedException_WhenTimerIsDisposed()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Run() );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void Run_WithDelay_ShouldRunTimerOnTheCurrentThreadAndDelayTheFirstEvent()
    {
        var interval = Duration.FromTicks( 1 );
        var delay = Duration.FromTicks( 5 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + delay };

        var expectedEvent = new WithInterval<long>( 0, timestamps[1], delay );
        var actualEvents = new List<WithInterval<long>>();

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );
        sut.Listen( listener );

        sut.Run( delay );

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public async Task Run_WithDelay_ShouldReturnFalse_WhenTimerIsAlreadyRunning()
    {
        var interval = Duration.FromMilliseconds( 50 );
        var delay = Duration.FromTicks( 5 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );

        var task = Task.Factory.StartNew( () => sut.Run() );
        while ( sut.State != ReactiveTimerState.Running )
            ;

        var action = Lambda.Of( () => sut.Run( delay ) );
        var assertion = action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Invoke();
        await task;

        assertion.Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Run_WithDelay_ShouldThrowArgumentOutOfRangeException_WhenDelayIsLessThanOne(long ticks)
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromTicks( ticks );
        var sut = new ReactiveTimer( timestampProvider, interval );

        var action = Lambda.Of( () => sut.Run( delay ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void Run_WithDelay_ShouldThrowArgumentOutOfRangeException_WhenDelayIsTooLarge(long ms)
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromMilliseconds( ms );
        var sut = new ReactiveTimer( timestampProvider, interval );

        var action = Lambda.Of( () => sut.Run( delay ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Run_WithDelay_ShouldReturnFalse_WhenTimerIsDisposed()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Run( delay ) );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void RunningTimer_ShouldPublishMultipleEventsInCorrectOrderAndDispose_WhenCountIsReached()
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

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: eventTimestamps.Count );
        sut.Listen( listener );

        sut.Run();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void RunningTimer_ShouldSkipFirstEvents_WhenAwaitingForFirstEventPreparationTakesTooLong()
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

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 4 );
        sut.Listen( listener );

        sut.Run();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void RunningTimer_ShouldSkipLastEvents_WhenAwaitingForLastEventPreparationTakesTooLong()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval, Timestamp.Zero + interval, Timestamp.Zero + interval * 10 };

        var expectedEvents = new[]
        {
            new WithInterval<long>( 0, Timestamp.Zero + interval, interval ),
            new WithInterval<long>( 3, Timestamp.Zero + interval * 10, interval * 9 )
        };

        var actualEvents = new List<WithInterval<long>>();

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 4 );
        sut.Listen( listener );

        sut.Run();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void RunningTimer_ShouldSkipIntermediateEvents_WhenAwaitingForTheirPreparationTakesTooLong()
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

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 6 );
        sut.Listen( listener );

        sut.Run();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void RunningTimer_ShouldSkipIntermediateEvents_WhenListenerReactionsTakeTooLong()
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

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 6 );
        sut.Listen( listener );

        sut.Run();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void RunningTimer_ShouldAttemptToSelfAlignToExpectedEventTimestamps()
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

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 5 );
        sut.Listen( listener );

        sut.Run();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void RunningTimer_ShouldSpinWait_WhenNextEventTimestampIsLessThanTheMinExpectedTimestamp()
    {
        var interval = Duration.FromTicks( 100 );
        var timestamps = new[]
        {
            Timestamp.Zero, Timestamp.Zero + interval * 0.8, Timestamp.Zero + interval * 0.9, Timestamp.Zero + interval
        };

        var expectedEvent = new WithInterval<long>( 0, Timestamp.Zero + interval, interval );
        var actualEvents = new List<WithInterval<long>>();

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );
        sut.Listen( listener );

        sut.Run();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public void RestartingStoppedTimer_ShouldContinueEventIndexesFromBefore()
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

        var sut = new ReactiveTimer( timestampProvider, interval, count: 4 );
        var listener = EventListener.Create<WithInterval<long>>( e =>
        {
            actualEvents.Add( e );
            if ( e.Event == 1 )
                sut.Stop();
        } );

        sut.Listen( listener );

        sut.Run();
        sut.Run();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldMarkTimerAsDisposed()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );

        sut.Dispose();

        sut.IsDisposed.TestTrue().Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenTimerIsAlreadyDisposed()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Dispose();

        sut.Dispose();

        sut.IsDisposed.TestTrue().Go();
    }

    [Fact]
    public async Task Dispose_ShouldStopRunningTimer()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( Timestamp.Zero );
        var interval = Duration.FromMilliseconds( 15 );
        var listener = Substitute.For<IEventListener<WithInterval<long>>>();
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Listen( listener );
        var task = Task.Factory.StartNew( () => sut.Run() );
        while ( sut.State != ReactiveTimerState.Running )
            ;

        sut.Dispose();
        await task;

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<WithInterval<long>>() ) ) )
            .Go();
    }

    [Fact]
    public async Task Stop_ShouldStopTheTimer_WhenTimerHasAlreadyStartedWaitingForNextStep()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( Timestamp.Zero );
        var interval = Duration.FromMilliseconds( 100 );
        var listener = Substitute.For<IEventListener<WithInterval<long>>>();
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Listen( listener );
        var task = Task.Factory.StartNew( () => sut.Run() );

        await Task.Delay( 1 );
        var result = sut.Stop();
        var state = sut.State;
        await task;

        Assertion.All(
                result.TestTrue(),
                state.TestNotEquals( ReactiveTimerState.Running ),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestFalse(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<WithInterval<long>>() ) ) )
            .Go();
    }

    [Fact]
    public void Stop_ShouldStopTheTimer_WhenTimerIsPreparingToWaitForTheNextStep()
    {
        var interval = Duration.FromTicks( 100 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval, Timestamp.Zero + interval };

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var expectedEvent = new WithInterval<long>( 0, Timestamp.Zero + interval, interval );
        var actualEvents = new List<WithInterval<long>>();

        var sut = new ReactiveTimer( timestampProvider, interval );

        var listener = EventListener.Create<WithInterval<long>>( e =>
        {
            actualEvents.Add( e );
            sut.Stop();
        } );

        sut.Listen( listener );
        sut.Run();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestFalse(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public void Stop_ShouldDoNothing_WhenTimerIsNotRunning()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );

        var result = sut.Stop();

        Assertion.All(
                result.TestFalse(),
                sut.State.TestEquals( ReactiveTimerState.Idle ) )
            .Go();
    }

    [Fact]
    public void Stop_ShouldReturnFalse_WhenTimerIsDisposed()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Dispose();

        var result = sut.Stop();

        Assertion.All(
                result.TestFalse(),
                sut.State.TestEquals( ReactiveTimerState.Idle ) )
            .Go();
    }
}
