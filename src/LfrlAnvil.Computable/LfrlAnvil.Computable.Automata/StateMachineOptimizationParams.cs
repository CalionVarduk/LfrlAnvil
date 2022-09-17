using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Automata;

public readonly struct StateMachineOptimizationParams<TState>
{
    private StateMachineOptimizationParams(StateMachineOptimization level, Func<TState, TState, TState>? stateMerger)
    {
        Level = level;
        StateMerger = stateMerger;
    }

    public StateMachineOptimization Level { get; }
    public Func<TState, TState, TState>? StateMerger { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StateMachineOptimizationParams<TState> None()
    {
        return new StateMachineOptimizationParams<TState>( StateMachineOptimization.None, stateMerger: null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StateMachineOptimizationParams<TState> RemoveUnreachableStates()
    {
        return new StateMachineOptimizationParams<TState>( StateMachineOptimization.RemoveUnreachableStates, stateMerger: null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StateMachineOptimizationParams<TState> Minimize(Func<TState, TState, TState> stateMerger)
    {
        return new StateMachineOptimizationParams<TState>( StateMachineOptimization.Minimize, stateMerger );
    }

    [Pure]
    public override string ToString()
    {
        return Level.ToString();
    }
}
