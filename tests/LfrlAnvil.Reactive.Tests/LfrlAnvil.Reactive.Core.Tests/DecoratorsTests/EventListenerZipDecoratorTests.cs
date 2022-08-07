using System.Collections.Generic;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerZipDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber_WhenTargetIsNotDisposed()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<(int, string)>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerZipDecorator<int, string, (int, string)>( target, (a, b) => (a, b) );

        var _ = sut.Decorate( next, subscriber );

        using ( new AssertionScope() )
        {
            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
            target.HasSubscribers.Should().BeTrue();
        }
    }

    [Fact]
    public void Decorate_ShouldDisposeTheSubscriber_WhenTargetIsDisposed()
    {
        var target = new EventPublisher<string>();
        target.Dispose();

        var next = Substitute.For<IEventListener<(int, string)>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerZipDecorator<int, string, (int, string)>( target, (a, b) => (a, b) );

        var _ = sut.Decorate( next, subscriber );

        using ( new AssertionScope() )
        {
            subscriber.VerifyCalls().Received( x => x.Dispose() );
            target.HasSubscribers.Should().BeFalse();
        }
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatDoesNotEmitAnything_WhenOnlySourceEmits()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<(int, string)>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<(int, string)>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerZipDecorator<int, string, (int, string)>( target, (a, b) => (a, b) );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.Should().BeEmpty();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatDoesNotEmitAnything_WhenOnlyTargetEmits()
    {
        var targetEvents = new[] { "1", "2", "3", "5", "7", "11", "13", "17", "19", "23" };
        var actualEvents = new List<(int, string)>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<(int, string)>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerZipDecorator<int, string, (int, string)>( target, (a, b) => (a, b) );
        var _ = sut.Decorate( next, subscriber );

        foreach ( var e in targetEvents )
            target.Publish( e );

        actualEvents.Should().BeEmpty();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatEmitsPairedEvents_WhenSourceEmitsFirstAndTargetEmitsSecond()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7 };
        var targetEvents = new[] { "1", "2", "3", "5", "7" };
        var expectedEvents = new[] { (1, "1"), (2, "2"), (3, "3"), (5, "5"), (7, "7") };
        var actualEvents = new List<(int, string)>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<(int, string)>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerZipDecorator<int, string, (int, string)>( target, (a, b) => (a, b) );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        foreach ( var e in targetEvents )
            target.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatEmitsPairedEvents_WhenTargetEmitsFirstAndSourceEmitsSecond()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7 };
        var targetEvents = new[] { "1", "2", "3", "5", "7" };
        var expectedEvents = new[] { (1, "1"), (2, "2"), (3, "3"), (5, "5"), (7, "7") };
        var actualEvents = new List<(int, string)>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<(int, string)>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerZipDecorator<int, string, (int, string)>( target, (a, b) => (a, b) );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in targetEvents )
            target.Publish( e );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatEmitsPairedEvents_WhenSourceAndTargetEmitAlternately_StartingWithSource()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7 };
        var targetEvents = new[] { "1", "2", "3", "5", "7" };
        var expectedEvents = new[] { (1, "1"), (2, "2"), (3, "3"), (5, "5"), (7, "7") };
        var actualEvents = new List<(int, string)>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<(int, string)>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerZipDecorator<int, string, (int, string)>( target, (a, b) => (a, b) );
        var listener = sut.Decorate( next, subscriber );

        for ( var i = 0; i < sourceEvents.Length; ++i )
        {
            listener.React( sourceEvents[i] );
            target.Publish( targetEvents[i] );
        }

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatEmitsPairedEvents_WhenSourceAndTargetEmitAlternately_StartingWithTarget()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7 };
        var targetEvents = new[] { "1", "2", "3", "5", "7" };
        var expectedEvents = new[] { (1, "1"), (2, "2"), (3, "3"), (5, "5"), (7, "7") };
        var actualEvents = new List<(int, string)>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<(int, string)>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerZipDecorator<int, string, (int, string)>( target, (a, b) => (a, b) );
        var listener = sut.Decorate( next, subscriber );

        for ( var i = 0; i < sourceEvents.Length; ++i )
        {
            target.Publish( targetEvents[i] );
            listener.React( sourceEvents[i] );
        }

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<(int, string)>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerZipDecorator<int, string, (int, string)>( target, (a, b) => (a, b) );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeDisposesTargetSubscriber(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<(int, string)>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerZipDecorator<int, string, (int, string)>( target, (a, b) => (a, b) );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        target.HasSubscribers.Should().BeFalse();
    }

    [Fact]
    public void ZipExtension_ShouldCreateEventStreamCreateListenerThatEmitsPairedEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7 };
        var targetEvents = new[] { "1", "2", "3", "5", "7" };
        var expectedEvents = new[] { (1, "1"), (2, "2"), (3, "3"), (5, "5"), (7, "7") };
        var actualEvents = new List<(int, string)>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<(int, string)>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Zip( target );
        decorated.Listen( next );

        for ( var i = 0; i < sourceEvents.Length; ++i )
        {
            sut.Publish( sourceEvents[i] );
            target.Publish( targetEvents[i] );
        }

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void ZipExtension_ShouldCreateEventStreamCreateListenerThatEmitsPairedEvents_WhenTargetIsSource()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7 };
        var expectedEvents = new[] { (1, 1), (2, 2), (3, 3), (5, 5), (7, 7) };
        var actualEvents = new List<(int, int)>();

        var next = EventListener.Create<(int, int)>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Zip( sut );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }
}
