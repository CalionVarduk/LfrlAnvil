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

        using ( new AssertionScope() )
        {
            sut.DefaultResult.Should().Be( defaultResult );
            sut.Optimization.Level.Should().Be( StateMachineOptimization.None );
            sut.StateComparer.Should().BeSameAs( EqualityComparer<string>.Default );
            sut.InputComparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.GetStates().Should().BeEmpty();
        }
    }

    [Fact]
    public void Ctor_WithComparers_ShouldReturnEmptyBuilder()
    {
        var defaultResult = Fixture.Create<string>();
        var stateComparer = EqualityComparerFactory<string>.Create( (a, b) => a == b );
        var inputComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = new StateMachineBuilder<string, int, string>( defaultResult, stateComparer, inputComparer );

        using ( new AssertionScope() )
        {
            sut.DefaultResult.Should().Be( defaultResult );
            sut.Optimization.Level.Should().Be( StateMachineOptimization.None );
            sut.StateComparer.Should().BeSameAs( stateComparer );
            sut.InputComparer.Should().BeSameAs( inputComparer );
            sut.GetStates().Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultResult_ShouldUpdateDefaultResult()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new StateMachineBuilder<string, int, string>( oldValue );

        var result = sut.SetDefaultResult( value );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.DefaultResult.Should().Be( value );
        }
    }

    [Fact]
    public void SetOptimization_ShouldUpdateOptimization_WhenParamsPointToNone()
    {
        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        var result = sut.SetOptimization( StateMachineOptimizationParams<string>.None() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Optimization.Level.Should().Be( StateMachineOptimization.None );
        }
    }

    [Fact]
    public void SetOptimization_ShouldUpdateOptimization_WhenParamsPointToRemoveUnreachableStates()
    {
        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        var result = sut.SetOptimization( StateMachineOptimizationParams<string>.RemoveUnreachableStates() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Optimization.Level.Should().Be( StateMachineOptimization.RemoveUnreachableStates );
        }
    }

    [Fact]
    public void SetOptimization_ShouldUpdateOptimization_WhenParamsPointToMinimize()
    {
        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        var result = sut.SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s, _) => s ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Optimization.Level.Should().Be( StateMachineOptimization.Minimize );
        }
    }

    [Fact]
    public void AddTransition_ShouldAddNewTransition_WhenSourceAndDestinationStatesDoNotExist()
    {
        var (source, destination) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var input = Fixture.Create<int>();
        var expectedStates = new[]
        {
            KeyValuePair.Create( source, StateMachineNodeType.Default ),
            KeyValuePair.Create( destination, StateMachineNodeType.Default )
        };

        var expectedSourceTransitions = new[] { KeyValuePair.Create( input, destination ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );

        var result = sut.AddTransition( source, destination, input );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( source ).Should().BeEquivalentTo( expectedSourceTransitions );
            sut.GetTransitions( destination ).Should().BeEmpty();
        }
    }

    [Fact]
    public void AddTransition_ShouldAddNewTransition_WhenSourceStateExistsAndDestinationStateDoesNotExist()
    {
        var (source, destination) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var input = Fixture.Create<int>();
        var expectedStates = new[]
        {
            KeyValuePair.Create( source, StateMachineNodeType.Accept ), KeyValuePair.Create( destination, StateMachineNodeType.Default )
        };

        var expectedSourceTransitions = new[] { KeyValuePair.Create( input, destination ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( source );

        var result = sut.AddTransition( source, destination, input );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( source ).Should().BeEquivalentTo( expectedSourceTransitions );
            sut.GetTransitions( destination ).Should().BeEmpty();
        }
    }

    [Fact]
    public void AddTransition_ShouldAddNewTransition_WhenSourceStateDoesNotExistAndDestinationStateExists()
    {
        var (source, destination) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var input = Fixture.Create<int>();
        var expectedStates = new[]
        {
            KeyValuePair.Create( source, StateMachineNodeType.Default ), KeyValuePair.Create( destination, StateMachineNodeType.Accept )
        };

        var expectedSourceTransitions = new[] { KeyValuePair.Create( input, destination ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( destination );

        var result = sut.AddTransition( source, destination, input );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( source ).Should().BeEquivalentTo( expectedSourceTransitions );
            sut.GetTransitions( destination ).Should().BeEmpty();
        }
    }

    [Fact]
    public void AddTransition_ShouldAddNewTransition_WhenSourceAndDestinationStatesExist()
    {
        var (source, destination) = Fixture.CreateDistinctCollection<string>( count: 2 );
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

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( source ).Should().BeEquivalentTo( expectedSourceTransitions );
            sut.GetTransitions( destination ).Should().BeEmpty();
        }
    }

    [Fact]
    public void AddTransition_ShouldAddNewTransition_WhenSourceStateAlreadyHasOtherTransition()
    {
        var (source, destination, existingDestination) = Fixture.CreateDistinctCollection<string>( count: 3 );
        var (input, existingInput) = Fixture.CreateDistinctCollection<int>( count: 2 );
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

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( source ).Should().BeEquivalentTo( expectedSourceTransitions );
            sut.GetTransitions( destination ).Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEquivalentTo( expectedTransitions );
        }
    }

    [Fact]
    public void AddTransition_ShouldThrowStateMachineTransitionException_WhenTransitionFromSourceStateWithProvidedInputAlreadyExists()
    {
        var (source, destination, otherDestination) = Fixture.CreateDistinctCollection<string>( count: 3 );
        var input = Fixture.Create<int>();

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.AddTransition( source, otherDestination, input );

        var action = Lambda.Of( () => sut.AddTransition( source, destination, input ) );

        action.Should().ThrowExactly<StateMachineTransitionException>();
    }

    [Fact]
    public void MarkAsAccept_ShouldAddNewAcceptState_WhenStateDoesNotExist()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Accept ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );

        var result = sut.MarkAsAccept( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsAccept_ShouldChangeStateTypeToAccept_WhenStateExistsAndIsMarkedAsDefault()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Accept ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsDefault( state );

        var result = sut.MarkAsAccept( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsAccept_ShouldDoNothing_WhenStateExistsAndIsMarkedAsAccept()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Accept ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( state );

        var result = sut.MarkAsAccept( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsAccept_ShouldAddAcceptTypeToState_WhenStateExistsAndIsMarkedAsInitial()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Initial | StateMachineNodeType.Accept ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsInitial( state );

        var result = sut.MarkAsAccept( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsDefault_ShouldAddNewDefaultState_WhenStateDoesNotExist()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Default ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );

        var result = sut.MarkAsDefault( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsDefault_ShouldChangeStateTypeToDefault_WhenStateExistsAndIsMarkedAsAccept()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Default ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( state );

        var result = sut.MarkAsDefault( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsDefault_ShouldDoNothing_WhenStateExistsAndIsMarkedAsDefault()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Default ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsDefault( state );

        var result = sut.MarkAsDefault( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsDefault_ShouldAddDefaultTypeToState_WhenStateExistsAndIsMarkedAsInitial()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Initial ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsInitial( state );

        var result = sut.MarkAsDefault( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsInitial_ShouldAddNewInitialState_WhenStateDoesNotExist()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Initial ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );

        var result = sut.MarkAsInitial( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsInitial_ShouldAddInitialTypeToState_WhenStateExistsAndIsMarkedAsAccept()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Accept | StateMachineNodeType.Initial ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsAccept( state );

        var result = sut.MarkAsInitial( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsInitial_ShouldAddInitialTypeToState_WhenStateExistsAndIsMarkedAsDefault()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Initial ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsDefault( state );

        var result = sut.MarkAsInitial( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsDefault_ShouldDoNothing_WhenStateExistsAndIsMarkedAsInitial()
    {
        var state = Fixture.Create<string>();
        var expectedStates = new[] { KeyValuePair.Create( state, StateMachineNodeType.Initial ) };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsInitial( state );

        var result = sut.MarkAsInitial( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsDefault_ShouldReplaceActiveInitialState_WhenOtherStateIsMarkedAsInitial()
    {
        var (state, other) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var expectedStates = new[]
        {
            KeyValuePair.Create( state, StateMachineNodeType.Initial ), KeyValuePair.Create( other, StateMachineNodeType.Default )
        };

        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsInitial( other );

        var result = sut.MarkAsInitial( state );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetStates().Should().BeEquivalentTo( expectedStates );
            sut.GetTransitions( state ).Should().BeEmpty();
        }
    }

    [Fact]
    public void Build_ShouldThrowStateMachineCreationException_WhenNoStateIsMarkedAsInitial()
    {
        var state = Fixture.Create<string>();
        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        sut.MarkAsDefault( state );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<StateMachineCreationException>();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_WhenOnlySingleStateExistsMarkedAsInitial()
    {
        var defaultResult = Fixture.Create<string>();
        var state = Fixture.Create<string>();
        var sut = new StateMachineBuilder<string, int, string>( defaultResult );
        sut.MarkAsInitial( state );

        var result = sut.Build();

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 1 );
            result.States.ContainsKey( result.InitialState.Value ).Should().BeTrue();
            result.InitialState.Value.Should().Be( state );
            result.InitialState.Type.Should().Be( StateMachineNodeType.Initial );
            result.InitialState.Transitions.Should().BeEmpty();
            result.States[result.InitialState.Value].Should().BeSameAs( result.InitialState );
        }
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult_ForMoreComplexStateMachine()
    {
        var defaultResult = Fixture.Create<string>();
        var (a, b, c, d, e, f) = ("a", "b", "c", "d", "e", "f");
        var (_0, _1) = (0, 1);

        var sut = new StateMachineBuilder<string, int, string>( defaultResult )
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 6 );
            result.States.Keys.Should().BeEquivalentTo( a, b, c, d, e, f );

            var aNode = result.States[a];
            var bNode = result.States[b];
            var cNode = result.States[c];
            var dNode = result.States[d];
            var eNode = result.States[e];
            var fNode = result.States[f];

            result.InitialState.Should().BeSameAs( aNode );

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( bNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( cNode );

            bNode.Type.Should().Be( StateMachineNodeType.Default );
            bNode.Transitions.Should().HaveCount( 2 );
            bNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            bNode.Transitions[_0].Destination.Should().BeSameAs( aNode );
            bNode.Transitions[_1].Destination.Should().BeSameAs( dNode );

            cNode.Type.Should().Be( StateMachineNodeType.Accept );
            cNode.Transitions.Should().HaveCount( 2 );
            cNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            cNode.Transitions[_0].Destination.Should().BeSameAs( eNode );
            cNode.Transitions[_1].Destination.Should().BeSameAs( fNode );

            dNode.Type.Should().Be( StateMachineNodeType.Accept );
            dNode.Transitions.Should().HaveCount( 2 );
            dNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            dNode.Transitions[_0].Destination.Should().BeSameAs( eNode );
            dNode.Transitions[_1].Destination.Should().BeSameAs( fNode );

            eNode.Type.Should().Be( StateMachineNodeType.Accept );
            eNode.Transitions.Should().HaveCount( 2 );
            eNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            eNode.Transitions[_0].Destination.Should().BeSameAs( eNode );
            eNode.Transitions[_1].Destination.Should().BeSameAs( fNode );

            fNode.Type.Should().Be( StateMachineNodeType.Default );
            fNode.Transitions.Should().HaveCount( 2 );
            fNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            fNode.Transitions[_0].Destination.Should().BeSameAs( fNode );
            fNode.Transitions[_1].Destination.Should().BeSameAs( fNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 1 );
            result.States.Keys.Should().BeEquivalentTo( a );

            var aNode = result.States[a];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 1 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( aNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 3 );
            result.States.Keys.Should().BeEquivalentTo( a, b, c );

            var aNode = result.States[a];
            var bNode = result.States[b];
            var cNode = result.States[c];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( bNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( aNode );

            bNode.Type.Should().Be( StateMachineNodeType.Default );
            bNode.Transitions.Should().HaveCount( 2 );
            bNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            bNode.Transitions[_0].Destination.Should().BeSameAs( aNode );
            bNode.Transitions[_1].Destination.Should().BeSameAs( cNode );

            cNode.Type.Should().Be( StateMachineNodeType.Default );
            cNode.Transitions.Should().HaveCount( 1 );
            cNode.Transitions.Keys.Should().BeEquivalentTo( _0 );
            cNode.Transitions[_0].Destination.Should().BeSameAs( cNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 2 );
            result.States.Keys.Should().BeEquivalentTo( a, b );

            var aNode = result.States[a];
            var bNode = result.States[b];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( bNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( aNode );

            bNode.Type.Should().Be( StateMachineNodeType.Default );
            bNode.Transitions.Should().HaveCount( 2 );
            bNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            bNode.Transitions[_0].Destination.Should().BeSameAs( aNode );
            bNode.Transitions[_1].Destination.Should().BeSameAs( aNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 2 );
            result.States.Keys.Should().BeEquivalentTo( a, b );

            var aNode = result.States[a];
            var bNode = result.States[b];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( bNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( bNode );

            bNode.Type.Should().Be( StateMachineNodeType.Accept );
            bNode.Transitions.Should().HaveCount( 2 );
            bNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            bNode.Transitions[_0].Destination.Should().BeSameAs( bNode );
            bNode.Transitions[_1].Destination.Should().BeSameAs( bNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 2 );
            result.States.Keys.Should().BeEquivalentTo( a, b + c );

            var aNode = result.States[a];
            var bcNode = result.States[b + c];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( bcNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( bcNode );

            bcNode.Type.Should().Be( StateMachineNodeType.Accept );
            bcNode.Transitions.Should().HaveCount( 2 );
            bcNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            bcNode.Transitions[_0].Destination.Should().BeSameAs( bcNode );
            bcNode.Transitions[_1].Destination.Should().BeSameAs( bcNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 1 );
            result.States.Keys.Should().BeEquivalentTo( a + b );

            var abNode = result.States[a + b];

            abNode.Type.Should().Be( StateMachineNodeType.Initial | StateMachineNodeType.Accept );
            abNode.Transitions.Should().HaveCount( 2 );
            abNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            abNode.Transitions[_0].Destination.Should().BeSameAs( abNode );
            abNode.Transitions[_1].Destination.Should().BeSameAs( abNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 2 );
            result.States.Keys.Should().BeEquivalentTo( a, b );

            var aNode = result.States[a];
            var bNode = result.States[b];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( aNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( bNode );

            bNode.Type.Should().Be( StateMachineNodeType.Accept );
            bNode.Transitions.Should().HaveCount( 2 );
            bNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            bNode.Transitions[_0].Destination.Should().BeSameAs( bNode );
            bNode.Transitions[_1].Destination.Should().BeSameAs( bNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 3 );
            result.States.Keys.Should().BeEquivalentTo( a, b, c );

            var aNode = result.States[a];
            var bNode = result.States[b];
            var cNode = result.States[c];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( cNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( bNode );

            bNode.Type.Should().Be( StateMachineNodeType.Dead );
            bNode.Transitions.Should().HaveCount( 2 );
            bNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            bNode.Transitions[_0].Destination.Should().BeSameAs( bNode );
            bNode.Transitions[_1].Destination.Should().BeSameAs( bNode );

            cNode.Type.Should().Be( StateMachineNodeType.Accept );
            cNode.Transitions.Should().HaveCount( 2 );
            cNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            cNode.Transitions[_0].Destination.Should().BeSameAs( cNode );
            cNode.Transitions[_1].Destination.Should().BeSameAs( cNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 3 );
            result.States.Keys.Should().BeEquivalentTo( a, b, c );

            var aNode = result.States[a];
            var bNode = result.States[b];
            var cNode = result.States[c];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( cNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( bNode );

            bNode.Type.Should().Be( StateMachineNodeType.Accept );
            bNode.Transitions.Should().HaveCount( 2 );
            bNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            bNode.Transitions[_0].Destination.Should().BeSameAs( bNode );
            bNode.Transitions[_1].Destination.Should().BeSameAs( bNode );
            bNode.Transitions[_1].Handler.Should().BeSameAs( handler );

            cNode.Type.Should().Be( StateMachineNodeType.Accept );
            cNode.Transitions.Should().HaveCount( 2 );
            cNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            cNode.Transitions[_0].Destination.Should().BeSameAs( cNode );
            cNode.Transitions[_1].Destination.Should().BeSameAs( cNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 3 );
            result.States.Keys.Should().BeEquivalentTo( a, b, c );

            var aNode = result.States[a];
            var bNode = result.States[b];
            var cNode = result.States[c];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( cNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( bNode );

            bNode.Type.Should().Be( StateMachineNodeType.Accept );
            bNode.Transitions.Should().HaveCount( 2 );
            bNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            bNode.Transitions[_0].Destination.Should().BeSameAs( bNode );
            bNode.Transitions[_1].Destination.Should().BeSameAs( bNode );
            bNode.Transitions[_1].Handler.Should().BeSameAs( handlerB );

            cNode.Type.Should().Be( StateMachineNodeType.Accept );
            cNode.Transitions.Should().HaveCount( 2 );
            cNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            cNode.Transitions[_0].Destination.Should().BeSameAs( cNode );
            cNode.Transitions[_1].Destination.Should().BeSameAs( cNode );
            cNode.Transitions[_1].Handler.Should().BeSameAs( handlerC );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 2 );
            result.States.Keys.Should().BeEquivalentTo( a, b + c );

            var aNode = result.States[a];
            var bcNode = result.States[b + c];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( bcNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( bcNode );

            bcNode.Type.Should().Be( StateMachineNodeType.Accept );
            bcNode.Transitions.Should().HaveCount( 2 );
            bcNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            bcNode.Transitions[_0].Destination.Should().BeSameAs( bcNode );
            bcNode.Transitions[_1].Destination.Should().BeSameAs( bcNode );
            bcNode.Transitions[_1].Handler.Should().BeSameAs( handler );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 4 );
            result.States.Keys.Should().BeEquivalentTo( a, b, c, d );

            var aNode = result.States[a];
            var bNode = result.States[b];
            var cNode = result.States[c];
            var dNode = result.States[d];

            aNode.Type.Should().Be( StateMachineNodeType.Initial );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( cNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( bNode );

            bNode.Type.Should().Be( StateMachineNodeType.Default );
            bNode.Transitions.Should().HaveCount( 1 );
            bNode.Transitions.Keys.Should().BeEquivalentTo( _1 );
            bNode.Transitions[_1].Destination.Should().BeSameAs( dNode );

            cNode.Type.Should().Be( StateMachineNodeType.Default );
            cNode.Transitions.Should().HaveCount( 1 );
            cNode.Transitions.Keys.Should().BeEquivalentTo( _0 );
            cNode.Transitions[_0].Destination.Should().BeSameAs( dNode );

            dNode.Type.Should().Be( StateMachineNodeType.Accept );
            dNode.Transitions.Should().HaveCount( 2 );
            dNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            dNode.Transitions[_0].Destination.Should().BeSameAs( dNode );
            dNode.Transitions[_1].Destination.Should().BeSameAs( dNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 2 );
            result.States.Keys.Should().BeEquivalentTo( a, b + c + d );

            var aNode = result.States[a];
            var bcdNode = result.States[b + c + d];

            aNode.Type.Should().Be( StateMachineNodeType.Initial | StateMachineNodeType.Accept );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( bcdNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( bcdNode );

            bcdNode.Type.Should().Be( StateMachineNodeType.Dead );
            bcdNode.Transitions.Should().HaveCount( 1 );
            bcdNode.Transitions.Keys.Should().BeEquivalentTo( _0 );
            bcdNode.Transitions[_0].Destination.Should().BeSameAs( bcdNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 3 );
            result.States.Keys.Should().BeEquivalentTo( a, c, b + d );

            var aNode = result.States[a];
            var cNode = result.States[c];
            var bdNode = result.States[b + d];

            aNode.Type.Should().Be( StateMachineNodeType.Initial | StateMachineNodeType.Accept );
            aNode.Transitions.Should().HaveCount( 2 );
            aNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            aNode.Transitions[_0].Destination.Should().BeSameAs( bdNode );
            aNode.Transitions[_1].Destination.Should().BeSameAs( cNode );

            cNode.Type.Should().Be( StateMachineNodeType.Default );
            cNode.Transitions.Should().HaveCount( 1 );
            cNode.Transitions.Keys.Should().BeEquivalentTo( _0 );
            cNode.Transitions[_0].Destination.Should().BeSameAs( bdNode );
            cNode.Transitions[_0].Handler.Should().BeSameAs( handler );

            bdNode.Type.Should().Be( StateMachineNodeType.Dead );
            bdNode.Transitions.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 1 );
            result.States.Keys.Should().BeEquivalentTo( a + b + c + d + e );

            var abcdeNode = result.States[a + b + c + d + e];

            abcdeNode.Type.Should().Be( StateMachineNodeType.Initial | StateMachineNodeType.Accept );
            abcdeNode.Transitions.Should().HaveCount( 1 );
            abcdeNode.Transitions.Keys.Should().BeEquivalentTo( 0 );
            abcdeNode.Transitions[0].Destination.Should().BeSameAs( abcdeNode );
        }
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

        using ( new AssertionScope() )
        {
            result.DefaultResult.Should().Be( defaultResult );
            result.Optimization.Should().Be( sut.Optimization.Level );
            result.StateComparer.Should().BeSameAs( sut.StateComparer );
            result.InputComparer.Should().BeSameAs( sut.InputComparer );
            result.States.Should().HaveCount( 3 );
            result.States.Keys.Should().BeEquivalentTo( a + b, c + d + e, f );

            var abNode = result.States[a + b];
            var cdeNode = result.States[c + d + e];
            var fNode = result.States[f];

            result.InitialState.Should().BeSameAs( abNode );

            abNode.Type.Should().Be( StateMachineNodeType.Initial );
            abNode.Transitions.Should().HaveCount( 2 );
            abNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            abNode.Transitions[_0].Destination.Should().BeSameAs( abNode );
            abNode.Transitions[_1].Destination.Should().BeSameAs( cdeNode );

            cdeNode.Type.Should().Be( StateMachineNodeType.Accept );
            cdeNode.Transitions.Should().HaveCount( 2 );
            cdeNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            cdeNode.Transitions[_0].Destination.Should().BeSameAs( cdeNode );
            cdeNode.Transitions[_1].Destination.Should().BeSameAs( fNode );

            fNode.Type.Should().Be( StateMachineNodeType.Dead );
            fNode.Transitions.Should().HaveCount( 2 );
            fNode.Transitions.Keys.Should().BeEquivalentTo( _0, _1 );
            fNode.Transitions[_0].Destination.Should().BeSameAs( fNode );
            fNode.Transitions[_1].Destination.Should().BeSameAs( fNode );
        }
    }
}
