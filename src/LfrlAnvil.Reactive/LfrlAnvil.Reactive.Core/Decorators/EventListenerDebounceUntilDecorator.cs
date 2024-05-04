using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Creates a new target event stream subscription on each emitted event, and drops the current one if it exists,
/// and keeps the event until the target stream emits its own event, which then notifies the decorated
/// event listener with the stored event, drops the target event stream subscription and repeats the process.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TTargetEvent">Target event type.</typeparam>
public sealed class EventListenerDebounceUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly IEventStream<TTargetEvent> _target;

    /// <summary>
    /// Creates a new <see cref="EventListenerDebounceUntilDecorator{TEvent,TTargetEvent}"/> instance.
    /// </summary>
    /// <param name="target">Target event stream to wait for before emitting the stored event.</param>
    public EventListenerDebounceUntilDecorator(IEventStream<TTargetEvent> target)
    {
        _target = target;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _target );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly IEventSubscriber _subscriber;
        private LazyDisposable<IEventSubscriber>? _targetSubscriber;
        private IEventStream<TTargetEvent>? _target;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, IEventStream<TTargetEvent> target)
            : base( next )
        {
            _subscriber = subscriber;
            _target = target;
            _targetSubscriber = null;

            if ( target.IsDisposed )
                DisposeSubscriber();
        }

        public override void React(TEvent @event)
        {
            Assume.IsNotNull( _target );
            _targetSubscriber?.Dispose();
            _targetSubscriber = new LazyDisposable<IEventSubscriber>();
            var targetListener = new TargetEventListener( this, @event );
            var actualTargetSubscriber = _target.Listen( targetListener );
            _targetSubscriber?.Assign( actualTargetSubscriber );
        }

        public override void OnDispose(DisposalSource source)
        {
            _target = null;
            _targetSubscriber?.Dispose();

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent(TEvent @event)
        {
            Next.React( @event );
            Assume.IsNotNull( _targetSubscriber );
            _targetSubscriber.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ClearTargetReferences()
        {
            _targetSubscriber = null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void DisposeSubscriber()
        {
            _subscriber.Dispose();
        }
    }

    private sealed class TargetEventListener : EventListener<TTargetEvent>
    {
        private EventListener? _sourceListener;
        private TEvent? _sourceEvent;

        internal TargetEventListener(EventListener sourceListener, TEvent sourceEvent)
        {
            _sourceListener = sourceListener;
            _sourceEvent = sourceEvent;
        }

        public override void React(TTargetEvent _)
        {
            Assume.IsNotNull( _sourceListener );
            _sourceListener.OnTargetEvent( _sourceEvent! );
        }

        public override void OnDispose(DisposalSource source)
        {
            _sourceEvent = default;
            Assume.IsNotNull( _sourceListener );
            _sourceListener.ClearTargetReferences();

            if ( source == DisposalSource.EventSource )
                _sourceListener.DisposeSubscriber();

            _sourceListener = null;
        }
    }
}
