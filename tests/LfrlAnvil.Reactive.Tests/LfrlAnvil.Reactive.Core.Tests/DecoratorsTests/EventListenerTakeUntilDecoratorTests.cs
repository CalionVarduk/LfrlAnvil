using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerTakeUntilDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber_WhenTargetDoesNotPublishEventImmediately()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );

        _ = sut.Decorate( next, subscriber );

        Assertion.All(
                subscriber.TestDidNotReceiveCall( x => x.Dispose() ),
                target.HasSubscribers.TestTrue() )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldDisposeTheSubscriber_WhenTargetPublishesEventImmediately()
    {
        var target = new HistoryEventPublisher<string>( capacity: 1 );
        target.Publish( Fixture.Create<string>() );

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestReceivedCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldDisposeTheSubscriber_WhenTargetIsAlreadyDisposed()
    {
        var target = new EventPublisher<string>();
        target.Dispose();

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestReceivedCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatDisposesTheSubscriber_WhenTargetPublishesAnyEvent()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );
        _ = sut.Decorate( next, subscriber );

        target.Publish( Fixture.Create<string>() );

        subscriber.TestReceivedCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatDisposesTheSubscriber_WhenTargetDisposes()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );
        _ = sut.Decorate( next, subscriber );

        target.Dispose();

        subscriber.TestReceivedCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardsEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.OnDispose( source ) ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeDisposesTheTargetSubscriber(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        target.HasSubscribers.TestFalse().Go();
    }

    [Fact]
    public void TakeUntilExtension_ShouldCreateEventStreamThatForwardsEvents_UntilTargetPublishesAnyEvent()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 1, 2, 3, 5, 7, 11 };
        var actualEvents = new List<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.TakeUntil( target );
        decorated.Listen( next );

        foreach ( var e in sourceEvents.Take( expectedEvents.Length ) )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( expectedEvents.Length ) )
            sut.Publish( e );

        Assertion.All(
                actualEvents.TestSequence( expectedEvents ),
                sut.HasSubscribers.TestFalse(),
                target.HasSubscribers.TestFalse() )
            .Go();
    }

    [Fact]
    public void TakeUntilExtension_ShouldDisposeSubscribers_WhenTargetIsSourceAndSourcePublishedAnyEvent()
    {
        var @event = Fixture.CreateNotDefault<int>();
        var next = Substitute.For<IEventListener<int>>();
        var sut = new EventPublisher<int>();
        var decorated = sut.TakeUntil( sut );
        decorated.Listen( next );

        sut.Publish( @event );

        Assertion.All(
                next.TestDidNotReceiveCall( x => x.React( Arg.Any<int>() ) ),
                next.TestReceivedCall( x => x.OnDispose( DisposalSource.Subscriber ) ),
                sut.HasSubscribers.TestFalse() )
            .Go();
    }
}
