using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Composites
{
    public readonly struct WithInterval<TEvent>
    {
        public WithInterval(TEvent @event, Timestamp timestamp, Duration interval)
        {
            Event = @event;
            Timestamp = timestamp;
            Interval = interval;
        }

        public TEvent Event { get; }
        public Timestamp Timestamp { get; }
        public Duration Interval { get; }

        [Pure]
        public override string ToString()
        {
            return $"[{Timestamp} ({Interval} dt)] {Event}";
        }
    }
}
