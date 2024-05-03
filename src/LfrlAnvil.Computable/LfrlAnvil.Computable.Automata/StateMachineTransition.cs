using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Automata;

/// <inheritdoc />
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

    /// <inheritdoc />
    public IStateMachineNode<TState, TInput, TResult> Destination { get; }

    /// <inheritdoc />
    public IStateTransitionHandler<TState, TInput, TResult>? Handler { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="StateMachineTransition{TState,TInput,TResult}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"=> {Destination}{(Handler is null ? string.Empty : $" ({nameof( Handler )})")}";
    }
}
