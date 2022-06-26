using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Composites;

public readonly struct WithTimestamp<TEvent>
{
    public WithTimestamp(TEvent @event, Timestamp timestamp)
    {
        Event = @event;
        Timestamp = timestamp;
    }

    public TEvent Event { get; }
    public Timestamp Timestamp { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{Timestamp}] {Event}";
    }
}