using System.Collections.Generic;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Decorators;
using LfrlAnvil.Reactive.Chrono.Extensions;

namespace LfrlAnvil.Reactive.Chrono.Tests.DecoratorsTests;

public class EventListenerDelayDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var delay = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var next = Substitute.For<IEventListener<WithInterval<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDelayDecorator<int>( ValueTaskDelaySource.Start(), delay, spinWaitDurationHint, null );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
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
        var next = EventListener.Create<WithInterval<int>>( e =>
        {
            actualEvents.Add( e );
            completion.SetResult();
        } );

        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDelayDecorator<int>(
            ValueTaskDelaySource.Start(),
            delay,
            ReactiveTimer.DefaultSpinWaitDurationHint,
            timestampProvider );

        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );
        await completion.Task;

        actualEvents.TestSequence( [ expectedEvent ] ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var delay = Duration.FromTicks( Fixture.Create<int>( x => x > 0 ) );
        var spinWaitDurationHint = Duration.FromTicks( Fixture.Create<uint>() );
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var next = Substitute.For<IEventListener<WithInterval<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDelayDecorator<int>( ValueTaskDelaySource.Start(), delay, spinWaitDurationHint, timestampProvider );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.OnDispose( source ) ).Go();
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
        var next = EventListener.Create<WithInterval<int>>( e =>
        {
            actualEvents.Add( e );
            completion.SetResult();
        } );

        var sut = new EventPublisher<int>();
        var decorated = sut.Delay( ValueTaskDelaySource.Start(), delay, timestampProvider );
        decorated.Listen( next );

        sut.Publish( sourceEvent );
        await completion.Task;

        actualEvents.TestSequence( [ expectedEvent ] ).Go();
    }
}
