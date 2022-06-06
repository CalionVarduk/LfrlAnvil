using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests
{
    public class EventListenerGroupByDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var next = Substitute.For<IEventListener<EventGrouping<int, int>>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerGroupByDecorator<int, int>( e => e / 10, EqualityComparer<int>.Default );

            var _ = sut.Decorate( next, subscriber );

            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
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

            using ( new AssertionScope() )
            {
                actualEvents.Should().HaveCount( expectedEvents.Length );
                for ( var i = 0; i < actualEvents.Count; ++i )
                {
                    actualEvents[i].Key.Should().Be( expectedEvents[i].Key );
                    actualEvents[i].Event.Should().Be( expectedEvents[i].Event );
                    actualEvents[i].AllEvents.Should().BeSequentiallyEqualTo( expectedEvents[i].AllEvents );
                }
            }
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

            next.VerifyCalls().Received( x => x.OnDispose( source ) );
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

            using ( new AssertionScope() )
            {
                actualEvents.Should().HaveCount( expectedEvents.Length );
                for ( var i = 0; i < actualEvents.Count; ++i )
                {
                    actualEvents[i].Key.Should().Be( expectedEvents[i].Key );
                    actualEvents[i].Event.Should().Be( expectedEvents[i].Event );
                    actualEvents[i].AllEvents.Should().BeSequentiallyEqualTo( expectedEvents[i].AllEvents );
                }
            }
        }
    }
}
