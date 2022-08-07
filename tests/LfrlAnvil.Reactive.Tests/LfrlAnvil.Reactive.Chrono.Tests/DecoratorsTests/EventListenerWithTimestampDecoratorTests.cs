using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Decorators;
using LfrlAnvil.Reactive.Chrono.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Reactive.Chrono.Tests.DecoratorsTests;

public class EventListenerWithTimestampDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var timestampProvider = Substitute.For<ITimestampProvider>();
        var next = Substitute.For<IEventListener<WithTimestamp<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerWithTimestampDecorator<int>( timestampProvider );

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactAttachesTimestamp()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var timestamps = sourceEvents.Select( (_, i) => new Timestamp( i ) ).ToList();
        var expectedEvents = new[]
        {
            new WithTimestamp<int>( 1, new Timestamp( 0 ) ),
            new WithTimestamp<int>( 2, new Timestamp( 1 ) ),
            new WithTimestamp<int>( 3, new Timestamp( 2 ) ),
            new WithTimestamp<int>( 5, new Timestamp( 3 ) ),
            new WithTimestamp<int>( 7, new Timestamp( 4 ) ),
            new WithTimestamp<int>( 11, new Timestamp( 5 ) ),
            new WithTimestamp<int>( 13, new Timestamp( 6 ) ),
            new WithTimestamp<int>( 17, new Timestamp( 7 ) ),
            new WithTimestamp<int>( 19, new Timestamp( 8 ) ),
            new WithTimestamp<int>( 23, new Timestamp( 9 ) )
        };

        var actualEvents = new List<WithTimestamp<int>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var next = EventListener.Create<WithTimestamp<int>>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerWithTimestampDecorator<int>( timestampProvider );
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
        var next = Substitute.For<IEventListener<WithTimestamp<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerWithTimestampDecorator<int>( timestampProvider );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Fact]
    public void WithTimestampExtension_ShouldCreateEventStreamThatAttachesAndIndex()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var timestamps = sourceEvents.Select( (_, i) => new Timestamp( i ) ).ToList();
        var expectedEvents = new[]
        {
            new WithTimestamp<int>( 1, new Timestamp( 0 ) ),
            new WithTimestamp<int>( 2, new Timestamp( 1 ) ),
            new WithTimestamp<int>( 3, new Timestamp( 2 ) ),
            new WithTimestamp<int>( 5, new Timestamp( 3 ) ),
            new WithTimestamp<int>( 7, new Timestamp( 4 ) ),
            new WithTimestamp<int>( 11, new Timestamp( 5 ) ),
            new WithTimestamp<int>( 13, new Timestamp( 6 ) ),
            new WithTimestamp<int>( 17, new Timestamp( 7 ) ),
            new WithTimestamp<int>( 19, new Timestamp( 8 ) ),
            new WithTimestamp<int>( 23, new Timestamp( 9 ) )
        };

        var actualEvents = new List<WithTimestamp<int>>();

        var timestampProvider = Substitute.For<ITimestampProvider>();
        timestampProvider.GetNow().Returns( timestamps );
        var next = EventListener.Create<WithTimestamp<int>>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.WithTimestamp( timestampProvider );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }
}
