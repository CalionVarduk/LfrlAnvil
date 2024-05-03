using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Automata.Exceptions;

namespace LfrlAnvil.Computable.Automata.Extensions;

/// <summary>
/// Contains <see cref="IStateMachineInstance{TState,TInput,TResult}"/> extension methods.
/// </summary>
public static class StateMachineInstanceExtensions
{
    /// <summary>
    /// Checks whether or not the <paramref name="instance"/> is in accepted state.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns><b>true</b> when <paramref name="instance"/> is in accepted state, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsAccepted<TState, TInput, TResult>(this IStateMachineInstance<TState, TInput, TResult> instance)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.IsAccept();
    }

    /// <summary>
    /// Checks whether or not the <paramref name="instance"/> can transition to an accepted state.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns><b>true</b> when <paramref name="instance"/> can transition to an accepted state, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool CanAccept<TState, TInput, TResult>(this IStateMachineInstance<TState, TInput, TResult> instance)
        where TState : notnull
        where TInput : notnull
    {
        return ! instance.CurrentState.IsDead();
    }

    /// <summary>
    /// Checks whether or not the <paramref name="instance"/> can transition to any other state.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns><b>true</b> when <paramref name="instance"/> can transition to any other state, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool CanTransition<TState, TInput, TResult>(this IStateMachineInstance<TState, TInput, TResult> instance)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.CanTransition();
    }

    /// <summary>
    /// Checks whether or not the <paramref name="instance"/> can transition to the provided <paramref name="destination"/> state.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <param name="destination">Destination state to check.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// <b>true</b> when <paramref name="instance"/> can transition to the <paramref name="destination"/> state, otherwise <b>false</b>.
    /// </returns>
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

    /// <summary>
    /// Checks whether or not the <paramref name="instance"/> can transition with the provided transition identifier.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <param name="input">Transition identifier.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// <b>true</b> when <paramref name="instance"/> can transition with the provided transition identifier, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool CanTransition<TState, TInput, TResult>(this IStateMachineInstance<TState, TInput, TResult> instance, TInput input)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.CanTransition( input );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all currently available destination states.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> GetAvailableDestinations<TState, TInput, TResult>(
        this IStateMachineInstance<TState, TInput, TResult> instance)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.GetAvailableDestinations( instance.Machine.StateComparer );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all currently available transitions
    /// to <paramref name="destination"/> state.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <param name="destination">Destination state.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
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

    /// <summary>
    /// Attempts to return an <see cref="IStateMachineTransition{TState,TInput,TResult}"/> instance
    /// for the current state and transition identifier.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <param name="input">Transition identifier.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns an <see cref="IStateMachineTransition{TState,TInput,TResult}"/> instance
    /// associated with the current state and transition identifier.
    /// </param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns><b>true</b> when transition exists, otherwise <b>false</b>.</returns>
    public static bool TryGetTransition<TState, TInput, TResult>(
        this IStateMachineInstance<TState, TInput, TResult> instance,
        TInput input,
        [MaybeNullWhen( false )] out IStateMachineTransition<TState, TInput, TResult> result)
        where TState : notnull
        where TInput : notnull
    {
        return instance.CurrentState.Transitions.TryGetValue( input, out result );
    }

    /// <summary>
    /// Returns an <see cref="IStateMachineTransition{TState,TInput,TResult}"/> instance for the current state and transition identifier.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <param name="input">Transition identifier.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// <see cref="IStateMachineTransition{TState,TInput,TResult}"/> instance associated with the current state and transition identifier.
    /// </returns>
    /// <exception cref="StateMachineTransitionException">When transition does not exist in the current state.</exception>
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

    /// <summary>
    /// Creates a clone of the provided state machine <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// New <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance equivalent to the provided <paramref name="instance"/>.
    /// </returns>
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

    /// <summary>
    /// Creates a clone of the provided state machine <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <param name="subject">Custom subject.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// New <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance equivalent to the provided <paramref name="instance"/>.
    /// </returns>
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

    /// <summary>
    /// Creates a clone of the provided state machine <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// New <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance equivalent to the provided <paramref name="instance"/>.
    /// </returns>
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

    /// <summary>
    /// Creates a clone of the provided state machine <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Source state machine instance.</param>
    /// <param name="subject">Custom subject.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>
    /// New <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance equivalent to the provided <paramref name="instance"/>.
    /// </returns>
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
