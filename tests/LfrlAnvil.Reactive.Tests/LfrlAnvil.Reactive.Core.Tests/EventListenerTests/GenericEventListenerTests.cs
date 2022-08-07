using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.EventListenerTests;

public abstract class GenericEventListenerTests<TEvent> : TestsBase
{
    [Fact]
    public void Create_ShouldReturnListenerWithCorrectReactSetup()
    {
        var @event = Fixture.Create<TEvent>();
        var react = Substitute.For<Action<TEvent>>();
        var sut = EventListener.Create( react );

        sut.React( @event );

        react.Verify().CallAt( 0 ).Arguments.Should().BeSequentiallyEqualTo( @event );
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Create_ShouldReturnListenerWithCorrectOnDisposeSetup(DisposalSource source)
    {
        var react = Substitute.For<Action<TEvent>>();
        var onDispose = Substitute.For<Action<DisposalSource>>();
        var sut = EventListener.Create( react, onDispose );

        sut.OnDispose( source );

        onDispose.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( source );
    }

    [Fact]
    public void IListenerReact_ShouldBeEquivalentToGenericReact_WhenEventIsOfCorrectType()
    {
        var @event = Fixture.Create<TEvent>();
        var listener = Substitute.For<EventListener<TEvent>>();
        IEventListener sut = listener;

        sut.React( @event );

        listener.VerifyCalls().Received( x => x.React( @event ) );
    }

    [Fact]
    public void IListenerReact_ShouldThrowInvalidArgumentTypeException_WhenEventIsNotOfCorrectType()
    {
        var @event = Fixture.Create<Invalid>();
        IEventListener sut = Substitute.For<EventListener<TEvent>>();

        var action = Lambda.Of( () => sut.React( @event ) );

        action.Should()
            .ThrowExactly<InvalidArgumentTypeException>()
            .AndMatch( e => e.Argument == @event && e.ExpectedType == typeof( TEvent ) );
    }

    private sealed class Invalid
    {
        public TEvent? Event { get; set; }
    }
}
