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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Automata.Extensions;

/// <summary>
/// Contains <see cref="IStateMachine{TState,TInput,TResult}"/> extension methods.
/// </summary>
public static class StateMachineExtensions
{
    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all states of <see cref="StateMachineNodeType.Accept"/> type.
    /// </summary>
    /// <param name="machine">State machine to check.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> FindAcceptStates<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Select( kv => kv.Value ).Where( s => s.IsAccept() );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all states of <see cref="StateMachineNodeType.Default"/> type.
    /// </summary>
    /// <param name="machine">State machine to check.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> FindDefaultStates<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Select( kv => kv.Value ).Where( s => ! s.IsAccept() );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all states of <see cref="StateMachineNodeType.Dead"/> type.
    /// </summary>
    /// <param name="machine">State machine to check.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> FindDeadStates<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Select( kv => kv.Value ).Where( s => s.IsDead() );
    }

    /// <summary>
    /// Checks whether or not a transition between two states exists.
    /// </summary>
    /// <param name="machine">State machine to check.</param>
    /// <param name="source">Source state.</param>
    /// <param name="destination">Destination state.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns><b>true</b> when transition exists, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool CanTransitionTo<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine,
        TState source,
        TState destination)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.TryGetValue( source, out var state ) && state.CanTransitionTo( destination, machine.StateComparer );
    }

    /// <summary>
    /// Checks whether or not a transition exists.
    /// </summary>
    /// <param name="machine">State machine to check.</param>
    /// <param name="source">Source state.</param>
    /// <param name="input">Transition identifier.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns><b>true</b> when transition exists, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool CanTransition<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine,
        TState source,
        TInput input)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.TryGetValue( source, out var sourceState ) && sourceState.CanTransition( input );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all possible transitions.
    /// </summary>
    /// <param name="machine">Source state machine.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<KeyValuePair<IStateMachineNode<TState, TInput, TResult>,
            KeyValuePair<TInput, IStateMachineTransition<TState, TInput, TResult>>>>
        GetTransitions<TState, TInput, TResult>(this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Flatten( kv => kv.Value.Transitions, (s, d) => KeyValuePair.Create( s.Value, d ) );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all possible transition identifiers.
    /// </summary>
    /// <param name="machine">Source state machine.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<TInput> GetAlphabet<TState, TInput, TResult>(this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States
            .SelectMany( kv => kv.Value.Transitions.Select( t => t.Key ) )
            .Distinct( machine.InputComparer );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all available destination states
    /// for the given <paramref name="source"/> state.
    /// </summary>
    /// <param name="machine">Source state machine.</param>
    /// <param name="source">Source state.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> GetAvailableDestinations<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine,
        TState source)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.TryGetValue( source, out var state )
            ? state.GetAvailableDestinations( machine.StateComparer )
            : Enumerable.Empty<IStateMachineNode<TState, TInput, TResult>>();
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all available transitions
    /// from <paramref name="source"/> state to <paramref name="destination"/> state.
    /// </summary>
    /// <param name="machine">Source state machine.</param>
    /// <param name="source">Source state.</param>
    /// <param name="destination">Destination state.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<KeyValuePair<TInput, IStateMachineTransition<TState, TInput, TResult>>> FindTransitionsTo<
        TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine,
        TState source,
        TState destination)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.TryGetValue( source, out var state )
            ? state.FindTransitionsTo( destination, machine.StateComparer )
            : Enumerable.Empty<KeyValuePair<TInput, IStateMachineTransition<TState, TInput, TResult>>>();
    }

    /// <summary>
    /// Attempts to return an <see cref="IStateMachineTransition{TState,TInput,TResult}"/> instance
    /// for the provided state and transition identifier.
    /// </summary>
    /// <param name="machine">Source state machine.</param>
    /// <param name="source">Source state.</param>
    /// <param name="input">Transition identifier.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns an <see cref="IStateMachineTransition{TState,TInput,TResult}"/> instance
    /// associated with the provided state and transition identifier.
    /// </param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns><b>true</b> when transition exists, otherwise <b>false</b>.</returns>
    public static bool TryGetTransition<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine,
        TState source,
        TInput input,
        [MaybeNullWhen( false )] out IStateMachineTransition<TState, TInput, TResult> result)
        where TState : notnull
        where TInput : notnull
    {
        if ( machine.States.TryGetValue( source, out var state ) )
            return state.Transitions.TryGetValue( input, out result );

        result = default;
        return false;
    }

    /// <summary>
    /// Returns an <see cref="IStateMachineTransition{TState,TInput,TResult}"/> instance for the provided state and transition identifier.
    /// </summary>
    /// <param name="machine">Source state machine.</param>
    /// <param name="source">Source state.</param>
    /// <param name="input">Transition identifier.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// <see cref="IStateMachineTransition{TState,TInput,TResult}"/> instance associated with the provided state and transition identifier.
    /// </returns>
    /// <exception cref="StateMachineStateException">When <paramref name="source"/> does not exist.</exception>
    /// <exception cref="StateMachineTransitionException">When transition does not exist in the source state.</exception>
    [Pure]
    public static IStateMachineTransition<TState, TInput, TResult> GetTransition<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine,
        TState source,
        TInput input)
        where TState : notnull
        where TInput : notnull
    {
        if ( ! machine.States.TryGetValue( source, out var state ) )
            throw new StateMachineStateException( Resources.StateDoesNotExist( source ), nameof( source ) );

        if ( ! state.Transitions.TryGetValue( input, out var result ) )
            throw new StateMachineTransitionException( Resources.TransitionDoesNotExist( source, input ), nameof( input ) );

        return result;
    }
}
