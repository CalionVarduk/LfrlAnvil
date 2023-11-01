using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerConcurrentDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentDecorator<int>( null );

        _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardsEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentDecorator<int>( null );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseReactIgnoresEventsAfterOnDisposeHasBeenCalled(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentDecorator<int>( null );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );
        listener.React( Fixture.Create<int>() );

        next.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<int>() ) );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeDoesNothingAfterOnDisposeHasAlreadyBeenCalled(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentDecorator<int>( null );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );
        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ), count: 1 );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactAcquiresUnderlyingLock()
    {
        var sync = new object();
        var hasLock = false;

        var next = Substitute.For<IEventListener<int>>();
        next.When( x => x.React( Arg.Any<int>() ) )
            .Do( _ => { hasLock = Monitor.IsEntered( sync ); } );

        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentDecorator<int>( sync );
        var listener = sut.Decorate( next, subscriber );

        listener.React( Fixture.Create<int>() );

        hasLock.Should().BeTrue();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeAcquiresUnderlyingLock(DisposalSource source)
    {
        var sync = new object();
        var hasLock = false;

        var next = Substitute.For<IEventListener<int>>();
        next.When( x => x.OnDispose( source ) )
            .Do( _ => { hasLock = Monitor.IsEntered( sync ); } );

        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentDecorator<int>( sync );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        hasLock.Should().BeTrue();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentDecorator<int>( null );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Fact]
    public void ConcurrentExtension_ShouldCreateEventStreamThatForwardsEventsAndEmitsValues()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Concurrent();
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
    }

    [Fact]
    public void ConcurrentExtension_WithExplicitSyncObject_ShouldCreateEventStreamThatForwardsEventsAndEmitsValues()
    {
        var sync = new object();
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Concurrent( sync );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
    }

    [Fact]
    public void ShareConcurrencyWithExtension_ShouldApplyLockToBothStreams()
    {
        var expectedSutDecorateResult = Substitute.For<IEventStream<int>>();
        var expectedOtherDecorateResult = Substitute.For<IEventStream<string>>();
        var sut = Substitute.For<IEventStream<int>>();
        sut.Decorate( Arg.Any<IEventListenerDecorator<int, int>>() ).Returns( expectedSutDecorateResult );
        var other = Substitute.For<IEventStream<string>>();
        other.Decorate( Arg.Any<IEventListenerDecorator<string, string>>() ).Returns( expectedOtherDecorateResult );

        var (sutDecorateResult, otherDecorateResult) = sut.ShareConcurrencyWith( other, (a, b) => (a, b) );

        using ( new AssertionScope() )
        {
            sutDecorateResult.Should().BeSameAs( expectedSutDecorateResult );
            otherDecorateResult.Should().BeSameAs( expectedOtherDecorateResult );

            var sutDecorator = sut.ReceivedCalls().First().GetArguments().First();
            sutDecorator.Should().BeOfType<EventListenerConcurrentDecorator<int>>();

            var otherDecorator = other.ReceivedCalls().First().GetArguments().First();
            otherDecorator.Should().BeOfType<EventListenerConcurrentDecorator<string>>();
        }
    }
}
