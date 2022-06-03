using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Events.Composites
{
    public readonly struct EventGrouping<TKey, TEvent>
    {
        public EventGrouping(TKey key, TEvent @event, ReadOnlyMemory<TEvent> allEvents)
        {
            Key = key;
            Event = @event;
            AllEvents = allEvents;
        }

        public TKey Key { get; }
        public TEvent Event { get; }
        public ReadOnlyMemory<TEvent> AllEvents { get; }

        [Pure]
        public override string ToString()
        {
            return $"[{Key}]: {Event} (Count = {AllEvents.Length})";
        }
    }
}
