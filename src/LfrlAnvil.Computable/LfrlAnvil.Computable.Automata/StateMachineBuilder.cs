// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Computable.Automata.Internal;

namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents a deterministic finite state machine builder.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TInput">Input type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class StateMachineBuilder<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    private readonly Dictionary<TState, State> _states;
    private (TState Value, State Data)? _initialState;

    /// <summary>
    /// Creates a new empty <see cref="StateMachineBuilder{TState,TInput,TResult}"/> instance
    /// with <see cref="EqualityComparer{T}.Default"/> state and input equality comparer.
    /// </summary>
    /// <param name="defaultResult">Default transition result.</param>
    public StateMachineBuilder(TResult defaultResult)
        : this( defaultResult, EqualityComparer<TState>.Default, EqualityComparer<TInput>.Default ) { }

    /// <summary>
    /// Creates a new empty <see cref="StateMachineBuilder{TState,TInput,TResult}"/> instance.
    /// </summary>
    /// <param name="defaultResult">Default transition result.</param>
    /// <param name="stateComparer">State equality comparer.</param>
    /// <param name="inputComparer">Input equality comparer.</param>
    public StateMachineBuilder(TResult defaultResult, IEqualityComparer<TState> stateComparer, IEqualityComparer<TInput> inputComparer)
    {
        DefaultResult = defaultResult;
        Optimization = StateMachineOptimizationParams<TState>.None();
        InputComparer = inputComparer;
        _states = new Dictionary<TState, State>( stateComparer );
        _initialState = null;
    }

    /// <summary>
    /// Represents the default transition result.
    /// </summary>
    public TResult DefaultResult { get; private set; }

    /// <summary>
    /// Specifies the chosen optimization parameters with which the state machine should be built.
    /// </summary>
    public StateMachineOptimizationParams<TState> Optimization { get; private set; }

    /// <summary>
    /// Input equality comparer.
    /// </summary>
    public IEqualityComparer<TInput> InputComparer { get; }

    /// <summary>
    /// State equality comparer.
    /// </summary>
    public IEqualityComparer<TState> StateComparer => _states.Comparer;

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all current states.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public IEnumerable<KeyValuePair<TState, StateMachineNodeType>> GetStates()
    {
        return _states.Select( static kv => KeyValuePair.Create( kv.Key, kv.Value.Type ) );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all current transitions for the given state.
    /// </summary>
    /// <param name="source">State to get all transitions for.</param>
    /// <returns>New <see cref="IEnumerable{T}"/> instance or empty when state does not exist.</returns>
    [Pure]
    public IEnumerable<KeyValuePair<TInput, TState>> GetTransitions(TState source)
    {
        return _states.TryGetValue( source, out var state )
            ? state.Transitions.Select( static kv => KeyValuePair.Create( kv.Key, kv.Value.Destination ) )
            : Enumerable.Empty<KeyValuePair<TInput, TState>>();
    }

    /// <summary>
    /// Sets <see cref="DefaultResult"/> of this instance.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    public StateMachineBuilder<TState, TInput, TResult> SetDefaultResult(TResult value)
    {
        DefaultResult = value;
        return this;
    }

    /// <summary>
    /// Sets <see cref="Optimization"/> of this instance.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    public StateMachineBuilder<TState, TInput, TResult> SetOptimization(StateMachineOptimizationParams<TState> value)
    {
        Optimization = value;
        return this;
    }

    /// <summary>
    /// Adds a new state self transition.
    /// </summary>
    /// <param name="source">State to self transition to.</param>
    /// <param name="input">Transition identifier.</param>
    /// <param name="handler">Optional transition handler. Equal to null by default.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="StateMachineTransitionException">When transition already exists.</exception>
    /// <remarks>States will be created if they don't exist.</remarks>
    public StateMachineBuilder<TState, TInput, TResult> AddTransition(
        TState source,
        TInput input,
        IStateTransitionHandler<TState, TInput, TResult>? handler = null)
    {
        return AddTransition( source, source, input, handler );
    }

    /// <summary>
    /// Adds a new state transition.
    /// </summary>
    /// <param name="source">Source state.</param>
    /// <param name="destination">Destination state.</param>
    /// <param name="input">Transition identifier.</param>
    /// <param name="handler">Optional transition handler. Equal to null by default.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="StateMachineTransitionException">When transition already exists.</exception>
    /// <remarks>States will be created if they don't exist.</remarks>
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

    /// <summary>
    /// Marks the given <paramref name="state"/> as <see cref="StateMachineNodeType.Accept"/> state.
    /// </summary>
    /// <param name="state">State to mark as <see cref="StateMachineNodeType.Accept"/>.</param>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Creates a new state when it does not exist.</remarks>
    public StateMachineBuilder<TState, TInput, TResult> MarkAsAccept(TState state)
    {
        ref var data = ref CollectionsMarshal.GetValueRefOrAddDefault( _states, state, out var exists )!;
        if ( exists )
            data.Type |= StateMachineNodeType.Accept;
        else
            data = new State( StateMachineNodeType.Accept, InputComparer );

        return this;
    }

    /// <summary>
    /// Marks the given <paramref name="state"/> as <see cref="StateMachineNodeType.Default"/> state.
    /// </summary>
    /// <param name="state">State to mark as <see cref="StateMachineNodeType.Default"/>.</param>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Creates a new state when it does not exist.</remarks>
    public StateMachineBuilder<TState, TInput, TResult> MarkAsDefault(TState state)
    {
        ref var data = ref CollectionsMarshal.GetValueRefOrAddDefault( _states, state, out var exists )!;
        if ( exists )
            data.Type &= ~StateMachineNodeType.Accept;
        else
            data = new State( StateMachineNodeType.Default, InputComparer );

        return this;
    }

    /// <summary>
    /// Marks the given <paramref name="state"/> as <see cref="StateMachineNodeType.Initial"/> state.
    /// </summary>
    /// <param name="state">State to mark as <see cref="StateMachineNodeType.Initial"/>.</param>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Creates a new state when it does not exist.</remarks>
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

    /// <summary>
    /// Creates a new <see cref="StateMachine{TState,TInput,TResult}"/> instance.
    /// </summary>
    /// <returns>New <see cref="StateMachine{TState,TInput,TResult}"/> instance.</returns>
    /// <exception cref="StateMachineCreationException">
    /// When an <see cref="StateMachineNodeType.Initial"/> state was not specified.
    /// </exception>
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
