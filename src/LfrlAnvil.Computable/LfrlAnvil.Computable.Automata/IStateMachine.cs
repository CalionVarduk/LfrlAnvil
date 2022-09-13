using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Automata;

public interface IStateMachine<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    IReadOnlyDictionary<TState, IStateMachineNode<TState, TInput, TResult>> States { get; }
    IStateMachineNode<TState, TInput, TResult> InitialState { get; }
    IEqualityComparer<TState> StateComparer { get; }
    IEqualityComparer<TInput> InputComparer { get; }
    TResult DefaultResult { get; }

    [Pure]
    IStateMachineInstance<TState, TInput, TResult> CreateInstance();

    [Pure]
    IStateMachineInstance<TState, TInput, TResult> CreateInstance(TState initialState);

    [Pure]
    IStateMachineInstance<TState, TInput, TResult> CreateInstanceWithSubject(object subject);

    [Pure]
    IStateMachineInstance<TState, TInput, TResult> CreateInstanceWithSubject(TState initialState, object subject);
}
