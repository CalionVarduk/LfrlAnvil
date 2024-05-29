using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Automata;

/// <inheritdoc cref="IStateMachineNode{TState,TInput,TResult}" />
public sealed class StateMachineNode<TState, TInput, TResult> : IStateMachineNode<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    internal StateMachineNode(
        TState value,
        StateMachineNodeType type,
        IReadOnlyDictionary<TInput, IStateMachineTransition<TState, TInput, TResult>> transitions)
    {
        Value = value;
        Type = type;
        Transitions = transitions;
    }

    /// <inheritdoc />
    public TState Value { get; }

    /// <inheritdoc />
    public StateMachineNodeType Type { get; internal set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<TInput, IStateMachineTransition<TState, TInput, TResult>> Transitions { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="StateMachineNode{TState,TInput,TResult}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"'{Value}' ({Type}), {nameof( Transitions )}: {Transitions.Count}";
    }
}
