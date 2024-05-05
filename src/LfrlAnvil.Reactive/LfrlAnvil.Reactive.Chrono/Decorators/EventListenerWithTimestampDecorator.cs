using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;

namespace LfrlAnvil.Reactive.Chrono.Decorators;

/// <summary>
/// Notifies the decorated event listener with <see cref="WithTimestamp{TEvent}"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerWithTimestampDecorator<TEvent> : IEventListenerDecorator<TEvent, WithTimestamp<TEvent>>
{
    private readonly ITimestampProvider _timestampProvider;

    /// <summary>
    /// Creates a new <see cref="EventListenerWithTimestampDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider to use for time tracking.</param>
    public EventListenerWithTimestampDecorator(ITimestampProvider timestampProvider)
    {
        _timestampProvider = timestampProvider;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<WithTimestamp<TEvent>> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _timestampProvider );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, WithTimestamp<TEvent>>
    {
        private readonly ITimestampProvider _timestampProvider;

        internal EventListener(IEventListener<WithTimestamp<TEvent>> next, ITimestampProvider timestampProvider)
            : base( next )
        {
            _timestampProvider = timestampProvider;
        }

        public override void React(TEvent @event)
        {
            var timestamp = _timestampProvider.GetNow();
            var nextEvent = new WithTimestamp<TEvent>( @event, timestamp );
            Next.React( nextEvent );
        }
    }
}
