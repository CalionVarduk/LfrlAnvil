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
