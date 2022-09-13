using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Automata;

public readonly struct StateMachineTransitionInfo<TState, TInput>
{
    public StateMachineTransitionInfo(TState destination, TInput input)
    {
        Destination = destination;
        Input = input;
    }

    public TState Destination { get; }
    public TInput Input { get; }

    [Pure]
    public override string ToString()
    {
        return $"'{Input}' => '{Destination}'";
    }
}
