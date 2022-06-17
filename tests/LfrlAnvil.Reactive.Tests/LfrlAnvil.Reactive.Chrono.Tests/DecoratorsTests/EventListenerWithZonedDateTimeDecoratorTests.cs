using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Decorators;
using LfrlAnvil.Reactive.Chrono.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Chrono.Tests.DecoratorsTests
{
    public class EventListenerWithZonedDateTimeDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var clock = Substitute.For<IZonedClock>();
            var next = Substitute.For<IEventListener<WithZonedDateTime<int>>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerWithZonedDateTimeDecorator<int>( clock );

            var _ = sut.Decorate( next, subscriber );

            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactAttachesTimestamp()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var dateTimes = sourceEvents.Select( (_, i) => ZonedDateTime.CreateUtc( new Timestamp( i ) ) ).ToList();
            var expectedEvents = new[]
            {
                new WithZonedDateTime<int>( 1, ZonedDateTime.CreateUtc( new Timestamp( 0 ) ) ),
                new WithZonedDateTime<int>( 2, ZonedDateTime.CreateUtc( new Timestamp( 1 ) ) ),
                new WithZonedDateTime<int>( 3, ZonedDateTime.CreateUtc( new Timestamp( 2 ) ) ),
                new WithZonedDateTime<int>( 5, ZonedDateTime.CreateUtc( new Timestamp( 3 ) ) ),
                new WithZonedDateTime<int>( 7, ZonedDateTime.CreateUtc( new Timestamp( 4 ) ) ),
                new WithZonedDateTime<int>( 11, ZonedDateTime.CreateUtc( new Timestamp( 5 ) ) ),
                new WithZonedDateTime<int>( 13, ZonedDateTime.CreateUtc( new Timestamp( 6 ) ) ),
                new WithZonedDateTime<int>( 17, ZonedDateTime.CreateUtc( new Timestamp( 7 ) ) ),
                new WithZonedDateTime<int>( 19, ZonedDateTime.CreateUtc( new Timestamp( 8 ) ) ),
                new WithZonedDateTime<int>( 23, ZonedDateTime.CreateUtc( new Timestamp( 9 ) ) )
            };

            var actualEvents = new List<WithZonedDateTime<int>>();

            var clock = Substitute.For<IZonedClock>();
            clock.GetNow().Returns( dateTimes );
            var next = EventListener.Create<WithZonedDateTime<int>>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerWithZonedDateTimeDecorator<int>( clock );
            var listener = sut.Decorate( next, subscriber );

            foreach ( var e in sourceEvents )
                listener.React( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
        {
            var clock = Substitute.For<IZonedClock>();
            var next = Substitute.For<IEventListener<WithZonedDateTime<int>>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerWithZonedDateTimeDecorator<int>( clock );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.VerifyCalls().Received( x => x.OnDispose( source ) );
        }

        [Fact]
        public void WithZonedDateTimeExtension_ShouldCreateEventStreamThatAttachesAndIndex()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var dateTimes = sourceEvents.Select( (_, i) => ZonedDateTime.CreateUtc( new Timestamp( i ) ) ).ToList();
            var expectedEvents = new[]
            {
                new WithZonedDateTime<int>( 1, ZonedDateTime.CreateUtc( new Timestamp( 0 ) ) ),
                new WithZonedDateTime<int>( 2, ZonedDateTime.CreateUtc( new Timestamp( 1 ) ) ),
                new WithZonedDateTime<int>( 3, ZonedDateTime.CreateUtc( new Timestamp( 2 ) ) ),
                new WithZonedDateTime<int>( 5, ZonedDateTime.CreateUtc( new Timestamp( 3 ) ) ),
                new WithZonedDateTime<int>( 7, ZonedDateTime.CreateUtc( new Timestamp( 4 ) ) ),
                new WithZonedDateTime<int>( 11, ZonedDateTime.CreateUtc( new Timestamp( 5 ) ) ),
                new WithZonedDateTime<int>( 13, ZonedDateTime.CreateUtc( new Timestamp( 6 ) ) ),
                new WithZonedDateTime<int>( 17, ZonedDateTime.CreateUtc( new Timestamp( 7 ) ) ),
                new WithZonedDateTime<int>( 19, ZonedDateTime.CreateUtc( new Timestamp( 8 ) ) ),
                new WithZonedDateTime<int>( 23, ZonedDateTime.CreateUtc( new Timestamp( 9 ) ) )
            };

            var actualEvents = new List<WithZonedDateTime<int>>();

            var clock = Substitute.For<IZonedClock>();
            clock.GetNow().Returns( dateTimes );
            var next = EventListener.Create<WithZonedDateTime<int>>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.WithZonedDateTime( clock );
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }
}
