using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

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

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
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

        actualEvents.Should().BeEmpty();
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

        using ( new AssertionScope() )
        {
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvent );
            isNextDisposed.Should().BeFalse();
        }
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

        next.VerifyCalls().Received( x => x.OnDispose( DisposalSource.Subscriber ) );
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

        next.VerifyCalls().Received( x => x.OnDispose( DisposalSource.EventSource ) );
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

        using ( new AssertionScope() )
        {
            next.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<string>() ) );
            next.VerifyCalls().Received( x => x.OnDispose( source ) );
        }
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

        using ( new AssertionScope() )
        {
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvent );
            isNextDisposed.Should().BeTrue();
        }
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

        using ( new AssertionScope() )
        {
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvent );
            isNextDisposed.Should().BeTrue();
        }
    }
}
