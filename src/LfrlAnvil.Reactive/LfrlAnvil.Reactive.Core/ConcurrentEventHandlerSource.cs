using System;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive
{
    public sealed class ConcurrentEventHandlerSource<TEvent> : ConcurrentEventSource<WithSender<TEvent>, EventHandlerSource<TEvent>>
    {
        public ConcurrentEventHandlerSource(Action<EventHandler<TEvent>> setup, Action<EventHandler<TEvent>> teardown)
            : base( new EventHandlerSource<TEvent>() )
        {
            setup( Handle );
            Base.Teardown = _ => teardown( Handle );
        }

        private void Handle(object? sender, TEvent args)
        {
            lock ( Sync )
            {
                Base.Handle( sender, args );
            }
        }
    }
}
