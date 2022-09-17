using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Automata.Exceptions;

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
    public static bool CanAccept<TState, TInput, TResult>(this IStateMachineInstance<TState, TInput, TResult> instance)
        where TState : notnull
        where TInput : notnull
    {
        return ! instance.CurrentState.IsDead();
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
    public static IEnumerable<KeyValuePair<TInput, IStateMachineTransition<TState, TInput, TResult>>> FindTransitionsTo<
        TState, TInput, TResult>(
        this IStateMachineInstance<TState, TInput, TResult> instance,
        TState destination)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.FindTransitionsTo( destination, instance.Machine.StateComparer );
    }

    public static bool TryGetTransition<TState, TInput, TResult>(
        this IStateMachineInstance<TState, TInput, TResult> instance,
        TInput input,
        [MaybeNullWhen( false )] out IStateMachineTransition<TState, TInput, TResult> result)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.Transitions.TryGetValue( input, out result );
    }

    [Pure]
    public static IStateMachineTransition<TState, TInput, TResult> GetTransition<TState, TInput, TResult>(
        this IStateMachineInstance<TState, TInput, TResult> instance,
        TInput input)
        where TState : notnull
        where TInput : notnull
    {
        if ( instance.TryGetTransition( input, out var result ) )
            return result;

        throw new StateMachineTransitionException(
            Resources.TransitionDoesNotExist( instance.CurrentState.Value, input ),
            nameof( input ) );
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
