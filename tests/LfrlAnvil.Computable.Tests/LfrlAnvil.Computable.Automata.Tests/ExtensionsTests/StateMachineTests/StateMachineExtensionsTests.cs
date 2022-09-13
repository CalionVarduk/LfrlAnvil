using System.Collections.Generic;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Computable.Automata.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Automata.Tests.ExtensionsTests.StateMachineTests;

public class StateMachineExtensionsTests : TestsBase
{
    [Fact]
    public void FindAcceptStates_ShouldReturnAllStatesMarkedAsAccept()
    {
        var (a, b, c, d) = ("a", "b", "c", "d");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsDefault( a )
            .MarkAsDefault( c )
            .MarkAsAccept( b )
            .MarkAsAccept( d )
            .MarkAsInitial( d );

        var sut = builder.Build();

        var result = sut.FindAcceptStates();

        result.Should().BeEquivalentTo( sut.States[b], sut.States[d] );
    }

    [Fact]
    public void FindDefaultStates_ShouldReturnAllStatesMarkedAsDefault()
    {
        var (a, b, c, d) = ("a", "b", "c", "d");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsDefault( a )
            .MarkAsAccept( c )
            .MarkAsAccept( b )
            .MarkAsDefault( d )
            .MarkAsInitial( d );

        var sut = builder.Build();

        var result = sut.FindDefaultStates();

        result.Should().BeEquivalentTo( sut.States[a], sut.States[d] );
    }

    [Fact]
    public void CanTransitionTo_ShouldReturnTrue_WhenAnyTransitionFromSourceToDestinationExists()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( a, b, 1 )
            .AddTransition( a, c, 2 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CanTransitionTo( a, b );

        result.Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_ShouldReturnFalse_WhenNoTransitionFromSourceToDestinationExists()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( a, b, 1 )
            .MarkAsDefault( c )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CanTransitionTo( a, c );

        result.Should().BeFalse();
    }

    [Fact]
    public void CanTransitionTo_ShouldReturnFalse_WhenSourceStateDoesNotExist()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CanTransitionTo( b, a );

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( -1, false )]
    [InlineData( 0, true )]
    [InlineData( 1, true )]
    [InlineData( 2, true )]
    [InlineData( 3, false )]
    public void CanTransition_ShouldReturnCorrectResult_WhenTransitionForSourceExists(int input, bool expected)
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( a, b, 1 )
            .AddTransition( a, c, 2 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CanTransition( a, input );

        result.Should().Be( expected );
    }

    [Fact]
    public void CanTransition_ShouldReturnFalse_WhenSourceStateDoesNotExist()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.CanTransition( b, 0 );

        result.Should().BeFalse();
    }

    [Fact]
    public void GetTransitions_ShouldReturnAllExistingTransitions()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, c, _0 )
            .AddTransition( c, _0 )
            .AddTransition( c, _1 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.GetTransitions();

        result.Should()
            .BeEquivalentTo(
                KeyValuePair.Create( sut.States[a], KeyValuePair.Create( _0, sut.States[a].Transitions[_0] ) ),
                KeyValuePair.Create( sut.States[a], KeyValuePair.Create( _1, sut.States[a].Transitions[_1] ) ),
                KeyValuePair.Create( sut.States[b], KeyValuePair.Create( _0, sut.States[b].Transitions[_0] ) ),
                KeyValuePair.Create( sut.States[c], KeyValuePair.Create( _0, sut.States[c].Transitions[_0] ) ),
                KeyValuePair.Create( sut.States[c], KeyValuePair.Create( _1, sut.States[c].Transitions[_1] ) ) );
    }

    [Fact]
    public void GetAlphabet_ShouldReturnAllPossibleInputs()
    {
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, c, _0 )
            .AddTransition( c, _0 )
            .AddTransition( c, _1 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.GetAlphabet();

        result.Should().BeEquivalentTo( _0, _1 );
    }

    [Fact]
    public void GetAvailableDestinations_ShouldReturnAllExistingTransitionDestinationsForSourceState()
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

        var sut = builder.Build();

        var result = sut.GetAvailableDestinations( a );

        result.Should().BeEquivalentTo( sut.States[a], sut.States[b] );
    }

    [Fact]
    public void GetAvailableDestinations_ShouldReturnEmptyResult_WhenSourceStateDoesNotExist()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.GetAvailableDestinations( b );

        result.Should().BeEmpty();
    }

    [Fact]
    public void FindTransitionsTo_ShouldReturnCorrectTransitionsFromSourceToDestinationState()
    {
        var (a, b, c) = ("a", "b", "c");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, b, 0 )
            .AddTransition( a, b, 1 )
            .AddTransition( a, c, 2 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.FindTransitionsTo( a, b );

        result.Should()
            .BeEquivalentTo(
                KeyValuePair.Create( 0, sut.States[a].Transitions[0] ),
                KeyValuePair.Create( 1, sut.States[a].Transitions[1] ) );
    }

    [Fact]
    public void FindTransitionsTo_ShouldReturnEmptyResult_WhenSourceStateDoesNotExist()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.FindTransitionsTo( b, a );

        result.Should().BeEmpty();
    }

    [Fact]
    public void TryGetTransition_ShouldReturnFalse_WhenSourceStateDoesNotExist()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.TryGetTransition( b, 0, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryGetTransition_ShouldReturnFalse_WhenSourceStateExistsButTransitionDoesNot()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.TryGetTransition( a, 0, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryGetTransition_ShouldReturnTrue_WhenSourceStateAndTransitionExist()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.TryGetTransition( a, 0, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( sut.States[a].Transitions[0] );
        }
    }

    [Fact]
    public void GetTransition_ShouldThrowStateMachineStateException_WhenSourceStateDoesNotExist()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.GetTransition( b, 0 ) );

        action.Should().ThrowExactly<StateMachineStateException>();
    }

    [Fact]
    public void GetTransition_ShouldThrowStateMachineTransitionException_WhenSourceStateExistsButTransitionDoesNot()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.GetTransition( a, 0 ) );

        action.Should().ThrowExactly<StateMachineTransitionException>();
    }

    [Fact]
    public void GetTransition_ShouldReturnCorrectResult_WhenSourceStateAndTransitionExist()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, 0 )
            .MarkAsInitial( a );

        var sut = builder.Build();

        var result = sut.GetTransition( a, 0 );

        result.Should().BeSameAs( sut.States[a].Transitions[0] );
    }
}
