using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerFirstDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerFirstDecorator<int>();

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardTheFirstEvent()
    {
        var sourceEvent = Fixture.Create<int>();
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerFirstDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );

        actualEvents.TestSequence( [ sourceEvent ] ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseFirstReactDisposesTheSubscriber()
    {
        var sourceEvent = Fixture.Create<int>();
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerFirstDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );

        subscriber.TestReceivedCall( x => x.Dispose() ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerFirstDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void FirstExtension_ShouldCreateEventStreamThatForwardsTheFirstEventAndThenDisposesSubscriberImmediately()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.First();
        var subscriber = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        Assertion.All(
                actualEvents.TestSequence( [ sourceEvents[0] ] ),
                subscriber.IsDisposed.TestTrue() )
            .Go();
    }
}
