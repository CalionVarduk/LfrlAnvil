using System.Collections.Generic;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Tests.CombineEventSourceTests;

public abstract class GenericCombineEventSourceTests<TEvent> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions()
    {
        var inner = new EventPublisher<TEvent>();
        var sut = new CombineEventSource<TEvent>( new[] { inner } );
        sut.HasSubscribers.TestFalse().Go();
    }

    [Fact]
    public void Listen_ShouldNotEmitAnyEventsAndDisposeSubscriberImmediately_WhenInnerStreamsAreEmpty()
    {
        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( Array.Empty<IEventStream<TEvent>>() );

        var subscriber = sut.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldReturnDisposedSubscriber_WhenEventSourceIsDisposed()
    {
        var inner = new EventPublisher<TEvent>();
        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( new[] { inner } );
        sut.Dispose();

        var subscriber = sut.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDisposes_WhenEventSourceIsDisposed()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        sut.Dispose();

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                firstStream.HasSubscribers.TestFalse(),
                secondStream.HasSubscribers.TestFalse(),
                thirdStream.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDoesNotEmitAnything_UntilAnyInnerStreamEmits()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        Assertion.All(
                firstStream.HasSubscribers.TestTrue(),
                secondStream.HasSubscribers.TestTrue(),
                thirdStream.HasSubscribers.TestTrue(),
                sut.HasSubscribers.TestTrue(),
                subscriber.IsDisposed.TestFalse(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDoesNotEmitAnything_UntilAllInnerStreamsEmitAtLeastOnce()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 4 );
        var firstStreamValues = new[] { values[0], values[1] };
        var thirdStreamValues = new[] { values[2], values[3] };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues ) thirdStream.Publish( e );

        foreach ( var e in firstStreamValues ) firstStream.Publish( e );

        Assertion.All(
                firstStream.HasSubscribers.TestTrue(),
                secondStream.HasSubscribers.TestTrue(),
                thirdStream.HasSubscribers.TestTrue(),
                sut.HasSubscribers.TestTrue(),
                subscriber.IsDisposed.TestFalse(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateDisposedSubscriber_WhenAtLeastOneInnerStreamIsDisposed()
    {
        var firstStream = new EventPublisher<TEvent>();
        firstStream.Dispose();

        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        Assertion.All(
                firstStream.HasSubscribers.TestFalse(),
                secondStream.HasSubscribers.TestFalse(),
                thirdStream.HasSubscribers.TestFalse(),
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) ) )
            .Go();
    }

    [Fact]
    public void
        Listen_ShouldCreateActiveSubscriberThatDisposes_WhenAtLeastOneInnerStreamDisposesAndNotAllInnerStreamsEmittedAtLeastOnce()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 4 );
        var firstStreamValues = new[] { values[0], values[1] };
        var thirdStreamValues = new[] { values[2], values[3] };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues ) thirdStream.Publish( e );

        foreach ( var e in firstStreamValues ) firstStream.Publish( e );

        firstStream.Dispose();

        Assertion.All(
                firstStream.HasSubscribers.TestFalse(),
                secondStream.HasSubscribers.TestFalse(),
                thirdStream.HasSubscribers.TestFalse(),
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldEmitEventContainingLastInnerStreamEventsEveryTimeInnerStreamEmits_WhenAllInnerStreamsEmittedAtLeastOnce()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 9 );
        var firstStreamValues = new[] { values[0], values[1], values[2] };
        var secondStreamValues = new[] { values[3], values[4], values[5] };
        var thirdStreamValues = new[] { values[6], values[7], values[8] };
        var expectedResult = new[]
        {
            new[] { firstStreamValues[1], secondStreamValues[0], thirdStreamValues[0] },
            new[] { firstStreamValues[1], secondStreamValues[0], thirdStreamValues[1] },
            new[] { firstStreamValues[1], secondStreamValues[1], thirdStreamValues[1] },
            new[] { firstStreamValues[1], secondStreamValues[2], thirdStreamValues[1] },
            new[] { firstStreamValues[2], secondStreamValues[2], thirdStreamValues[1] },
            new[] { firstStreamValues[2], secondStreamValues[2], thirdStreamValues[2] }
        };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var result = new List<TEvent[]>();
        var listener = EventListener.Create<ReadOnlyMemory<TEvent>>( e => result.Add( e.ToArray() ) );
        var sut = new CombineEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        firstStream.Publish( firstStreamValues[0] );
        firstStream.Publish( firstStreamValues[1] );
        secondStream.Publish( secondStreamValues[0] );
        thirdStream.Publish( thirdStreamValues[0] );
        thirdStream.Publish( thirdStreamValues[1] );
        secondStream.Publish( secondStreamValues[1] );
        secondStream.Publish( secondStreamValues[2] );
        firstStream.Publish( firstStreamValues[2] );
        thirdStream.Publish( thirdStreamValues[2] );

        Assertion.All(
                sut.HasSubscribers.TestTrue(),
                firstStream.HasSubscribers.TestTrue(),
                secondStream.HasSubscribers.TestTrue(),
                thirdStream.HasSubscribers.TestTrue(),
                subscriber.IsDisposed.TestFalse(),
                result.Count.TestEquals( expectedResult.Length ),
                result.TestAll( (events, i) => events.TestSequence( expectedResult[i] ) ) )
            .Go();
    }

    [Fact]
    public void
        Listen_ShouldCreateActiveSubscriberThatDoesNotDispose_WhenAllInnerStreamsEmittedAtLeastOnceAndNotAllInnerStreamsDispose()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 4 );
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();
        var expectedResult = new[] { new[] { values[0], values[1], values[2] }, new[] { values[0], values[3], values[2] } };

        var result = new List<TEvent[]>();
        var listener = EventListener.Create<ReadOnlyMemory<TEvent>>( e => result.Add( e.ToArray() ) );
        var sut = new CombineEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        firstStream.Publish( values[0] );
        secondStream.Publish( values[1] );
        thirdStream.Publish( values[2] );

        firstStream.Dispose();
        thirdStream.Dispose();

        secondStream.Publish( values[3] );

        Assertion.All(
                sut.HasSubscribers.TestTrue(),
                firstStream.HasSubscribers.TestFalse(),
                secondStream.HasSubscribers.TestTrue(),
                thirdStream.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestFalse(),
                result.Count.TestEquals( expectedResult.Length ),
                result.TestAll( (events, i) => events.TestSequence( expectedResult[i] ) ) )
            .Go();
    }

    [Fact]
    public void
        Listen_ShouldCreateActiveSubscriberThatDisposes_WhenAllInnerStreamsEmittedAtLeastOnceAndAllInnerStreamsDispose()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 3 );
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var result = new List<TEvent[]>();
        var listener = EventListener.Create<ReadOnlyMemory<TEvent>>( e => result.Add( e.ToArray() ) );
        var sut = new CombineEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        firstStream.Publish( values[0] );
        secondStream.Publish( values[1] );
        thirdStream.Publish( values[2] );

        firstStream.Dispose();
        thirdStream.Dispose();
        secondStream.Dispose();

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                firstStream.HasSubscribers.TestFalse(),
                secondStream.HasSubscribers.TestFalse(),
                thirdStream.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                result.Count.TestEquals( 1 ),
                result.TestAll( (r, _) => r.TestSequence( values ) ) )
            .Go();
    }

    [Fact]
    public void
        Combine_ThenListen_ShouldEmitEventContainingLastInnerStreamEventsEveryTimeInnerStreamEmits_WhenAllInnerStreamsEmittedAtLeastOnce()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 9 );
        var firstStreamValues = new[] { values[0], values[1], values[2] };
        var secondStreamValues = new[] { values[3], values[4], values[5] };
        var thirdStreamValues = new[] { values[6], values[7], values[8] };
        var expectedResult = new[]
        {
            new[] { firstStreamValues[1], secondStreamValues[0], thirdStreamValues[0] },
            new[] { firstStreamValues[1], secondStreamValues[0], thirdStreamValues[1] },
            new[] { firstStreamValues[1], secondStreamValues[1], thirdStreamValues[1] },
            new[] { firstStreamValues[1], secondStreamValues[2], thirdStreamValues[1] },
            new[] { firstStreamValues[2], secondStreamValues[2], thirdStreamValues[1] },
            new[] { firstStreamValues[2], secondStreamValues[2], thirdStreamValues[2] }
        };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var result = new List<TEvent[]>();
        var listener = EventListener.Create<ReadOnlyMemory<TEvent>>( e => result.Add( e.ToArray() ) );
        var sut = EventSource.Combine( firstStream, secondStream, thirdStream );
        var subscriber = sut.Listen( listener );

        firstStream.Publish( firstStreamValues[0] );
        firstStream.Publish( firstStreamValues[1] );
        secondStream.Publish( secondStreamValues[0] );
        thirdStream.Publish( thirdStreamValues[0] );
        thirdStream.Publish( thirdStreamValues[1] );
        secondStream.Publish( secondStreamValues[1] );
        secondStream.Publish( secondStreamValues[2] );
        firstStream.Publish( firstStreamValues[2] );
        thirdStream.Publish( thirdStreamValues[2] );

        Assertion.All(
                sut.HasSubscribers.TestTrue(),
                firstStream.HasSubscribers.TestTrue(),
                secondStream.HasSubscribers.TestTrue(),
                thirdStream.HasSubscribers.TestTrue(),
                subscriber.IsDisposed.TestFalse(),
                result.Count.TestEquals( expectedResult.Length ),
                result.TestAll( (events, i) => events.TestSequence( expectedResult[i] ) ) )
            .Go();
    }
}
