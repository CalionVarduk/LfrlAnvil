using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Internal;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Reactive.Chrono.Tests;

public class IntervalEventSourceTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.Create<int>( x => x > 0 );
        var sut = new IntervalEventSource( null, interval, spinWaitDurationHint, null, count );

        Assertion.All(
                sut.IsDisposed.TestFalse(),
                sut.Subscribers.TestEmpty(),
                sut.HasSubscribers.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanOneTick(long ticks)
    {
        var interval = Duration.FromTicks( ticks );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.Create<int>( x => x > 0 );

        var action = Lambda.Of( () => new IntervalEventSource( null, interval, spinWaitDurationHint, null, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsTooLarge(long ms)
    {
        var interval = Duration.FromMilliseconds( ms );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.Create<int>( x => x > 0 );

        var action = Lambda.Of( () => new IntervalEventSource( null, interval, spinWaitDurationHint, null, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenSpinWaitDurationHintIsLessThanZero()
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( -1 );
        var count = Fixture.Create<int>( x => x > 0 );

        var action = Lambda.Of( () => new IntervalEventSource( null, interval, spinWaitDurationHint, null, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(long count)
    {
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );

        var action = Lambda.Of( () => new IntervalEventSource( null, interval, spinWaitDurationHint, null, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Listen_ShouldReturnDisposedSubscriber_WhenEventSourceIsDisposed()
    {
        var listener = Substitute.For<IEventListener<WithInterval<long>>>();
        var interval = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.Create<int>( x => x > 0 );
        var sut = new IntervalEventSource( null, interval, spinWaitDurationHint, null, count );
        sut.Dispose();

        var subscriber = sut.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<WithInterval<long>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDisposes_WhenEventSourceIsDisposed()
    {
        var listener = Substitute.For<IEventListener<WithInterval<long>>>();
        var interval = Duration.FromSeconds( 1 );
        var count = Fixture.Create<int>( x => x > 0 );
        var sut = new IntervalEventSource(
            null,
            interval,
            ReactiveTimer.DefaultSpinWaitDurationHint,
            null,
            count );

        var subscriber = sut.Listen( listener );

        sut.Dispose();

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<WithInterval<long>>() ) ) )
            .Go();
    }

    [Fact]
    public async Task Listen_ShouldCreateActiveSubscriberThatPublishesMultipleEventsInCorrectOrderAndDisposes_WhenCountIsReached()
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

        var completion = new SafeTaskCompletionSource( completionCount: 2 );
        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new IntervalEventSource(
            timestampProvider,
            interval,
            ReactiveTimer.DefaultSpinWaitDurationHint,
            null,
            count: eventTimestamps.Count );

        var subscriber = sut.Listen( listener );
        await completion.Task;

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                sut.IsDisposed.TestFalse(),
                sut.HasSubscribers.TestFalse(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateSubscribersThatSpawnTheirOwnTimers()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[]
        {
            Timestamp.Zero, Timestamp.Zero + interval, Timestamp.Zero + interval * 10, Timestamp.Zero + interval * 11
        };

        var expectedFirstEvent = new WithInterval<long>( 0, Timestamp.Zero + interval, interval );
        var expectedSecondEvent = new WithInterval<long>( 0, Timestamp.Zero + interval * 11, interval );
        var actualFirstEvents = new List<WithInterval<long>>();
        var actualSecondEvents = new List<WithInterval<long>>();

        var firstListener = EventListener.Create<WithInterval<long>>( actualFirstEvents.Add );
        var secondListener = EventListener.Create<WithInterval<long>>( actualSecondEvents.Add );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new IntervalEventSource(
            timestampProvider,
            interval,
            ReactiveTimer.DefaultSpinWaitDurationHint,
            null,
            count: 1 );

        sut.Listen( firstListener );
        sut.Listen( secondListener );

        Assertion.All(
                sut.IsDisposed.TestFalse(),
                sut.HasSubscribers.TestFalse(),
                actualFirstEvents.TestSequence( [ expectedFirstEvent ] ),
                actualSecondEvents.TestSequence( [ expectedSecondEvent ] ) )
            .Go();
    }

    [Fact]
    public async Task Interval_WithImplicitMaxCount_ThenListen_ShouldCreateActiveSubscriberThatPublishesMultipleEventsInCorrectOrder()
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

        var completion = new SafeTaskCompletionSource( completionCount: 2 );
        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Interval( interval, timestampProvider );
        var listener = EventListener.Create<WithInterval<long>>(
            e =>
            {
                actualEvents.Add( e );
                if ( e.Event == eventTimestamps.Count - 1 )
                    sut.Dispose();
            },
            _ => completion.Complete() );

        sut.Listen( listener );
        await completion.Task;

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Fact]
    public async Task Interval_WithCount_ThenListen_ShouldCreateActiveSubscriberThatPublishesMultipleEventsInCorrectOrder()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval, Timestamp.Zero + interval, Timestamp.Zero + interval * 2 };

        var completion = new SafeTaskCompletionSource( completionCount: 2 );
        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Interval( interval, count: eventTimestamps.Count, timestampProvider );
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );

        sut.Listen( listener );
        await completion.Task;

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Fact]
    public async Task
        Interval_WithSpinWaitDurationHint_ThenListen_ShouldCreateActiveSubscriberThatPublishesMultipleEventsInCorrectOrder()
    {
        var interval = Duration.FromTicks( 1 );
        var spinWaitDurationHint = Duration.FromMilliseconds( 0.5 );
        var timestamps = new[]
        {
            Timestamp.Zero,
            Timestamp.Zero + interval,
            Timestamp.Zero + interval,
            Timestamp.Zero + interval * 2,
            Timestamp.Zero + interval * 2,
            Timestamp.Zero + interval * 3
        };

        var completion = new SafeTaskCompletionSource( completionCount: 2 );
        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Interval( interval, spinWaitDurationHint, timestampProvider );
        var listener = EventListener.Create<WithInterval<long>>(
            e =>
            {
                actualEvents.Add( e );
                if ( e.Event == eventTimestamps.Count - 1 )
                    sut.Dispose();
            },
            _ => completion.Complete() );

        sut.Listen( listener );
        await completion.Task;

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Fact]
    public async Task Timeout_ThenListen_ShouldCreateActiveSubscriberThatPublishesSingleEvent()
    {
        var timeout = Duration.FromTicks( 1 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + timeout };

        var completion = new SafeTaskCompletionSource( completionCount: 2 );
        var expectedEvent = new WithInterval<long>( 0, Timestamp.Zero + timeout, timeout );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Timeout( timeout, timestampProvider );
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.Complete() );

        sut.Listen( listener );
        await completion.Task;

        actualEvents.TestSequence( [ expectedEvent ] ).Go();
    }
}
