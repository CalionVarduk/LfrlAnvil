using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Automata;

public readonly struct StateMachineNodeInfo<TState>
{
    public StateMachineNodeInfo(TState state, StateMachineNodeType type)
    {
        State = state;
        Type = type;
    }

    public TState State { get; }
    public StateMachineNodeType Type { get; }

    [Pure]
    public override string ToString()
    {
        return $"'{State}' ({Type})";
    }
}
