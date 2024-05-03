using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Automata.Exceptions;
using LfrlAnvil.Computable.Automata.Internal;

namespace LfrlAnvil.Computable.Automata;

/// <inheritdoc />
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

    /// <inheritdoc />
    public IStateMachineNode<TState, TInput, TResult> InitialState { get; }

    /// <inheritdoc />
    public IEqualityComparer<TInput> InputComparer { get; }

    /// <inheritdoc />
    public TResult DefaultResult { get; }

    /// <inheritdoc />
    public StateMachineOptimization Optimization { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<TState, IStateMachineNode<TState, TInput, TResult>> States => _states;

    /// <inheritdoc />
    public IEqualityComparer<TState> StateComparer => _states.Comparer;

    /// <inheritdoc cref="IStateMachine{TState,TInput,TResult}.CreateInstance()" />
    [Pure]
    public StateMachineInstance<TState, TInput, TResult> CreateInstance()
    {
        return new StateMachineInstance<TState, TInput, TResult>( this, InitialState );
    }

    /// <inheritdoc cref="IStateMachine{TState,TInput,TResult}.CreateInstance(TState)" />
    [Pure]
    public StateMachineInstance<TState, TInput, TResult> CreateInstance(TState initialState)
    {
        return new StateMachineInstance<TState, TInput, TResult>( this, GetInitialStateNodeOrThrow( initialState ) );
    }

    /// <inheritdoc cref="IStateMachine{TState,TInput,TResult}.CreateInstanceWithSubject(Object)" />
    [Pure]
    public StateMachineInstance<TState, TInput, TResult> CreateInstanceWithSubject(object subject)
    {
        return new StateMachineInstance<TState, TInput, TResult>( this, InitialState, subject );
    }

    /// <inheritdoc cref="IStateMachine{TState,TInput,TResult}.CreateInstanceWithSubject(TState,Object)" />
    [Pure]
    public StateMachineInstance<TState, TInput, TResult> CreateInstanceWithSubject(TState initialState, object subject)
    {
        return new StateMachineInstance<TState, TInput, TResult>( this, GetInitialStateNodeOrThrow( initialState ), subject );
    }

    /// <inheritdoc cref="IStateMachine{TState,TInput,TResult}.WithOptimization(StateMachineOptimizationParams{TState})" />
    [Pure]
    public StateMachine<TState, TInput, TResult> WithOptimization(StateMachineOptimizationParams<TState> optimization)
    {
        if ( Optimization >= optimization.Level )
            return this;

        var optimizationResult = StateMachineOptimizer.OptimizeExisting( Optimization, _states, InitialState, optimization, InputComparer );
        return new StateMachine<TState, TInput, TResult>(
            optimizationResult.States,
            optimizationResult.InitialState,
            InputComparer,
            DefaultResult,
            optimization.Level );
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
    IStateMachine<TState, TInput, TResult> IStateMachine<TState, TInput, TResult>.WithOptimization(
        StateMachineOptimizationParams<TState> optimization)
    {
        return WithOptimization( optimization );
    }
}
