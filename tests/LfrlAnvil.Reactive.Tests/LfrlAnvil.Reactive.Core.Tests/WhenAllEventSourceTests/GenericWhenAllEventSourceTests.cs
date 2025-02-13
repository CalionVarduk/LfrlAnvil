using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Tests.WhenAllEventSourceTests;

public abstract class GenericWhenAllEventSourceTests<TEvent> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions()
    {
        var inner = new EventPublisher<TEvent>();
        var sut = new WhenAllEventSource<TEvent>( new[] { inner } );
        sut.HasSubscribers.TestFalse().Go();
    }

    [Fact]
    public void Listen_ShouldEmitEmptyResultAndDisposeSubscriberImmediately_WhenInnerStreamsAreEmpty()
    {
        TEvent?[]? result = null;
        var listener = EventListener.Create<ReadOnlyMemory<TEvent?>>( e => result = e.ToArray() );
        var sut = new WhenAllEventSource<TEvent>( Array.Empty<IEventStream<TEvent>>() );

        var subscriber = sut.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                result.TestNotNull( r => r.TestEmpty() ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldReturnDisposedSubscriber_WhenEventSourceIsDisposed()
    {
        var inner = new EventPublisher<TEvent>();
        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent?>>>();
        var sut = new WhenAllEventSource<TEvent>( new[] { inner } );
        sut.Dispose();

        var subscriber = sut.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<ReadOnlyMemory<TEvent?>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDisposesAndEmitsDefaultResult_WhenEventSourceIsDisposed()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();
        var expectedResult = new TEvent?[] { default, default, default };

        TEvent?[]? result = null;
        var listener = EventListener.Create<ReadOnlyMemory<TEvent?>>( e => result = e.ToArray() );
        var sut = new WhenAllEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        sut.Dispose();

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                firstStream.HasSubscribers.TestFalse(),
                secondStream.HasSubscribers.TestFalse(),
                thirdStream.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                result.TestNotNull( r => r.TestSequence( expectedResult ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDoesNotEmitAnything_UntilAllInnerStreamsDispose()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent?>>>();
        var sut = new WhenAllEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        firstStream.Dispose();
        thirdStream.Dispose();

        Assertion.All(
                secondStream.HasSubscribers.TestTrue(),
                sut.HasSubscribers.TestTrue(),
                subscriber.IsDisposed.TestFalse(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<ReadOnlyMemory<TEvent?>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldEmitOnlyOnceEveryInnerStreamDisposesWithResultContainingTheirLastEventsInOrder()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        TEvent?[] expectedResult = { firstStreamValues[^1], secondStreamValues[^1], thirdStreamValues[^1] };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        TEvent?[]? result = null;
        var listener = EventListener.Create<ReadOnlyMemory<TEvent?>>( e => result = e.ToArray() );
        var sut = new WhenAllEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        thirdStream.Dispose();

        foreach ( var e in secondStreamValues )
            secondStream.Publish( e );

        secondStream.Dispose();

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        firstStream.Dispose();

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                result.TestNotNull( r => r.TestSequence( expectedResult ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldEmitPartialResultContainingDefaultEventForInnerStreamThatDidNotEmitAnyEvent_WhenEventSourceIsDisposed()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 4 );
        var firstStreamValues = new[] { values[0], values[1] };
        var thirdStreamValues = new[] { values[2], values[3] };
        var expectedResult = new[] { firstStreamValues[^1], default, thirdStreamValues[^1] };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        TEvent?[]? result = null;
        var listener = EventListener.Create<ReadOnlyMemory<TEvent?>>( e => result = e.ToArray() );
        var sut = new WhenAllEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        thirdStream.Dispose();

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        firstStream.Dispose();

        sut.Dispose();

        Assertion.All(
                secondStream.HasSubscribers.TestFalse(),
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                result.TestNotNull( r => r.TestSequence( expectedResult ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldEmitPartialResultContainingLastEventsFromInnerStreams_WhenEventSourceIsDisposed()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        TEvent?[] expectedResult = { firstStreamValues[^1], secondStreamValues[^1], thirdStreamValues[^1] };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        TEvent?[]? result = null;
        var listener = EventListener.Create<ReadOnlyMemory<TEvent?>>( e => result = e.ToArray() );
        var sut = new WhenAllEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        foreach ( var e in secondStreamValues )
            secondStream.Publish( e );

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        sut.Dispose();

        Assertion.All(
                firstStream.HasSubscribers.TestFalse(),
                secondStream.HasSubscribers.TestFalse(),
                thirdStream.HasSubscribers.TestFalse(),
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                result.TestNotNull( r => r.TestSequence( expectedResult ) ) )
            .Go();
    }

    [Fact]
    public void WhenAll_ThenListen_ShouldEmitOnlyOnceEveryInnerStreamDisposesWithResultContainingTheirLastEventsInOrder()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        TEvent?[] expectedResult = { firstStreamValues[^1], secondStreamValues[^1], thirdStreamValues[^1] };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        TEvent?[]? result = null;
        var listener = EventListener.Create<ReadOnlyMemory<TEvent?>>( e => result = e.ToArray() );
        var sut = EventSource.WhenAll( firstStream, secondStream, thirdStream );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        thirdStream.Dispose();

        foreach ( var e in secondStreamValues )
            secondStream.Publish( e );

        secondStream.Dispose();

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        firstStream.Dispose();

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                result.TestNotNull( r => r.TestSequence( expectedResult ) ) )
            .Go();
    }
}
