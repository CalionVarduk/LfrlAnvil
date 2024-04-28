using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Computable.Automata.Extensions;

namespace LfrlAnvil.Computable.Automata.Internal;

internal static class StateMachineOptimizer
{
    internal readonly struct Result<TState, TInput, TResult>
        where TState : notnull
        where TInput : notnull
    {
        internal readonly Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> States;
        internal readonly IStateMachineNode<TState, TInput, TResult> InitialState;

        public Result(
            Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> states,
            IStateMachineNode<TState, TInput, TResult> initialState)
        {
            States = states;
            InitialState = initialState;
        }
    }

    internal static Result<TState, TInput, TResult> OptimizeNew<TState, TInput, TResult>(
        Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> states,
        IStateMachineNode<TState, TInput, TResult> initialState,
        StateMachineOptimizationParams<TState> @params,
        IEqualityComparer<TInput> inputComparer)
        where TState : notnull
        where TInput : notnull
    {
        switch ( @params.Level )
        {
            case StateMachineOptimization.RemoveUnreachableStates:
            {
                var reachableStates = FindReachableStates( initialState, states.Comparer );
                var unreachableStates = CreateUnreachableStatesCollection( states, reachableStates );

                if ( unreachableStates.Length == 0 )
                    break;

                foreach ( var state in unreachableStates )
                    states.Remove( state );

                states.TrimExcess();
                break;
            }
            case StateMachineOptimization.Minimize:
            {
                var reachableStates = FindReachableStates( initialState, states.Comparer );
                var unreachableStates = CreateUnreachableStatesCollection( states, reachableStates );

                foreach ( var state in unreachableStates )
                    states.Remove( state );

                var stateNodes = states.Select( static kv => kv.Value ).ToArray();
                var equivalency = FindInitialEquivalentStatePairCandidates( stateNodes, states.Comparer );
                var deadStates = RemoveNonEquivalentStatePairCandidatesAndFindDeadStates( equivalency, states.Comparer );

                if ( equivalency.StatePairs.Count == 0 )
                {
                    if ( unreachableStates.Length > 0 )
                        states.TrimExcess();

                    MarkDeadStates( states, deadStates );
                    break;
                }

                Assume.IsNotNull( @params.StateMerger );
                var stateMappings = CreateEquivalentStateMappings( equivalency, @params.StateMerger, states.Comparer );
                initialState = RecreateMinimizedStatesAndGetInitialState( states, stateNodes, stateMappings, deadStates, inputComparer );
                break;
            }
        }

        return new Result<TState, TInput, TResult>( states, initialState );
    }

    [Pure]
    internal static Result<TState, TInput, TResult> OptimizeExisting<TState, TInput, TResult>(
        StateMachineOptimization currentOptimization,
        Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> states,
        IStateMachineNode<TState, TInput, TResult> initialState,
        StateMachineOptimizationParams<TState> @params,
        IEqualityComparer<TInput> inputComparer)
        where TState : notnull
        where TInput : notnull
    {
        Assume.IsLessThan( currentOptimization, @params.Level );

        switch ( @params.Level )
        {
            case StateMachineOptimization.RemoveUnreachableStates:
            {
                var reachableStates = FindReachableStates( initialState, states.Comparer );
                states = CreateReachableStatesDictionary( states, reachableStates, states.Comparer );
                break;
            }
            case StateMachineOptimization.Minimize:
            {
                var originalStates = states;
                if ( currentOptimization == StateMachineOptimization.None )
                {
                    var reachableStates = FindReachableStates( initialState, states.Comparer );
                    states = CreateReachableStatesDictionary( states, reachableStates, states.Comparer );
                }

                var stateNodes = states.Select( static kv => kv.Value ).ToArray();
                var equivalency = FindInitialEquivalentStatePairCandidates( stateNodes, states.Comparer );
                var deadStates = RemoveNonEquivalentStatePairCandidatesAndFindDeadStates( equivalency, states.Comparer );

                if ( equivalency.StatePairs.Count == 0 && deadStates.Count == 0 )
                {
                    if ( ReferenceEquals( originalStates, states ) )
                        states = new Dictionary<TState, IStateMachineNode<TState, TInput, TResult>>( states, states.Comparer );

                    break;
                }

                if ( ReferenceEquals( originalStates, states ) )
                    states = new Dictionary<TState, IStateMachineNode<TState, TInput, TResult>>( states.Comparer );

                Assume.IsNotNull( @params.StateMerger );
                var stateMappings = CreateEquivalentStateMappings( equivalency, @params.StateMerger!, states.Comparer );
                initialState = RecreateMinimizedStatesAndGetInitialState( states, stateNodes, stateMappings, deadStates, inputComparer );
                break;
            }
        }

        return new Result<TState, TInput, TResult>( states, initialState );
    }

