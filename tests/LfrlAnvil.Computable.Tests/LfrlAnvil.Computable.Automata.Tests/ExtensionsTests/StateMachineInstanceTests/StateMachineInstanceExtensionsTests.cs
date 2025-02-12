using System.Collections.Generic;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Computable.Automata.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Automata.Tests.ExtensionsTests.StateMachineInstanceTests;

public class StateMachineInstanceExtensionsTests : TestsBase
{
    [Fact]
    public void IsAccepted_ShouldReturnTrue_WhenCurrentStateIsMarkedAsAcceptAndInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsAccept( a ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.IsAccepted();

        result.TestTrue().Go();
    }

    [Fact]
    public void IsAccepted_ShouldReturnFalse_WhenCurrentStateIsMarkedAsDeadAndInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s1, s2) => s1 + s2 ) )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.IsAccepted();

        result.TestFalse().Go();
    }

    [Fact]
    public void IsAccepted_ShouldReturnTrue_WhenCurrentStateIsMarkedAsAccept()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsAccept( b ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.IsAccepted();

        result.TestTrue().Go();
    }

    [Fact]
    public void IsAccepted_ShouldReturnFalse_WhenCurrentStateIsMarkedAsInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.IsAccepted();

        result.TestFalse().Go();
    }

    [Fact]
    public void IsAccepted_ShouldReturnFalse_WhenCurrentStateIsMarkedAsDefault()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsDefault( b ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.IsAccepted();

        result.TestFalse().Go();
    }

    [Fact]
    public void IsAccepted_ShouldReturnFalse_WhenCurrentStateIsMarkedAsDead()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s1, s2) => s1 + s2 ) )
            .AddTransition( a, b, 0 )
            .MarkAsInitial( a )
            .MarkAsAccept( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.IsAccepted();

        result.TestFalse().Go();
    }

    [Fact]
    public void CanAccept_ShouldReturnTrue_WhenCurrentStateIsMarkedAsAcceptAndInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsAccept( a ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanAccept();

        result.TestTrue().Go();
    }

    [Fact]
    public void CanAccept_ShouldReturnFalse_WhenCurrentStateIsMarkedAsDeadAndInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s1, s2) => s1 + s2 ) )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanAccept();

        result.TestFalse().Go();
    }

    [Fact]
    public void CanAccept_ShouldReturnTrue_WhenCurrentStateIsMarkedAsAccept()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsAccept( b ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.CanAccept();

        result.TestTrue().Go();
    }

    [Fact]
    public void CanAccept_ShouldReturnTrue_WhenCurrentStateIsMarkedAsInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanAccept();

        result.TestTrue().Go();
    }

    [Fact]
    public void CanAccept_ShouldReturnTrue_WhenCurrentStateIsMarkedAsDefault()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsDefault( b ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.CanAccept();

        result.TestTrue().Go();
    }

    [Fact]
    public void CanAccept_ShouldReturnFalse_WhenCurrentStateIsMarkedAsDead()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s1, s2) => s1 + s2 ) )
            .AddTransition( a, b, 0 )
            .MarkAsInitial( a )
            .MarkAsAccept( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.CanAccept();

        result.TestFalse().Go();
    }

    [Fact]
    public void CanTransition_ShouldReturnTrue_WhenCurrentStateHasAnyTransition()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, 0 ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanTransition();

        result.TestTrue().Go();
    }

    [Fact]
    public void CanTransition_ShouldReturnFalse_WhenCurrentStateDoesNotHaveAnyTransitions()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanTransition();

        result.TestFalse().Go();
    }

    [Fact]
    public void CanTransitionTo_ShouldReturnTrue_WhenAnyTransitionFromCurrentStateToDestinationExists()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 )
            .AddTransition( a, b, 1 )
            .AddTransition( a, c, 2 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanTransitionTo( b );

        result.TestTrue().Go();
    }

    [Fact]
    public void CanTransitionTo_ShouldReturnFalse_WhenNoTransitionFromCurrentStateToDestinationExists()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 )
            .AddTransition( a, b, 1 )
            .MarkAsDefault( c )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanTransitionTo( c );

        result.TestFalse().Go();
    }

    [Theory]
    [InlineData( -1, false )]
    [InlineData( 0, true )]
    [InlineData( 1, true )]
    [InlineData( 2, false )]
    public void CanTransition_WithInput_ShouldReturnCorrectResult(int input, bool expected)
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, 0 )
            .AddTransition( a, 1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanTransition( input );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetAvailableDestinations_ShouldReturnAllExistingTransitionDestinationsForCurrentState()
    {
        var (a, b) = ("a", "b");
        var (_0, _1, _2) = (0, 1, 2);

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( a, b, _2 )
            .AddTransition( b, _0 )
            .AddTransition( b, a, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.GetAvailableDestinations();

        result.TestSetEqual( [ sut.Machine.States[a], sut.Machine.States[b] ] ).Go();
    }

    [Fact]
    public void FindTransitionsTo_ShouldReturnCorrectTransitionsFromCurrentStateToDestinationState()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 )
            .AddTransition( a, b, 1 )
            .AddTransition( a, c, 2 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.FindTransitionsTo( b );

        result.TestSetEqual(
            [
                KeyValuePair.Create( 0, sut.Machine.States[a].Transitions[0] ),
                KeyValuePair.Create( 1, sut.Machine.States[a].Transitions[1] )
            ] )
            .Go();
    }

    [Fact]
    public void GetTransition_ShouldThrowStateMachineTransitionException_WhenTransitionInCurrentStateDoesNotExist()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var action = Lambda.Of( () => sut.GetTransition( 0 ) );

        action.Test( exc => exc.TestType().Exact<StateMachineTransitionException>() ).Go();
    }

    [Fact]
    public void GetTransition_ShouldReturnCorrectResult_WhenTransitionInCurrentStateExists()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, 0 ).MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.GetTransition( 0 );

        result.TestRefEquals( sut.Machine.States[a].Transitions[0] ).Go();
    }

    [Fact]
    public void Clone_ShouldReturnCorrectResult_WhenInstanceDoesNotHaveCustomSubject()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.Clone();

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.CurrentState ),
                result.Machine.TestRefEquals( sut.Machine ),
                result.Subject.TestRefEquals( result ) )
            .Go();
    }

    [Fact]
    public void Clone_ShouldReturnCorrectResult_WhenInstanceHasCustomSubject()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstanceWithSubject( b, subject );

        var result = sut.Clone();

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.CurrentState ),
                result.Machine.TestRefEquals( sut.Machine ),
                result.Subject.TestRefEquals( subject ) )
            .Go();
    }

    [Fact]
    public void Clone_WithSubject_ShouldReturnCorrectResult()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.Clone( subject );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.CurrentState ),
                result.Machine.TestRefEquals( sut.Machine ),
                result.Subject.TestRefEquals( subject ) )
            .Go();
    }

    [Fact]
    public void IStateMachineInstanceClone_ShouldReturnCorrectResult_WhenInstanceDoesNotHaveCustomSubject()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        IStateMachineInstance<string, int, string> sut = machine.CreateInstance( b );

        var result = sut.Clone();

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.CurrentState ),
                result.Machine.TestRefEquals( sut.Machine ),
                result.Subject.TestRefEquals( result ) )
            .Go();
    }

    [Fact]
    public void IStateMachineInstanceClone_ShouldReturnCorrectResult_WhenInstanceHasCustomSubject()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        IStateMachineInstance<string, int, string> sut = machine.CreateInstanceWithSubject( b, subject );

        var result = sut.Clone();

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.CurrentState ),
                result.Machine.TestRefEquals( sut.Machine ),
                result.Subject.TestRefEquals( subject ) )
            .Go();
    }

    [Fact]
    public void IStateMachineInstanceClone_WithSubject_ShouldReturnCorrectResult()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        IStateMachineInstance<string, int, string> sut = machine.CreateInstance( b );

        var result = sut.Clone( subject );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.CurrentState ),
                result.Machine.TestRefEquals( sut.Machine ),
                result.Subject.TestRefEquals( subject ) )
            .Go();
    }
}
