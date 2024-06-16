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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Automata.Extensions;

/// <summary>
/// Contains <see cref="IStateMachineNode{TState,TInput,TResult}"/> extension methods.
/// </summary>
public static class StateMachineNodeExtensions
{
    /// <summary>
    /// Checks whether or not the provided <paramref name="node"/> is of <see cref="StateMachineNodeType.Accept"/> type.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// <b>true</b> when <paramref name="node"/> is an <see cref="StateMachineNodeType.Accept"/> node, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsAccept<TState, TInput, TResult>(this IStateMachineNode<TState, TInput, TResult> node)
        where TState : notnull
        where TInput : notnull
    {
        return (node.Type & StateMachineNodeType.Accept) != StateMachineNodeType.Default;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="node"/> is of <see cref="StateMachineNodeType.Initial"/> type.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// <b>true</b> when <paramref name="node"/> is an <see cref="StateMachineNodeType.Initial"/> node, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsInitial<TState, TInput, TResult>(this IStateMachineNode<TState, TInput, TResult> node)
        where TState : notnull
        where TInput : notnull
    {
        return (node.Type & StateMachineNodeType.Initial) != StateMachineNodeType.Default;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="node"/> is of <see cref="StateMachineNodeType.Dead"/> type.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// <b>true</b> when <paramref name="node"/> is an <see cref="StateMachineNodeType.Dead"/> node, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsDead<TState, TInput, TResult>(this IStateMachineNode<TState, TInput, TResult> node)
        where TState : notnull
        where TInput : notnull
    {
        return (node.Type & StateMachineNodeType.Dead) != StateMachineNodeType.Default;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="node"/> contains any transitions.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// <b>true</b> when <paramref name="node"/> contains at least one transition, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool CanTransition<TState, TInput, TResult>(this IStateMachineNode<TState, TInput, TResult> node)
        where TState : notnull
        where TInput : notnull
    {
        return node.Transitions.Count > 0;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="node"/> contains transition with the provided identifier.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <param name="input">Transition identifier to check.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// <b>true</b> when <paramref name="node"/> contains the provided transition, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool CanTransition<TState, TInput, TResult>(this IStateMachineNode<TState, TInput, TResult> node, TInput input)
        where TState : notnull
        where TInput : notnull
    {
        return node.Transitions.ContainsKey( input );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool CanTransitionTo<TState, TInput, TResult>(
        this IStateMachineNode<TState, TInput, TResult> node,
        TState destination,
        IEqualityComparer<TState> comparer)
        where TState : notnull
        where TInput : notnull
    {
        return node.Transitions.Any( t => comparer.Equals( t.Value.Destination.Value, destination ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IEnumerable<IStateMachineNode<TState, TInput, TResult>> GetAvailableDestinations<TState, TInput, TResult>(
        this IStateMachineNode<TState, TInput, TResult> node,
        IEqualityComparer<TState> comparer)
        where TState : notnull
        where TInput : notnull
    {
        return node.Transitions.Select( kv => kv.Value.Destination ).DistinctBy( n => n.Value, comparer );
    }

    [Pure]
    internal static IEnumerable<KeyValuePair<TInput, IStateMachineTransition<TState, TInput, TResult>>> FindTransitionsTo<
        TState, TInput, TResult>(
        this IStateMachineNode<TState, TInput, TResult> node,
        TState destination,
        IEqualityComparer<TState> comparer)
        where TState : notnull
        where TInput : notnull
    {
        return node.Transitions.Where( t => comparer.Equals( t.Value.Destination.Value, destination ) );
    }
}
