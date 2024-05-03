using System.Collections.Generic;

namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents a single node of <see cref="IStateMachine{TState,TInput,TResult}"/>.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TInput">Input type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IStateMachineNode<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    /// <summary>
    /// Value of this state.
    /// </summary>
    TState Value { get; }

    /// <summary>
    /// Type of this state.
    /// </summary>
    StateMachineNodeType Type { get; }

    /// <summary>
    /// Dictionary of available transitions from this state.
    /// </summary>
    IReadOnlyDictionary<TInput, IStateMachineTransition<TState, TInput, TResult>> Transitions { get; }
}
