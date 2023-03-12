using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Automata.Tests.StateMachineInstanceTests;

public class StateMachineInstanceTests : TestsBase
{
    [Fact]
    public void Transition_ShouldChangeStateAndInvokeHandlerWithCorrectArgs_WhenTransitionExistsForCurrentState()
    {
        var (a, b) = ("a", "b");
        var input = Fixture.Create<int>();
        var (expectedResult, defaultResult) = Fixture.CreateDistinctCollection<string>( count: 2 );

        var handler = Substitute.For<IStateTransitionHandler<string, int, string>>();
        handler.Handle( Arg.Any<StateTransitionHandlerArgs<string, int, string>>() ).Returns( expectedResult );

        var builder = new StateMachineBuilder<string, int, string>( defaultResult )
            .AddTransition( a, b, input, handler )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance();

        var result = sut.Transition( input );

        using ( new AssertionScope() )
        {
            sut.CurrentState.Should().BeSameAs( machine.States[b] );
            result.Should().Be( expectedResult );

            handler.VerifyCalls()
                .Received(
                    h => h.Handle(
                        new StateTransitionHandlerArgs<string, int, string>( sut, machine.InitialState, sut.CurrentState, input ) ),
                    count: 1 );
        }
    }

    [Fact]
    public void Transition_ShouldChangeStateAndInvokeHandlerWithCorrectArgs_WhenTransitionExistsForCurrentStateAndInstanceHasCustomSubject()
    {
        var (a, b) = ("a", "b");
        var input = Fixture.Create<int>();
        var (expectedResult, defaultResult) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var subject = new object();

        var handler = Substitute.For<IStateTransitionHandler<string, int, string>>();
        handler.Handle( Arg.Any<StateTransitionHandlerArgs<string, int, string>>() ).Returns( expectedResult );

        var builder = new StateMachineBuilder<string, int, string>( defaultResult )
            .AddTransition( a, b, input, handler )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstanceWithSubject( subject );

        var result = sut.Transition( input );

        using ( new AssertionScope() )
        {
            sut.CurrentState.Should().BeSameAs( machine.States[b] );
            result.Should().Be( expectedResult );

            handler.VerifyCalls()
                .Received(
                    h => h.Handle(
                        new StateTransitionHandlerArgs<string, int, string>( subject, machine.InitialState, sut.CurrentState, input ) ),
                    count: 1 );
        }
    }

    [Fact]
    public void Transition_ShouldChangeStateAndReturnDefaultResult_WhenTransitionWithoutHandlerExistsForCurrentState()
    {
        var (a, b) = ("a", "b");
        var input = Fixture.Create<int>();
        var defaultResult = Fixture.Create<string>();

        var builder = new StateMachineBuilder<string, int, string>( defaultResult )
            .AddTransition( a, b, input )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance();

        var result = sut.Transition( input );

        using ( new AssertionScope() )
        {
            sut.CurrentState.Should().BeSameAs( machine.States[b] );
            result.Should().Be( defaultResult );
        }
    }

    [Fact]
    public void Transition_ShouldThrowStateMachineTransitionException_WhenTransitionDoesNotExistForCurrentState()
    {
        var a = "a";
        var input = Fixture.Create<int>();
        var defaultResult = Fixture.Create<string>();

        var builder = new StateMachineBuilder<string, int, string>( defaultResult )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance();

        var action = Lambda.Of( () => sut.Transition( input ) );

        action.Should().ThrowExactly<StateMachineTransitionException>();
    }

    [Theory]
    [InlineData( 0, "b", "a(0) => b" )]
    [InlineData( 1, "c", "a(1) => c" )]
    [InlineData( 2, "a", "a(2) => a" )]
    public void Transition_ShouldChangeStateAndInvokeCorrectHandler_WhenMultipleTransitionsExistForInitialState(
        int input,
        string expectedState,
        string expectedResult)
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1, _2) = (0, 1, 2);
        var defaultResult = Fixture.Create<string>();

        var handler = StateTransitionHandler.Create<string, int, string>(
            args => $"{args.Source.Value}({args.Input}) => {args.Destination.Value}" );

        var builder = new StateMachineBuilder<string, int, string>( defaultResult )
            .AddTransition( a, b, _0, handler )
            .AddTransition( a, c, _1, handler )
            .AddTransition( a, _2, handler )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance();

        var result = sut.Transition( input );

        using ( new AssertionScope() )
        {
            sut.CurrentState.Should().BeSameAs( machine.States[expectedState] );
            result.Should().Be( expectedResult );
        }
    }

    [Theory]
    [InlineData( 0, "b", "b(0) => b" )]
    [InlineData( 1, "c", "b(1) => c" )]
    [InlineData( 2, "a", "b(2) => a" )]
    public void Transition_ShouldChangeStateAndInvokeCorrectHandler_WhenMultipleTransitionsExistForNonInitialState(
        int input,
        string expectedState,
        string expectedResult)
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1, _2) = (0, 1, 2);
        var defaultResult = Fixture.Create<string>();

        var handler = StateTransitionHandler.Create<string, int, string>(
            args => $"{args.Source.Value}({args.Input}) => {args.Destination.Value}" );

        var builder = new StateMachineBuilder<string, int, string>( defaultResult )
            .AddTransition( b, b, _0, handler )
            .AddTransition( b, c, _1, handler )
            .AddTransition( b, a, _2, handler )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.Transition( input );

        using ( new AssertionScope() )
        {
            sut.CurrentState.Should().BeSameAs( machine.States[expectedState] );
            result.Should().Be( expectedResult );
        }
    }
}
