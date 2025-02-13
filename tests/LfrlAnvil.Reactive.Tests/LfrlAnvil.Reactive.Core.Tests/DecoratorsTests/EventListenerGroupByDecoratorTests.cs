using System.Collections.Generic;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerGroupByDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<EventGrouping<int, int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerGroupByDecorator<int, int>( e => e / 10, EqualityComparer<int>.Default );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactGroupsEverySourceEventByTheirKeys()
    {
        var sourceEvents = new[] { 1, 2, 11, 3, 5, 17, 7, 13, 23, 19 };
        var expectedEvents = new (int Key, int Event, int[] AllEvents)[]
        {
            (0, 1, new[] { 1 }),
            (0, 2, new[] { 1, 2 }),
            (1, 11, new[] { 11 }),
            (0, 3, new[] { 1, 2, 3 }),
            (0, 5, new[] { 1, 2, 3, 5 }),
            (1, 17, new[] { 11, 17 }),
            (0, 7, new[] { 1, 2, 3, 5, 7 }),
            (1, 13, new[] { 11, 17, 13 }),
            (2, 23, new[] { 23 }),
            (1, 19, new[] { 11, 17, 13, 19 })
        };

        var actualEvents = new List<(int Key, int Event, int[] AllEvents)>();

        var next = EventListener.Create<EventGrouping<int, int>>( e => actualEvents.Add( (e.Key, e.Event, e.AllEvents.ToArray()) ) );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerGroupByDecorator<int, int>( e => e / 10, EqualityComparer<int>.Default );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        Assertion.All(
                actualEvents.Count.TestEquals( expectedEvents.Length ),
                actualEvents.TestAll(
                    (e, i) => Assertion.All(
                        "event",
                        e.Key.TestEquals( expectedEvents[i].Key ),
                        e.Event.TestEquals( expectedEvents[i].Event ),
                        e.AllEvents.TestSequence( expectedEvents[i].AllEvents ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<EventGrouping<int, int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerGroupByDecorator<int, int>( e => e / 10, EqualityComparer<int>.Default );
        var listener = sut.Decorate( next, subscriber );
        listener.React( Fixture.Create<int>() );

        listener.OnDispose( source );

        next.TestReceivedCalls( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void GroupByExtension_ShouldCreateEventStreamThatGroupsEverySourceEventByTheirKeys()
    {
        var sourceEvents = new[] { 1, 2, 11, 3, 5, 17, 7, 13, 23, 19 };
        var expectedEvents = new (int Key, int Event, int[] AllEvents)[]
        {
            (0, 1, new[] { 1 }),
            (0, 2, new[] { 1, 2 }),
            (1, 11, new[] { 11 }),
            (0, 3, new[] { 1, 2, 3 }),
            (0, 5, new[] { 1, 2, 3, 5 }),
            (1, 17, new[] { 11, 17 }),
            (0, 7, new[] { 1, 2, 3, 5, 7 }),
            (1, 13, new[] { 11, 17, 13 }),
            (2, 23, new[] { 23 }),
            (1, 19, new[] { 11, 17, 13, 19 })
        };

        var actualEvents = new List<(int Key, int Event, int[] AllEvents)>();

        var next = EventListener.Create<EventGrouping<int, int>>( e => actualEvents.Add( (e.Key, e.Event, e.AllEvents.ToArray()) ) );
        var sut = new EventPublisher<int>();
        var decorated = sut.GroupBy( e => e / 10 );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        Assertion.All(
                actualEvents.Count.TestEquals( expectedEvents.Length ),
                actualEvents.TestAll(
                    (e, i) => Assertion.All(
                        "event",
                        e.Key.TestEquals( expectedEvents[i].Key ),
                        e.Event.TestEquals( expectedEvents[i].Event ),
                        e.AllEvents.TestSequence( expectedEvents[i].AllEvents ) ) ) )
            .Go();
    }
}
