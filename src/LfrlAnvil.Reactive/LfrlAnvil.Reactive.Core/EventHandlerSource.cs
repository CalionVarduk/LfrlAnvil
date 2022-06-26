using System;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive;

public sealed class EventHandlerSource<TEvent> : EventSource<WithSender<TEvent>>
{
    internal Action<EventHandler<TEvent>>? Teardown;

    public EventHandlerSource(Action<EventHandler<TEvent>> setup, Action<EventHandler<TEvent>> teardown)
    {
        setup( Handle );
        Teardown = teardown;
    }

    internal EventHandlerSource()
    {
        Teardown = null;
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        Teardown!( Handle );
        Teardown = null;
    }

    internal void Handle(object? sender, TEvent args)
    {
        NotifyListeners( new WithSender<TEvent>( sender, args ) );
    }
}