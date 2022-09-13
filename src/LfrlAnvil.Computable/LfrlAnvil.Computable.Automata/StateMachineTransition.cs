using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Automata;

public sealed class StateMachineTransition<TState, TInput, TResult> : IStateMachineTransition<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    internal StateMachineTransition(
        IStateMachineNode<TState, TInput, TResult> destination,
        IStateTransitionHandler<TState, TInput, TResult>? handler)
    {
        Destination = destination;
        Handler = handler;
    }

    public IStateMachineNode<TState, TInput, TResult> Destination { get; }
    public IStateTransitionHandler<TState, TInput, TResult>? Handler { get; }

    [Pure]
    public override string ToString()
    {
        return $"=> {Destination}{(Handler is null ? string.Empty : $" ({nameof( Handler )})")}";
    }
}
