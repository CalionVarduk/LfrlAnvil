namespace LfrlAnvil.Computable.Automata;

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

    public object Subject { get; }
    public IStateMachineNode<TState, TInput, TResult> Source { get; }
    public IStateMachineNode<TState, TInput, TResult> Destination { get; }
    public TInput Input { get; }
}
