using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Internal;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.MergeEventSourceTests;

public abstract class GenericMergeEventSourceTests<TEvent> : TestsBase
{
    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMaxConcurrencyIsLessThanOne(int maxConcurrency)
    {
        var action = Lambda.Of( () => new MergeEventSource<TEvent>( Enumerable.Empty<IEventStream<TEvent>>(), maxConcurrency ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( int.MaxValue )]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions(int maxConcurrency)
    {
        var inner = new EventPublisher<TEvent>();
        var sut = new MergeEventSource<TEvent>( new[] { inner }, maxConcurrency );
        sut.HasSubscribers.Should().BeFalse();
    }

    [Fact]
    public void Listen_ShouldNotEmitAnyEventsAndDisposeSubscriberImmediately_WhenInnerStreamsAreEmpty()
    {
        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new MergeEventSource<TEvent>( Array.Empty<IEventStream<TEvent>>(), maxConcurrency: 1 );

        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<TEvent>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldReturnDisposedSubscriber_WhenEventSourceIsDisposed()
    {
        var inner = new EventPublisher<TEvent>();
        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new MergeEventSource<TEvent>( new[] { inner }, maxConcurrency: 1 );
        sut.Dispose();

        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<TEvent>() ) );
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( int.MaxValue )]
    public void Listen_ShouldCreateActiveSubscriberThatDisposes_WhenEventSourceIsDisposed(int maxConcurrency)
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency );
        var subscriber = sut.Listen( listener );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<TEvent>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDoesNotEmitAnything_UntilFirstInnerStreamEmits_WithMaxConcurrencyEqualToOne()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency: 1 );
        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<TEvent>() ) );
        }
    }

    [Fact]
    public void
        Listen_ShouldCreateActiveSubscriberThatDoesNotEmitAnything_UntilFirstOrSecondInnerStreamEmits_WithMaxConcurrencyEqualToTwo()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency: 2 );
        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeFalse();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<TEvent>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDoesNotEmitAnything_UntilAnyInnerStreamEmits_WithMaxConcurrencyEqualToMax()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency: int.MaxValue );
        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<TEvent>() ) );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatEmitsOnlyFirstInnerStreamEvents_WithMaxConcurrencyEqualToOne()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        var expectedEvents = firstStreamValues;

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var actualEvents = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualEvents.Add );
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency: 1 );
        var subscriber = sut.Listen( listener );

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        foreach ( var e in secondStreamValues )
            secondStream.Publish( e );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatEmitsOnlyFirstOrSecondInnerStreamEvents_WithMaxConcurrencyEqualToTwo()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        var expectedEvents = new[] { firstStreamValues[0], secondStreamValues[0], secondStreamValues[1], firstStreamValues[1] };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var actualEvents = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualEvents.Add );
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency: 2 );
        var subscriber = sut.Listen( listener );

        firstStream.Publish( firstStreamValues[0] );
        secondStream.Publish( secondStreamValues[0] );
        secondStream.Publish( secondStreamValues[1] );
        firstStream.Publish( firstStreamValues[1] );

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeFalse();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatEmitsAllInnerStreamEvents_WithMaxConcurrencyEqualToMax()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        var expectedEvents = new[]
        {
            firstStreamValues[0],
            secondStreamValues[0],
            thirdStreamValues[0],
            secondStreamValues[1],
            thirdStreamValues[1],
            firstStreamValues[1]
        };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var actualEvents = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualEvents.Add );
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency: int.MaxValue );
        var subscriber = sut.Listen( listener );

        firstStream.Publish( firstStreamValues[0] );
        secondStream.Publish( secondStreamValues[0] );
        thirdStream.Publish( thirdStreamValues[0] );
        secondStream.Publish( secondStreamValues[1] );
        thirdStream.Publish( thirdStreamValues[1] );
        firstStream.Publish( firstStreamValues[1] );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }

    [Fact]
    public void
        Listen_ShouldCreateActiveSubscriberThatStartsListeningToNextStream_WhenActiveStreamDisposes_WithMaxConcurrencyEqualToOne()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        var expectedEvents = firstStreamValues.Concat( secondStreamValues ).Concat( thirdStreamValues );

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var actualEvents = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualEvents.Add );
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency: 1 );
        var subscriber = sut.Listen( listener );

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        firstStream.Dispose();

        foreach ( var e in secondStreamValues )
            secondStream.Publish( e );

        secondStream.Dispose();

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }

    [Fact]
    public void
        Listen_ShouldCreateActiveSubscriberThatStartsListeningToNextStream_WhenActiveStreamDisposes_WithMaxConcurrencyEqualToTwo()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        var expectedEvents = new[]
        {
            firstStreamValues[0],
            secondStreamValues[0],
            secondStreamValues[1],
            firstStreamValues[1],
            thirdStreamValues[0],
            thirdStreamValues[1]
        };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var actualEvents = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualEvents.Add );
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency: 2 );
        var subscriber = sut.Listen( listener );

        firstStream.Publish( firstStreamValues[0] );
        secondStream.Publish( secondStreamValues[0] );
        secondStream.Publish( secondStreamValues[1] );
        firstStream.Publish( firstStreamValues[1] );

        firstStream.Dispose();

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( int.MaxValue )]
    public void Listen_ShouldCreateActiveSubscriberThatDisposes_WhenLastInnerStreamDisposesInOrder(int maxConcurrency)
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency );
        var subscriber = sut.Listen( listener );

        firstStream.Dispose();
        secondStream.Dispose();
        thirdStream.Dispose();

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatStartsListeningToSecondInnerStream_WhenFirstInnerStreamIsDisposed()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        firstStream.Dispose();

        var actualEvents = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualEvents.Add );
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency: 1 );
        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeFalse();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatStartsListeningToThirdInnerStream_WhenFirstAndSecondInnerStreamsAreDisposed()
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        firstStream.Dispose();
        secondStream.Dispose();

        var actualEvents = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualEvents.Add );
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency: 2 );
        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( int.MaxValue )]
    public void Listen_ShouldCreateDisposedSubscriber_WhenAllInnerStreamsAreDisposed(int maxConcurrency)
    {
        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        firstStream.Dispose();
        secondStream.Dispose();
        thirdStream.Dispose();

        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new MergeEventSource<TEvent>( new[] { firstStream, secondStream, thirdStream }, maxConcurrency );
        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeFalse();
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void Merge_ThenListen_ShouldCreateActiveSubscriberThatEmitsAllInnerStreamEvents()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        var expectedEvents = new[]
        {
            firstStreamValues[0],
            secondStreamValues[0],
            thirdStreamValues[0],
            secondStreamValues[1],
            thirdStreamValues[1],
            firstStreamValues[1]
        };

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var actualEvents = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualEvents.Add );
        var sut = EventSource.Merge( firstStream, secondStream, thirdStream );
        var subscriber = sut.Listen( listener );

        firstStream.Publish( firstStreamValues[0] );
        secondStream.Publish( secondStreamValues[0] );
        thirdStream.Publish( thirdStreamValues[0] );
        secondStream.Publish( secondStreamValues[1] );
        thirdStream.Publish( thirdStreamValues[1] );
        firstStream.Publish( firstStreamValues[1] );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeTrue();
            secondStream.HasSubscribers.Should().BeTrue();
            thirdStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }

    [Fact]
    public void Concat_ThenListen_ShouldCreateActiveSubscriberThatStartsListeningToNextStream_WhenActiveStreamDisposes()
    {
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 6 );
        var firstStreamValues = new[] { values[0], values[1] };
        var secondStreamValues = new[] { values[2], values[3] };
        var thirdStreamValues = new[] { values[4], values[5] };
        var expectedEvents = firstStreamValues.Concat( secondStreamValues ).Concat( thirdStreamValues );

        var firstStream = new EventPublisher<TEvent>();
        var secondStream = new EventPublisher<TEvent>();
        var thirdStream = new EventPublisher<TEvent>();

        var actualEvents = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualEvents.Add );
        var sut = EventSource.Concat( firstStream, secondStream, thirdStream );
        var subscriber = sut.Listen( listener );

        foreach ( var e in firstStreamValues )
            firstStream.Publish( e );

        firstStream.Dispose();

        foreach ( var e in secondStreamValues )
            secondStream.Publish( e );

        secondStream.Dispose();

        foreach ( var e in thirdStreamValues )
            thirdStream.Publish( e );

        using ( new AssertionScope() )
        {
            firstStream.HasSubscribers.Should().BeFalse();
            secondStream.HasSubscribers.Should().BeFalse();
            thirdStream.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }
}