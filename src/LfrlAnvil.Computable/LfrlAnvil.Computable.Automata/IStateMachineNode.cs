using System.Collections.Generic;

namespace LfrlAnvil.Computable.Automata;

public interface IStateMachineNode<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    TState Value { get; }
    StateMachineNodeType Type { get; }
    IReadOnlyDictionary<TInput, IStateMachineTransition<TState, TInput, TResult>> Transitions { get; }
}
