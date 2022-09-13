using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Automata.Extensions;

public static class StateMachineNodeExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsAccept<TState, TInput, TResult>(this IStateMachineNode<TState, TInput, TResult> node)
        where TState : notnull
        where TInput : notnull
    {
        return (node.Type & StateMachineNodeType.Accept) != StateMachineNodeType.Default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsInitial<TState, TInput, TResult>(this IStateMachineNode<TState, TInput, TResult> node)
        where TState : notnull
        where TInput : notnull
    {
        return (node.Type & StateMachineNodeType.Initial) != StateMachineNodeType.Default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool CanTransition<TState, TInput, TResult>(this IStateMachineNode<TState, TInput, TResult> node)
        where TState : notnull
        where TInput : notnull
    {
        return node.Transitions.Count > 0;
    }

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
}
