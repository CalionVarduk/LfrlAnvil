namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents a transition to the specified <see cref="IStateMachineNode{TState,TInput,TResult}"/>.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TInput">Input type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IStateMachineTransition<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    /// <summary>
    /// Destination state.
    /// </summary>
    IStateMachineNode<TState, TInput, TResult> Destination { get; }

    /// <summary>
    /// Optional handler invoked during the transition to <see cref="Destination"/>.
    /// </summary>
    IStateTransitionHandler<TState, TInput, TResult>? Handler { get; }
}
