using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Automata.Tests.StateMachineTests;

public class StateMachineTests : TestsBase
{
    [Fact]
    public void CreateInstance_ShouldReturnCorrectResult()
    {
        var (a, b, c) = ("a", "b", "c");

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CreateInstance();

        using ( new AssertionScope() )
        {
            result.Machine.Should().BeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.InitialState );
            result.Subject.Should().BeSameAs( result );
        }
    }

    [Fact]
    public void CreateInstance_WithExplicitInitialState_ShouldReturnCorrectResult_WhenStateExists()
    {
        var (a, b, c) = ("a", "b", "c");

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CreateInstance( c );

        using ( new AssertionScope() )
        {
            result.Machine.Should().BeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.States[c] );
            result.Subject.Should().BeSameAs( result );
        }
    }

    [Fact]
    public void CreateInstance_WithExplicitInitialState_ShouldThrowStateMachineStateException_WhenStateDoesNotExist()
    {
        var (a, b, c, d) = ("a", "b", "c", "d");

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.CreateInstance( d ) );

        action.Should().ThrowExactly<StateMachineStateException>();
    }

    [Fact]
    public void CreateInstanceWithSubject_ShouldReturnCorrectResult()
    {
        var (a, b, c) = ("a", "b", "c");
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CreateInstanceWithSubject( subject );

        using ( new AssertionScope() )
        {
            result.Machine.Should().BeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.InitialState );
            result.Subject.Should().BeSameAs( subject );
        }
    }

    [Fact]
    public void CreateInstanceWithSubject_WithExplicitInitialState_ShouldReturnCorrectResult_WhenStateExists()
    {
        var (a, b, c) = ("a", "b", "c");
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CreateInstanceWithSubject( c, subject );

        using ( new AssertionScope() )
        {
            result.Machine.Should().BeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.States[c] );
            result.Subject.Should().BeSameAs( subject );
        }
    }

    [Fact]
    public void CreateInstanceWithSubject_WithExplicitInitialState_ShouldThrowStateMachineStateException_WhenStateDoesNotExist()
    {
        var (a, b, c, d) = ("a", "b", "c", "d");
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.CreateInstanceWithSubject( d, subject ) );

        action.Should().ThrowExactly<StateMachineStateException>();
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

        result.Should().BeSameAs( sut );
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

        result.Should().BeSameAs( sut );
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

        result.Should().BeSameAs( sut );
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

        result.Should().BeSameAs( sut );
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

        result.Should().BeSameAs( sut );
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

        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void WithOptimization_ShouldReturnNewStateMachine_WhenCurrentOptimizationIsNoneAndNewOptimizationRemovesUnreachableStates()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsDefault( b )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.States.Should().NotBeSameAs( sut.States );
            result.Optimization.Should().Be( StateMachineOptimization.RemoveUnreachableStates );
            result.DefaultResult.Should().Be( sut.DefaultResult );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.InitialState.Should().BeSameAs( sut.InitialState );
            result.States.Should().HaveCount( 1 );
            result.States.Keys.Should().BeEquivalentTo( a );
        }
    }

    [Fact]
    public void WithOptimization_ShouldReturnNewStateMachine_WhenCurrentOptimizationIsNoneAndNewOptimizationMinimizes()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( b, 0 )
            .MarkAsDefault( c )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization(
            StateMachineOptimizationParams<string>.Minimize(
                (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.States.Should().NotBeSameAs( sut.States );
            result.Optimization.Should().Be( StateMachineOptimization.Minimize );
            result.DefaultResult.Should().Be( sut.DefaultResult );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.InitialState.Should().NotBeSameAs( sut.InitialState );
            result.States.Should().HaveCount( 1 );
            result.States.Keys.Should().BeEquivalentTo( a + b );
            result.States[a + b].Should().BeSameAs( result.InitialState );
        }
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
            StateMachineOptimizationParams<string>.Minimize(
                (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.States.Should().NotBeSameAs( sut.States );
            result.Optimization.Should().Be( StateMachineOptimization.Minimize );
            result.DefaultResult.Should().Be( sut.DefaultResult );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.InitialState.Should().NotBeSameAs( sut.InitialState );
            result.States.Should().HaveCount( 1 );
            result.States.Keys.Should().BeEquivalentTo( a + b );
            result.States[a + b].Should().BeSameAs( result.InitialState );
        }
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
            StateMachineOptimizationParams<string>.Minimize(
                (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.States.Should().NotBeSameAs( sut.States );
            result.Optimization.Should().Be( StateMachineOptimization.Minimize );
            result.DefaultResult.Should().Be( sut.DefaultResult );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.InitialState.Should().BeSameAs( sut.InitialState );
            result.States.Should().HaveCount( 2 );
            result.States.Keys.Should().BeEquivalentTo( a, b );
            result.States[a].Should().BeSameAs( result.InitialState );
            result.States[b].Should().BeSameAs( sut.States[b] );
        }
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
            StateMachineOptimizationParams<string>.Minimize(
                (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.States.Should().NotBeSameAs( sut.States );
            result.Optimization.Should().Be( StateMachineOptimization.Minimize );
            result.DefaultResult.Should().Be( sut.DefaultResult );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.InitialState.Should().BeSameAs( sut.InitialState );
            result.States.Should().HaveCount( 2 );
            result.States.Keys.Should().BeEquivalentTo( a, b );
            result.States[a].Should().BeSameAs( result.InitialState );
            result.States[b].Should().BeSameAs( sut.States[b] );
        }
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
            StateMachineOptimizationParams<string>.Minimize(
                (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.States.Should().NotBeSameAs( sut.States );
            result.Optimization.Should().Be( StateMachineOptimization.Minimize );
            result.DefaultResult.Should().Be( sut.DefaultResult );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.InitialState.Should().BeSameAs( sut.InitialState );
            result.States.Should().HaveCount( 2 );
            result.States.Keys.Should().BeEquivalentTo( a, b );
            result.States[a].Should().BeSameAs( result.InitialState );
            result.States[b].Should().BeSameAs( sut.States[b] );
        }
    }

    [Fact]
    public void IStateMachineCreateInstance_ShouldBeEquivalentToCreateInstance()
    {
        var a = "a";

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, 0 )
            .MarkAsInitial( a );

        IStateMachine<string, int, string> sut = builder.Build();

        var result = sut.CreateInstance();

        using ( new AssertionScope() )
        {
            result.Machine.Should().BeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.InitialState );
            result.Subject.Should().BeSameAs( result );
        }
    }

    [Fact]
    public void IStateMachineCreateInstance_WithExplicitInitialState_ShouldBeEquivalentToCreateInstance()
    {
        var (a, b) = ("a", "b");

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .MarkAsInitial( a );

        IStateMachine<string, int, string> sut = builder.Build();

        var result = sut.CreateInstance( b );

        using ( new AssertionScope() )
        {
            result.Machine.Should().BeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.States[b] );
            result.Subject.Should().BeSameAs( result );
        }
    }

    [Fact]
    public void IStateMachineCreateInstanceWithSubject_ShouldBeEquivalentToCreateInstanceWithSubject()
    {
        var a = "a";
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, 0 )
            .MarkAsInitial( a );

        IStateMachine<string, int, string> sut = builder.Build();

        var result = sut.CreateInstanceWithSubject( subject );

        using ( new AssertionScope() )
        {
            result.Machine.Should().BeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.InitialState );
            result.Subject.Should().BeSameAs( subject );
        }
    }

    [Fact]
    public void IStateMachineCreateInstanceWithSubject_WithExplicitInitialState_ShouldBeEquivalentToCreateInstanceWithSubject()
    {
        var (a, b) = ("a", "b");
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .MarkAsInitial( a );

        IStateMachine<string, int, string> sut = builder.Build();

        var result = sut.CreateInstanceWithSubject( b, subject );

        using ( new AssertionScope() )
        {
            result.Machine.Should().BeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.States[b] );
            result.Subject.Should().BeSameAs( subject );
        }
    }

    [Fact]
    public void IStateMachineWithOptimization_ShouldBeEquivalentToWithOptimization()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsDefault( b )
            .MarkAsInitial( a );

        IStateMachine<string, int, string> sut = builder.Build();

        var result = sut.WithOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.Optimization.Should().Be( StateMachineOptimization.RemoveUnreachableStates );
            result.States.Should().HaveCount( 1 );
            result.States.Keys.Should().BeEquivalentTo( a );
        }
    }
}
