using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Composites;

public readonly struct WithSender<TEvent>
{
    public WithSender(object? sender, TEvent @event)
    {
        Sender = sender;
        Event = @event;
    }

    public object? Sender { get; }
    public TEvent Event { get; }

    [Pure]
    public override string ToString()
    {
        return $"{Sender} => {Event}";
    }
}