    [Pure]
    private static HashSet<TState> FindReachableStates<TState, TInput, TResult>(
        IStateMachineNode<TState, TInput, TResult> initialState,
        IEqualityComparer<TState> stateComparer)
        where TState : notnull
        where TInput : notnull
    {
        var currentState = initialState;
        var reachableStates = new HashSet<TState>( stateComparer ) { currentState.Value };
        var remainingDestinations = new Stack<IStateMachineNode<TState, TInput, TResult>>();

        do
        {
            foreach ( var (_, transition) in currentState.Transitions )
            {
                if ( ReferenceEquals( currentState, transition.Destination ) )
                    continue;

                if ( ! reachableStates.Add( transition.Destination.Value ) )
                    continue;

                remainingDestinations.Push( transition.Destination );
            }
        }
        while ( remainingDestinations.TryPop( out currentState ) );

        return reachableStates;
    }

    [Pure]
    private static TState[] CreateUnreachableStatesCollection<TState, TInput, TResult>(
        IReadOnlyDictionary<TState, IStateMachineNode<TState, TInput, TResult>> states,
        IReadOnlySet<TState> reachableStates)
        where TState : notnull
        where TInput : notnull
    {
        if ( reachableStates.Count == states.Count )
            return Array.Empty<TState>();

        var index = 0;
        var unreachableStates = new TState[states.Count - reachableStates.Count];
        foreach ( var (state, _) in states )
        {
            if ( reachableStates.Contains( state ) )
                continue;

            unreachableStates[index++] = state;
        }

        return unreachableStates;
    }

    [Pure]
    private static Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> CreateReachableStatesDictionary<TState, TInput, TResult>(
        IReadOnlyDictionary<TState, IStateMachineNode<TState, TInput, TResult>> states,
        IReadOnlySet<TState> reachableStates,
        IEqualityComparer<TState> stateComparer)
        where TState : notnull
        where TInput : notnull
    {
        if ( reachableStates.Count == states.Count )
            return new Dictionary<TState, IStateMachineNode<TState, TInput, TResult>>( states, stateComparer );

        var result = new Dictionary<TState, IStateMachineNode<TState, TInput, TResult>>( stateComparer );
        foreach ( var (state, node) in states )
        {
            if ( ! reachableStates.Contains( state ) )
                continue;

            result.Add( state, node );
        }

        return result;
    }

