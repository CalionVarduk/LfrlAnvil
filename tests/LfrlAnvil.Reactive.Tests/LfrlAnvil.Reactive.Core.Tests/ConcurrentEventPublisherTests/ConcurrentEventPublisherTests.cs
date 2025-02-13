using System.Linq;
using System.Threading;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Tests.ConcurrentEventPublisherTests;

public class ConcurrentEventPublisherTests : TestsBase
{
    [Fact]
    public void Publish_ShouldCallBaseEventPublisherPublish()
    {
        var @event = Fixture.Create<int>();
        var listener = Substitute.For<IEventListener<int>>();
        var @base = new EventPublisher<int>();
        @base.Listen( listener );

        var sut = new ConcurrentEventPublisher<int, EventPublisher<int>>( @base );

        sut.Publish( @event );

        listener.TestReceivedCalls( x => x.React( @event ) ).Go();
    }

    [Fact]
    public void Publish_ShouldAcquireLock()
    {
        var @event = Fixture.Create<int>();
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventPublisher<int, EventPublisher<int>>( @base );
        var sync = sut.Sync;
        var hasLock = false;
        var listener = EventListener.Create<int>( _ => hasLock = Monitor.IsEntered( sync ) );
        @base.Listen( listener );

        sut.Publish( @event );

        hasLock.TestTrue().Go();
    }

    [Fact]
    public void IEventPublisherPublish_ShouldBeEquivalentToGenericPublish_WhenEventIsOfCorrectType()
    {
        var @event = Fixture.Create<int>();
        var listener = Substitute.For<IEventListener<int>>();
        var @base = new EventPublisher<int>();
        var source = new ConcurrentEventPublisher<int, EventPublisher<int>>( @base );
        source.Listen( listener );
        IEventPublisher sut = source;

        sut.Publish( @event );

        listener.TestReceivedCalls( x => x.React( @event ) ).Go();
    }

    [Fact]
    public void IEventPublisherPublish_ShouldThrowInvalidArgumentTypeException_WhenEventIsNotOfCorrectType()
    {
        var @event = Fixture.CreateMany<int>().ToArray();
        var @base = new EventPublisher<int>();
        IEventPublisher sut = new ConcurrentEventPublisher<int, EventPublisher<int>>( @base );

        var action = Lambda.Of( () => sut.Publish( @event ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<InvalidArgumentTypeException>(
                        e => Assertion.All( e.Argument.TestRefEquals( @event ), e.ExpectedType.TestEquals( typeof( int ) ) ) ) )
            .Go();
    }

    [Fact]
    public void ToConcurrentExtension_ShouldCreateConcurrentWrapperForEventPublisher()
    {
        var @base = new EventPublisher<int>();
        var sut = @base.ToConcurrent();
        sut.TestType().AssignableTo<ConcurrentEventPublisher<int, EventPublisher<int>>>().Go();
    }
}
