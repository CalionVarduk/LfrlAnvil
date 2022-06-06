using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.HistoryEventPublisherTests
{
    public abstract class GenericHistoryEventPublisherTests<TEvent> : TestsBase
    {
        [Theory]
        [InlineData( 1 )]
        [InlineData( 5 )]
        [InlineData( 10 )]
        public void Ctor_ShouldCreateWithEmptyHistoryAndCorrectCapacity(int capacity)
        {
            var sut = new HistoryEventPublisher<TEvent>( capacity );

            using ( new AssertionScope() )
            {
                sut.Subscribers.Should().BeEmpty();
                sut.History.Should().BeEmpty();
                sut.Capacity.Should().Be( capacity );
            }
        }

        [Theory]
        [InlineData( -1 )]
        [InlineData( 0 )]
        public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCapacityIsLessThanOne(int capacity)
        {
            var action = Lambda.Of( () => new HistoryEventPublisher<TEvent>( capacity ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Publish_ShouldAddFirstEventToHistory()
        {
            var @event = Fixture.Create<TEvent>();
            var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );

            sut.Publish( @event );

            sut.History.Should().BeSequentiallyEqualTo( @event );
        }

        [Fact]
        public void Publish_ShouldAddNextEventAsLastEntryToHistory()
        {
            var events = Fixture.CreateDistinctCollection<TEvent>( count: 3 );
            var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );

            foreach ( var @event in events )
                sut.Publish( @event );

            sut.History.Should().BeSequentiallyEqualTo( events );
        }

        [Fact]
        public void Publish_ShouldAddNextEventAsLastEntryToHistoryAndRemoveFirstEntry_WhenCapacityIsExceeded()
        {
            var events = Fixture.CreateDistinctCollection<TEvent>( count: 3 );
            var sut = new HistoryEventPublisher<TEvent>( capacity: 2 );

            foreach ( var @event in events )
                sut.Publish( @event );

            sut.History.Should().BeSequentiallyEqualTo( events[1], events[2] );
        }

        [Fact]
        public void Publish_ShouldCallListenerReact()
        {
            var @event = Fixture.Create<TEvent>();
            var listener = Substitute.For<IEventListener<TEvent>>();
            var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );
            sut.Listen( listener );

            sut.Publish( @event );

            listener.VerifyCalls().Received( x => x.React( @event ) );
        }

        [Fact]
        public void Listen_ShouldCallListenerReactImmediatelyForEachHistoryEntry()
        {
            var events = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
            var listener = Substitute.For<IEventListener<TEvent>>();
            var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );
            foreach ( var @event in events )
                sut.Publish( @event );

            sut.Listen( listener );

            listener.VerifyCalls().Received( x => x.React( events[0] ) );
            listener.VerifyCalls().Received( x => x.React( events[1] ) );
        }

        [Fact]
        public void Listen_ShouldCallImmediatelyListenerReactOnlyForTheFirstHistoryEntry_WhenFirstListenerReactDisposedTheSubscriber()
        {
            var events = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
            var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );
            var listener = Substitute.For<IEventListener<TEvent>>();
            listener.When( l => l.React( events[0] ) ).Do( _ => sut.Subscribers.First().Dispose() );
            foreach ( var @event in events )
                sut.Publish( @event );

            var subscriber = sut.Listen( listener );

            using ( new AssertionScope() )
            {
                subscriber.IsDisposed.Should().BeTrue();
                listener.VerifyCalls().Received( x => x.React( events[0] ) );
                listener.VerifyCalls().DidNotReceive( x => x.React( events[1] ) );
            }
        }

        [Fact]
        public void ClearHistory_ShouldRemoveAllHistoryEntries()
        {
            var @event = Fixture.Create<TEvent>();
            var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );
            sut.Publish( @event );

            sut.ClearHistory();

            sut.History.Should().BeEmpty();
        }

        [Fact]
        public void Dispose_ShouldClearHistory()
        {
            var events = Fixture.CreateDistinctCollection<TEvent>( count: 3 );
            var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );
            foreach ( var @event in events )
                sut.Publish( @event );

            sut.Dispose();

            using ( new AssertionScope() )
            {
                sut.IsDisposed.Should().BeTrue();
                sut.History.Should().BeEmpty();
            }
        }
    }
}