    [Pure]
    private static StatePairEquivalencyResult<TState> FindInitialEquivalentStatePairCandidates<TState, TInput, TResult>(
        IStateMachineNode<TState, TInput, TResult>[] stateNodes,
        IEqualityComparer<TState> stateComparer)
        where TState : notnull
        where TInput : notnull
    {
        var transitionsBuffer = new List<StatePair<TState>>();
        var statePairs = new Dictionary<StatePair<TState>, (int StartIndex, int TransitionCount)>(
            StatePair<TState>.GetComparer( stateComparer ) );

        var nodeCountMinusOne = stateNodes.Length - 1;
        for ( var i = 0; i < nodeCountMinusOne; ++i )
        {
            var first = stateNodes[i];

            for ( var j = i + 1; j < stateNodes.Length; ++j )
            {
                var second = stateNodes[j];
                if ( first.IsAccept() != second.IsAccept() )
                    continue;

                var candidateResult = FindInitialTransitionsForEquivalentStatePairCandidate(
                    first,
                    second,
                    transitionsBuffer,
                    stateComparer,
                    statePairs.Comparer );

                if ( candidateResult.IsValid )
                    statePairs.Add( candidateResult.StatePair, candidateResult.TransitionRange );
            }
        }

        foreach ( var state in stateNodes )
        {
            if ( state.IsAccept() )
                continue;

            var candidateResult = FindInitialTransitionsForEquivalentStatePairWithSinkCandidate( state, transitionsBuffer, stateComparer );
            if ( candidateResult.IsValid )
                statePairs.Add( candidateResult.StatePair, candidateResult.TransitionRange );
        }

        return new StatePairEquivalencyResult<TState>( statePairs, transitionsBuffer );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StatePairCandidateResult<TState> FindInitialTransitionsForEquivalentStatePairCandidate<TState, TInput, TResult>(
        IStateMachineNode<TState, TInput, TResult> first,
        IStateMachineNode<TState, TInput, TResult> second,
        List<StatePair<TState>> transitionsBuffer,
        IEqualityComparer<TState> stateComparer,
        IEqualityComparer<StatePair<TState>> statePairComparer)
        where TState : notnull
        where TInput : notnull
    {
        var startIndex = transitionsBuffer.Count;
        var pair = StatePair<TState>.Create( first.Value, second.Value );

        foreach ( var (input, firstStateTransition) in first.Transitions )
        {
            if ( ! second.Transitions.TryGetValue( input, out var secondStateTransition ) )
            {
                if ( firstStateTransition.Handler is not null )
                    return StatePairCandidateResult<TState>.CreateInvalid( pair, startIndex, transitionsBuffer );

                transitionsBuffer.Add( StatePair<TState>.CreateWithSink( firstStateTransition.Destination.Value ) );
                continue;
            }

            if ( ! ReferenceEquals( firstStateTransition.Handler, secondStateTransition.Handler ) )
                return StatePairCandidateResult<TState>.CreateInvalid( pair, startIndex, transitionsBuffer );

            if ( stateComparer.Equals( firstStateTransition.Destination.Value, secondStateTransition.Destination.Value ) )
                continue;

            var transitionPair = StatePair<TState>.Create(
                firstStateTransition.Destination.Value,
                secondStateTransition.Destination.Value );

            if ( statePairComparer.Equals( pair, transitionPair ) )
                continue;

            transitionsBuffer.Add( transitionPair );
        }

        foreach ( var (input, secondStateTransition) in second.Transitions )
        {
            if ( first.Transitions.ContainsKey( input ) )
                continue;

            if ( secondStateTransition.Handler is not null )
                return StatePairCandidateResult<TState>.CreateInvalid( pair, startIndex, transitionsBuffer );

            transitionsBuffer.Add( StatePair<TState>.CreateWithSink( secondStateTransition.Destination.Value ) );
        }

        return StatePairCandidateResult<TState>.CreateValid( pair, startIndex, transitionsBuffer );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StatePairCandidateResult<TState> FindInitialTransitionsForEquivalentStatePairWithSinkCandidate<TState, TInput, TResult>(
        IStateMachineNode<TState, TInput, TResult> state,
        List<StatePair<TState>> transitionsBuffer,
        IEqualityComparer<TState> stateComparer)
        where TState : notnull
        where TInput : notnull
    {
        var startIndex = transitionsBuffer.Count;
        var pair = StatePair<TState>.CreateWithSink( state.Value );

        foreach ( var (_, transition) in state.Transitions )
        {
            if ( transition.Handler is not null )
                return StatePairCandidateResult<TState>.CreateInvalid( pair, startIndex, transitionsBuffer );

            if ( stateComparer.Equals( state.Value, transition.Destination.Value ) )
                continue;

            transitionsBuffer.Add( StatePair<TState>.CreateWithSink( transition.Destination.Value ) );
        }

        return StatePairCandidateResult<TState>.CreateValid( pair, startIndex, transitionsBuffer );
    }

    private static HashSet<TState> RemoveNonEquivalentStatePairCandidatesAndFindDeadStates<TState>(
        StatePairEquivalencyResult<TState> equivalency,
        IEqualityComparer<TState> stateComparer)
        where TState : notnull
    {
        var removedPairs = new List<StatePair<TState>>();
        var deadStates = new HashSet<TState>( stateComparer );

        do
        {
            removedPairs.Clear();

            foreach ( var (pair, transitionRange) in equivalency.StatePairs )
            {
                for ( var i = transitionRange.StartIndex; i < transitionRange.EndIndex; ++i )
                {
                    var transition = equivalency.TransitionsBuffer[i];
                    if ( equivalency.StatePairs.ContainsKey( transition ) )
                        continue;

                    removedPairs.Add( pair );
                    break;
                }
            }

            foreach ( var pair in removedPairs )
                equivalency.StatePairs.Remove( pair );
        }
        while ( removedPairs.Count > 0 );

        removedPairs.Clear();

        foreach ( var (pair, _) in equivalency.StatePairs )
        {
            if ( pair.HasSecond )
                continue;

            removedPairs.Add( pair );
            deadStates.Add( pair.First );
        }

        foreach ( var pair in removedPairs )
            equivalency.StatePairs.Remove( pair );

        return deadStates;
    }

    private static void MarkDeadStates<TState, TInput, TResult>(
        Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> states,
        HashSet<TState> deadStates)
        where TState : notnull
        where TInput : notnull
    {
        if ( deadStates.Count == 0 )
            return;

        foreach ( var state in deadStates )
        {
            var node = states[state];
            var mutableNode = ReinterpretCast.To<StateMachineNode<TState, TInput, TResult>>( node );
            mutableNode.Type |= StateMachineNodeType.Dead;
        }
    }

    [Pure]
    private static Dictionary<TState, Ref<TState>> CreateEquivalentStateMappings<TState>(
        StatePairEquivalencyResult<TState> equivalency,
        Func<TState, TState, TState> stateMerger,
        IEqualityComparer<TState> stateComparer)
        where TState : notnull
    {
        var stateMappings = new Dictionary<TState, Ref<TState>>( stateComparer );
        foreach ( var (pair, _) in equivalency.StatePairs )
        {
            Assume.True( pair.HasSecond );
            Assume.IsNotNull( pair.Second );

            if ( stateMappings.TryGetValue( pair.First, out var mapping ) )
            {
                if ( stateMappings.ContainsKey( pair.Second ) )
                    continue;

                mapping.Value = stateMerger( mapping.Value, pair.Second );
                stateMappings.Add( pair.Second, mapping );
                continue;
            }

            if ( stateMappings.TryGetValue( pair.Second, out mapping ) )
            {
                mapping.Value = stateMerger( mapping.Value, pair.First );
                stateMappings.Add( pair.First, mapping );
                continue;
            }

            mapping = Ref.Create( stateMerger( pair.First, pair.Second ) );
            stateMappings.Add( pair.First, mapping );
            stateMappings.Add( pair.Second, mapping );
        }

        return stateMappings;
    }

    private static IStateMachineNode<TState, TInput, TResult> RecreateMinimizedStatesAndGetInitialState<TState, TInput, TResult>(
        Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> states,
        IStateMachineNode<TState, TInput, TResult>[] originalStateNodes,
        Dictionary<TState, Ref<TState>> equivalentStateMappings,
        HashSet<TState> deadStates,
        IEqualityComparer<TInput> inputComparer)
        where TState : notnull
        where TInput : notnull
    {
        IStateMachineNode<TState, TInput, TResult>? initialState = null;
        states.Clear();

        foreach ( var originalNode in originalStateNodes )
        {
            if ( ! equivalentStateMappings.TryGetValue( originalNode.Value, out var mapping ) )
            {
                var newNode = RecreateStateMachineNode( originalNode, originalNode.Value, deadStates, inputComparer );
                states.Add( originalNode.Value, newNode );
                continue;
            }

            ref var node = ref CollectionsMarshal.GetValueRefOrAddDefault( states, mapping.Value, out var exists )!;
            if ( ! exists )
            {
                node = RecreateStateMachineNode( originalNode, mapping.Value, deadStates, inputComparer );
                continue;
            }

            var mutableNode = ReinterpretCast.To<StateMachineNode<TState, TInput, TResult>>( node );
            mutableNode.Type |= originalNode.Type;
        }

        foreach ( var originalNode in originalStateNodes )
        {
            var node = equivalentStateMappings.TryGetValue( originalNode.Value, out var mapping )
                ? states[mapping.Value]
                : states[originalNode.Value];

            if ( node.IsInitial() )
                initialState = node;

            foreach ( var (input, originalTransition) in originalNode.Transitions )
            {
                ref var transition = ref CollectionsMarshal.GetValueRefOrAddDefault(
                    ReinterpretCast.To<Dictionary<TInput, IStateMachineTransition<TState, TInput, TResult>>>( node.Transitions ),
                    input,
                    out var exists );

                if ( exists )
                    continue;

                var destination = equivalentStateMappings.TryGetValue( originalTransition.Destination.Value, out mapping )
                    ? states[mapping.Value]
                    : states[originalTransition.Destination.Value];

                transition = new StateMachineTransition<TState, TInput, TResult>( destination, originalTransition.Handler );
            }
        }

        Assume.IsNotNull( initialState );
        return initialState;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StateMachineNode<TState, TInput, TResult> RecreateStateMachineNode<TState, TInput, TResult>(
        IStateMachineNode<TState, TInput, TResult> originalNode,
        TState state,
        HashSet<TState> deadStates,
        IEqualityComparer<TInput> inputComparer)
        where TState : notnull
        where TInput : notnull
    {
        var deadStateMask = deadStates.Contains( originalNode.Value ) ? StateMachineNodeType.Dead : StateMachineNodeType.Default;

        var result = new StateMachineNode<TState, TInput, TResult>(
            state,
            originalNode.Type | deadStateMask,
            new Dictionary<TInput, IStateMachineTransition<TState, TInput, TResult>>( inputComparer ) );

        return result;
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
                return unchecked( _stateComparer.GetHashCode( obj.First )
                    + (obj.HasSecond ? _stateComparer.GetHashCode( obj.Second ) : 0) );
            }
        }
    }

    private readonly struct StatePairEquivalencyResult<TState>
        where TState : notnull
    {
        internal readonly Dictionary<StatePair<TState>, (int StartIndex, int EndIndex)> StatePairs;
        internal readonly List<StatePair<TState>> TransitionsBuffer;

        internal StatePairEquivalencyResult(
            Dictionary<StatePair<TState>, (int StartIndex, int EndIndex)> statePairs,
            List<StatePair<TState>> transitionsBuffer)
        {
            StatePairs = statePairs;
            TransitionsBuffer = transitionsBuffer;
        }
    }

    private readonly struct StatePairCandidateResult<TState>
        where TState : notnull
    {
        internal readonly StatePair<TState> StatePair;
        internal readonly (int StartIndex, int EndIndex) TransitionRange;
        internal readonly bool IsValid;

        private StatePairCandidateResult(StatePair<TState> statePair, int startIndex, int endIndex, bool isValid)
        {
            StatePair = statePair;
            TransitionRange = (startIndex, endIndex);
            IsValid = isValid;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static StatePairCandidateResult<TState> CreateValid(
            StatePair<TState> statePair,
            int startIndex,
            IReadOnlyCollection<StatePair<TState>> transitionsBuffer)
        {
            return new StatePairCandidateResult<TState>( statePair, startIndex, transitionsBuffer.Count, isValid: true );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static StatePairCandidateResult<TState> CreateInvalid(
            StatePair<TState> statePair,
            int startIndex,
            List<StatePair<TState>> transitionsBuffer)
        {
            var transitionCount = transitionsBuffer.Count - startIndex;
            transitionsBuffer.RemoveRange( startIndex, transitionCount );
            return new StatePairCandidateResult<TState>( statePair, startIndex, transitionsBuffer.Count, isValid: false );
        }
    }
}
