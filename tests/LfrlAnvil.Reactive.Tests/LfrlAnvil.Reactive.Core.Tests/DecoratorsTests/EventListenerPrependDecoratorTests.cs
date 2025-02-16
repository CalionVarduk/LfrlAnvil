using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerPrependDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var values = new[] { 100, 101, 102 };
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerPrependDecorator<int>( values );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatImmediatelyEmitsPrependedValues()
    {
        var values = new[] { 100, 101, 102 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerPrependDecorator<int>( values );
        _ = sut.Decorate( next, subscriber );

        actualEvents.TestSequence( values ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardsEvents()
    {
        var values = new[] { 100, 101, 102 };
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = values.Concat( sourceEvents );
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerPrependDecorator<int>( values );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var values = new[] { 100, 101, 102 };
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerPrependDecorator<int>( values );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void PrependExtension_ShouldCreateEventStreamThatEmitsPrependedValuesImmediatelyAndForwardsEvents()
    {
        var values = new[] { 100, 101, 102 };
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = values.Concat( sourceEvents );
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Prepend( values );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.TestSequence( expectedEvents ).Go();
    }
}
