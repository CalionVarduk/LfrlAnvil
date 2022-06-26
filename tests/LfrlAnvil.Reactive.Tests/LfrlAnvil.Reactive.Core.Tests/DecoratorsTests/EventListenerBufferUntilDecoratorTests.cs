using System;
using System.Collections.Generic;
using System.Linq;
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

public class EventListenerBufferUntilDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber_WhenTargetIsNotDisposed()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );

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

        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );

        var _ = sut.Decorate( next, subscriber );

        using ( new AssertionScope() )
        {
            subscriber.VerifyCalls().Received( x => x.Dispose() );
            target.HasSubscribers.Should().BeFalse();
        }
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatCallsNextReactWithBufferedEventsWhenTargetEmits()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { new[] { 1, 2 }, new[] { 3, 5, 7 }, new[] { 11, 13, 17, 19 }, Array.Empty<int>(), new[] { 23 } };
        var actualEvents = new List<int[]>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents.Add( e.ToArray() ) );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents.Take( 2 ) )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 2 ).Take( 3 ) )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 5 ).Take( 4 ) )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );
        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 9 ) )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );

        using ( new AssertionScope() )
        {
            actualEvents.Should().HaveCount( expectedEvents.Length );
            for ( var i = 0; i < actualEvents.Count; ++i )
                actualEvents[i].Should().BeSequentiallyEqualTo( expectedEvents[i] );
        }
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatCallsNextReactCorrectly_WhenBufferGetsLarge()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47 };
        var actualEvents = Array.Empty<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents = e.ToArray() );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
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
        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        target.HasSubscribers.Should().BeFalse();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatDisposes_WhenTargetDisposes()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var _ = sut.Decorate( next, subscriber );

        target.Dispose();

        subscriber.VerifyCalls().Received( x => x.Dispose() );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeEmitsRemainingBufferedEvents(
        DisposalSource source)
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = Array.Empty<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents = e.ToArray() );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        listener.OnDispose( source );

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatEmitsRemainingBufferedEvents_WhenTargetDisposes()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = Array.Empty<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents = e.ToArray() );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        target.Dispose();

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
    }

    [Fact]
    public void BufferUntilExtension_ShouldCreateEventStreamThatCallsNextReactWithBufferedEventsWhenTargetEmits()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { new[] { 1, 2 }, new[] { 3, 5, 7 }, new[] { 11, 13, 17, 19 }, Array.Empty<int>(), new[] { 23 } };
        var actualEvents = new List<int[]>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents.Add( e.ToArray() ) );
        var sut = new EventPublisher<int>();
        var decorated = sut.BufferUntil( target );
        decorated.Listen( next );

        foreach ( var e in sourceEvents.Take( 2 ) )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 2 ).Take( 3 ) )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 5 ).Take( 4 ) )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );
        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 9 ) )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );

        using ( new AssertionScope() )
        {
            actualEvents.Should().HaveCount( expectedEvents.Length );
            for ( var i = 0; i < actualEvents.Count; ++i )
                actualEvents[i].Should().BeSequentiallyEqualTo( expectedEvents[i] );
        }
    }

    [Fact]
    public void BufferUntilExtension_ShouldCreateEventStreamThatEmitsRemainingBufferedEventsWhenSubscriptionIsDisposed()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = Array.Empty<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents = e.ToArray() );
        var sut = new EventPublisher<int>();
        var decorated = sut.BufferUntil( target );
        var subscriber = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        subscriber.Dispose();

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
    }

    [Fact]
    public void BufferUntilExtension_ShouldEmitBufferWithOnePreviousEvent_WhenTargetIsSource()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7 };
        var actualEvents = new List<int[]>();

        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents.Add( e.ToArray() ) );
        var sut = new EventPublisher<int>();
        var decorated = sut.BufferUntil( sut );
        var _ = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        using ( new AssertionScope() )
        {
            actualEvents.Should().HaveCount( sourceEvents.Length );
            actualEvents[0].Should().BeEmpty();
            for ( var i = 1; i < actualEvents.Count; ++i )
                actualEvents[i].Should().BeSequentiallyEqualTo( sourceEvents[i - 1] );
        }
    }
}