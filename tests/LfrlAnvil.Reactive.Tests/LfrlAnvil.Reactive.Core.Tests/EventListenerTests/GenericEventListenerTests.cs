using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;

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

        react.CallAt( 0 ).Arguments.TestSequence( [ @event ] ).Go();
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

        onDispose.CallAt( 0 ).Arguments.TestSequence( [ source ] ).Go();
    }

    [Fact]
    public void IListenerReact_ShouldBeEquivalentToGenericReact_WhenEventIsOfCorrectType()
    {
        var @event = Fixture.Create<TEvent>();
        var listener = Substitute.For<EventListener<TEvent>>();
        IEventListener sut = listener;

        sut.React( @event );

        listener.TestReceivedCall( x => x.React( @event ) ).Go();
    }

    [Fact]
    public void IListenerReact_ShouldThrowInvalidArgumentTypeException_WhenEventIsNotOfCorrectType()
    {
        var @event = new Invalid { Event = Fixture.Create<TEvent>() };
        IEventListener sut = Substitute.For<EventListener<TEvent>>();

        var action = Lambda.Of( () => sut.React( @event ) );

        action.Test( exc => exc.TestType()
                .Exact<InvalidArgumentTypeException>( e => Assertion.All(
                    e.Argument.TestRefEquals( @event ),
                    e.ExpectedType.TestEquals( typeof( TEvent ) ) ) ) )
            .Go();
    }

    private sealed class Invalid
    {
        public TEvent? Event { get; set; }
    }
}
