using System.Collections.Generic;
using FluentAssertions.Execution;
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
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsAccept( a )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.IsAccepted();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsAccepted_ShouldReturnTrue_WhenCurrentStateIsMarkedAsAccept()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsAccept( b )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.IsAccepted();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsAccepted_ShouldReturnFalse_WhenCurrentStateIsMarkedAsInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.IsAccepted();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAccepted_ShouldReturnFalse_WhenCurrentStateIsMarkedAsDefault()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsDefault( b )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.IsAccepted();

        result.Should().BeFalse();
    }

    [Fact]
    public void CanTransition_ShouldReturnTrue_WhenCurrentStateHasAnyTransition()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, 0 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanTransition();

        result.Should().BeTrue();
    }

    [Fact]
    public void CanTransition_ShouldReturnFalse_WhenCurrentStateDoesNotHaveAnyTransitions()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanTransition();

        result.Should().BeFalse();
    }

    [Fact]
    public void CanTransitionTo_ShouldReturnTrue_WhenAnyTransitionFromCurrentStateToDestinationExists()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( a, b, 1 )
            .AddTransition( a, c, 2 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanTransitionTo( b );

        result.Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_ShouldReturnFalse_WhenNoTransitionFromCurrentStateToDestinationExists()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( a, b, 1 )
            .MarkAsDefault( c )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanTransitionTo( c );

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( -1, false )]
    [InlineData( 0, true )]
    [InlineData( 1, true )]
    [InlineData( 2, false )]
    public void CanTransition_WithInput_ShouldReturnCorrectResult(int input, bool expected)
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, 0 )
            .AddTransition( a, 1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.CanTransition( input );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetAvailableDestinations_ShouldReturnAllExistingTransitionDestinationsForCurrentState()
    {
        var (a, b) = ("a", "b");
        var (_0, _1, _2) = (0, 1, 2);

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( a, b, _2 )
            .AddTransition( b, _0 )
            .AddTransition( b, a, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.GetAvailableDestinations();

        result.Should().BeEquivalentTo( sut.Machine.States[a], sut.Machine.States[b] );
    }

    [Fact]
    public void FindTransitionsTo_ShouldReturnCorrectTransitionsFromCurrentStateToDestinationState()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( a, b, 1 )
            .AddTransition( a, c, 2 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.FindTransitionsTo( b );

        result.Should()
            .BeEquivalentTo(
                KeyValuePair.Create( 0, sut.Machine.States[a].Transitions[0] ),
                KeyValuePair.Create( 1, sut.Machine.States[a].Transitions[1] ) );
    }

    [Fact]
    public void GetTransition_ShouldThrowStateMachineTransitionException_WhenTransitionInCurrentStateDoesNotExist()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var action = Lambda.Of( () => sut.GetTransition( 0 ) );

        action.Should().ThrowExactly<StateMachineTransitionException>();
    }

    [Fact]
    public void GetTransition_ShouldReturnCorrectResult_WhenTransitionInCurrentStateExists()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, 0 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( a );

        var result = sut.GetTransition( 0 );

        result.Should().BeSameAs( sut.Machine.States[a].Transitions[0] );
    }

    [Fact]
    public void Clone_ShouldReturnCorrectResult_WhenInstanceDoesNotHaveCustomSubject()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.Clone();

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.CurrentState );
            result.Machine.Should().BeSameAs( sut.Machine );
            result.Subject.Should().BeSameAs( result );
        }
    }

    [Fact]
    public void Clone_ShouldReturnCorrectResult_WhenInstanceHasCustomSubject()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstanceWithSubject( b, subject );

        var result = sut.Clone();

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.CurrentState );
            result.Machine.Should().BeSameAs( sut.Machine );
            result.Subject.Should().BeSameAs( subject );
        }
    }

    [Fact]
    public void Clone_WithSubject_ShouldReturnCorrectResult()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.CreateInstance( b );

        var result = sut.Clone( subject );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.CurrentState );
            result.Machine.Should().BeSameAs( sut.Machine );
            result.Subject.Should().BeSameAs( subject );
        }
    }

    [Fact]
    public void IStateMachineInstanceClone_ShouldReturnCorrectResult_WhenInstanceDoesNotHaveCustomSubject()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        IStateMachineInstance<string, int, string> sut = machine.CreateInstance( b );

        var result = sut.Clone();

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.CurrentState );
            result.Machine.Should().BeSameAs( sut.Machine );
            result.Subject.Should().BeSameAs( result );
        }
    }

    [Fact]
    public void IStateMachineInstanceClone_ShouldReturnCorrectResult_WhenInstanceHasCustomSubject()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        IStateMachineInstance<string, int, string> sut = machine.CreateInstanceWithSubject( b, subject );

        var result = sut.Clone();

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.CurrentState );
            result.Machine.Should().BeSameAs( sut.Machine );
            result.Subject.Should().BeSameAs( subject );
        }
    }

    [Fact]
    public void IStateMachineInstanceClone_WithSubject_ShouldReturnCorrectResult()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);
        var subject = new object();

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, c, _1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        IStateMachineInstance<string, int, string> sut = machine.CreateInstance( b );

        var result = sut.Clone( subject );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.CurrentState.Should().BeSameAs( sut.CurrentState );
            result.Machine.Should().BeSameAs( sut.Machine );
            result.Subject.Should().BeSameAs( subject );
        }
    }
}
