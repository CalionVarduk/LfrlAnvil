using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerMergeAllDecoratorTests : TestsBase
{
    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMaxConcurrencyIsLessThanOne(int maxConcurrency)
    {
        var action = Lambda.Of( () => new EventListenerMergeAllDecorator<int>( maxConcurrency ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( int.MaxValue )]
    public void Decorate_ShouldNotDisposeTheSubscriber(int maxConcurrency)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( int.MaxValue )]
    public void Decorate_ShouldCreateListenerWhoseReactInitiatesInnerStreamSubscribersInOrder(int maxConcurrency)
    {
        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        Assertion.All(
                innerStreams.Take( maxConcurrency ).TestAll( (s, _) => s.HasSubscribers.TestTrue() ),
                innerStreams.Skip( maxConcurrency ).TestAll( (s, _) => s.HasSubscribers.TestFalse() ),
                next.TestDidNotReceiveCall( x => x.React( Arg.Any<int>() ) ) )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatForwardsFirstInnerStreamEvents_WithMaxConcurrencyEqualToOne()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = firstStreamValues;
        var actualEvents = new List<int>();

        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency: 1 );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        foreach ( var e in firstStreamValues )
            innerStreams[0].Publish( e );

        foreach ( var e in secondStreamValues )
            innerStreams[1].Publish( e );

        foreach ( var e in thirdStreamValues )
            innerStreams[2].Publish( e );

        Assertion.All(
                innerStreams[0].HasSubscribers.TestTrue(),
                innerStreams[1].HasSubscribers.TestFalse(),
                innerStreams[2].HasSubscribers.TestFalse(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatForwardsFirstOrSecondInnerStreamEvents_WithMaxConcurrencyEqualToTwo()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = new[] { firstStreamValues[0], secondStreamValues[0], secondStreamValues[1], firstStreamValues[1] };
        var actualEvents = new List<int>();

        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency: 2 );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        innerStreams[0].Publish( firstStreamValues[0] );
        innerStreams[1].Publish( secondStreamValues[0] );
        innerStreams[1].Publish( secondStreamValues[1] );
        innerStreams[0].Publish( firstStreamValues[1] );

        foreach ( var e in thirdStreamValues )
            innerStreams[2].Publish( e );

        Assertion.All(
                innerStreams[0].HasSubscribers.TestTrue(),
                innerStreams[1].HasSubscribers.TestTrue(),
                innerStreams[2].HasSubscribers.TestFalse(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatForwardsAnyInnerStreamEvents_WithMaxConcurrencyEqualToMax()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = new[]
        {
            firstStreamValues[0],
            secondStreamValues[0],
            thirdStreamValues[0],
            secondStreamValues[1],
            thirdStreamValues[1],
            firstStreamValues[1]
        };

        var actualEvents = new List<int>();

        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency: int.MaxValue );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        innerStreams[0].Publish( firstStreamValues[0] );
        innerStreams[1].Publish( secondStreamValues[0] );
        innerStreams[2].Publish( thirdStreamValues[0] );
        innerStreams[1].Publish( secondStreamValues[1] );
        innerStreams[2].Publish( thirdStreamValues[1] );
        innerStreams[0].Publish( firstStreamValues[1] );

        Assertion.All(
                innerStreams[0].HasSubscribers.TestTrue(),
                innerStreams[1].HasSubscribers.TestTrue(),
                innerStreams[2].HasSubscribers.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void
        Decorate_ShouldCreateListenerThatStartsListeningToNextInnerStream_WhenActiveStreamDisposes_WithMaxConcurrencyEqualToOne()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = firstStreamValues.Concat( secondStreamValues ).Concat( thirdStreamValues );
        var actualEvents = new List<int>();

        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency: 1 );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        foreach ( var e in firstStreamValues )
            innerStreams[0].Publish( e );

        innerStreams[0].Dispose();

        foreach ( var e in secondStreamValues )
            innerStreams[1].Publish( e );

        innerStreams[1].Dispose();

        foreach ( var e in thirdStreamValues )
            innerStreams[2].Publish( e );

        Assertion.All(
                innerStreams[0].HasSubscribers.TestFalse(),
                innerStreams[1].HasSubscribers.TestFalse(),
                innerStreams[2].HasSubscribers.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void
        Decorate_ShouldCreateListenerThatStartsListeningToNextInnerStream_WhenActiveStreamDisposes_WithMaxConcurrencyEqualToTwo()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = new[]
        {
            firstStreamValues[0],
            secondStreamValues[0],
            secondStreamValues[1],
            firstStreamValues[1],
            thirdStreamValues[0],
            thirdStreamValues[1]
        };

        var actualEvents = new List<int>();

        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency: 2 );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        innerStreams[0].Publish( firstStreamValues[0] );
        innerStreams[1].Publish( secondStreamValues[0] );
        innerStreams[1].Publish( secondStreamValues[1] );
        innerStreams[0].Publish( firstStreamValues[1] );

        innerStreams[0].Dispose();

        foreach ( var e in thirdStreamValues )
            innerStreams[2].Publish( e );

        Assertion.All(
                innerStreams[0].HasSubscribers.TestFalse(),
                innerStreams[1].HasSubscribers.TestTrue(),
                innerStreams[2].HasSubscribers.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( int.MaxValue )]
    public void Decorate_ShouldCreateListenerThatDisposesInnerSubscribers_WhenInnerStreamsDispose(int maxConcurrency)
    {
        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        foreach ( var stream in innerStreams )
            stream.Dispose();

        Assertion.All(
                innerStreams.TestAll( (s, _) => s.HasSubscribers.TestFalse() ),
                subscriber.TestDidNotReceiveCall( x => x.Dispose() ) )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatStartsListeningToSecondInnerStream_WhenFirstInnerStreamIsDisposed()
    {
        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };
        innerStreams[0].Dispose();

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency: 1 );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        Assertion.All(
                innerStreams[0].HasSubscribers.TestFalse(),
                innerStreams[1].HasSubscribers.TestTrue(),
                innerStreams[2].HasSubscribers.TestFalse() )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatStartsListeningToThirdInnerStream_WhenFirstAndSecondInnerStreamsAreDisposed()
    {
        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };
        innerStreams[0].Dispose();
        innerStreams[1].Dispose();

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency: 2 );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        Assertion.All(
                innerStreams[0].HasSubscribers.TestFalse(),
                innerStreams[1].HasSubscribers.TestFalse(),
                innerStreams[2].HasSubscribers.TestTrue() )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( int.MaxValue )]
    public void Decorate_ShouldCreateListenerThatDoesNotDispose_WhenAllInnerStreamsAreDisposed(int maxConcurrency)
    {
        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };
        foreach ( var stream in innerStreams )
            stream.Dispose();

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        Assertion.All(
                innerStreams.TestAll( (s, _) => s.HasSubscribers.TestFalse() ),
                subscriber.TestDidNotReceiveCall( x => x.Dispose() ) )
            .Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource, 1 )]
    [InlineData( DisposalSource.EventSource, 2 )]
    [InlineData( DisposalSource.EventSource, int.MaxValue )]
    [InlineData( DisposalSource.Subscriber, 1 )]
    [InlineData( DisposalSource.Subscriber, 2 )]
    [InlineData( DisposalSource.Subscriber, int.MaxValue )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeDisposesAllInnerStreamSubscribers(DisposalSource source, int maxConcurrency)
    {
        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var stream in innerStreams )
            listener.React( stream );

        subscriber.IsDisposed.Returns( true );
        listener.OnDispose( source );

        innerStreams.TestAll( (s, _) => s.HasSubscribers.TestFalse() ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerMergeAllDecorator<int>( maxConcurrency: 1 );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void MergeAllExtension_ShouldCreateEventStreamThatForwardsAnyInnerStreamEvents()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = new[]
        {
            firstStreamValues[0],
            secondStreamValues[0],
            thirdStreamValues[0],
            secondStreamValues[1],
            thirdStreamValues[1],
            firstStreamValues[1]
        };

        var actualEvents = new List<int>();

        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<IEventStream<int>>();
        var decorated = sut.MergeAll();
        decorated.Listen( next );

        foreach ( var stream in innerStreams )
            sut.Publish( stream );

        innerStreams[0].Publish( firstStreamValues[0] );
        innerStreams[1].Publish( secondStreamValues[0] );
        innerStreams[2].Publish( thirdStreamValues[0] );
        innerStreams[1].Publish( secondStreamValues[1] );
        innerStreams[2].Publish( thirdStreamValues[1] );
        innerStreams[0].Publish( firstStreamValues[1] );

        Assertion.All(
                innerStreams.TestAll( (s, _) => s.HasSubscribers.TestTrue() ),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void ConcatAllExtension_ShouldCreateEventStreamThatStartsListeningToNextInnerStream_WhenActiveStreamDisposes()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = firstStreamValues.Concat( secondStreamValues ).Concat( thirdStreamValues );
        var actualEvents = new List<int>();

        var innerStreams = new[] { new EventPublisher<int>(), new EventPublisher<int>(), new EventPublisher<int>() };

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<IEventStream<int>>();
        var decorated = sut.ConcatAll();
        decorated.Listen( next );

        foreach ( var stream in innerStreams )
            sut.Publish( stream );

        foreach ( var e in firstStreamValues )
            innerStreams[0].Publish( e );

        innerStreams[0].Dispose();

        foreach ( var e in secondStreamValues )
            innerStreams[1].Publish( e );

        innerStreams[1].Dispose();

        foreach ( var e in thirdStreamValues )
            innerStreams[2].Publish( e );

        Assertion.All(
                innerStreams[0].HasSubscribers.TestFalse(),
                innerStreams[1].HasSubscribers.TestFalse(),
                innerStreams[2].HasSubscribers.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }
}
