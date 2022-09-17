using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Computable.Automata.Internal;

namespace LfrlAnvil.Computable.Automata;

public sealed class StateMachineBuilder<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    private readonly Dictionary<TState, State> _states;
    private (TState Value, State Data)? _initialState;

    public StateMachineBuilder(TResult defaultResult)
        : this( defaultResult, EqualityComparer<TState>.Default, EqualityComparer<TInput>.Default ) { }

    public StateMachineBuilder(TResult defaultResult, IEqualityComparer<TState> stateComparer, IEqualityComparer<TInput> inputComparer)
    {
        DefaultResult = defaultResult;
        Optimization = StateMachineOptimizationParams<TState>.None();
        InputComparer = inputComparer;
        _states = new Dictionary<TState, State>( stateComparer );
        _initialState = null;
    }

    public TResult DefaultResult { get; private set; }
    public StateMachineOptimizationParams<TState> Optimization { get; private set; }
    public IEqualityComparer<TInput> InputComparer { get; }
    public IEqualityComparer<TState> StateComparer => _states.Comparer;

    [Pure]
    public IEnumerable<KeyValuePair<TState, StateMachineNodeType>> GetStates()
    {
        return _states.Select( kv => KeyValuePair.Create( kv.Key, kv.Value.Type ) );
    }

    [Pure]
    public IEnumerable<KeyValuePair<TInput, TState>> GetTransitions(TState source)
    {
        return _states.TryGetValue( source, out var state )
            ? state.Transitions.Select( kv => KeyValuePair.Create( kv.Key, kv.Value.Destination ) )
            : Enumerable.Empty<KeyValuePair<TInput, TState>>();
    }

    public StateMachineBuilder<TState, TInput, TResult> SetDefaultResult(TResult value)
    {
        DefaultResult = value;
        return this;
    }

    public StateMachineBuilder<TState, TInput, TResult> SetOptimization(StateMachineOptimizationParams<TState> value)
    {
        Optimization = value;
        return this;
    }

    public StateMachineBuilder<TState, TInput, TResult> AddTransition(
        TState source,
        TInput input,
        IStateTransitionHandler<TState, TInput, TResult>? handler = null)
    {
        return AddTransition( source, source, input, handler );
    }

    public StateMachineBuilder<TState, TInput, TResult> AddTransition(
        TState source,
        TState destination,
        TInput input,
        IStateTransitionHandler<TState, TInput, TResult>? handler = null)
    {
        if ( ! _states.TryGetValue( source, out var sourceState ) )
            sourceState = AddState( source, StateMachineNodeType.Default );

        if ( sourceState.Transitions.ContainsKey( input ) )
            throw new StateMachineTransitionException( Resources.TransitionAlreadyExists( source, input ), nameof( input ) );

        if ( ! _states.ContainsKey( destination ) )
            AddState( destination, StateMachineNodeType.Default );

        sourceState.Transitions.Add( input, (destination, handler) );
        return this;
    }

    public StateMachineBuilder<TState, TInput, TResult> MarkAsAccept(TState state)
    {
        if ( _states.TryGetValue( state, out var data ) )
        {
            data.Type |= StateMachineNodeType.Accept;
            return this;
        }

        AddState( state, StateMachineNodeType.Accept );
        return this;
    }

    public StateMachineBuilder<TState, TInput, TResult> MarkAsDefault(TState state)
    {
        if ( _states.TryGetValue( state, out var data ) )
        {
            data.Type &= ~StateMachineNodeType.Accept;
            return this;
        }

        AddState( state, StateMachineNodeType.Default );
        return this;
    }

    public StateMachineBuilder<TState, TInput, TResult> MarkAsInitial(TState state)
    {
        if ( _initialState is not null )
            _initialState.Value.Data.Type &= ~StateMachineNodeType.Initial;

        if ( _states.TryGetValue( state, out var data ) )
            data.Type |= StateMachineNodeType.Initial;
        else
            data = AddState( state, StateMachineNodeType.Initial );

        _initialState = (state, data);
        return this;
    }

    [Pure]
    public StateMachine<TState, TInput, TResult> Build()
    {
        if ( _initialState is null )
            throw new StateMachineCreationException( Resources.InitialStateIsMissing );

        var states = _states.ToDictionary(
            s => s.Key,
            s =>
            {
                IStateMachineNode<TState, TInput, TResult> node = new StateMachineNode<TState, TInput, TResult>(
                    value: s.Key,
                    type: s.Value.Type,
                    transitions: new Dictionary<TInput, IStateMachineTransition<TState, TInput, TResult>>( InputComparer ) );

                return node;
            } );

        foreach ( var (source, data) in _states )
        {
            var sourceNode = ReinterpretCast.To<StateMachineNode<TState, TInput, TResult>>( states[source] );
            var sourceTransitions = ReinterpretCast.To<Dictionary<TInput, IStateMachineTransition<TState, TInput, TResult>>>(
                sourceNode.Transitions );

            foreach ( var (input, (destination, handler)) in data.Transitions )
            {
                var destinationNode = ReinterpretCast.To<StateMachineNode<TState, TInput, TResult>>( states[destination] );
                var transition = new StateMachineTransition<TState, TInput, TResult>( destinationNode, handler );
                sourceTransitions.Add( input, transition );
            }
        }

        var initialStateNode = states[_initialState.Value.Value];

        if ( Optimization.Level >= StateMachineOptimization.RemoveUnreachableStates )
            StateMachineOptimizer.RemoveUnreachableStates( states, initialStateNode );

        if ( Optimization.Level == StateMachineOptimization.Minimize )
            StateMachineOptimizer.Minimize( states, ref initialStateNode, Optimization.StateMerger!, InputComparer );

        return new StateMachine<TState, TInput, TResult>(
            states,
            initialStateNode,
            InputComparer,
            DefaultResult,
            Optimization.Level );
    }

    private State AddState(TState state, StateMachineNodeType type)
    {
        var result = new State( type, InputComparer );
        _states.Add( state, result );
        return result;
    }

    private sealed class State
    {
        internal readonly Dictionary<TInput, (TState Destination, IStateTransitionHandler<TState, TInput, TResult>? Handler)> Transitions;
        internal StateMachineNodeType Type;

        internal State(StateMachineNodeType type, IEqualityComparer<TInput> inputComparer)
        {
            Type = type;
            Transitions = new Dictionary<TInput, (TState, IStateTransitionHandler<TState, TInput, TResult>?)>( inputComparer );
        }
    }
}
