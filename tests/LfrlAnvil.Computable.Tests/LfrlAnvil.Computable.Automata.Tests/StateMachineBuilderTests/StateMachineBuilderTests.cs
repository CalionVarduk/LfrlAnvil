using System.Collections.Generic;
using FluentAssertions.Execution;
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
            sut.Optimization.Should().Be( StateMachineOptimization.None );
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
            sut.Optimization.Should().Be( StateMachineOptimization.None );
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

    [Theory]
    [InlineData( StateMachineOptimization.None )]
    [InlineData( StateMachineOptimization.RemoveUnreachableStates )]
    public void SetOptimization_ShouldUpdateOptimization(StateMachineOptimization value)
    {
        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );

        var result = sut.SetOptimization( value );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Optimization.Should().Be( value );
        }
    }

    [Fact]
    public void SetOptimization_ShouldThrowArgumentException_WhenValueIsNotDefinedInEnum()
    {
        var sut = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() );
        var action = Lambda.Of( () => sut.SetOptimization( (StateMachineOptimization)10 ) );
        action.Should().ThrowExactly<ArgumentException>();
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
            KeyValuePair.Create( source, StateMachineNodeType.Accept ),
            KeyValuePair.Create( destination, StateMachineNodeType.Default )
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
            KeyValuePair.Create( source, StateMachineNodeType.Default ),
            KeyValuePair.Create( destination, StateMachineNodeType.Accept )
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
            KeyValuePair.Create( source, StateMachineNodeType.Accept ),
            KeyValuePair.Create( destination, StateMachineNodeType.Accept )
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
            KeyValuePair.Create( existingInput, existingDestination ),
            KeyValuePair.Create( input, destination )
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
        var expectedStates = new[]
        {
            KeyValuePair.Create( state, StateMachineNodeType.Initial | StateMachineNodeType.Accept )
        };

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
        var expectedStates = new[]
        {
            KeyValuePair.Create( state, StateMachineNodeType.Initial )
        };

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
        var expectedStates = new[]
        {
            KeyValuePair.Create( state, StateMachineNodeType.Accept | StateMachineNodeType.Initial )
        };

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
        var expectedStates = new[]
        {
            KeyValuePair.Create( state, StateMachineNodeType.Initial )
        };

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
            KeyValuePair.Create( state, StateMachineNodeType.Initial ),
            KeyValuePair.Create( other, StateMachineNodeType.Default )
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
            result.Optimization.Should().Be( sut.Optimization );
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
    public void Build_ShouldReturnCorrectResult_ForActualStateMachineExample()
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
            result.Optimization.Should().Be( sut.Optimization );
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
            .SetOptimization( StateMachineOptimization.RemoveUnreachableStates )
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
            result.Optimization.Should().Be( sut.Optimization );
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
            .SetOptimization( StateMachineOptimization.RemoveUnreachableStates )
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
            result.Optimization.Should().Be( sut.Optimization );
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
            .SetOptimization( StateMachineOptimization.RemoveUnreachableStates )
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
            result.Optimization.Should().Be( sut.Optimization );
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
}
