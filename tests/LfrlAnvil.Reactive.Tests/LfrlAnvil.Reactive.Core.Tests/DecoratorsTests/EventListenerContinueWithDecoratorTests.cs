using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerContinueWithDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<string>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerContinueWithDecorator<int, string>( e => EventSource.From( e.ToString() ) );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactDoesNotForwardAnyEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<string>();

        var next = EventListener.Create<string>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerContinueWithDecorator<int, string>( e => EventSource.From( e.ToString() ) );

        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestEmpty().Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsContinuationFactoryAndStartsListeningToItsResultImmediately(
        DisposalSource source)
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvent = sourceEvents[^1].ToString();
        var actualEvents = new List<string>();
        var isNextDisposed = false;
        var continuation = new HistoryEventPublisher<string>( capacity: 1 );

        var next = EventListener.Create<string>( actualEvents.Add, _ => isNextDisposed = true );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerContinueWithDecorator<int, string>(
            e =>
            {
                continuation.Publish( e.ToString() );
                return continuation;
            } );

        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        listener.OnDispose( source );

        Assertion.All(
                actualEvents.TestSequence( [ expectedEvent ] ),
                isNextDisposed.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void
        Decorate_ShouldCreateListenerWhoseOnDisposeCallsContinuationFactoryAndStartsListeningToItsResultImmediatelyAndCallsOnNextDispose_WhenContinuationSubscriberDisposes(
            DisposalSource source)
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };

        var next = Substitute.For<IEventListener<string>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerContinueWithDecorator<int, string>( e => EventSource.From( e.ToString() ) );

        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        listener.OnDispose( source );

        next.TestReceivedCalls( x => x.OnDispose( DisposalSource.Subscriber ) ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void
        Decorate_ShouldCreateListenerWhoseOnDisposeCallsContinuationFactoryAndStartsListeningToItsResultImmediatelyAndCallsNextOnDispose_WhenContinuationDisposes(
            DisposalSource source)
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var continuation = new HistoryEventPublisher<string>( capacity: 1 );

        var next = Substitute.For<IEventListener<string>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerContinueWithDecorator<int, string>( _ => continuation );

        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        listener.OnDispose( source );
        continuation.Dispose();

        next.TestReceivedCalls( x => x.OnDispose( DisposalSource.EventSource ) ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void
        Decorate_ShouldCreateListenerWhoseOnDisposeDoesNotCallContinuationFactoryAndCallsNextOnDisposeWhenNoEventHasBeenReceived(
            DisposalSource source)
    {
        var next = Substitute.For<IEventListener<string>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerContinueWithDecorator<int, string>( e => EventSource.From( e.ToString() ) );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        Assertion.All(
                next.TestDidNotReceiveCall( x => x.React( Arg.Any<string>() ) ),
                next.TestReceivedCalls( x => x.OnDispose( source ) ) )
            .Go();
    }

    [Fact]
    public void
        ContinueWithExtension_ShouldCreateEventStreamThatOnDisposeCallsContinuationFactoryAndStartsListeningToItsResultImmediately()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvent = sourceEvents[^1].ToString();
        var actualEvents = new List<string>();
        var isNextDisposed = false;

        var next = EventListener.Create<string>( actualEvents.Add, _ => isNextDisposed = true );
        var sut = new EventPublisher<int>();
        var decorated = sut.ContinueWith( e => EventSource.From( e.ToString() ) );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        sut.Dispose();

        Assertion.All(
                actualEvents.TestSequence( [ expectedEvent ] ),
                isNextDisposed.TestTrue() )
            .Go();
    }

    [Fact]
    public void
        ContinueWithExtension_ShouldCreateEventStreamThatOnDisposeCallsContinuationFactoryAndStartsListeningToItsResultImmediately_WhenChained()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvent = "46";
        var actualEvents = new List<string>();
        var isNextDisposed = false;

        var next = EventListener.Create<string>( actualEvents.Add, _ => isNextDisposed = true );
        var sut = new EventPublisher<int>();
        var decorated = sut
            .ContinueWith( e => EventSource.From( e, e * 2 ) )
            .ContinueWith( e => EventSource.From( e.ToString() ) );

        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        sut.Dispose();

        Assertion.All(
                actualEvents.TestSequence( [ expectedEvent ] ),
                isNextDisposed.TestTrue() )
            .Go();
    }
}
