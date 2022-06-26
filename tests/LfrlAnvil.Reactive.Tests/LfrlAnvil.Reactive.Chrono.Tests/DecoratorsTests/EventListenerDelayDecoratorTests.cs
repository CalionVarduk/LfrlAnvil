using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Decorators;
using LfrlAnvil.Reactive.Chrono.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Chrono.Tests.DecoratorsTests;

public class EventListenerDelayDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var scheduler = TaskScheduler.Current;
        var delay = Duration.FromTicks( Fixture.CreatePositiveInt32() );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var next = Substitute.For<IEventListener<WithInterval<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDelayDecorator<int>( timestampProvider, delay, scheduler, spinWaitDurationHint );

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public async Task Decorate_ShouldCreateListenerThatDelaysEmissionsOfEvents()
    {
        var sourceEvent = Fixture.Create<int>();
        var delay = Duration.FromMilliseconds( 1 );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( Timestamp.Zero, Timestamp.Zero + delay );

        var completion = new TaskCompletionSource();
        var expectedEvent = new WithInterval<int>( sourceEvent, Timestamp.Zero + delay, delay );
        var actualEvents = new List<WithInterval<int>>();
        var next = EventListener.Create<WithInterval<int>>(
            e =>
            {
                actualEvents.Add( e );
                completion.SetResult();
            } );

        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDelayDecorator<int>(
            timestampProvider,
            delay,
            scheduler: null,
            ReactiveTimer.DefaultSpinWaitDurationHint );

        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );
        await completion.Task;

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvent );
    }

    [Fact]
    public void Decorate_WithScheduler_ShouldCreateListenerThatDelaysEmissionsOfEvents()
    {
        var scheduler = new SynchronousTaskScheduler();
        var sourceEvent = Fixture.Create<int>();
        var delay = Duration.FromMilliseconds( 1 );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( Timestamp.Zero, Timestamp.Zero + delay );

        var expectedEvent = new WithInterval<int>( sourceEvent, Timestamp.Zero + delay, delay );
        var actualEvents = new List<WithInterval<int>>();
        var next = EventListener.Create<WithInterval<int>>( actualEvents.Add );

        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDelayDecorator<int>(
            timestampProvider,
            delay,
            scheduler,
            ReactiveTimer.DefaultSpinWaitDurationHint );

        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvent );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var scheduler = TaskScheduler.Current;
        var delay = Duration.FromTicks( Fixture.CreatePositiveInt32() );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var next = Substitute.For<IEventListener<WithInterval<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDelayDecorator<int>( timestampProvider, delay, scheduler, spinWaitDurationHint );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Fact]
    public async Task DelayExtension_ShouldCreateEventStreamThatDelaysEmissionsOfEvents()
    {
        var sourceEvent = Fixture.Create<int>();
        var delay = Duration.FromMilliseconds( 1 );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( Timestamp.Zero, Timestamp.Zero + delay );

        var completion = new TaskCompletionSource();
        var expectedEvent = new WithInterval<int>( sourceEvent, Timestamp.Zero + delay, delay );
        var actualEvents = new List<WithInterval<int>>();
        var next = EventListener.Create<WithInterval<int>>(
            e =>
            {
                actualEvents.Add( e );
                completion.SetResult();
            } );

        var sut = new EventPublisher<int>();
        var decorated = sut.Delay( timestampProvider, delay );
        decorated.Listen( next );

        sut.Publish( sourceEvent );
        await completion.Task;

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvent );
    }

    [Fact]
    public void DelayExtension_WithScheduler_ShouldCreateListenerThatDelaysEmissionsOfEvents()
    {
        var scheduler = new SynchronousTaskScheduler();
        var sourceEvent = Fixture.Create<int>();
        var delay = Duration.FromMilliseconds( 1 );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( Timestamp.Zero, Timestamp.Zero + delay );

        var expectedEvent = new WithInterval<int>( sourceEvent, Timestamp.Zero + delay, delay );
        var actualEvents = new List<WithInterval<int>>();
        var next = EventListener.Create<WithInterval<int>>( actualEvents.Add );

        var sut = new EventPublisher<int>();
        var decorated = sut.Delay( timestampProvider, delay, scheduler );
        decorated.Listen( next );

        sut.Publish( sourceEvent );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvent );
    }
}
