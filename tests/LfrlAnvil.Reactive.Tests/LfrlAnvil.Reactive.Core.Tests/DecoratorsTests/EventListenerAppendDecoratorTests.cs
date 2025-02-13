using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerAppendDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var values = new[] { 100, 101, 102 };
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerAppendDecorator<int>( values );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldReturnNext_WhenValuesAreEmpty()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerAppendDecorator<int>( Array.Empty<int>() );

        var listener = sut.Decorate( next, subscriber );

        listener.TestRefEquals( next ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardsEvents()
    {
        var values = new[] { 100, 101, 102 };
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerAppendDecorator<int>( values );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeEmitsValues(DisposalSource source)
    {
        var values = new[] { 100, 101, 102 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerAppendDecorator<int>( values );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        actualEvents.TestSequence( values ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var values = new[] { 100, 101, 102 };
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerAppendDecorator<int>( values );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCalls( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void AppendExtension_ShouldCreateEventStreamThatForwardsEventsAndEmitsValues_WhenSourceStreamDisposes()
    {
        var values = new[] { 100, 101, 102 };
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = sourceEvents.Concat( values );
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Append( values );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        sut.Dispose();

        actualEvents.TestSequence( expectedEvents ).Go();
    }
}
