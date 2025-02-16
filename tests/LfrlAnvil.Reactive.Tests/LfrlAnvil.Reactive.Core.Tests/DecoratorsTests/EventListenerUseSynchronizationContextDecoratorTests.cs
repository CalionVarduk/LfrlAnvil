using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerUseSynchronizationContextDecoratorTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldThrowInvalidOperationException_WhenCurrentSynchronizationContextIsNull()
    {
        using var @switch = new SynchronizationContextSwitch( null );
        var action = Lambda.Of( () => new EventListenerUseSynchronizationContextDecorator<int>() );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerUseSynchronizationContextDecorator<int>();

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
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

        Assertion.All(
                context.TestReceivedCalls( x => x.Post( Arg.Any<SendOrPostCallback>(), Arg.Is<object?>( o => o == null ) ), count: 1 ),
                next.TestReceivedCall( x => x.React( sourceEvent ) ) )
            .Go();
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

        Assertion.All(
                context.TestReceivedCalls( x => x.Post( Arg.Any<SendOrPostCallback>(), Arg.Is<object?>( o => o == null ) ), count: 1 ),
                next.TestReceivedCall( x => x.OnDispose( source ) ) )
            .Go();
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

        Assertion.All(
                context.TestReceivedCalls( x => x.Post( Arg.Any<SendOrPostCallback>(), Arg.Is<object?>( o => o == null ) ), count: 1 ),
                next.TestReceivedCall( x => x.React( sourceEvent ) ) )
            .Go();
    }
}
