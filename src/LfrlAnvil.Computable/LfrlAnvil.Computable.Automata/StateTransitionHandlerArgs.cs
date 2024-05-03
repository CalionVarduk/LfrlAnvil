namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents arguments of <see cref="IStateTransitionHandler{TState,TInput,TResult}"/> invocation.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TInput">Input type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public readonly struct StateTransitionHandlerArgs<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    internal StateTransitionHandlerArgs(
        object subject,
        IStateMachineNode<TState, TInput, TResult> source,
        IStateMachineNode<TState, TInput, TResult> destination,
        TInput input)
    {
        Subject = subject;
        Source = source;
        Destination = destination;
        Input = input;
    }

    /// <summary>
    /// State machine's <see cref="IStateMachineInstance{TState,TInput,TResult}.Subject"/>.
    /// </summary>
    public object Subject { get; }

    /// <summary>
    /// Source node.
    /// </summary>
    public IStateMachineNode<TState, TInput, TResult> Source { get; }

    /// <summary>
    /// Destination node.
    /// </summary>
    public IStateMachineNode<TState, TInput, TResult> Destination { get; }

    /// <summary>
    /// Invocation input.
    /// </summary>
    public TInput Input { get; }
}
