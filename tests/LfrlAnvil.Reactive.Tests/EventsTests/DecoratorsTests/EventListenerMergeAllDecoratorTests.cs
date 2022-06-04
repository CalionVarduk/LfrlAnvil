using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Events;
using LfrlAnvil.Reactive.Events.Decorators;
using LfrlAnvil.Reactive.Events.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.EventsTests.DecoratorsTests
{
    public class EventListenerMergeAllDecoratorTests : TestsBase
    {
        [Theory]
        [InlineData( -1 )]
        [InlineData( 0 )]
        public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMaxConcurrencyIsLessThanOne(int maxConcurrency)
        {
            var action = Lambda.Of( () => new EventListenerMergeAllDecorator<int>( maxConcurrency ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
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

            var _ = sut.Decorate( next, subscriber );

            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
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

            using ( new AssertionScope() )
            {
                innerStreams.Take( maxConcurrency ).Should().OnlyContain( s => s.HasSubscribers );
                innerStreams.Skip( maxConcurrency ).Should().BeEmptyOrOnlyContain( s => ! s.HasSubscribers );
                next.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<int>() ) );
            }
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

            using ( new AssertionScope() )
            {
                innerStreams[0].HasSubscribers.Should().BeTrue();
                innerStreams[1].HasSubscribers.Should().BeFalse();
                innerStreams[2].HasSubscribers.Should().BeFalse();
                actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
            }
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

            using ( new AssertionScope() )
            {
                innerStreams[0].HasSubscribers.Should().BeTrue();
                innerStreams[1].HasSubscribers.Should().BeTrue();
                innerStreams[2].HasSubscribers.Should().BeFalse();
                actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
            }
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

            using ( new AssertionScope() )
            {
                innerStreams[0].HasSubscribers.Should().BeTrue();
                innerStreams[1].HasSubscribers.Should().BeTrue();
                innerStreams[2].HasSubscribers.Should().BeTrue();
                actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
            }
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

            using ( new AssertionScope() )
            {
                innerStreams[0].HasSubscribers.Should().BeFalse();
                innerStreams[1].HasSubscribers.Should().BeFalse();
                innerStreams[2].HasSubscribers.Should().BeTrue();
                actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
            }
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

            using ( new AssertionScope() )
            {
                innerStreams[0].HasSubscribers.Should().BeFalse();
                innerStreams[1].HasSubscribers.Should().BeTrue();
                innerStreams[2].HasSubscribers.Should().BeTrue();
                actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
            }
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

            using ( new AssertionScope() )
            {
                innerStreams.Should().OnlyContain( s => ! s.HasSubscribers );
                subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
            }
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

            using ( new AssertionScope() )
            {
                innerStreams[0].HasSubscribers.Should().BeFalse();
                innerStreams[1].HasSubscribers.Should().BeTrue();
                innerStreams[2].HasSubscribers.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                innerStreams[0].HasSubscribers.Should().BeFalse();
                innerStreams[1].HasSubscribers.Should().BeFalse();
                innerStreams[2].HasSubscribers.Should().BeTrue();
            }
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

            using ( new AssertionScope() )
            {
                innerStreams.Should().OnlyContain( s => ! s.HasSubscribers );
                subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
            }
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

            innerStreams.Should().OnlyContain( s => ! s.HasSubscribers );
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

            next.VerifyCalls().Received( x => x.OnDispose( source ) );
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

            using ( new AssertionScope() )
            {
                innerStreams.Should().OnlyContain( s => s.HasSubscribers );
                actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
            }
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

            using ( new AssertionScope() )
            {
                innerStreams[0].HasSubscribers.Should().BeFalse();
                innerStreams[1].HasSubscribers.Should().BeFalse();
                innerStreams[2].HasSubscribers.Should().BeTrue();
                actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
            }
        }
    }
}
