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

    [Theory]
    [InlineData( StateMachineOptimization.None, StateMachineOptimization.None )]
    [InlineData( StateMachineOptimization.RemoveUnreachableStates, StateMachineOptimization.None )]
    [InlineData( StateMachineOptimization.RemoveUnreachableStates, StateMachineOptimization.RemoveUnreachableStates )]
    public void WithOptimization_ShouldDoNothingAndReturnSelf_WhenProvidedOptimizationIsNotMoreAdvancedThanTheCurrentOne(
        StateMachineOptimization current,
        StateMachineOptimization value)
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( current )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.WithOptimization( value );

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

        var result = sut.WithOptimization( StateMachineOptimization.RemoveUnreachableStates );

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
    public void WithOptimization_ShouldThrowArgumentException_WhenNewOptimizationIsNotDefinedInEnum()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.WithOptimization( (StateMachineOptimization)10 ) );

        action.Should().ThrowExactly<ArgumentException>();
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

        var result = sut.WithOptimization( StateMachineOptimization.RemoveUnreachableStates );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.Optimization.Should().Be( StateMachineOptimization.RemoveUnreachableStates );
            result.States.Should().HaveCount( 1 );
            result.States.Keys.Should().BeEquivalentTo( a );
        }
    }
}
