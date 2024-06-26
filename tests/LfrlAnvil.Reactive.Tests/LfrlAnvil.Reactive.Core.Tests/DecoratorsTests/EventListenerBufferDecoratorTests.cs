﻿using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerBufferDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferDecorator<int>( bufferLength: 1 );

        _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenBufferLengthIsLessThanOne(int length)
    {
        var action = Lambda.Of( () => new EventListenerBufferDecorator<int>( length ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactEmitsFullyPopulatedBuffer()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { new[] { 1, 2, 3 }, new[] { 5, 7, 11 }, new[] { 13, 17, 19 } };
        var actualEvents = new List<int[]>();

        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents.Add( e.ToArray() ) );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferDecorator<int>( bufferLength: 3 );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        using ( new AssertionScope() )
        {
            actualEvents.Should().HaveCount( expectedEvents.Length );
            for ( var i = 0; i < actualEvents.Count; ++i )
                actualEvents[i].Should().BeSequentiallyEqualTo( expectedEvents[i] );
        }
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferDecorator<int>( bufferLength: 3 );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeEmitsPartiallyPopulatedBufferIfAtLeastOneEventHasBeenBuffered(
        DisposalSource source)
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 23 };
        var actualEvents = Array.Empty<int>();

        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents = e.ToArray() );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferDecorator<int>( bufferLength: 3 );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        listener.OnDispose( source );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void BufferExtension_ShouldCreateEventStreamThatEmitsFullyPopulatedBuffer()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { new[] { 1, 2, 3 }, new[] { 5, 7, 11 }, new[] { 13, 17, 19 } };
        var actualEvents = new List<int[]>();

        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents.Add( e.ToArray() ) );
        var sut = new EventPublisher<int>();
        var decorated = sut.Buffer( bufferLength: 3 );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        using ( new AssertionScope() )
        {
            actualEvents.Should().HaveCount( expectedEvents.Length );
            for ( var i = 0; i < actualEvents.Count; ++i )
                actualEvents[i].Should().BeSequentiallyEqualTo( expectedEvents[i] );
        }
    }

    [Fact]
    public void BufferExtension_ShouldCreateListenerWhoseOnDisposeEmitsPartiallyPopulatedBufferIfAtLeastOneEventHasBeenBuffered()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 23 };
        var actualEvents = Array.Empty<int>();

        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents = e.ToArray() );
        var sut = new EventPublisher<int>();
        var decorated = sut.Buffer( bufferLength: 3 );
        var subscriber = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        subscriber.Dispose();

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }
}
