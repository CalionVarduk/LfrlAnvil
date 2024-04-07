using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Reactive.Chrono.Tests.IntervalEventSourceTests;

public class IntervalEventSourceTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions()
    {
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromTicks( Fixture.CreatePositiveInt32() );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.CreatePositiveInt32();
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var sut = new IntervalEventSource( timestampProvider, interval, scheduler, spinWaitDurationHint, count );

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeFalse();
            sut.Subscribers.Should().BeEmpty();
            sut.HasSubscribers.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanOneTick(long ticks)
    {
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromTicks( ticks );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.CreatePositiveInt32();
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new IntervalEventSource( timestampProvider, interval, scheduler, spinWaitDurationHint, count ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( int.MaxValue + 1L )]
    [InlineData( int.MaxValue + 2L )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsTooLarge(long ms)
    {
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromMilliseconds( ms );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.CreatePositiveInt32();
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new IntervalEventSource( timestampProvider, interval, scheduler, spinWaitDurationHint, count ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenSpinWaitDurationHintIsLessThanZero()
    {
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromTicks( Fixture.CreatePositiveInt32() );
        var spinWaitDurationHint = Duration.FromTicks( -1 );
        var count = Fixture.CreatePositiveInt32();
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new IntervalEventSource( timestampProvider, interval, scheduler, spinWaitDurationHint, count ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(long count)
    {
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromTicks( Fixture.CreatePositiveInt32() );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var timestampProvider = Substitute.For<ITimestampProvider>();

        var action = Lambda.Of( () => new IntervalEventSource( timestampProvider, interval, scheduler, spinWaitDurationHint, count ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Listen_ShouldReturnDisposedSubscriber_WhenEventSourceIsDisposed()
    {
        var listener = Substitute.For<IEventListener<WithInterval<long>>>();
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromTicks( Fixture.CreatePositiveInt32() );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var count = Fixture.CreatePositiveInt32();
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var sut = new IntervalEventSource( timestampProvider, interval, scheduler, spinWaitDurationHint, count );
        sut.Dispose();

        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<WithInterval<long>>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDisposes_WhenEventSourceIsDisposed()
    {
        var listener = Substitute.For<IEventListener<WithInterval<long>>>();
        var interval = Duration.FromSeconds( 1 );
        var count = Fixture.CreatePositiveInt32();
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( Timestamp.Zero );
        var sut = new IntervalEventSource(
            timestampProvider,
            interval,
            scheduler: null,
            ReactiveTimer.DefaultSpinWaitDurationHint,
            count );

        var subscriber = sut.Listen( listener );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<WithInterval<long>>() ) );
        }
    }

    [Fact]
    public void
        Listen_WithCustomScheduler_ShouldCreateActiveSubscriberThatPublishesMultipleEventsInCorrectOrderAndDisposes_WhenCountIsReached()
    {
        var scheduler = new SynchronousTaskScheduler();
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

        var sut = new IntervalEventSource(
            timestampProvider,
            interval,
            scheduler,
            ReactiveTimer.DefaultSpinWaitDurationHint,
            count: eventTimestamps.Count );

        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            sut.IsDisposed.Should().BeFalse();
            sut.HasSubscribers.Should().BeFalse();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
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

        var completion = new TaskCompletionSource();
        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.SetResult() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = new IntervalEventSource(
            timestampProvider,
            interval,
            scheduler: null,
            ReactiveTimer.DefaultSpinWaitDurationHint,
            count: eventTimestamps.Count );

        var subscriber = sut.Listen( listener );
        await completion.Task;

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            sut.IsDisposed.Should().BeFalse();
            sut.HasSubscribers.Should().BeFalse();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }

    [Fact]
    public void Listen_ShouldCreateSubscribersThatSpawnTheirOwnTimers()
    {
        var scheduler = new SynchronousTaskScheduler();
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
            scheduler,
            ReactiveTimer.DefaultSpinWaitDurationHint,
            count: 1 );

        sut.Listen( firstListener );
        sut.Listen( secondListener );

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeFalse();
            sut.HasSubscribers.Should().BeFalse();
            actualFirstEvents.Should().BeSequentiallyEqualTo( expectedFirstEvent );
            actualSecondEvents.Should().BeSequentiallyEqualTo( expectedSecondEvent );
        }
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

        var completion = new TaskCompletionSource();
        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Interval( timestampProvider, interval );
        var listener = EventListener.Create<WithInterval<long>>(
            e =>
            {
                actualEvents.Add( e );
                if ( e.Event == eventTimestamps.Count - 1 )
                    sut.Dispose();
            },
            _ => completion.SetResult() );

        sut.Listen( listener );
        await completion.Task;

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public async Task Interval_WithCount_ThenListen_ShouldCreateActiveSubscriberThatPublishesMultipleEventsInCorrectOrder()
    {
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval, Timestamp.Zero + interval, Timestamp.Zero + interval * 2 };

        var completion = new TaskCompletionSource();
        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Interval( timestampProvider, interval, count: eventTimestamps.Count );
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.SetResult() );

        sut.Listen( listener );
        await completion.Task;

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
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

        var completion = new TaskCompletionSource();
        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Interval( timestampProvider, interval, spinWaitDurationHint );
        var listener = EventListener.Create<WithInterval<long>>(
            e =>
            {
                actualEvents.Add( e );
                if ( e.Event == eventTimestamps.Count - 1 )
                    sut.Dispose();
            },
            _ => completion.SetResult() );

        sut.Listen( listener );
        await completion.Task;

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public async Task
        Interval_WithSchedulerAndImplicitMaxCount_ThenListen_ShouldCreateActiveSubscriberThatPublishesMultipleEventsInCorrectOrder()
    {
        var scheduler = TaskScheduler.Current;
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

        var completion = new TaskCompletionSource();
        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Interval( timestampProvider, interval, scheduler );
        var listener = EventListener.Create<WithInterval<long>>(
            e =>
            {
                actualEvents.Add( e );
                if ( e.Event == eventTimestamps.Count - 1 )
                    sut.Dispose();
            },
            _ => completion.SetResult() );

        sut.Listen( listener );
        await completion.Task;

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void Interval_WithSchedulerAndCount_ThenListen_ShouldCreateActiveSubscriberThatPublishesMultipleEventsInCorrectOrder()
    {
        var scheduler = new SynchronousTaskScheduler();
        var interval = Duration.FromTicks( 1 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + interval, Timestamp.Zero + interval, Timestamp.Zero + interval * 2 };

        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Interval( timestampProvider, interval, scheduler, count: eventTimestamps.Count );
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );

        sut.Listen( listener );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public async Task
        Interval_WithSchedulerAndSpinWaitDurationHint_ThenListen_ShouldCreateActiveSubscriberThatPublishesMultipleEventsInCorrectOrder()
    {
        var scheduler = TaskScheduler.Current;
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

        var completion = new TaskCompletionSource();
        var eventTimestamps = timestamps.Skip( 1 ).Distinct().ToList();
        var expectedEvents = eventTimestamps.Select( (t, i) => new WithInterval<long>( i, t, interval ) );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Interval( timestampProvider, interval, scheduler, spinWaitDurationHint );
        var listener = EventListener.Create<WithInterval<long>>(
            e =>
            {
                actualEvents.Add( e );
                if ( e.Event == eventTimestamps.Count - 1 )
                    sut.Dispose();
            },
            _ => completion.SetResult() );

        sut.Listen( listener );
        await completion.Task;

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public async Task Timeout_ThenListen_ShouldCreateActiveSubscriberThatPublishesSingleEvent()
    {
        var timeout = Duration.FromTicks( 1 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + timeout };

        var completion = new TaskCompletionSource();
        var expectedEvent = new WithInterval<long>( 0, Timestamp.Zero + timeout, timeout );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Timeout( timestampProvider, timeout );
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add, _ => completion.SetResult() );

        sut.Listen( listener );
        await completion.Task;

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvent );
    }

    [Fact]
    public void Timeout_WithScheduler_ThenListen_ShouldCreateActiveSubscriberThatPublishesSingleEvent()
    {
        var scheduler = new SynchronousTaskScheduler();
        var timeout = Duration.FromTicks( 1 );
        var timestamps = new[] { Timestamp.Zero, Timestamp.Zero + timeout };

        var expectedEvent = new WithInterval<long>( 0, Timestamp.Zero + timeout, timeout );
        var actualEvents = new List<WithInterval<long>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );

        var sut = ChronoEventSource.Timeout( timestampProvider, timeout, scheduler );
        var listener = EventListener.Create<WithInterval<long>>( actualEvents.Add );

        sut.Listen( listener );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvent );
    }
}
