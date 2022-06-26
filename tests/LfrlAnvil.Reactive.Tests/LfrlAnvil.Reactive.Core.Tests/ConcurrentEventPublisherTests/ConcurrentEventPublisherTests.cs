using System.Threading;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.Reactive.Internal;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

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

        listener.VerifyCalls().Received( x => x.React( @event ) );
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

        hasLock.Should().BeTrue();
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

        listener.VerifyCalls().Received( x => x.React( @event ) );
    }

    [Fact]
    public void IEventPublisherPublish_ShouldThrowInvalidArgumentTypeException_WhenEventIsNotOfCorrectType()
    {
        var @event = Fixture.Create<int[]>();
        var @base = new EventPublisher<int>();
        IEventPublisher sut = new ConcurrentEventPublisher<int, EventPublisher<int>>( @base );

        var action = Lambda.Of( () => sut.Publish( @event ) );

        action.Should()
            .ThrowExactly<InvalidArgumentTypeException>()
            .AndMatch( e => e.Argument == @event && e.ExpectedType == typeof( int ) );
    }

    [Fact]
    public void ToConcurrentExtension_ShouldCreateConcurrentWrapperForEventPublisher()
    {
        var @base = new EventPublisher<int>();
        var sut = @base.ToConcurrent();
        sut.Should().BeOfType<ConcurrentEventPublisher<int, EventPublisher<int>>>();
    }
}
