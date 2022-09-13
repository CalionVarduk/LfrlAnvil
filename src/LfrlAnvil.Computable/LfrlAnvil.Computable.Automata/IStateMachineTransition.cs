namespace LfrlAnvil.Computable.Automata;

public interface IStateMachineTransition<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    IStateMachineNode<TState, TInput, TResult> Destination { get; }
    IStateTransitionHandler<TState, TInput, TResult>? Handler { get; }
}
