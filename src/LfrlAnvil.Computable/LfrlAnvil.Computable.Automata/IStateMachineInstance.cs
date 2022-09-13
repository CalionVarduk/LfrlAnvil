using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Computable.Automata;

public interface IStateMachineInstance<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    object Subject { get; }
    IStateMachine<TState, TInput, TResult> Machine { get; }
    IStateMachineNode<TState, TInput, TResult> CurrentState { get; }

    bool TryTransition(TInput input, [MaybeNullWhen( false )] out TResult result);
    TResult Transition(TInput input);
}
