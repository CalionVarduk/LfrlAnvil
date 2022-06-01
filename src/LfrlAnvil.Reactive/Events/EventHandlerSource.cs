using System;
using LfrlAnvil.Reactive.Events.Composites;

namespace LfrlAnvil.Reactive.Events
{
    public sealed class EventHandlerSource<TEvent> : EventSource<WithSender<TEvent>>
    {
        private Action<EventHandler<TEvent>>? _teardown;

        public EventHandlerSource(Action<EventHandler<TEvent>> setup, Action<EventHandler<TEvent>> teardown)
        {
            setup( Handle );
            _teardown = teardown;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _teardown!( Handle );
            _teardown = null;
        }

        private void Handle(object? sender, TEvent args)
        {
            NotifyListeners( new WithSender<TEvent>( sender, args ) );
        }
    }
}
