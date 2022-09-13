using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Automata.Extensions;

public static class StateMachineExtensions
{
    [Pure]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> GetAcceptStates<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Select( kv => kv.Value ).Where( s => s.IsAccept() );
    }

    [Pure]
    public static IEnumerable<IStateMachineNode<TState, TInput, TResult>> GetDefaultStates<TState, TInput, TResult>(
        this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Select( kv => kv.Value ).Where( s => ! s.IsAccept() );
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
    public static IEnumerable<KeyValuePair<IStateMachineNode<TState, TInput, TResult>, IStateMachineTransition<TState, TInput, TResult>>>
        GetTransitions<TState, TInput, TResult>(this IStateMachine<TState, TInput, TResult> machine)
        where TState : notnull
        where TInput : notnull
    {
        return machine.States.Flatten( kv => kv.Value.Transitions, (s, d) => KeyValuePair.Create( s.Value, d.Value ) );
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
}
