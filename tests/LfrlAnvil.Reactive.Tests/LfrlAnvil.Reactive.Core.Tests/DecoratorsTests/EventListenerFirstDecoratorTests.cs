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

public class EventListenerFirstDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerFirstDecorator<int>();

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardTheFirstEvent()
    {
        var sourceEvent = Fixture.Create<int>();
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerFirstDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvent );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseFirstReactDisposesTheSubscriber()
    {
        var sourceEvent = Fixture.Create<int>();
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerFirstDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );

        subscriber.VerifyCalls().Received( x => x.Dispose() );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerFirstDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Fact]
    public void FirstExtension_ShouldCreateEventStreamThatForwardsTheFirstEventAndThenDisposesSubscriberImmediately()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.First();
        var subscriber = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        using ( new AssertionScope() )
        {
            actualEvents.Should().BeSequentiallyEqualTo( sourceEvents[0] );
            subscriber.IsDisposed.Should().BeTrue();
        }
    }
}