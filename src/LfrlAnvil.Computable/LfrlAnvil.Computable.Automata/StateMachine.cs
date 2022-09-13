using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Computable.Automata.Internal;

namespace LfrlAnvil.Computable.Automata;

public sealed class StateMachine<TState, TInput, TResult> : IStateMachine<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    private readonly Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> _states;

    internal StateMachine(
        Dictionary<TState, IStateMachineNode<TState, TInput, TResult>> states,
        IStateMachineNode<TState, TInput, TResult> initialState,
        IEqualityComparer<TInput> inputComparer,
        TResult defaultResult,
        StateMachineOptimization optimization)
    {
        _states = states;
        InitialState = initialState;
        InputComparer = inputComparer;
        DefaultResult = defaultResult;
        Optimization = optimization;
    }

    public IStateMachineNode<TState, TInput, TResult> InitialState { get; }
    public IEqualityComparer<TInput> InputComparer { get; }
    public TResult DefaultResult { get; }
    public StateMachineOptimization Optimization { get; }

    public IReadOnlyDictionary<TState, IStateMachineNode<TState, TInput, TResult>> States => _states;
    public IEqualityComparer<TState> StateComparer => _states.Comparer;

    [Pure]
    public StateMachineInstance<TState, TInput, TResult> CreateInstance()
    {
        return new StateMachineInstance<TState, TInput, TResult>( this, InitialState );
    }

    [Pure]
    public StateMachineInstance<TState, TInput, TResult> CreateInstance(TState initialState)
    {
        return new StateMachineInstance<TState, TInput, TResult>( this, GetInitialStateNodeOrThrow( initialState ) );
    }

    [Pure]
    public StateMachineInstance<TState, TInput, TResult> CreateInstanceWithSubject(object subject)
    {
        return new StateMachineInstance<TState, TInput, TResult>( this, InitialState, subject );
    }

    [Pure]
    public StateMachineInstance<TState, TInput, TResult> CreateInstanceWithSubject(TState initialState, object subject)
    {
        return new StateMachineInstance<TState, TInput, TResult>( this, GetInitialStateNodeOrThrow( initialState ), subject );
    }

    [Pure]
    public StateMachine<TState, TInput, TResult> WithOptimization(StateMachineOptimization optimization)
    {
        Ensure.IsDefined( optimization, nameof( optimization ) );
        if ( Optimization >= optimization )
            return this;

        var states = new Dictionary<TState, IStateMachineNode<TState, TInput, TResult>>( _states, StateComparer );
        StateMachineOptimizer.RemoveUnreachableStates( states, InitialState );
        return new StateMachine<TState, TInput, TResult>( states, InitialState, InputComparer, DefaultResult, optimization );
    }

    [Pure]
    private IStateMachineNode<TState, TInput, TResult> GetInitialStateNodeOrThrow(TState initialState)
    {
        if ( ! States.TryGetValue( initialState, out var state ) )
            throw new StateMachineStateException( Resources.StateDoesNotExist( initialState ), nameof( initialState ) );

        return state;
    }

    [Pure]
    IStateMachineInstance<TState, TInput, TResult> IStateMachine<TState, TInput, TResult>.CreateInstance()
    {
        return CreateInstance();
    }

    [Pure]
    IStateMachineInstance<TState, TInput, TResult> IStateMachine<TState, TInput, TResult>.CreateInstance(TState initialState)
    {
        return CreateInstance( initialState );
    }

    [Pure]
    IStateMachineInstance<TState, TInput, TResult> IStateMachine<TState, TInput, TResult>.CreateInstanceWithSubject(object subject)
    {
        return CreateInstanceWithSubject( subject );
    }

    [Pure]
    IStateMachineInstance<TState, TInput, TResult> IStateMachine<TState, TInput, TResult>.CreateInstanceWithSubject(
        TState initialState,
        object subject)
    {
        return CreateInstanceWithSubject( initialState, subject );
    }

    [Pure]
    IStateMachine<TState, TInput, TResult> IStateMachine<TState, TInput, TResult>.WithOptimization(StateMachineOptimization optimization)
    {
        return WithOptimization( optimization );
    }
}
