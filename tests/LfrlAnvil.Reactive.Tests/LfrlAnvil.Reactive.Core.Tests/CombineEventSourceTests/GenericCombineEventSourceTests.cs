using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.CombineEventSourceTests;

public abstract class GenericCombineEventSourceTests<TEvent> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions()
    {
        var inner = new EventPublisher<TEvent>();
        var sut = new CombineEventSource<TEvent>( new[] { inner } );
        sut.HasSubscribers.Should().BeFalse();
    }

    [Fact]
    public void Listen_ShouldNotEmitAnyEventsAndDisposeSubscriberImmediately_WhenInnerStreamsAreEmpty()
    {
        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( Array.Empty<IEventStream<TEvent>>() );

        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldReturnDisposedSubscriber_WhenEventSourceIsDisposed()
    {
        var inner = new EventPublisher<TEvent>();
        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( new[] { inner } );
        sut.Dispose();

        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) );
        }
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) );
        }
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

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDoesNotEmitAnything_UntilAllInnerStreamsEmitAtLeastOnce()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 4 );
        var firstStreamValues = new[] { values[0], values[1] };
        var thirdStreamValues = new[] { values[2], values[3] };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) );
        }
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

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) );
        }
    }

    [Fact]
    public void
        Listen_ShouldCreateActiveSubscriberThatDisposes_WhenAtLeastOneInnerStreamDisposesAndNotAllInnerStreamsEmittedAtLeastOnce()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 4 );
        var firstStreamValues = new[] { values[0], values[1] };
        var thirdStreamValues = new[] { values[2], values[3] };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<ReadOnlyMemory<TEvent>>>();
        var sut = new CombineEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream } );
        var subscriber = sut.Listen( listener );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        firstStream.Dispose();

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<ReadOnlyMemory<TEvent>>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldEmitEventContainingLastInnerStreamEventsEveryTimeInnerStreamEmits_WhenAllInnerStreamsEmittedAtLeastOnce()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 9 );
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeTrue();
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();

            result.Should().HaveCount( expectedResult.Length );
            for ( var i = 0; i < result.Count; ++i )
                result[i].Should().BeSequentiallyEqualTo( expectedResult[i] );
        }
    }

    [Fact]
    public void
        Listen_ShouldCreateActiveSubscriberThatDoesNotDispose_WhenAllInnerStreamsEmittedAtLeastOnceAndNotAllInnerStreamsDispose()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 4 );
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeTrue();
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeFalse();

            result.Should().HaveCount( expectedResult.Length );
            for ( var i = 0; i < result.Count; ++i )
                result[i].Should().BeSequentiallyEqualTo( expectedResult[i] );
        }
    }

    [Fact]
    public void
        Listen_ShouldCreateActiveSubscriberThatDisposes_WhenAllInnerStreamsEmittedAtLeastOnceAndAllInnerStreamsDispose()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 3 );
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            result.Should().HaveCount( 1 ).And.Subject.First().Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void
        Combine_ThenListen_ShouldEmitEventContainingLastInnerStreamEventsEveryTimeInnerStreamEmits_WhenAllInnerStreamsEmittedAtLeastOnce()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 9 );
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeTrue();
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();

            result.Should().HaveCount( expectedResult.Length );
            for ( var i = 0; i < result.Count; ++i )
                result[i].Should().BeSequentiallyEqualTo( expectedResult[i] );
        }
    }
}
