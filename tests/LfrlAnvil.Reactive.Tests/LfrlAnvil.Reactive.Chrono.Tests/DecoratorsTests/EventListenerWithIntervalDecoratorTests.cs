using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Decorators;
using LfrlAnvil.Reactive.Chrono.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Chrono.Tests.DecoratorsTests
{
    public class EventListenerWithIntervalDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var timestampProvider = Substitute.For<ITimestampProvider>();
            var next = Substitute.For<IEventListener<WithInterval<int>>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerWithIntervalDecorator<int>( timestampProvider );

            var _ = sut.Decorate( next, subscriber );

            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactAttachesTimestamp()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var timestamps = sourceEvents.Select( e => new Timestamp( e * 2 ) ).ToList();
            var expectedEvents = new[]
            {
                new WithInterval<int>( 1, new Timestamp( 2 ), Duration.FromTicks( -1 ) ),
                new WithInterval<int>( 2, new Timestamp( 4 ), Duration.FromTicks( 2 ) ),
                new WithInterval<int>( 3, new Timestamp( 6 ), Duration.FromTicks( 2 ) ),
                new WithInterval<int>( 5, new Timestamp( 10 ), Duration.FromTicks( 4 ) ),
                new WithInterval<int>( 7, new Timestamp( 14 ), Duration.FromTicks( 4 ) ),
                new WithInterval<int>( 11, new Timestamp( 22 ), Duration.FromTicks( 8 ) ),
                new WithInterval<int>( 13, new Timestamp( 26 ), Duration.FromTicks( 4 ) ),
                new WithInterval<int>( 17, new Timestamp( 34 ), Duration.FromTicks( 8 ) ),
                new WithInterval<int>( 19, new Timestamp( 38 ), Duration.FromTicks( 4 ) ),
                new WithInterval<int>( 23, new Timestamp( 46 ), Duration.FromTicks( 8 ) )
            };

            var actualEvents = new List<WithInterval<int>>();

            var timestampProvider = Substitute.For<ITimestampProvider>();
            timestampProvider.GetNow().Returns( timestamps[0], timestamps.Skip( 1 ).ToArray() );
            var next = EventListener.Create<WithInterval<int>>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerWithIntervalDecorator<int>( timestampProvider );
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
            var timestampProvider = Substitute.For<ITimestampProvider>();
            var next = Substitute.For<IEventListener<WithInterval<int>>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerWithIntervalDecorator<int>( timestampProvider );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.VerifyCalls().Received( x => x.OnDispose( source ) );
        }

        [Fact]
        public void WithIndexExtension_ShouldCreateEventStreamThatAttachesAndIndex()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var timestamps = sourceEvents.Select( e => new Timestamp( e * 2 ) ).ToList();
            var expectedEvents = new[]
            {
                new WithInterval<int>( 1, new Timestamp( 2 ), Duration.FromTicks( -1 ) ),
                new WithInterval<int>( 2, new Timestamp( 4 ), Duration.FromTicks( 2 ) ),
                new WithInterval<int>( 3, new Timestamp( 6 ), Duration.FromTicks( 2 ) ),
                new WithInterval<int>( 5, new Timestamp( 10 ), Duration.FromTicks( 4 ) ),
                new WithInterval<int>( 7, new Timestamp( 14 ), Duration.FromTicks( 4 ) ),
                new WithInterval<int>( 11, new Timestamp( 22 ), Duration.FromTicks( 8 ) ),
                new WithInterval<int>( 13, new Timestamp( 26 ), Duration.FromTicks( 4 ) ),
                new WithInterval<int>( 17, new Timestamp( 34 ), Duration.FromTicks( 8 ) ),
                new WithInterval<int>( 19, new Timestamp( 38 ), Duration.FromTicks( 4 ) ),
                new WithInterval<int>( 23, new Timestamp( 46 ), Duration.FromTicks( 8 ) )
            };

            var actualEvents = new List<WithInterval<int>>();

            var timestampProvider = Substitute.For<ITimestampProvider>();
            timestampProvider.GetNow().Returns( timestamps[0], timestamps.Skip( 1 ).ToArray() );
            var next = EventListener.Create<WithInterval<int>>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.WithInterval( timestampProvider );
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }
}
