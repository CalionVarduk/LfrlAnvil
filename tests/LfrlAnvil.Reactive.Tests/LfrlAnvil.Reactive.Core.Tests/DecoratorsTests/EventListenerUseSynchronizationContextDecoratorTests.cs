using System;
using System.Threading;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerUseSynchronizationContextDecoratorTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldThrowInvalidOperationException_WhenCurrentSynchronizationContextIsNull()
    {
        using var @switch = new SynchronizationContextSwitch( null );
        var action = Lambda.Of( () => new EventListenerUseSynchronizationContextDecorator<int>() );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerUseSynchronizationContextDecorator<int>();

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatForwardsEventsToTheMemorizedSynchronizationContext()
    {
        var context = Substitute.ForPartsOf<SynchronizationContext>();
        context.When( x => x.Post( Arg.Any<SendOrPostCallback>(), Arg.Any<object?>() ) )
            .Do( c => c.ArgAt<SendOrPostCallback>( 0 )( c.ArgAt<object?>( 1 ) ) );

        var sourceEvent = Fixture.Create<int>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();

        EventListenerUseSynchronizationContextDecorator<int> sut;
        using ( new SynchronizationContextSwitch( context ) )
        {
            sut = new EventListenerUseSynchronizationContextDecorator<int>();
        }

        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );

        using ( new AssertionScope() )
        {
            context.VerifyCalls().Received( x => x.Post( Arg.Any<SendOrPostCallback>(), null ), count: 1 );
            next.VerifyCalls().Received( x => x.React( sourceEvent ) );
        }
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var context = Substitute.ForPartsOf<SynchronizationContext>();
        context.When( x => x.Post( Arg.Any<SendOrPostCallback>(), Arg.Any<object?>() ) )
            .Do( c => c.ArgAt<SendOrPostCallback>( 0 )( c.ArgAt<object?>( 1 ) ) );

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();

        EventListenerUseSynchronizationContextDecorator<int> sut;
        using ( new SynchronizationContextSwitch( context ) )
        {
            sut = new EventListenerUseSynchronizationContextDecorator<int>();
        }

        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        using ( new AssertionScope() )
        {
            context.VerifyCalls().Received( x => x.Post( Arg.Any<SendOrPostCallback>(), null ), count: 1 );
            next.VerifyCalls().Received( x => x.OnDispose( source ) );
        }
    }

    [Fact]
    public void UseSynchronizationContextExtension_ShouldCreateEventStreamThatForwardsEventsToTheMemorizedSynchronizationContext()
    {
        var context = Substitute.ForPartsOf<SynchronizationContext>();
        context.When( x => x.Post( Arg.Any<SendOrPostCallback>(), Arg.Any<object?>() ) )
            .Do( c => c.ArgAt<SendOrPostCallback>( 0 )( c.ArgAt<object?>( 1 ) ) );

        var sourceEvent = Fixture.Create<int>();
        var next = Substitute.For<IEventListener<int>>();

        var sut = new EventPublisher<int>();
        IEventStream<int> decorated;
        using ( new SynchronizationContextSwitch( context ) )
        {
            decorated = sut.UseSynchronizationContext();
        }

        decorated.Listen( next );

        sut.Publish( sourceEvent );

        using ( new AssertionScope() )
        {
            context.VerifyCalls().Received( x => x.Post( Arg.Any<SendOrPostCallback>(), null ), count: 1 );
            next.VerifyCalls().Received( x => x.React( sourceEvent ) );
        }
    }
}