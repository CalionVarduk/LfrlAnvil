using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerCatchDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var onError = Substitute.For<Action<Exception>>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerCatchDecorator<int, Exception>( onError );

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardsEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerCatchDecorator<int, Exception>( onError );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatCallsOnError_WhenNextReactThrowsAnException()
    {
        var exception = new Exception();
        var sourceEvent = Fixture.Create<int>();
        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( _ => throw exception );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerCatchDecorator<int, Exception>( onError );
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );

        onError.Verify().CallAt( 0 ).Exists().And.Arguments.First().Should().BeSameAs( exception );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerThatCallsOnError_WhenNextOnDisposeThrowsAnException(DisposalSource source)
    {
        var exception = new Exception();
        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( _ => { }, _ => throw exception );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerCatchDecorator<int, Exception>( onError );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        onError.Verify().CallAt( 0 ).Exists().And.Arguments.First().Should().BeSameAs( exception );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var onError = Substitute.For<Action<Exception>>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerCatchDecorator<int, Exception>( onError );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Fact]
    public void CatchExtension_ShouldCreateListenerWhoseReactForwardsEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Catch( onError );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
    }

    [Fact]
    public void CatchExtension_ShouldCreateListenerThatCallsOnError_WhenNextReactThrowsAnException()
    {
        var exception = new Exception();
        var sourceEvent = Fixture.Create<int>();
        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( _ => throw exception );
        var sut = new EventPublisher<int>();
        var decorated = sut.Catch( onError );
        decorated.Listen( next );

        sut.Publish( sourceEvent );

        onError.Verify().CallAt( 0 ).Exists().And.Arguments.First().Should().BeSameAs( exception );
    }

    [Fact]
    public void CatchExtension_ShouldCreateEventStreamThatCallsOnError_WhenNextOnDisposeThrowsAnException()
    {
        var exception = new Exception();
        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( _ => { }, _ => throw exception );
        var sut = new EventPublisher<int>();
        var decorated = sut.Catch( onError );
        var subscriber = decorated.Listen( next );

        subscriber.Dispose();

        onError.Verify().CallAt( 0 ).Exists().And.Arguments.First().Should().BeSameAs( exception );
    }
}
