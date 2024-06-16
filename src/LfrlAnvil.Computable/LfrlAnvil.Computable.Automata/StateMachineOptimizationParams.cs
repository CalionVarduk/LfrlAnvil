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

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents parameters of state machine optimization.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
public readonly struct StateMachineOptimizationParams<TState>
{
    private StateMachineOptimizationParams(StateMachineOptimization level, Func<TState, TState, TState>? stateMerger)
    {
        Level = level;
        StateMerger = stateMerger;
    }

    /// <summary>
    /// Chosen <see cref="StateMachineOptimization"/> level.
    /// </summary>
    public StateMachineOptimization Level { get; }

    /// <summary>
    /// Optional state merger used in <see cref="StateMachineOptimization.Minimize"/> level
    /// that creates a single state out of two merged states.
    /// </summary>
    public Func<TState, TState, TState>? StateMerger { get; }

    /// <summary>
    /// Creates a new <see cref="StateMachineOptimizationParams{TState}"/> instance with <see cref="StateMachineOptimization.None"/> level.
    /// </summary>
    /// <returns>New <see cref="StateMachineOptimizationParams{TState}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StateMachineOptimizationParams<TState> None()
    {
        return new StateMachineOptimizationParams<TState>( StateMachineOptimization.None, stateMerger: null );
    }

    /// <summary>
    /// Creates a new <see cref="StateMachineOptimizationParams{TState}"/> instance
    /// with <see cref="StateMachineOptimization.RemoveUnreachableStates"/> level.
    /// </summary>
    /// <returns>New <see cref="StateMachineOptimizationParams{TState}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StateMachineOptimizationParams<TState> RemoveUnreachableStates()
    {
        return new StateMachineOptimizationParams<TState>( StateMachineOptimization.RemoveUnreachableStates, stateMerger: null );
    }

    /// <summary>
    /// Creates a new <see cref="StateMachineOptimizationParams{TState}"/> instance
    /// with <see cref="StateMachineOptimization.Minimize"/> level.
    /// </summary>
    /// <param name="stateMerger">State merger that creates a single state out of two merged states.</param>
    /// <returns>New <see cref="StateMachineOptimizationParams{TState}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StateMachineOptimizationParams<TState> Minimize(Func<TState, TState, TState> stateMerger)
    {
        return new StateMachineOptimizationParams<TState>( StateMachineOptimization.Minimize, stateMerger );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="StateMachineOptimizationParams{TState}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Level.ToString();
    }
}
