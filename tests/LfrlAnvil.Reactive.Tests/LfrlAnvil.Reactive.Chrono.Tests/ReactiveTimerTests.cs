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
    public void Start_ShouldRunTimerOnTheCurrentThread()
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

        var result = sut.Start();

        Assertion.All(
                result.TestTrue(),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldReturnFalse_WhenTimerIsAlreadyRunning()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );

        var task = sut.StartAsync();
        var state = sut.State;
        var result = sut.Start();
        await task;

        Assertion.All(
                state.TestEquals( ReactiveTimerState.Running ),
                result.TestFalse() )
            .Go();
    }

    [Fact]
    public void Start_ShouldReturnFalse_WhenTimerIsDisposed()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Dispose();

        var result = sut.Start();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                result.TestFalse() )
            .Go();
    }

    [Fact]
    public void Start_WithDelay_ShouldRunTimerOnTheCurrentThreadAndDelayTheFirstEvent()
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

        var result = sut.Start( delay );

        Assertion.All(
                result.TestTrue(),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_WithDelay_ShouldReturnFalse_WhenTimerIsAlreadyRunning()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var delay = Duration.FromTicks( 5 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );

        var task = sut.StartAsync();
        var state = sut.State;
        var result = sut.Start( delay );
        await task;

        Assertion.All(
                state.TestEquals( ReactiveTimerState.Running ),
                result.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Start_WithDelay_ShouldThrowArgumentOutOfRangeException_WhenDelayIsLessThanOne(long ticks)
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromTicks( ticks );
        var sut = new ReactiveTimer( timestampProvider, interval );

        var action = Lambda.Of( () => sut.Start( delay ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void Start_WithDelay_ShouldThrowArgumentOutOfRangeException_WhenDelayIsTooLarge(long ms)
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromMilliseconds( ms );
        var sut = new ReactiveTimer( timestampProvider, interval );

        var action = Lambda.Of( () => sut.Start( delay ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Start_WithDelay_ShouldReturnFalse_WhenTimerIsDisposed()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Dispose();

        var result = sut.Start( delay );

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                result.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldRunTimerOnThreadPool()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };

        var expectedEvent = new WithInterval<long>( 0, timestamps[1], interval );
        var actualEvents = new List<WithInterval<long>>();

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );
        sut.Listen( listener );

        var task = sut.StartAsync();
        var state = sut.State;
        await task;

        Assertion.All(
                state.TestEquals( ReactiveTimerState.Running ),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnCancelledTask_WhenTimerIsAlreadyRunning()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );

        var task = sut.StartAsync();
        var state = sut.State;
        var result = sut.StartAsync();
        var isCancelled = result.IsCanceled;
        await task;

        Assertion.All(
                state.TestEquals( ReactiveTimerState.Running ),
                isCancelled.TestTrue() )
            .Go();
    }

    [Fact]
    public void StartAsync_ShouldReturnCompletedTask_WhenTimerIsDisposed()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Dispose();

        var result = sut.StartAsync();

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                result.IsCompleted.TestTrue() )
            .Go();
    }

    [Fact]
    public async Task StartAsync_WithDelay_ShouldRunTimerOnThreadPoolAndDelayTheFirstEvent()
    {
        var interval = Duration.FromTicks( 1 );
        var delay = Duration.FromMilliseconds( 15 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + delay };

        var expectedEvent = new WithInterval<long>( 0, timestamps[1], delay );
        var actualEvents = new List<WithInterval<long>>();

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );
        sut.Listen( listener );

        var task = sut.StartAsync( delay );
        var state = sut.State;
        await task;

        Assertion.All(
                state.TestEquals( ReactiveTimerState.Running ),
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_WithDelay_ShouldReturnCancelledTask_WhenTimerIsAlreadyRunning()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var delay = Duration.FromTicks( 5 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );

        var task = sut.StartAsync();
        var state = sut.State;
        var result = sut.StartAsync( delay );
        var isCancelled = result.IsCanceled;
        await task;

        Assertion.All(
                state.TestEquals( ReactiveTimerState.Running ),
                isCancelled.TestTrue() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void StartAsync_WithDelay_ShouldThrowArgumentOutOfRangeException_WhenDelayIsLessThanOne(long ticks)
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromTicks( ticks );
        var sut = new ReactiveTimer( timestampProvider, interval );

        var action = Lambda.Of( () => sut.StartAsync( delay ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void StartAsync_WithDelay_ShouldThrowArgumentOutOfRangeException_WhenDelayIsTooLarge(long ms)
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromMilliseconds( ms );
        var sut = new ReactiveTimer( timestampProvider, interval );

        var action = Lambda.Of( () => sut.StartAsync( delay ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void StartAsync_WithDelay_ShouldReturnCompletedTask_WhenTimerIsDisposed()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Dispose();

        var result = sut.StartAsync( delay );

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                result.IsCompleted.TestTrue() )
            .Go();
    }

    [Fact]
    public async Task StartAsync_WithScheduler_ShouldRunTimerOnProvidedScheduler()
    {
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromMilliseconds( 15 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };

        var expectedEvent = new WithInterval<long>( 0, timestamps[1], interval );
        var actualEvents = new List<WithInterval<long>>();

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );
        sut.Listen( listener );

        await sut.StartAsync( scheduler );

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_WithScheduler_ShouldReturnCancelledTask_WhenTimerIsAlreadyRunning()
    {
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromMilliseconds( 15 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );

        var task = sut.StartAsync();
        var state = sut.State;
        var result = sut.StartAsync( scheduler );
        var isCancelled = result.IsCanceled;
        await task;

        Assertion.All(
                state.TestEquals( ReactiveTimerState.Running ),
                isCancelled.TestTrue() )
            .Go();
    }

    [Fact]
    public void StartAsync_WithScheduler_ShouldReturnCompletedTask_WhenTimerIsDisposed()
    {
        var scheduler = new SynchronousTaskScheduler();
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Dispose();

        var result = sut.StartAsync( scheduler );

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                result.IsCompleted.TestTrue() )
            .Go();
    }

    [Fact]
    public async Task StartAsync_WithSchedulerAndDelay_ShouldRunTimerOnProvidedSchedulerAndDelayTheFirstEvent()
    {
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromTicks( 1 );
        var delay = Duration.FromMilliseconds( 15 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + delay };

        var expectedEvent = new WithInterval<long>( 0, timestamps[1], delay );
        var actualEvents = new List<WithInterval<long>>();

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );
        sut.Listen( listener );

        await sut.StartAsync( scheduler, delay );

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                sut.IsDisposed.TestTrue(),
                actualEvents.TestSequence( [ expectedEvent ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_WithSchedulerAndDelay_ShouldReturnCancelledTask_WhenTimerIsAlreadyRunning()
    {
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromMilliseconds( 15 );
        var delay = Duration.FromTicks( 5 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval };
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var sut = new ReactiveTimer( timestampProvider, interval, count: 1 );

        var task = sut.StartAsync();
        var state = sut.State;
        var result = sut.StartAsync( scheduler, delay );
        var isCancelled = result.IsCanceled;
        await task;

        Assertion.All(
                state.TestEquals( ReactiveTimerState.Running ),
                isCancelled.TestTrue() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void StartAsync_WithSchedulerAndDelay_ShouldThrowArgumentOutOfRangeException_WhenDelayIsLessThanOne(long ticks)
    {
        var scheduler = new SynchronousTaskScheduler();
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromTicks( ticks );
        var sut = new ReactiveTimer( timestampProvider, interval );

        var action = Lambda.Of( () => sut.StartAsync( scheduler, delay ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void StartAsync_WithSchedulerAndDelay_ShouldThrowArgumentOutOfRangeException_WhenDelayIsTooLarge(long ms)
    {
        var scheduler = new SynchronousTaskScheduler();
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromMilliseconds( ms );
        var sut = new ReactiveTimer( timestampProvider, interval );

        var action = Lambda.Of( () => sut.StartAsync( scheduler, delay ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void StartAsync_WithSchedulerAndDelay_ShouldReturnCompletedTask_WhenTimerIsDisposed()
    {
        var scheduler = new SynchronousTaskScheduler();
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var delay = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var sut = new ReactiveTimer( timestampProvider, interval );
        sut.Dispose();

        var result = sut.StartAsync( scheduler, delay );

        Assertion.All(
                sut.State.TestEquals( ReactiveTimerState.Idle ),
                result.IsCompleted.TestTrue() )
            .Go();
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

        sut.Start();

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

        sut.Start();

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

        sut.Start();

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

        sut.Start();

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

        sut.Start();

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

        sut.Start();

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

        sut.Start();

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
        var listener = EventListener.Create<WithInterval<long>>(
            e =>
            {
                actualEvents.Add( e );
                if ( e.Event == 1 )
                    sut.Stop();
            } );

        sut.Listen( listener );

        sut.Start();
        sut.Start();

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
        var task = sut.StartAsync();

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
        var task = sut.StartAsync();

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

        var listener = EventListener.Create<WithInterval<long>>(
            e =>
            {
                actualEvents.Add( e );
                sut.Stop();
            } );

        sut.Listen( listener );
        sut.Start();

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
