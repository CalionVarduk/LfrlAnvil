using System.Collections.Generic;
using LfrlAnvil.Extensions;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerWhereDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerWhereDecorator<int>( x => x.IsOdd() );

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactMapsFiltersEverySourceEvent()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 3, 7, 13, 17, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerWhereDecorator<int>( x => x % 10 == 3 || x % 10 == 7 );
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
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerWhereDecorator<int>( x => x.IsOdd() );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Fact]
    public void WhereExtension_ShouldCreateEventStreamThatFiltersEverySourceEvent()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 3, 7, 13, 17, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Where( x => x % 10 == 3 || x % 10 == 7 );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void WhereNotNullExtension_ForRefType_ShouldCreateEventStreamThatIgnoresNullSourceEvents()
    {
        var sourceEvents = new[] { null, "1", null, "2", null, "3", null, "5", null, "7", null };
        var expectedEvents = new[] { "1", "2", "3", "5", "7" };
        var actualEvents = new List<string>();

        var next = EventListener.Create<string>( actualEvents.Add );
        var sut = new EventPublisher<string?>();
        var decorated = sut.WhereNotNull();
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void WhereNotNullExtension_ForStructType_ShouldCreateEventStreamThatIgnoresNullSourceEvents()
    {
        var sourceEvents = new int?[] { null, 1, null, 2, null, 3, null, 5, null, 7, null };
        var expectedEvents = new[] { 1, 2, 3, 5, 7 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int?>();
        var decorated = sut.WhereNotNull();
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void WhereNotNullExtension_WithExplicitComparer_ForRefType_ShouldCreateEventStreamThatIgnoresNullSourceEvents()
    {
        var sourceEvents = new[] { null, "1", null, "2", null, "3", null, "5", null, "7", null };
        var expectedEvents = new[] { "1", "2", "3", "5", "7" };
        var actualEvents = new List<string>();

        var next = EventListener.Create<string>( actualEvents.Add );
        var sut = new EventPublisher<string?>();
        var decorated = sut.WhereNotNull( EqualityComparer<string>.Default );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void WhereNotNullExtension_WithExplicitComparer_ForNullableStructType_ShouldCreateEventStreamThatIgnoresNullSourceEvents()
    {
        var sourceEvents = new int?[] { null, 1, null, 2, null, 3, null, 5, null, 7, null };
        var expectedEvents = new int?[] { 1, 2, 3, 5, 7 };
        var actualEvents = new List<int?>();

        var next = EventListener.Create<int?>( actualEvents.Add );
        var sut = new EventPublisher<int?>();
        var decorated = sut.WhereNotNull( EqualityComparer<int?>.Default );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void WhereNotNullExtension_WithExplicitComparer_ForStructType_ShouldReturnSource()
    {
        var sut = new EventPublisher<int>();
        var decorated = sut.WhereNotNull( EqualityComparer<int>.Default );
        decorated.Should().BeSameAs( sut );
    }
}
