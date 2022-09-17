using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Automata.Extensions;

public static class StateMachineExtensions
{
    [Pure]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> FindAcceptStates<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Select( kv => kv.Value ).Where( s => s.IsAccept() );
    }

    [Pure]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> FindDefaultStates<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Select( kv => kv.Value ).Where( s => ! s.IsAccept() );
    }

    [Pure]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> FindDeadStates<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Select( kv => kv.Value ).Where( s => s.IsDead() );
    }

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

    [Pure]
    public static IEnumerable<KeyValuePair<IStateMachineNode<TState, TInput, TResult>,
            KeyValuePair<TInput, IStateMachineTransition<TState, TInput, TResult>>>>
        GetTransitions<TState, TInput, TResult>(this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Flatten( kv => kv.Value.Transitions, (s, d) => KeyValuePair.Create( s.Value, d ) );
    }

    [Pure]
    public static IEnumerable<TInput> GetAlphabet<TState, TInput, TResult>(this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States
            .SelectMany( kv => kv.Value.Transitions.Select( t => t.Key ) )
            .Distinct( machine.InputComparer );
    }

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
