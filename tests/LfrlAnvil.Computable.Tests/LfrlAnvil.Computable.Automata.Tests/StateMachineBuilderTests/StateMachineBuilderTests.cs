using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Automata.Tests.StateMachineBuilderTests;

public class StateMachineBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyBuilder()
    {
        var defaultResult = Fixture.Create<string>();
        var sut = new StateMachineBuilder<string, int, string>( defaultResult );

        Assertion.All(
                sut.DefaultResult.TestEquals( defaultResult ),
                sut.Optimization.Level.TestEquals( StateMachineOptimization.None ),
                sut.StateComparer.TestRefEquals( EqualityComparer<string>.Default ),
                sut.InputComparer.TestRefEquals( EqualityComparer<int>.Default ),
                sut.GetStates().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_WithComparers_ShouldReturnEmptyBuilder()
    {
        var defaultResult = Fixture.Create<string>();
        var stateComparer = EqualityComparerFactory<string>.Create( (a, b) => a == b );
        var inputComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = new StateMachineBuilder<string, int, string>( defaultResult, stateComparer, inputComparer );

        Assertion.All(
                sut.DefaultResult.TestEquals( defaultResult ),
                sut.Optimization.Level.TestEquals( StateMachineOptimization.None ),
                sut.StateComparer.TestRefEquals( stateComparer ),
                sut.InputComparer.TestRefEquals( inputComparer ),
                sut.GetStates().TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetDefaultResult_ShouldUpdateDefaultResult()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new StateMachineBuilder<string, int, string>( oldValue );

        var result = sut.SetDefaultResult( value );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.DefaultResult.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void SetOptimization_ShouldUpdateOptimization_WhenParamsPointToNone()
    {
        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        var result = sut.SetOptimization( StateMachineOptimizationParams<string>.None() );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.Optimization.Level.TestEquals( StateMachineOptimization.None ) )
            .Go();
    }

    [Fact]
    public void SetOptimization_ShouldUpdateOptimization_WhenParamsPointToRemoveUnreachableStates()
    {
        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        var result = sut.SetOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.Optimization.Level.TestEquals( StateMachineOptimization.RemoveUnreachableStates ) )
            .Go();
    }

    [Fact]
    public void SetOptimization_ShouldUpdateOptimization_WhenParamsPointToMinimize()
    {
        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        var result = sut.SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s, _) => s ) );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.Optimization.Level.TestEquals( StateMachineOptimization.Minimize ) )
            .Go();
    }

    [Fact]
    public void AddTransition_ShouldAddNewTransition_WhenSourceAndDestinationStatesDoNotExist()
    {
        var (source, destination) = Fixture.CreateManyDistinct<string>( count: 2 );
        var input = Fixture.Create<int>();
        var expectedStates = new[]
        {
            KeyValuePair.Create( source, StateMachineNodeType.Default ),
            KeyValuePair.Create( destination, StateMachineNodeType.Default )
        };

        var expectedSourceTransitions = new[] { KeyValuePair.Create( input, destination ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );

        var result = sut.AddTransition( source, destination, input );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( source ).TestSetEqual( expectedSourceTransitions ),
                sut.GetTransitions( destination ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void AddTransition_ShouldAddNewTransition_WhenSourceStateExistsAndDestinationStateDoesNotExist()
    {
        var (source, destination) = Fixture.CreateManyDistinct<string>( count: 2 );
        var input = Fixture.Create<int>();
        var expectedStates = new[]
        {
            KeyValuePair.Create( source, StateMachineNodeType.Accept ), KeyValuePair.Create( destination, StateMachineNodeType.Default )
        };

        var expectedSourceTransitions = new[] { KeyValuePair.Create( input, destination ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( source );

        var result = sut.AddTransition( source, destination, input );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( source ).TestSetEqual( expectedSourceTransitions ),
                sut.GetTransitions( destination ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void AddTransition_ShouldAddNewTransition_WhenSourceStateDoesNotExistAndDestinationStateExists()
    {
        var (source, destination) = Fixture.CreateManyDistinct<string>( count: 2 );
        var input = Fixture.Create<int>();
        var expectedStates = new[]
        {
            KeyValuePair.Create( source, StateMachineNodeType.Default ), KeyValuePair.Create( destination, StateMachineNodeType.Accept )
        };

        var expectedSourceTransitions = new[] { KeyValuePair.Create( input, destination ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( destination );

        var result = sut.AddTransition( source, destination, input );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( source ).TestSetEqual( expectedSourceTransitions ),
                sut.GetTransitions( destination ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void AddTransition_ShouldAddNewTransition_WhenSourceAndDestinationStatesExist()
    {
        var (source, destination) = Fixture.CreateManyDistinct<string>( count: 2 );
        var input = Fixture.Create<int>();
        var expectedStates = new[]
        {
            KeyValuePair.Create( source, StateMachineNodeType.Accept ), KeyValuePair.Create( destination, StateMachineNodeType.Accept )
        };

        var expectedSourceTransitions = new[] { KeyValuePair.Create( input, destination ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( source );
        sut.MarkAsAccept( destination );

        var result = sut.AddTransition( source, destination, input );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( source ).TestSetEqual( expectedSourceTransitions ),
                sut.GetTransitions( destination ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void AddTransition_ShouldAddNewTransition_WhenSourceStateAlreadyHasOtherTransition()
    {
        var (source, destination, existingDestination) = Fixture.CreateManyDistinct<string>( count: 3 );
        var (input, existingInput) = Fixture.CreateManyDistinct<int>( count: 2 );
        var expectedStates = new[]
        {
            KeyValuePair.Create( source, StateMachineNodeType.Default ),
            KeyValuePair.Create( existingDestination, StateMachineNodeType.Default ),
            KeyValuePair.Create( destination, StateMachineNodeType.Default )
        };

        var expectedSourceTransitions = new[]
        {
            KeyValuePair.Create( existingInput, existingDestination ), KeyValuePair.Create( input, destination )
        };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.AddTransition( source, existingDestination, existingInput );

        var result = sut.AddTransition( source, destination, input );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( source ).TestSetEqual( expectedSourceTransitions ),
                sut.GetTransitions( destination ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void AddTransition_ToSelf_ShouldAddNewTransition()
    {
        var state = Fixture.Create<string>();
        var input = Fixture.Create<int>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Default ) };
        var expectedTransitions = new[] { KeyValuePair.Create( input, state ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );

        var result = sut.AddTransition( state, input );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestSetEqual( expectedTransitions ) )
            .Go();
    }

    [Fact]
    public void AddTransition_ShouldThrowStateMachineTransitionException_WhenTransitionFromSourceStateWithProvidedInputAlreadyExists()
    {
        var (source, destination, otherDestination) = Fixture.CreateManyDistinct<string>( count: 3 );
        var input = Fixture.Create<int>();

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.AddTransition( source, otherDestination, input );

        var action = Lambda.Of( () => sut.AddTransition( source, destination, input ) );

        action.Test( exc => exc.TestType().Exact<StateMachineTransitionException>() ).Go();
    }

    [Fact]
    public void MarkAsAccept_ShouldAddNewAcceptState_WhenStateDoesNotExist()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Accept ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );

        var result = sut.MarkAsAccept( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsAccept_ShouldChangeStateTypeToAccept_WhenStateExistsAndIsMarkedAsDefault()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Accept ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsDefault( state );

        var result = sut.MarkAsAccept( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsAccept_ShouldDoNothing_WhenStateExistsAndIsMarkedAsAccept()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Accept ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( state );

        var result = sut.MarkAsAccept( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsAccept_ShouldAddAcceptTypeToState_WhenStateExistsAndIsMarkedAsInitial()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Initial | StateMachineNodeType.Accept ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsInitial( state );

        var result = sut.MarkAsAccept( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsDefault_ShouldAddNewDefaultState_WhenStateDoesNotExist()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Default ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );

        var result = sut.MarkAsDefault( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsDefault_ShouldChangeStateTypeToDefault_WhenStateExistsAndIsMarkedAsAccept()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Default ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( state );

        var result = sut.MarkAsDefault( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsDefault_ShouldDoNothing_WhenStateExistsAndIsMarkedAsDefault()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Default ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsDefault( state );

        var result = sut.MarkAsDefault( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsDefault_ShouldAddDefaultTypeToState_WhenStateExistsAndIsMarkedAsInitial()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Initial ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsInitial( state );

        var result = sut.MarkAsDefault( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsInitial_ShouldAddNewInitialState_WhenStateDoesNotExist()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Initial ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );

        var result = sut.MarkAsInitial( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsInitial_ShouldAddInitialTypeToState_WhenStateExistsAndIsMarkedAsAccept()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Accept | StateMachineNodeType.Initial ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( state );

        var result = sut.MarkAsInitial( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsInitial_ShouldAddInitialTypeToState_WhenStateExistsAndIsMarkedAsDefault()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Initial ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsDefault( state );

        var result = sut.MarkAsInitial( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsDefault_ShouldDoNothing_WhenStateExistsAndIsMarkedAsInitial()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Initial ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsInitial( state );

        var result = sut.MarkAsInitial( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsDefault_ShouldReplaceActiveInitialState_WhenOtherStateIsMarkedAsInitial()
    {
        var (state, other) = Fixture.CreateManyDistinct<string>( count: 2 );
        var expectedStates = new[]
        {
            KeyValuePair.Create( state, StateMachineNodeType.Initial ), KeyValuePair.Create( other, StateMachineNodeType.Default )
        };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsInitial( other );

        var result = sut.MarkAsInitial( state );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetStates().TestSetEqual( expectedStates ),
                sut.GetTransitions( state ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void Build_ShouldThrowStateMachineCreationException_WhenNoStateIsMarkedAsInitial()
    {
        var state = Fixture.Create<string>();
        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsDefault( state );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<StateMachineCreationException>() ).Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenOnlySingleStateExistsMarkedAsInitial()
    {
        var defaultResult = Fixture.Create<string>();
        var state = Fixture.Create<string>();
        var sut = new StateMachineBuilder<string, int, string>( defaultResult );
        sut.MarkAsInitial( state );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 1 ),
                result.States.ContainsKey( result.InitialState.Value ).TestTrue(),
                result.InitialState.Value.TestEquals( state ),
                result.InitialState.Type.TestEquals( StateMachineNodeType.Initial ),
                result.InitialState.Transitions.TestEmpty(),
                result.States[result.InitialState.Value].TestRefEquals( result.InitialState ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_ForMoreComplexStateMachine()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c, d, e, f) = ("a", "b", "c", "d", "e", "f");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult ).AddTransition( a, b, _0 )
            .AddTransition( a, c, _1 )
            .AddTransition( b, a, _0 )
            .AddTransition( b, d, _1 )
            .AddTransition( c, e, _0 )
            .AddTransition( c, f, _1 )
            .AddTransition( d, e, _0 )
            .AddTransition( d, f, _1 )
            .AddTransition( e, _0 )
            .AddTransition( e, f, _1 )
            .AddTransition( f, _0 )
            .AddTransition( f, _1 )
            .MarkAsInitial( a )
            .MarkAsAccept( c )
            .MarkAsAccept( d )
            .MarkAsAccept( e );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 6 ),
                result.States.Keys.TestSetEqual( [ a, b, c, d, e, f ] ),
                result.InitialState.TestRefEquals( result.States[a] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[b] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[c] ),
                result.States[b].Type.TestEquals( StateMachineNodeType.Default ),
                result.States[b].Transitions.Count.TestEquals( 2 ),
                result.States[b].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[b].Transitions[_0].Destination.TestRefEquals( result.States[a] ),
                result.States[b].Transitions[_1].Destination.TestRefEquals( result.States[d] ),
                result.States[c].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[c].Transitions.Count.TestEquals( 2 ),
                result.States[c].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[c].Transitions[_0].Destination.TestRefEquals( result.States[e] ),
                result.States[c].Transitions[_1].Destination.TestRefEquals( result.States[f] ),
                result.States[d].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[d].Transitions.Count.TestEquals( 2 ),
                result.States[d].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[d].Transitions[_0].Destination.TestRefEquals( result.States[e] ),
                result.States[d].Transitions[_1].Destination.TestRefEquals( result.States[f] ),
                result.States[e].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[e].Transitions.Count.TestEquals( 2 ),
                result.States[e].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[e].Transitions[_0].Destination.TestRefEquals( result.States[e] ),
                result.States[e].Transitions[_1].Destination.TestRefEquals( result.States[f] ),
                result.States[f].Type.TestEquals( StateMachineNodeType.Default ),
                result.States[f].Transitions.Count.TestEquals( 2 ),
                result.States[f].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[f].Transitions[_0].Destination.TestRefEquals( result.States[f] ),
                result.States[f].Transitions[_1].Destination.TestRefEquals( result.States[f] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenOnlyInitialStateIsReachableAndRemovalOfUnreachableStatesIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() )
            .AddTransition( b, c, _0 )
            .AddTransition( b, a, _1 )
            .AddTransition( c, _0 )
            .AddTransition( c, b, _1 )
            .AddTransition( a, _0 )
            .MarkAsInitial( a );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 1 ),
                result.States.Keys.TestSetEqual( [ a ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 1 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[a] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenAllStatesAreReachableAndRemovalOfUnreachableStatesIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() )
            .AddTransition( a, b, _0 )
            .AddTransition( a, _1 )
            .AddTransition( b, a, _0 )
            .AddTransition( b, c, _1 )
            .AddTransition( c, _0 )
            .MarkAsInitial( a );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 3 ),
                result.States.Keys.TestSetEqual( [ a, b, c ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[b] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[a] ),
                result.States[b].Type.TestEquals( StateMachineNodeType.Default ),
                result.States[b].Transitions.Count.TestEquals( 2 ),
                result.States[b].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[b].Transitions[_0].Destination.TestRefEquals( result.States[a] ),
                result.States[b].Transitions[_1].Destination.TestRefEquals( result.States[c] ),
                result.States[c].Type.TestEquals( StateMachineNodeType.Default ),
                result.States[c].Transitions.Count.TestEquals( 1 ),
                result.States[c].Transitions.Keys.TestSetEqual( [ _0 ] ),
                result.States[c].Transitions[_0].Destination.TestRefEquals( result.States[c] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenNotAllStatesAreReachableAndRemovalOfUnreachableStatesIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c, d) = ("a", "b", "c", "d");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() )
            .AddTransition( a, b, _0 )
            .AddTransition( a, _1 )
            .AddTransition( b, a, _0 )
            .AddTransition( b, a, _1 )
            .AddTransition( c, _0 )
            .AddTransition( c, d, _1 )
            .AddTransition( d, c, _0 )
            .AddTransition( d, a, _1 )
            .MarkAsInitial( a );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 2 ),
                result.States.Keys.TestSetEqual( [ a, b ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[b] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[a] ),
                result.States[b].Type.TestEquals( StateMachineNodeType.Default ),
                result.States[b].Transitions.Count.TestEquals( 2 ),
                result.States[b].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[b].Transitions[_0].Destination.TestRefEquals( result.States[a] ),
                result.States[b].Transitions[_1].Destination.TestRefEquals( result.States[a] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenStateMachineIsMinimizedAndMinimizationIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b) = ("a", "b");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, b, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, _1 )
            .MarkAsInitial( a )
            .MarkAsAccept( b );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 2 ),
                result.States.Keys.TestSetEqual( [ a, b ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[b] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[b].Transitions.Count.TestEquals( 2 ),
                result.States[b].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[b].Transitions[_0].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Transitions[_1].Destination.TestRefEquals( result.States[b] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenStateMachineIsNotMinimizedAndMinimizationIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, c, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, _1 )
            .AddTransition( c, _0 )
            .AddTransition( c, _1 )
            .MarkAsInitial( a )
            .MarkAsAccept( b )
            .MarkAsAccept( c );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 2 ),
                result.States.Keys.TestSetEqual( [ a, b + c ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[b + c] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[b + c] ),
                result.States[b + c].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[b + c].Transitions.Count.TestEquals( 2 ),
                result.States[b + c].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[b + c].Transitions[_0].Destination.TestRefEquals( result.States[b + c] ),
                result.States[b + c].Transitions[_1].Destination.TestRefEquals( result.States[b + c] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenStateMachineIsNotMinimizedAndContainsUnreachableStatesAndMinimizationIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, _1 )
            .AddTransition( c, b, _0 )
            .AddTransition( c, _1 )
            .MarkAsInitial( a )
            .MarkAsAccept( a )
            .MarkAsAccept( b );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 1 ),
                result.States.Keys.TestSetEqual( [ a + b ] ),
                result.States[a + b].Type.TestEquals( StateMachineNodeType.Initial | StateMachineNodeType.Accept ),
                result.States[a + b].Transitions.Count.TestEquals( 2 ),
                result.States[a + b].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a + b].Transitions[_0].Destination.TestRefEquals( result.States[a + b] ),
                result.States[a + b].Transitions[_1].Destination.TestRefEquals( result.States[a + b] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenStateMachineIsMinimizedButContainsUnreachableStatesAndMinimizationIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, _1 )
            .AddTransition( c, b, _0 )
            .AddTransition( c, _1 )
            .MarkAsInitial( a )
            .MarkAsAccept( b );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 2 ),
                result.States.Keys.TestSetEqual( [ a, b ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[a] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[b].Transitions.Count.TestEquals( 2 ),
                result.States[b].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[b].Transitions[_0].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Transitions[_1].Destination.TestRefEquals( result.States[b] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenMinimizationIsEnabledAndTwoStatesCouldBeMergedButHaveDifferentAcceptFlag()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, c, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, _1 )
            .AddTransition( c, _0 )
            .AddTransition( c, _1 )
            .MarkAsInitial( a )
            .MarkAsDefault( b )
            .MarkAsAccept( c );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 3 ),
                result.States.Keys.TestSetEqual( [ a, b, c ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[c] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Type.TestEquals( StateMachineNodeType.Dead ),
                result.States[b].Transitions.Count.TestEquals( 2 ),
                result.States[b].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[b].Transitions[_0].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Transitions[_1].Destination.TestRefEquals( result.States[b] ),
                result.States[c].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[c].Transitions.Count.TestEquals( 2 ),
                result.States[c].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[c].Transitions[_0].Destination.TestRefEquals( result.States[c] ),
                result.States[c].Transitions[_1].Destination.TestRefEquals( result.States[c] ) )
            .Go();
    }

    [Fact]
    public void
        Build_ShouldReturnCorrectResult_WhenMinimizationIsEnabledAndTwoStatesCouldBeMergedButOneHasHandlerOnAnyOfItsTransitionsAndOtherDoesNot()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var handler = Substitute.For<IStateTransitionHandler<string, int, string>>();

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, c, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, _1, handler )
            .AddTransition( c, _0 )
            .AddTransition( c, _1 )
            .MarkAsInitial( a )
            .MarkAsAccept( b )
            .MarkAsAccept( c );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 3 ),
                result.States.Keys.TestSetEqual( [ a, b, c ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[c] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[b].Transitions.Count.TestEquals( 2 ),
                result.States[b].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[b].Transitions[_0].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Transitions[_1].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Transitions[_1].Handler.TestRefEquals( handler ),
                result.States[c].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[c].Transitions.Count.TestEquals( 2 ),
                result.States[c].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[c].Transitions[_0].Destination.TestRefEquals( result.States[c] ),
                result.States[c].Transitions[_1].Destination.TestRefEquals( result.States[c] ) )
            .Go();
    }

    [Fact]
    public void
        Build_ShouldReturnCorrectResult_WhenMinimizationIsEnabledAndTwoStatesCouldBeMergedButTheyHaveDifferentHandlerOnTheSameTransition()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var handlerB = Substitute.For<IStateTransitionHandler<string, int, string>>();
        var handlerC = Substitute.For<IStateTransitionHandler<string, int, string>>();

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, c, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, _1, handlerB )
            .AddTransition( c, _0 )
            .AddTransition( c, _1, handlerC )
            .MarkAsInitial( a )
            .MarkAsAccept( b )
            .MarkAsAccept( c );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 3 ),
                result.States.Keys.TestSetEqual( [ a, b, c ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[c] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[b].Transitions.Count.TestEquals( 2 ),
                result.States[b].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[b].Transitions[_0].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Transitions[_1].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Transitions[_1].Handler.TestRefEquals( handlerB ),
                result.States[c].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[c].Transitions.Count.TestEquals( 2 ),
                result.States[c].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[c].Transitions[_0].Destination.TestRefEquals( result.States[c] ),
                result.States[c].Transitions[_1].Destination.TestRefEquals( result.States[c] ),
                result.States[c].Transitions[_1].Handler.TestRefEquals( handlerC ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenMinimizationIsEnabledAndTwoStatesHaveHandlerAndCanBeMerged()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c) = ("a", "b", "c");
        var (_0, _1) = (0, 1);

        var handler = Substitute.For<IStateTransitionHandler<string, int, string>>();

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, c, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, _0 )
            .AddTransition( b, _1, handler )
            .AddTransition( c, _0 )
            .AddTransition( c, _1, handler )
            .MarkAsInitial( a )
            .MarkAsAccept( b )
            .MarkAsAccept( c );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 2 ),
                result.States.Keys.TestSetEqual( [ a, b + c ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[b + c] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[b + c] ),
                result.States[b + c].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[b + c].Transitions.Count.TestEquals( 2 ),
                result.States[b + c].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[b + c].Transitions[_0].Destination.TestRefEquals( result.States[b + c] ),
                result.States[b + c].Transitions[_1].Destination.TestRefEquals( result.States[b + c] ),
                result.States[b + c].Transitions[_1].Handler.TestRefEquals( handler ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenStateMachineIsMinimizedAndIsIncompleteAndMinimizationIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c, d) = ("a", "b", "c", "d");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, c, _0 )
            .AddTransition( a, b, _1 )
            .AddTransition( b, d, _1 )
            .AddTransition( c, d, _0 )
            .AddTransition( d, _0 )
            .AddTransition( d, _1 )
            .MarkAsInitial( a )
            .MarkAsAccept( d );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 4 ),
                result.States.Keys.TestSetEqual( [ a, b, c, d ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[c] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[b] ),
                result.States[b].Type.TestEquals( StateMachineNodeType.Default ),
                result.States[b].Transitions.Count.TestEquals( 1 ),
                result.States[b].Transitions.Keys.TestSetEqual( [ _1 ] ),
                result.States[b].Transitions[_1].Destination.TestRefEquals( result.States[d] ),
                result.States[c].Type.TestEquals( StateMachineNodeType.Default ),
                result.States[c].Transitions.Count.TestEquals( 1 ),
                result.States[c].Transitions.Keys.TestSetEqual( [ _0 ] ),
                result.States[c].Transitions[_0].Destination.TestRefEquals( result.States[d] ),
                result.States[d].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[d].Transitions.Count.TestEquals( 2 ),
                result.States[d].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[d].Transitions[_0].Destination.TestRefEquals( result.States[d] ),
                result.States[d].Transitions[_1].Destination.TestRefEquals( result.States[d] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenStateMachineCanBeMinimizedAndIsIncompleteAndMinimizationIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c, d) = ("a", "b", "c", "d");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, b, _0 )
            .AddTransition( a, c, _1 )
            .AddTransition( c, d, _0 )
            .MarkAsInitial( a )
            .MarkAsAccept( a );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 2 ),
                result.States.Keys.TestSetEqual( [ a, b + c + d ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial | StateMachineNodeType.Accept ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[b + c + d] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[b + c + d] ),
                result.States[b + c + d].Type.TestEquals( StateMachineNodeType.Dead ),
                result.States[b + c + d].Transitions.Count.TestEquals( 1 ),
                result.States[b + c + d].Transitions.Keys.TestSetEqual( [ _0 ] ),
                result.States[b + c + d].Transitions[_0].Destination.TestRefEquals( result.States[b + c + d] ) )
            .Go();
    }

    [Fact]
    public void
        Build_ShouldReturnCorrectResult_WhenStateMachineCanBeMinimizedAndIsIncompleteAndCandidateStateForMergingHasHandlerOnOneOfItsTransitionsAndMinimizationIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c, d) = ("a", "b", "c", "d");
        var (_0, _1) = (0, 1);

        var handler = Substitute.For<IStateTransitionHandler<string, int, string>>();

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, b, _0 )
            .AddTransition( a, c, _1 )
            .AddTransition( c, d, _0, handler )
            .MarkAsInitial( a )
            .MarkAsAccept( a );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 3 ),
                result.States.Keys.TestSetEqual( [ a, c, b + d ] ),
                result.States[a].Type.TestEquals( StateMachineNodeType.Initial | StateMachineNodeType.Accept ),
                result.States[a].Transitions.Count.TestEquals( 2 ),
                result.States[a].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a].Transitions[_0].Destination.TestRefEquals( result.States[b + d] ),
                result.States[a].Transitions[_1].Destination.TestRefEquals( result.States[c] ),
                result.States[c].Type.TestEquals( StateMachineNodeType.Default ),
                result.States[c].Transitions.Count.TestEquals( 1 ),
                result.States[c].Transitions.Keys.TestSetEqual( [ _0 ] ),
                result.States[c].Transitions[_0].Destination.TestRefEquals( result.States[b + d] ),
                result.States[c].Transitions[_0].Handler.TestRefEquals( handler ),
                result.States[b + d].Type.TestEquals( StateMachineNodeType.Dead ),
                result.States[b + d].Transitions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenAllStatesAreEquivalentAndMinimizationIsEnabled()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c, d, e) = ("a", "b", "c", "d", "e");

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, b, 0 )
            .AddTransition( b, c, 0 )
            .AddTransition( c, d, 0 )
            .AddTransition( d, e, 0 )
            .AddTransition( e, a, 0 )
            .MarkAsInitial( a )
            .MarkAsAccept( a )
            .MarkAsAccept( b )
            .MarkAsAccept( c )
            .MarkAsAccept( d )
            .MarkAsAccept( e );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 1 ),
                result.States.Keys.TestSetEqual( [ a + b + c + d + e ] ),
                result.States[a + b + c + d + e].Type.TestEquals( StateMachineNodeType.Initial | StateMachineNodeType.Accept ),
                result.States[a + b + c + d + e].Transitions.Count.TestEquals( 1 ),
                result.States[a + b + c + d + e].Transitions.Keys.TestSetEqual( [ 0 ] ),
                result.States[a + b + c + d + e].Transitions[0].Destination.TestRefEquals( result.States[a + b + c + d + e] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_ForMoreComplexStateMachineWithEnabledMinimization()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c, d, e, f) = ("a", "b", "c", "d", "e", "f");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
            .SetOptimization(
                StateMachineOptimizationParams<string>.Minimize( (s1, s2) => new string( s1.Concat( s2 ).OrderBy( x => x ).ToArray() ) ) )
            .AddTransition( a, b, _0 )
            .AddTransition( a, c, _1 )
            .AddTransition( b, a, _0 )
            .AddTransition( b, d, _1 )
            .AddTransition( c, e, _0 )
            .AddTransition( c, f, _1 )
            .AddTransition( d, e, _0 )
            .AddTransition( d, f, _1 )
            .AddTransition( e, _0 )
            .AddTransition( e, f, _1 )
            .AddTransition( f, _0 )
            .AddTransition( f, _1 )
            .MarkAsInitial( a )
            .MarkAsAccept( c )
            .MarkAsAccept( d )
            .MarkAsAccept( e );

        var result = sut.Build();

        Assertion.All(
                result.DefaultResult.TestEquals( defaultResult ),
                result.Optimization.TestEquals( sut.Optimization.Level ),
                result.StateComparer.TestRefEquals( sut.StateComparer ),
                result.InputComparer.TestRefEquals( sut.InputComparer ),
                result.States.Count.TestEquals( 3 ),
                result.States.Keys.TestSetEqual( [ a + b, c + d + e, f ] ),
                result.InitialState.TestRefEquals( result.States[a + b] ),
                result.States[a + b].Type.TestEquals( StateMachineNodeType.Initial ),
                result.States[a + b].Transitions.Count.TestEquals( 2 ),
                result.States[a + b].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[a + b].Transitions[_0].Destination.TestRefEquals( result.States[a + b] ),
                result.States[a + b].Transitions[_1].Destination.TestRefEquals( result.States[c + d + e] ),
                result.States[c + d + e].Type.TestEquals( StateMachineNodeType.Accept ),
                result.States[c + d + e].Transitions.Count.TestEquals( 2 ),
                result.States[c + d + e].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[c + d + e].Transitions[_0].Destination.TestRefEquals( result.States[c + d + e] ),
                result.States[c + d + e].Transitions[_1].Destination.TestRefEquals( result.States[f] ),
                result.States[f].Type.TestEquals( StateMachineNodeType.Dead ),
                result.States[f].Transitions.Count.TestEquals( 2 ),
                result.States[f].Transitions.Keys.TestSetEqual( [ _0, _1 ] ),
                result.States[f].Transitions[_0].Destination.TestRefEquals( result.States[f] ),
                result.States[f].Transitions[_1].Destination.TestRefEquals( result.States[f] ) )
            .Go();
    }
}
