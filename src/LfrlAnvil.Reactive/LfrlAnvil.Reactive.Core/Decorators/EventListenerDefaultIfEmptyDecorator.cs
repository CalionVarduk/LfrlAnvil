using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Specifies the default value to notify the decorated event listener with when listener gets disposed,
/// but only when no events have been emitted.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public sealed class EventListenerDefaultIfEmptyDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly TEvent _defaultValue;

    /// <summary>
    /// Creates a new <see cref="EventListenerDefaultIfEmptyDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="defaultValue">Default value.</param>
    public EventListenerDefaultIfEmptyDecorator(TEvent defaultValue)
    {
        _defaultValue = defaultValue;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _defaultValue );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private bool _shouldEmitDefault;
        private TEvent? _defaultValue;

        internal EventListener(IEventListener<TEvent> next, TEvent defaultValue)
            : base( next )
        {
            _shouldEmitDefault = true;
            _defaultValue = defaultValue;
        }

        public override void React(TEvent @event)
        {
            _shouldEmitDefault = false;
            Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            if ( _shouldEmitDefault )
                Next.React( _defaultValue! );

            _defaultValue = default;
            base.OnDispose( source );
        }
    }
}
