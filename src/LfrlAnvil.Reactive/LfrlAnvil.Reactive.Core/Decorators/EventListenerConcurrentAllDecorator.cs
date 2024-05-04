using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies decorated event listener with concurrent versions of emitted event streams.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerConcurrentAllDecorator<TEvent> : IEventListenerDecorator<IEventStream<TEvent>, IEventStream<TEvent>>
{
    private readonly object? _sync;

    /// <summary>
    /// Creates a new <see cref="EventListenerConcurrentAllDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="sync">Optional shared synchronization object.</param>
    public EventListenerConcurrentAllDecorator(object? sync)
    {
        _sync = sync;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<IEventStream<TEvent>> Decorate(IEventListener<IEventStream<TEvent>> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _sync );
    }

    private sealed class EventListener : DecoratedEventListener<IEventStream<TEvent>, IEventStream<TEvent>>
    {
        private readonly object _sync;

        public EventListener(IEventListener<IEventStream<TEvent>> next, object? sync)
            : base( next )
        {
            _sync = sync ?? new object();
        }

        public override void React(IEventStream<TEvent> @event)
        {
            Next.React( @event.Concurrent( _sync ) );
        }
    }
}
