using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Composites;

public readonly struct WithZonedDateTime<TEvent>
{
    public WithZonedDateTime(TEvent @event, ZonedDateTime dateTime)
    {
        Event = @event;
        DateTime = dateTime;
    }

    public TEvent Event { get; }
    public ZonedDateTime DateTime { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{DateTime}] {Event}";
    }
}