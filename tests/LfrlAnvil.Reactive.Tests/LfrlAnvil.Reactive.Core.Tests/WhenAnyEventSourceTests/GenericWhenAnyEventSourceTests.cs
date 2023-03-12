using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.WhenAnyEventSourceTests;

public abstract class GenericWhenAnyEventSourceTests<TEvent> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions()
    {
        var inner = new EventPublisher<TEvent>();
        var sut = new WhenAnyEventSource<TEvent>( new[] { inner } );
        sut.HasSubscribers.Should().BeFalse();
    }

    [Fact]
    public void Listen_ShouldNotEmitAnyEventsAndDisposeSubscriberImmediately_WhenInnerStreamsAreEmpty()
    {
        var listener = Substitute.For<IEventListener<WithIndex<TEvent>>>();
        var sut = new WhenAnyEventSource<TEvent>( Array.Empty<IEventStream<TEvent>>() );

        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<WithIndex<TEvent>>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldReturnDisposedSubscriber_WhenEventSourceIsDisposed()
    {
        var inner = new EventPublisher<TEvent>();
        var listener = Substitute.For<IEventListener<WithIndex<TEvent>>>();
        var sut = new WhenAnyEventSource<TEvent>( new[] { inner } );
        sut.Dispose();

        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<WithIndex<TEvent>>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDoesNotEmitAnything_UntilAnyInnerStreamEmits()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<WithIndex<TEvent>>>();
        var sut = new WhenAnyEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<WithIndex<TEvent>>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDisposes_WhenEventSourceIsDisposed()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<WithIndex<TEvent>>>();
        var sut = new WhenAnyEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<WithIndex<TEvent>>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDoesNotDispose_UntilAllInnerStreamsDispose()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<WithIndex<TEvent>>>();
        var sut = new WhenAnyEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        firstStream.Dispose();
        thirdStream.Dispose();

        using ( new AssertionScope() )
        {
            secondStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<WithIndex<TEvent>>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldCreateListenerThatDisposesWithoutEmittingAnyEvent_WhenAllInnerStreamsDisposeBeforeEmittingAnything()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<WithIndex<TEvent>>>();
        var sut = new WhenAnyEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        firstStream.Dispose();
        secondStream.Dispose();
        thirdStream.Dispose();

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<WithIndex<TEvent>>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldEmitOnlyOnceAnyInnerStreamEmitsWithResultContainingTheirFirstEventWithCorrectIndex()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        var expectedResult = new WithIndex<TEvent>( thirdStreamValues[0], 2 );

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var result = new List<WithIndex<TEvent>>();
        var listener = EventListener.Create<WithIndex<TEvent>>( result.Add );
        var sut = new WhenAnyEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        foreach ( var e in secondStreamValues )
            secondStream.Publish( e );

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            result.Should().HaveCount( 1 ).And.Subject.First().Should().BeEquivalentTo( expectedResult );
        }
    }

    [Fact]
    public void Listen_ShouldEmitOnlyOnceFirstInnerStreamImmediatelyEmitsWithResultContainingTheirFirstEventWithCorrectIndex()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        var expectedResult = new WithIndex<TEvent>( firstStreamValues[0], 0 );

        var firstStream = new HistoryEventPublisher<TEvent>( capacity: 2 );
        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var result = new List<WithIndex<TEvent>>();
        var listener = EventListener.Create<WithIndex<TEvent>>( result.Add );
        var sut = new WhenAnyEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        foreach ( var e in secondStreamValues )
            secondStream.Publish( e );

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            result.Should().HaveCount( 1 ).And.Subject.First().Should().BeEquivalentTo( expectedResult );
        }
    }

    [Fact]
    public void WhenAny_ThenListen_ShouldEmitOnlyOnceAnyInnerStreamEmitsWithResultContainingTheirFirstEventWithCorrectIndex()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        var expectedResult = new WithIndex<TEvent>( thirdStreamValues[0], 2 );

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var result = new List<WithIndex<TEvent>>();
        var listener = EventListener.Create<WithIndex<TEvent>>( result.Add );
        var sut = EventSource.WhenAny( firstStream, secondStream, thirdStream );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        foreach ( var e in secondStreamValues )
            secondStream.Publish( e );

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            result.Should().HaveCount( 1 ).And.Subject.First().Should().BeEquivalentTo( expectedResult );
        }
    }
}
