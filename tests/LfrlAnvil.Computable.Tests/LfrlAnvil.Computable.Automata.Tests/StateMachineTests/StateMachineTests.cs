using System.Linq;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Automata.Tests.StateMachineTests;

public class StateMachineTests : TestsBase
{
    [Fact]
    public void CreateInstance_ShouldReturnCorrectResult()
    {
        var (a, b, c) = ("a", "b", "c");

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CreateInstance();

        Assertion.All(
                result.Machine.TestRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.InitialState ),
                result.Subject.TestRefEquals( result ) )
            .Go();
    }

    [Fact]
    public void CreateInstance_WithExplicitInitialState_ShouldReturnCorrectResult_WhenStateExists()
    {
        var (a, b, c) = ("a", "b", "c");

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CreateInstance( c );

        Assertion.All(
                result.Machine.TestRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.States[c] ),
                result.Subject.TestRefEquals( result ) )
            .Go();
    }

    [Fact]
    public void CreateInstance_WithExplicitInitialState_ShouldThrowStateMachineStateException_WhenStateDoesNotExist()
    {
        var (a, b, c, d) = ("a", "b", "c", "d");

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.CreateInstance( d ) );

        action.Test( exc => exc.TestType().Exact<StateMachineStateException>() ).Go();
    }

    [Fact]
    public void CreateInstanceWithSubject_ShouldReturnCorrectResult()
    {
        var (a, b, c) = ("a", "b", "c");
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CreateInstanceWithSubject( subject );

        Assertion.All(
                result.Machine.TestRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.InitialState ),
                result.Subject.TestRefEquals( subject ) )
            .Go();
    }

    [Fact]
    public void CreateInstanceWithSubject_WithExplicitInitialState_ShouldReturnCorrectResult_WhenStateExists()
    {
        var (a, b, c) = ("a", "b", "c");
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CreateInstanceWithSubject( c, subject );

        Assertion.All(
                result.Machine.TestRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.States[c] ),
                result.Subject.TestRefEquals( subject ) )
            .Go();
    }

    [Fact]
    public void CreateInstanceWithSubject_WithExplicitInitialState_ShouldThrowStateMachineStateException_WhenStateDoesNotExist()
    {
        var (a, b, c, d) = ("a", "b", "c", "d");
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.CreateInstanceWithSubject( d, subject ) );

        action.Test( exc => exc.TestType().Exact<StateMachineStateException>() ).Go();
    }

    [Fact]
    public void WithOptimization_ShouldDoNothingAndReturnSelf_WhenCurrentAndNewOptimizationIsNone()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.None() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization( StateMachineOptimizationParams<string>.None() );

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void WithOptimization_ShouldDoNothingAndReturnSelf_WhenCurrentOptimizationRemovesUnreachableStatesAndNewOptimizationIsNone()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization( StateMachineOptimizationParams<string>.None() );

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void WithOptimization_ShouldDoNothingAndReturnSelf_WhenCurrentAndNewOptimizationRemoveUnreachableStates()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() );

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void WithOptimization_ShouldDoNothingAndReturnSelf_WhenCurrentOptimizationMinimizedAndNewOptimizationIsNone()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s, _) => s ) )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization( StateMachineOptimizationParams<string>.None() );

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void WithOptimization_ShouldDoNothingAndReturnSelf_WhenCurrentOptimizationMinimizedAndNewOptimizationRemovesUnreachableStates()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s, _) => s ) )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() );

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void WithOptimization_ShouldDoNothingAndReturnSelf_WhenCurrentAndNewOptimizationMinimize()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s, _) => s ) )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization( StateMachineOptimizationParams<string>.Minimize( (s, _) => s ) );

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void WithOptimization_ShouldReturnNewStateMachine_WhenCurrentOptimizationIsNoneAndNewOptimizationRemovesUnreachableStates()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsDefault( b ).MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.States.TestNotRefEquals( sut.States ),
                result.Optimization.TestEquals( StateMachineOptimization.RemoveUnreachableStates ),
                result.DefaultResult.TestEquals( sut.DefaultResult ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.InitialState.TestRefEquals( sut.InitialState ),
                result.States.Count.TestEquals( 1 ),
                result.States.Keys.TestSetEqual( [ a ] ) )
            .Go();
    }

    [Fact]
    public void WithOptimization_ShouldReturnNewStateMachine_WhenCurrentOptimizationIsNoneAndNewOptimizationMinimizes()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 )
            .AddTransition( b, 0 )
            .MarkAsDefault( c )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization(
            StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.States.TestNotRefEquals( sut.States ),
                result.Optimization.TestEquals( StateMachineOptimization.Minimize ),
                result.DefaultResult.TestEquals( sut.DefaultResult ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.InitialState.TestNotRefEquals( sut.InitialState ),
                result.States.Count.TestEquals( 1 ),
                result.States.Keys.TestSetEqual( [ a + b ] ),
                result.States[a + b].TestRefEquals( result.InitialState ) )
            .Go();
    }

    [Fact]
    public void WithOptimization_ShouldReturnNewStateMachine_WhenCurrentOptimizationIsRemoveUnreachableStatesAndNewOptimizationMinimizes()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization(
            StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.States.TestNotRefEquals( sut.States ),
                result.Optimization.TestEquals( StateMachineOptimization.Minimize ),
                result.DefaultResult.TestEquals( sut.DefaultResult ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.InitialState.TestNotRefEquals( sut.InitialState ),
                result.States.Count.TestEquals( 1 ),
                result.States.Keys.TestSetEqual( [ a + b ] ),
                result.States[a + b].TestRefEquals( result.InitialState ) )
            .Go();
    }

    [Fact]
    public void
        WithOptimization_ShouldReturnNewStateMachine_WhenStateMachineIsMinimizedAndCurrentOptimizationHasRemovedUnreachableStatesDuringMinimization()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, 0 )
            .MarkAsInitial( a )
            .MarkAsAccept( b );

        var sut = builder.Build();

        var result = sut.WithOptimization(
            StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.States.TestNotRefEquals( sut.States ),
                result.Optimization.TestEquals( StateMachineOptimization.Minimize ),
                result.DefaultResult.TestEquals( sut.DefaultResult ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.InitialState.TestRefEquals( sut.InitialState ),
                result.States.Count.TestEquals( 2 ),
                result.States.Keys.TestSetEqual( [ a, b ] ),
                result.States[a].TestRefEquals( result.InitialState ),
                result.States[b].TestRefEquals( sut.States[b] ) )
            .Go();
    }

    [Fact]
    public void WithOptimization_ShouldReturnNewStateMachine_WhenStateMachineIsMinimizedAndAllStatesAreReachableDuringMinimization()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.None() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, 0 )
            .MarkAsInitial( a )
            .MarkAsAccept( b );

        var sut = builder.Build();

        var result = sut.WithOptimization(
            StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.States.TestNotRefEquals( sut.States ),
                result.Optimization.TestEquals( StateMachineOptimization.Minimize ),
                result.DefaultResult.TestEquals( sut.DefaultResult ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.InitialState.TestRefEquals( sut.InitialState ),
                result.States.Count.TestEquals( 2 ),
                result.States.Keys.TestSetEqual( [ a, b ] ),
                result.States[a].TestRefEquals( result.InitialState ),
                result.States[b].TestRefEquals( sut.States[b] ) )
            .Go();
    }

    [Fact]
    public void WithOptimization_ShouldReturnNewStateMachine_WhenStateMachineIsMinimizedButSomeStatesAreNotReachableDuringMinimization()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.None() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, 0 )
            .MarkAsDefault( c )
            .MarkAsInitial( a )
            .MarkAsAccept( b );

        var sut = builder.Build();

        var result = sut.WithOptimization(
            StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.States.TestNotRefEquals( sut.States ),
                result.Optimization.TestEquals( StateMachineOptimization.Minimize ),
                result.DefaultResult.TestEquals( sut.DefaultResult ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.InitialState.TestRefEquals( sut.InitialState ),
                result.States.Count.TestEquals( 2 ),
                result.States.Keys.TestSetEqual( [ a, b ] ),
                result.States[a].TestRefEquals( result.InitialState ),
                result.States[b].TestRefEquals( sut.States[b] ) )
            .Go();
    }

    [Fact]
    public void IStateMachineCreateInstance_ShouldBeEquivalentToCreateInstance()
    {
        var a = "a";

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, 0 ).MarkAsInitial( a );

        IStateMachine<string, int, string> sut = builder.Build();

        var result = sut.CreateInstance();

        Assertion.All(
                result.Machine.TestRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.InitialState ),
                result.Subject.TestRefEquals( result ) )
            .Go();
    }

    [Fact]
    public void IStateMachineCreateInstance_WithExplicitInitialState_ShouldBeEquivalentToCreateInstance()
    {
        var (a, b) = ("a", "b");

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 ).MarkAsInitial( a );

        IStateMachine<string, int, string> sut = builder.Build();

        var result = sut.CreateInstance( b );

        Assertion.All(
                result.Machine.TestRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.States[b] ),
                result.Subject.TestRefEquals( result ) )
            .Go();
    }

    [Fact]
    public void IStateMachineCreateInstanceWithSubject_ShouldBeEquivalentToCreateInstanceWithSubject()
    {
        var a = "a";
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, 0 ).MarkAsInitial( a );

        IStateMachine<string, int, string> sut = builder.Build();

        var result = sut.CreateInstanceWithSubject( subject );

        Assertion.All(
                result.Machine.TestRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.InitialState ),
                result.Subject.TestRefEquals( subject ) )
            .Go();
    }

    [Fact]
    public void IStateMachineCreateInstanceWithSubject_WithExplicitInitialState_ShouldBeEquivalentToCreateInstanceWithSubject()
    {
        var (a, b) = ("a", "b");
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).AddTransition( a, b, 0 ).MarkAsInitial( a );

        IStateMachine<string, int, string> sut = builder.Build();

        var result = sut.CreateInstanceWithSubject( b, subject );

        Assertion.All(
                result.Machine.TestRefEquals( sut ),
                result.CurrentState.TestRefEquals( sut.States[b] ),
                result.Subject.TestRefEquals( subject ) )
            .Go();
    }

    [Fact]
    public void IStateMachineWithOptimization_ShouldBeEquivalentToWithOptimization()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() ).MarkAsDefault( b ).MarkAsInitial( a );

        IStateMachine<string, int, string> sut = builder.Build();

        var result = sut.WithOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.Optimization.TestEquals( StateMachineOptimization.RemoveUnreachableStates ),
                result.States.Count.TestEquals( 1 ),
                result.States.Keys.TestSetEqual( [ a ] ) )
            .Go();
    }
}
