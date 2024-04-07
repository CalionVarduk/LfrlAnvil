using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
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
        return _states.Select( static kv => KeyValuePair.Create( kv.Key, kv.Value.Type ) );
    }

    [Pure]
    public IEnumerable<KeyValuePair<TInput, TState>> GetTransitions(TState source)
    {
        return _states.TryGetValue( source, out var state )
            ? state.Transitions.Select( static kv => KeyValuePair.Create( kv.Key, kv.Value.Destination ) )
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
        ref var sourceState = ref CollectionsMarshal.GetValueRefOrAddDefault( _states, source, out var exists )!;
        if ( ! exists )
            sourceState = new State( StateMachineNodeType.Default, InputComparer );

        ref var transition = ref CollectionsMarshal.GetValueRefOrAddDefault( sourceState.Transitions, input, out exists );
        if ( exists )
            throw new StateMachineTransitionException( Resources.TransitionAlreadyExists( source, input ), nameof( input ) );

        ref var destinationState = ref CollectionsMarshal.GetValueRefOrAddDefault( _states, destination, out exists )!;
        if ( ! exists )
            destinationState = new State( StateMachineNodeType.Default, InputComparer );

        transition = (destination, handler);
        return this;
    }

    public StateMachineBuilder<TState, TInput, TResult> MarkAsAccept(TState state)
    {
        ref var data = ref CollectionsMarshal.GetValueRefOrAddDefault( _states, state, out var exists )!;
        if ( exists )
            data.Type |= StateMachineNodeType.Accept;
        else
            data = new State( StateMachineNodeType.Accept, InputComparer );

        return this;
    }

    public StateMachineBuilder<TState, TInput, TResult> MarkAsDefault(TState state)
    {
        ref var data = ref CollectionsMarshal.GetValueRefOrAddDefault( _states, state, out var exists )!;
        if ( exists )
            data.Type &= ~StateMachineNodeType.Accept;
        else
            data = new State( StateMachineNodeType.Default, InputComparer );

        return this;
    }

    public StateMachineBuilder<TState, TInput, TResult> MarkAsInitial(TState state)
    {
        if ( _initialState is not null )
            _initialState.Value.Data.Type &= ~StateMachineNodeType.Initial;

        ref var data = ref CollectionsMarshal.GetValueRefOrAddDefault( _states, state, out var exists )!;
        if ( exists )
            data.Type |= StateMachineNodeType.Initial;
        else
            data = new State( StateMachineNodeType.Initial, InputComparer );

        _initialState = (state, data);
        return this;
    }

    [Pure]
    public StateMachine<TState, TInput, TResult> Build()
    {
        if ( _initialState is null )
            throw new StateMachineCreationException( Resources.InitialStateIsMissing );

        var states = _states.ToDictionary(
            static s => s.Key,
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
            var sourceTransitions
                = ReinterpretCast.To<Dictionary<TInput, IStateMachineTransition<TState, TInput, TResult>>>( sourceNode.Transitions );

            foreach ( var (input, (destination, handler)) in data.Transitions )
            {
                var destinationNode = ReinterpretCast.To<StateMachineNode<TState, TInput, TResult>>( states[destination] );
                var transition = new StateMachineTransition<TState, TInput, TResult>( destinationNode, handler );
                sourceTransitions.Add( input, transition );
            }
        }

        var initialStateNode = states[_initialState.Value.Value];
        var optimizationResult = StateMachineOptimizer.OptimizeNew( states, initialStateNode, Optimization, InputComparer );

        return new StateMachine<TState, TInput, TResult>(
            optimizationResult.States,
            optimizationResult.InitialState,
            InputComparer,
            DefaultResult,
            Optimization.Level );
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
