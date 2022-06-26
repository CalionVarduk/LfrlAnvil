using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerSingleDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSingleDecorator<int>();

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactDoesNotForwardTheFirstEvent()
    {
        var sourceEvent = Fixture.Create<int>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSingleDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );

        next.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<int>() ) );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseFirstReactDoesNotDisposeTheSubscriber()
    {
        var sourceEvent = Fixture.Create<int>();
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSingleDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeForwardsTheFirstEventWhenDisposing(DisposalSource source)
    {
        var sourceEvent = Fixture.Create<int>();
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSingleDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );
        listener.OnDispose( source );

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvent );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseSecondReactDoesNotForwardAnyEvent()
    {
        var sourceEvents = new[] { 1, 2 };
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSingleDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        next.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<int>() ) );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseSecondReactDisposesTheSubscriber()
    {
        var sourceEvents = new[] { 1, 2 };
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSingleDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        subscriber.VerifyCalls().Received( x => x.Dispose() );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSingleDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Fact]
    public void SingleExtension_ShouldCreateEventStreamThatForwardsFirstEventAndDisposesSubscriber_WhenExactlyOneEventWasReceived()
    {
        var sourceEvents = new[] { 1 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Single();
        var subscriber = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
            subscriber.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void
        SingleExtension_ShouldCreateEventStreamThatDoesNotForwardAnyEventsAndDisposesSubscriber_WhenMoreThanOneEventWasReceived()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Single();
        var subscriber = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        using ( new AssertionScope() )
        {
            actualEvents.Should().BeEmpty();
            subscriber.IsDisposed.Should().BeTrue();
        }
    }
}