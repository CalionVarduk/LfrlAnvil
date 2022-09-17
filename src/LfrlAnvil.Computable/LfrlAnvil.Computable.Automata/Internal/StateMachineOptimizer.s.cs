using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Automata.Extensions;
using LfrlAnvil.Extensions;

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
            // TODO: move this to the inner foreach
            // if a state has already been marked as reached, then transition to that state no longer need to be added to the stack
            // optimize that for the initial state as well
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

    internal static void Minimize<TState, TInput, TResult>(
        Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> states,
        ref IStateMachineNode<TState, TInput, TResult> initialState,
        Func<TState, TState, TState> stateMerger,
        IEqualityComparer<TInput> inputComparer)
        where TState : notnull
        where TInput : notnull
    {
        var stateNodes = states.ToArray( kv => kv.Value );
        var statePairs = new Dictionary<StatePair<TState>, List<StatePair<TState>>>( StatePair<TState>.GetComparer( states.Comparer ) );

        // this loop gets the initial equivalency candidate state pairs
        for ( var i = 0; i < stateNodes.Length - 1; ++i )
        {
            var a = stateNodes[i];

            for ( var j = i + 1; j < stateNodes.Length; ++j )
            {
                var b = stateNodes[j];

                if ( a.IsAccept() != b.IsAccept() )
                    continue;

                var omit = false;
                var pair = StatePair<TState>.Create( a.Value, b.Value );
                var transitionPairs = new List<StatePair<TState>>();

                foreach ( var (input, aTransition) in a.Transitions )
                {
                    if ( ! b.Transitions.TryGetValue( input, out var bTransition ) )
                    {
                        if ( aTransition.Handler is not null )
                        {
                            omit = true;
                            break;
                        }

                        transitionPairs.Add( StatePair<TState>.CreateWithSink( aTransition.Destination.Value ) );
                        continue;
                    }

                    if ( ! ReferenceEquals( aTransition.Handler, bTransition.Handler ) )
                    {
                        omit = true;
                        break;
                    }

                    if ( states.Comparer.Equals( aTransition.Destination.Value, bTransition.Destination.Value ) )
                        continue;

                    var transitionPair = StatePair<TState>.Create( aTransition.Destination.Value, bTransition.Destination.Value );
                    if ( statePairs.Comparer.Equals( pair, transitionPair ) )
                        continue;

                    transitionPairs.Add( transitionPair );
                }

                if ( omit )
                    continue;

                foreach ( var (input, bTransition) in b.Transitions )
                {
                    if ( a.Transitions.ContainsKey( input ) )
                        continue;

                    if ( bTransition.Handler is not null )
                    {
                        omit = true;
                        break;
                    }

                    transitionPairs.Add( StatePair<TState>.CreateWithSink( bTransition.Destination.Value ) );
                }

                if ( omit )
                    continue;

                statePairs.Add( pair, transitionPairs );
            }
        }

        // this loop adds possible equivalency to sink state, for each actual state
        foreach ( var state in stateNodes )
        {
            if ( state.IsAccept() )
                continue;

            var omit = false;
            var pair = StatePair<TState>.CreateWithSink( state.Value );
            var transitionPairs = new List<StatePair<TState>>();

            foreach ( var (_, transition) in state.Transitions )
            {
                if ( transition.Handler is not null )
                {
                    omit = true;
                    break;
                }

                if ( states.Comparer.Equals( state.Value, transition.Destination.Value ) )
                    continue;

                transitionPairs.Add( StatePair<TState>.CreateWithSink( transition.Destination.Value ) );
            }

            if ( omit )
                continue;

            statePairs.Add( pair, transitionPairs );
        }

        // this loop removes non-equivalent state pairs by checking their transition pairs
        var removedPairs = new List<StatePair<TState>>();
        do
        {
            removedPairs.Clear();

            foreach ( var (pair, transitions) in statePairs )
            {
                foreach ( var transition in transitions )
                {
                    if ( statePairs.ContainsKey( transition ) )
                        continue;

                    removedPairs.Add( pair );
                    break;
                }
            }

            foreach ( var pair in removedPairs )
                statePairs.Remove( pair );
        }
        while ( removedPairs.Count > 0 );

        // this & the next loop remove equivalent state pairs when one of the states is a sink (this won't change anything)
        // these states could actually be marked as Dead (no combination of transitions can lead from these states to any accept state)
        removedPairs.Clear();
        foreach ( var (pair, _) in statePairs )
        {
            if ( ! pair.HasSecond )
                removedPairs.Add( pair );
        }

        foreach ( var pair in removedPairs )
            statePairs.Remove( pair );

        // if none of the state pairs are equivalent, then don't change the state dictionary
        if ( statePairs.Count == 0 )
            return;

        // this loop constructs state symbol mappings for equivalent states
        var stateMappings = new Dictionary<TState, Ref<TState>>( states.Comparer );
        foreach ( var (pair, _) in statePairs )
        {
            Assume.Equals( pair.HasSecond, true, nameof( pair.HasSecond ) );

            if ( stateMappings.TryGetValue( pair.First, out var mapping ) )
            {
                if ( stateMappings.ContainsKey( pair.Second! ) )
                    continue;

                mapping.Value = stateMerger( mapping.Value, pair.Second! );
                stateMappings.Add( pair.Second!, mapping );
                continue;
            }

            if ( stateMappings.TryGetValue( pair.Second!, out mapping ) )
            {
                mapping.Value = stateMerger( mapping.Value, pair.First );
                stateMappings.Add( pair.First, mapping );
                continue;
            }

            mapping = Ref.Create( stateMerger( pair.First, pair.Second! ) );
            stateMappings.Add( pair.First, mapping );
            stateMappings.Add( pair.Second!, mapping );
        }

        // this loop constructs new states
        // TODO: move initial state assignment to this loop, since it will be faster than iterating over a dictionary
        states.Clear();
        foreach ( var oldNode in stateNodes )
        {
            if ( stateMappings.TryGetValue( oldNode.Value, out var mapping ) )
            {
                if ( states.TryGetValue( mapping.Value, out var node ) )
                {
                    // TODO: refactor, there is no need to create a whole new object, simply adjust its type
                    states[mapping.Value] = new StateMachineNode<TState, TInput, TResult>(
                        mapping.Value,
                        oldNode.Type | node.Type,
                        new Dictionary<TInput, IStateMachineTransition<TState, TInput, TResult>>( inputComparer ) );

                    continue;
                }

                states.Add(
                    mapping.Value,
                    new StateMachineNode<TState, TInput, TResult>(
                        mapping.Value,
                        oldNode.Type,
                        new Dictionary<TInput, IStateMachineTransition<TState, TInput, TResult>>( inputComparer ) ) );

                continue;
            }

            states.Add(
                oldNode.Value,
                new StateMachineNode<TState, TInput, TResult>(
                    oldNode.Value,
                    oldNode.Type,
                    new Dictionary<TInput, IStateMachineTransition<TState, TInput, TResult>>( inputComparer ) ) );
        }

        // this loop reconstructs state transitions for new states
        foreach ( var oldNode in stateNodes )
        {
            var node = stateMappings.TryGetValue( oldNode.Value, out var mapping ) ? states[mapping.Value] : states[oldNode.Value];
            foreach ( var (input, oldTransition) in oldNode.Transitions )
            {
                if ( node.Transitions.ContainsKey( input ) )
                    continue;

                var destination = stateMappings.TryGetValue( oldTransition.Destination.Value, out var destMapping )
                    ? states[destMapping.Value]
                    : states[oldTransition.Destination.Value];

                var transition = new StateMachineTransition<TState, TInput, TResult>( destination, oldTransition.Handler );
                var mutTransitions = ReinterpretCast.To<Dictionary<TInput, IStateMachineTransition<TState, TInput, TResult>>>(
                    node.Transitions );

                mutTransitions.Add( input, transition );
            }
        }

        initialState = states.First( kv => kv.Value.IsInitial() ).Value;
    }

    private readonly struct StatePair<TState>
        where TState : notnull
    {
        internal readonly TState First;
        internal readonly TState? Second;

        private StatePair(TState first, TState? second, bool hasSecond)
        {
            First = first;
            Second = second;
            HasSecond = hasSecond;
        }

        [MemberNotNullWhen( true, nameof( Second ) )]
        internal bool HasSecond { get; }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static StatePair<TState> Create(TState first, TState second)
        {
            return new StatePair<TState>( first, second, true );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static StatePair<TState> CreateWithSink(TState first)
        {
            return new StatePair<TState>( first, default, false );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static IEqualityComparer<StatePair<TState>> GetComparer(IEqualityComparer<TState> stateComparer)
        {
            return new Comparer( stateComparer );
        }

        [Pure]
        public override string ToString()
        {
            return $"('{First}', {(HasSecond ? $"'{Second}'" : "<sink>")})";
        }

        private sealed class Comparer : IEqualityComparer<StatePair<TState>>
        {
            private readonly IEqualityComparer<TState> _stateComparer;

            internal Comparer(IEqualityComparer<TState> stateComparer)
            {
                _stateComparer = stateComparer;
            }

            [Pure]
            public bool Equals(StatePair<TState> x, StatePair<TState> y)
            {
                if ( ! x.HasSecond )
                    return ! y.HasSecond && _stateComparer.Equals( x.First, y.First );

                if ( ! y.HasSecond )
                    return false;

                if ( _stateComparer.Equals( x.First, y.First ) )
                    return _stateComparer.Equals( x.Second, y.Second );

                return _stateComparer.Equals( x.First, y.Second ) && _stateComparer.Equals( x.Second, y.First );
            }

            [Pure]
            public int GetHashCode(StatePair<TState> obj)
            {
                return unchecked( _stateComparer.GetHashCode( obj.First ) +
                    (obj.HasSecond ? _stateComparer.GetHashCode( obj.Second ) : 0) );
            }
        }
    }
}
