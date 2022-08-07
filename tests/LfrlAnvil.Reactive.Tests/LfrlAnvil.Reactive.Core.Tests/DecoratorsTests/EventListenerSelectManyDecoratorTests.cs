using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerSelectManyDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<string>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSelectManyDecorator<int, string>( x => new[] { x.ToString(), (x * 2).ToString() } );

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactMapsEverySourceEventIntoMultipleNextEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[]
            { "1", "2", "2", "4", "3", "6", "5", "10", "7", "14", "11", "22", "13", "26", "17", "34", "19", "38", "23", "46" };

        var actualEvents = new List<string>();

        var next = EventListener.Create<string>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSelectManyDecorator<int, string>( x => new[] { x.ToString(), (x * 2).ToString() } );
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
        var next = Substitute.For<IEventListener<string>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSelectManyDecorator<int, string>( x => new[] { x.ToString(), (x * 2).ToString() } );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Fact]
    public void SelectManyExtension_ShouldCreateEventStreamThatMapsEverySourceEventIntoMultipleNextEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[]
            { "1", "2", "2", "4", "3", "6", "5", "10", "7", "14", "11", "22", "13", "26", "17", "34", "19", "38", "23", "46" };

        var actualEvents = new List<string>();

        var next = EventListener.Create<string>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.SelectMany( x => new[] { x.ToString(), (x * 2).ToString() } );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void FlattenExtension_ShouldCreateEventStreamThatMapsEverySourceEventIntoPairsOfSourceAndNextEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7 };
        var expectedEvents = new[]
        {
            Pair.Create( 1, "1" ),
            Pair.Create( 1, "2" ),
            Pair.Create( 2, "2" ),
            Pair.Create( 2, "4" ),
            Pair.Create( 3, "3" ),
            Pair.Create( 3, "6" ),
            Pair.Create( 5, "5" ),
            Pair.Create( 5, "10" ),
            Pair.Create( 7, "7" ),
            Pair.Create( 7, "14" )
        };

        var actualEvents = new List<Pair<int, string>>();

        var next = EventListener.Create<Pair<int, string>>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Flatten( x => new[] { x.ToString(), (x * 2).ToString() } );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void FlattenExtension_WithoutParameters_ShouldCreateEventStreamThatReducesSourceEventCollection()
    {
        var sourceEvents = new[] { new[] { 1, 2, 3 }, new[] { 5, 7, 11, 13, 17 }, new[] { 19, 23 } };
        var expectedEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int[]>();
        var decorated = sut.Flatten();
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }
}
