using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.Computable.Automata.Internal;

internal static class StateMachineOptimizer
{
    internal static void RemoveUnreachableStates<TState, TInput, TResult>(
        Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> states,
        IStateMachineNode<TState, TInput, TResult> initialState)
        where TState : notnull
        where TInput : notnull
    {
        var reachedStates = new HashSet<TState>( states.Comparer ) { initialState.Value };
        var remainingDestinations = new Stack<IStateMachineNode<TState, TInput, TResult>>(
            initialState.Transitions.Select( kv => kv.Value.Destination ).Where( d => ! ReferenceEquals( d, initialState ) ) );

        while ( remainingDestinations.TryPop( out var destination ) )
        {
            if ( ! reachedStates.Add( destination.Value ) )
                continue;

            foreach ( var (_, transition) in destination.Transitions )
            {
                if ( ReferenceEquals( destination, transition.Destination ) )
                    continue;

                remainingDestinations.Push( transition.Destination );
            }
        }

        if ( reachedStates.Count == states.Count )
            return;

        var index = 0;
        var unreachableStates = new TState[states.Count - reachedStates.Count];
        foreach ( var (state, _) in states )
        {
            if ( reachedStates.Contains( state ) )
                continue;

            unreachableStates[index++] = state;
        }

        foreach ( var state in unreachableStates )
            states.Remove( state );

        states.TrimExcess();
    }
}
