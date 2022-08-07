using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerExhaustAllDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerExhaustAllDecorator<int>();

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactSubscribesToFirstInnerStream()
    {
        var inner = new EventPublisher<int>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerExhaustAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( inner );

        inner.HasSubscribers.Should().BeTrue();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactDoesNotDispose_WhenFirstInnerStreamIsDisposed()
    {
        var inner = new EventPublisher<int>();
        inner.Dispose();

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerExhaustAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( inner );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatForwardsFirstInnerStreamEvents()
    {
        var firstStreamValues = new[] { 1, 2 };
        var actualEvents = new List<int>();

        var inner = new EventPublisher<int>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerExhaustAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( inner );

        foreach ( var e in firstStreamValues )
            inner.Publish( e );

        using ( new AssertionScope() )
        {
            inner.HasSubscribers.Should().BeTrue();
            actualEvents.Should().BeSequentiallyEqualTo( firstStreamValues );
        }
    }

    [Fact]
    public void
        Decorate_ShouldCreateListenerThatForwardsFirstAndSecondInnerStreamEvents_WhenFirstStreamDisposesBeforeSecondStreamSubscriberIsCreated()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var expectedEvents = firstStreamValues.Concat( secondStreamValues );
        var actualEvents = new List<int>();

        var firstInner = new EventPublisher<int>();
        var secondInner = new EventPublisher<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerExhaustAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( firstInner );

        foreach ( var e in firstStreamValues )
            firstInner.Publish( e );

        firstInner.Dispose();

        listener.React( secondInner );

        foreach ( var e in secondStreamValues )
            secondInner.Publish( e );

        using ( new AssertionScope() )
        {
            firstInner.HasSubscribers.Should().BeFalse();
            secondInner.HasSubscribers.Should().BeTrue();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatIgnoresNextInnerStreams_WhenActiveStreamDoesNotDisposeBeforeNextStreamIsCreated()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = firstStreamValues.Concat( thirdStreamValues );
        var actualEvents = new List<int>();

        var firstInner = new EventPublisher<int>();
        var secondInner = new EventPublisher<int>();
        var thirdInner = new EventPublisher<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerExhaustAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( firstInner );
        firstInner.Publish( firstStreamValues[0] );
        listener.React( secondInner );
        firstInner.Publish( firstStreamValues[1] );
        firstInner.Dispose();

        secondInner.Publish( secondStreamValues[0] );
        listener.React( thirdInner );
        secondInner.Publish( secondStreamValues[1] );
        secondInner.Dispose();

        foreach ( var e in thirdStreamValues )
            thirdInner.Publish( e );

        using ( new AssertionScope() )
        {
            firstInner.HasSubscribers.Should().BeFalse();
            secondInner.HasSubscribers.Should().BeFalse();
            thirdInner.HasSubscribers.Should().BeTrue();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeDisposesInnerStreamSubscriber(DisposalSource source)
    {
        var inner = new EventPublisher<int>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerExhaustAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( inner );
        listener.OnDispose( source );

        inner.HasSubscribers.Should().BeFalse();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerExhaustAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Fact]
    public void
        ExhaustAllExtension_ShouldCreateEventStreamThatIgnoresNextInnerStreams_WhenActiveStreamDoesNotDisposeBeforeNextStreamIsCreated()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = firstStreamValues.Concat( thirdStreamValues );
        var actualEvents = new List<int>();

        var firstInner = new EventPublisher<int>();
        var secondInner = new EventPublisher<int>();
        var thirdInner = new EventPublisher<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<IEventStream<int>>();
        var decorated = sut.ExhaustAll();
        var subscriber = decorated.Listen( next );

        sut.Publish( firstInner );
        firstInner.Publish( firstStreamValues[0] );
        sut.Publish( secondInner );
        firstInner.Publish( firstStreamValues[1] );
        firstInner.Dispose();

        secondInner.Publish( secondStreamValues[0] );
        sut.Publish( thirdInner );
        secondInner.Publish( secondStreamValues[1] );
        secondInner.Dispose();

        foreach ( var e in thirdStreamValues )
            thirdInner.Publish( e );

        using ( new AssertionScope() )
        {
            firstInner.HasSubscribers.Should().BeFalse();
            secondInner.HasSubscribers.Should().BeFalse();
            thirdInner.HasSubscribers.Should().BeTrue();
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }
}
