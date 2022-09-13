using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Automata.Extensions;

public static class StateMachineInstanceExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsAccepted<TState, TInput, TResult>(this IStateMachineInstance<TState, TInput, TResult> instance)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.IsAccept();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool CanTransition<TState, TInput, TResult>(this IStateMachineInstance<TState, TInput, TResult> instance)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.CanTransition();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool CanTransitionTo<TState, TInput, TResult>(
        this IStateMachineInstance<TState, TInput, TResult> instance,
        TState destination)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.CanTransitionTo( destination, instance.Machine.StateComparer );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool CanTransition<TState, TInput, TResult>(this IStateMachineInstance<TState, TInput, TResult> instance, TInput input)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.CanTransition( input );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> GetAvailableDestinations<TState, TInput, TResult>(
        this IStateMachineInstance<TState, TInput, TResult> instance)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.GetAvailableDestinations( instance.Machine.StateComparer );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IStateMachineInstance<TState, TInput, TResult> Clone<TState, TInput, TResult>(
        this IStateMachineInstance<TState, TInput, TResult> instance)
        where TState : notnull
        where TInput : notnull
    {
        return ReferenceEquals( instance, instance.Subject )
            ? instance.Machine.CreateInstance( instance.CurrentState.Value )
            : instance.Clone( instance.Subject );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IStateMachineInstance<TState, TInput, TResult> Clone<TState, TInput, TResult>(
        this IStateMachineInstance<TState, TInput, TResult> instance,
        object subject)
        where TState : notnull
        where TInput : notnull
    {
        return instance.Machine.CreateInstanceWithSubject( instance.CurrentState.Value, subject );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StateMachineInstance<TState, TInput, TResult> Clone<TState, TInput, TResult>(
        this StateMachineInstance<TState, TInput, TResult> instance)
        where TState : notnull
        where TInput : notnull
    {
        return ReferenceEquals( instance, instance.Subject )
            ? new StateMachineInstance<TState, TInput, TResult>( instance.Machine, instance.CurrentState )
            : instance.Clone( instance.Subject );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StateMachineInstance<TState, TInput, TResult> Clone<TState, TInput, TResult>(
        this StateMachineInstance<TState, TInput, TResult> instance,
        object subject)
        where TState : notnull
        where TInput : notnull
    {
        return new StateMachineInstance<TState, TInput, TResult>( instance.Machine, instance.CurrentState, subject );
    }
}
