﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

public class EventListenerLockAllDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<IEventStream<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerLockAllDecorator<int>( null );

        var _ = sut.Decorate( next, subscriber );

        subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactAppliesLockDecoratorWithTheSameSyncObjectToEvents()
    {
        var sourceEvents = new[]
        {
            Substitute.For<IEventStream<int>>(),
            Substitute.For<IEventStream<int>>(),
            Substitute.For<IEventStream<int>>()
        };

        var expectedSourceEvents = new[]
        {
            Substitute.For<IEventStream<int>>(),
            Substitute.For<IEventStream<int>>(),
            Substitute.For<IEventStream<int>>()
        };

        foreach ( var (source, result) in sourceEvents.Zip( expectedSourceEvents ) )
            source.Decorate( Arg.Any<IEventListenerDecorator<int, int>>() ).Returns( result );

        var actualEvents = new List<IEventStream<int>>();
        var next = EventListener.Create<IEventStream<int>>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerLockAllDecorator<int>( null );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        using ( new AssertionScope() )
        {
            actualEvents.Should().BeSequentiallyEqualTo( expectedSourceEvents );

            foreach ( var e in sourceEvents )
            {
                var decorator = e.ReceivedCalls().First().GetArguments().First();
                decorator.Should().BeOfType<EventListenerLockDecorator<int>>();
            }
        }
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseEmittedEventStreamsAcquireUnderlyingLock()
    {
        var sync = new object();
        var hasLock = false;

        var inner = new EventPublisher<int>();
        var innerNext = Substitute.For<IEventListener<int>>();
        innerNext.When( x => x.React( Arg.Any<int>() ) )
            .Do( _ => { hasLock = Monitor.IsEntered( sync ); } );

        var next = EventListener.Create<IEventStream<int>>( e => e.Listen( innerNext ) );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerLockAllDecorator<int>( sync );
        var listener = sut.Decorate( next, subscriber );

        listener.React( inner );
        inner.Publish( Fixture.Create<int>() );

        hasLock.Should().BeTrue();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<IEventStream<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerLockAllDecorator<int>( null );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }

    [Fact]
    public void LockAllExtension_ShouldCreateEventStreamThatAppliesLockDecoratorWithTheSameSyncObjectToEvents()
    {
        var inner = Substitute.For<IEventStream<int>>();
        var expectedInner = Substitute.For<IEventStream<int>>();
        inner.Decorate( Arg.Any<IEventListenerDecorator<int, int>>() ).Returns( expectedInner );

        var actualEvents = new List<IEventStream<int>>();
        var next = EventListener.Create<IEventStream<int>>( actualEvents.Add );
        var sut = new EventPublisher<IEventStream<int>>();
        var decorated = sut.LockAll();
        decorated.Listen( next );

        sut.Publish( inner );

        using ( new AssertionScope() )
        {
            actualEvents.Should().BeSequentiallyEqualTo( expectedInner );
            var decorator = inner.ReceivedCalls().First().GetArguments().First();
            decorator.Should().BeOfType<EventListenerLockDecorator<int>>();
        }
    }

    [Fact]
    public void LockAllExtension_WithExplicitSyncObject_ShouldCreateEventStreamThatAppliesLockDecoratorWithTheSameSyncObjectToEvents()
    {
        var sync = new object();
        var inner = Substitute.For<IEventStream<int>>();
        var expectedInner = Substitute.For<IEventStream<int>>();
        inner.Decorate( Arg.Any<IEventListenerDecorator<int, int>>() ).Returns( expectedInner );

        var actualEvents = new List<IEventStream<int>>();
        var next = EventListener.Create<IEventStream<int>>( actualEvents.Add );
        var sut = new EventPublisher<IEventStream<int>>();
        var decorated = sut.LockAll( sync );
        decorated.Listen( next );

        sut.Publish( inner );

        using ( new AssertionScope() )
        {
            actualEvents.Should().BeSequentiallyEqualTo( expectedInner );
            var decorator = inner.ReceivedCalls().First().GetArguments().First();
            decorator.Should().BeOfType<EventListenerLockDecorator<int>>();
        }
    }

    [Fact]
    public void ShareLockWithAllExtension_ShouldApplyLockToOuterStreamAndAllInnerStreams()
    {
        var expectedSecondDecorateResult = Substitute.For<IEventStream<IEventStream<int>>>();

        var expectedFirstDecorateResult = Substitute.For<IEventStream<IEventStream<int>>>();
        expectedFirstDecorateResult.Decorate( Arg.Any<IEventListenerDecorator<IEventStream<int>, IEventStream<int>>>() )
            .Returns( expectedSecondDecorateResult );

        var sut = Substitute.For<IEventStream<IEventStream<int>>>();
        sut.Decorate( Arg.Any<IEventListenerDecorator<IEventStream<int>, IEventStream<int>>>() )
            .Returns( expectedFirstDecorateResult );

        var result = sut.ShareLockWithAll();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( expectedSecondDecorateResult );

            var sutDecorator = sut.ReceivedCalls().First().GetArguments().First();
            sutDecorator.Should().BeOfType<EventListenerLockDecorator<IEventStream<int>>>();

            var nextDecorator = expectedFirstDecorateResult.ReceivedCalls().First().GetArguments().First();
            nextDecorator.Should().BeOfType<EventListenerLockAllDecorator<int>>();
        }
    }
}