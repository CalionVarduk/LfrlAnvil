using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerSampleWhenDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber_WhenTargetIsNotDisposed()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSampleWhenDecorator<int, string>( target );

        _ = sut.Decorate( next, subscriber );

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

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSampleWhenDecorator<int, string>( target );

        _ = sut.Decorate( next, subscriber );

        using ( new AssertionScope() )
        {
            subscriber.VerifyCalls().Received( x => x.Dispose() );
            target.HasSubscribers.Should().BeFalse();
        }
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatEmitsLastEventIfExists_WhenTargetEmitsAnyEvent()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 1, 3, 5, 11, 23 };
        var actualEvents = new List<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSampleWhenDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvents[0] );
        target.Publish( Fixture.Create<string>() );

        listener.React( sourceEvents[1] );
        listener.React( sourceEvents[2] );
        target.Publish( Fixture.Create<string>() );

        listener.React( sourceEvents[3] );
        target.Publish( Fixture.Create<string>() );

        listener.React( sourceEvents[4] );
        listener.React( sourceEvents[5] );
        target.Publish( Fixture.Create<string>() );

        target.Publish( Fixture.Create<string>() );

        listener.React( sourceEvents[6] );
        listener.React( sourceEvents[7] );
        listener.React( sourceEvents[8] );
        listener.React( sourceEvents[9] );
        target.Publish( Fixture.Create<string>() );
        target.Publish( Fixture.Create<string>() );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSampleWhenDecorator<int, string>( target );
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
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSampleWhenDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        target.HasSubscribers.Should().BeFalse();
    }

    [Fact]
    public void SampleWhenExtension_ShouldCreateEventStreamThatEmitsLastEventIfExists_WhenTargetEmitsAnyEvent()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 1, 3, 5, 11, 23 };
        var actualEvents = new List<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.SampleWhen( target );
        decorated.Listen( next );

        sut.Publish( sourceEvents[0] );
        target.Publish( Fixture.Create<string>() );

        sut.Publish( sourceEvents[1] );
        sut.Publish( sourceEvents[2] );
        target.Publish( Fixture.Create<string>() );

        sut.Publish( sourceEvents[3] );
        target.Publish( Fixture.Create<string>() );

        sut.Publish( sourceEvents[4] );
        sut.Publish( sourceEvents[5] );
        target.Publish( Fixture.Create<string>() );

        target.Publish( Fixture.Create<string>() );

        sut.Publish( sourceEvents[6] );
        sut.Publish( sourceEvents[7] );
        sut.Publish( sourceEvents[8] );
        sut.Publish( sourceEvents[9] );
        target.Publish( Fixture.Create<string>() );
        target.Publish( Fixture.Create<string>() );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }

    [Fact]
    public void SampleWhenExtension_ShouldCreateEventStreamThatForwardsAllEventsExceptForTheLastOne_WhenTargetIsSource()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.SampleWhen( sut );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
    }
}
