using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerDefaultIfEmptyDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var value = Fixture.Create<int>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDefaultIfEmptyDecorator<int>( value );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardsEvents()
    {
        var value = 29;
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDefaultIfEmptyDecorator<int>( value );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeDoesNotEmitDefaultValue_WhenAtLeastOneEventWasReceived(DisposalSource source)
    {
        var value = 1;
        var sourceEvent = 2;

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDefaultIfEmptyDecorator<int>( value );
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );
        listener.OnDispose( source );

        next.TestDidNotReceiveCall( x => x.React( value ) ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeEmitsDefaultValue_WhenNoEventsWereReceived(DisposalSource source)
    {
        var value = 1;
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDefaultIfEmptyDecorator<int>( value );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.React( value ) ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var value = Fixture.Create<int>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDefaultIfEmptyDecorator<int>( value );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void DefaultIfEmptyExtension_ShouldCreateEventStreamThatDoesNotEmitDefaultValue_WhenAtLeastOneEventWasReceived()
    {
        var value = 29;
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.DefaultIfEmpty( value );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Fact]
    public void DefaultIfEmptyExtension_ShouldCreateEventStreamThatEmitsDefaultValue_WhenNoEventsWereReceived()
    {
        var value = 29;
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.DefaultIfEmpty( value );
        decorated.Listen( next );

        sut.Dispose();

        actualEvents.TestSequence( [ value ] ).Go();
    }

    [Fact]
    public void DefaultIfEmptyExtension_WithoutParameter_ShouldCreateEventStreamThatEmitsDefaultValue_WhenNoEventsWereReceived()
    {
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.DefaultIfEmpty();
        decorated.Listen( next );

        sut.Dispose();

        actualEvents.TestSequence( [ default( int ) ] ).Go();
    }
}
