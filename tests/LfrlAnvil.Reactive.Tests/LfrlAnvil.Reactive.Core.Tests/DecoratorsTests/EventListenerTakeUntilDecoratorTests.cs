using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerTakeUntilDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber_WhenTargetDoesNotPublishEventImmediately()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );

        var _ = sut.Decorate( next, subscriber );

        using ( new AssertionScope() )
        {
            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
            target.HasSubscribers.Should().BeTrue();
        }
    }

    [Fact]
    public void Decorate_ShouldDisposeTheSubscriber_WhenTargetPublishesEventImmediately()
    {
        var target = new HistoryEventPublisher<string>( capacity: 1 );
        target.Publish( Fixture.Create<string>() );

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().Received( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldDisposeTheSubscriber_WhenTargetIsAlreadyDisposed()
    {
        var target = new EventPublisher<string>();
        target.Dispose();

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().Received( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatDisposesTheSubscriber_WhenTargetPublishesAnyEvent()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );
        var _ = sut.Decorate( next, subscriber );

        target.Publish( Fixture.Create<string>() );

        subscriber.VerifyCalls().Received( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatDisposesTheSubscriber_WhenTargetDisposes()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );
        var _ = sut.Decorate( next, subscriber );

        target.Dispose();

        subscriber.VerifyCalls().Received( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardsEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeDisposesTheTargetSubscriber(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        target.HasSubscribers.Should().BeFalse();
    }

    [Fact]
    public void TakeUntilExtension_ShouldCreateEventStreamThatForwardsEvents_UntilTargetPublishesAnyEvent()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 1, 2, 3, 5, 7, 11 };
        var actualEvents = new List<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.TakeUntil( target );
        decorated.Listen( next );

        foreach ( var e in sourceEvents.Take( expectedEvents.Length ) )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( expectedEvents.Length ) )
            sut.Publish( e );

        using ( new AssertionScope() )
        {
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
            sut.HasSubscribers.Should().BeFalse();
            target.HasSubscribers.Should().BeFalse();
        }
    }

    [Fact]
    public void TakeUntilExtension_ShouldDisposeSubscribers_WhenTargetIsSourceAndSourcePublishedAnyEvent()
    {
        var @event = Fixture.CreateNotDefault<int>();
        var next = Substitute.For<IEventListener<int>>();
        var sut = new EventPublisher<int>();
        var decorated = sut.TakeUntil( sut );
        decorated.Listen( next );

        sut.Publish( @event );

        using ( new AssertionScope() )
        {
            next.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<int>() ) );
            next.VerifyCalls().Received( x => x.OnDispose( DisposalSource.Subscriber ) );
            sut.HasSubscribers.Should().BeFalse();
        }
    }
}
