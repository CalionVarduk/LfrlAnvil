using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Automata.Exceptions;

namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents a deterministic finite state machine.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TInput">Input type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IStateMachine<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    /// <summary>
    /// Collection that contains all nodes of this state machine.
    /// </summary>
    IReadOnlyDictionary<TState, IStateMachineNode<TState, TInput, TResult>> States { get; }

    /// <summary>
    /// <see cref="IStateMachineNode{TState,TInput,TResult}"/> in the <see cref="StateMachineNodeType.Initial"/> state.
    /// </summary>
    IStateMachineNode<TState, TInput, TResult> InitialState { get; }

    /// <summary>
    /// State equality comparer.
    /// </summary>
    IEqualityComparer<TState> StateComparer { get; }

    /// <summary>
    /// Input equality comparer.
    /// </summary>
    IEqualityComparer<TInput> InputComparer { get; }

    /// <summary>
    /// Represents the default transition result.
    /// </summary>
    TResult DefaultResult { get; }

    /// <summary>
    /// Specifies the chosen <see cref="StateMachineOptimization"/> with which this state machine was created.
    /// </summary>
    StateMachineOptimization Optimization { get; }

    /// <summary>
    /// Creates a new <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance from this state.
    /// </summary>
    /// <returns>New <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance.</returns>
    [Pure]
    IStateMachineInstance<TState, TInput, TResult> CreateInstance();

    /// <summary>
    /// Creates a new <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance from this state.
    /// </summary>
    /// <param name="initialState">State to start the created instance in.</param>
    /// <returns>New <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance.</returns>
    /// <exception cref="StateMachineStateException">When the provided <paramref name="initialState"/> does not exist.</exception>
    [Pure]
    IStateMachineInstance<TState, TInput, TResult> CreateInstance(TState initialState);

    /// <summary>
    /// Creates a new <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance from this state.
    /// </summary>
    /// <param name="subject">Custom subject.</param>
    /// <returns>New <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance.</returns>
    [Pure]
    IStateMachineInstance<TState, TInput, TResult> CreateInstanceWithSubject(object subject);

    /// <summary>
    /// Creates a new <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance from this state.
    /// </summary>
    /// <param name="initialState">State to start the created instance in.</param>
    /// <param name="subject">Custom subject.</param>
    /// <returns>New <see cref="IStateMachineInstance{TState,TInput,TResult}"/> instance.</returns>
    /// <exception cref="StateMachineStateException">When the provided <paramref name="initialState"/> does not exist.</exception>
    [Pure]
    IStateMachineInstance<TState, TInput, TResult> CreateInstanceWithSubject(TState initialState, object subject);

    /// <summary>
    /// Creates a new <see cref="IStateMachine{TState,TInput,TResult}"/>
    /// equivalent to this state machine with the provided <paramref name="optimization"/>.
    /// </summary>
    /// <param name="optimization">Optimization parameters.</param>
    /// <returns>
    /// New <see cref="IStateMachine{TState,TInput,TResult}"/> instance or <b>this</b>
    /// when current <see cref="Optimization"/> level is the same or more advanced than the provided desired optimization.
    /// </returns>
    [Pure]
    IStateMachine<TState, TInput, TResult> WithOptimization(StateMachineOptimizationParams<TState> optimization);
}
