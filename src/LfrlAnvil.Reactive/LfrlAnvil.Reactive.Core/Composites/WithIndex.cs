using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Composites;

public readonly struct WithIndex<TEvent>
{
    public WithIndex(TEvent @event, int index)
    {
        Event = @event;
        Index = index;
    }

    public TEvent Event { get; }
    public int Index { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{Index}]: {Event}";
    }
}