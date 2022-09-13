using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Automata;

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

    public TState Value { get; }
    public StateMachineNodeType Type { get; }
    public IReadOnlyDictionary<TInput, IStateMachineTransition<TState, TInput, TResult>> Transitions { get; }

    [Pure]
    public override string ToString()
    {
        return $"'{Value}' ({Type}), {nameof( Transitions )}: {Transitions.Count}";
    }
}
