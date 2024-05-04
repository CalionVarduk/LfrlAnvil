﻿using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Creates a new target event stream subscription on each emitted event, unless an active one already exists,
/// immediately notifies the decorated event listener with that event, and ignores all subsequent events until the target event stream
/// emits its own event, which drops the target event stream subscription and repeats the process.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TTargetEvent">Target event type.</typeparam>
public sealed class EventListenerThrottleUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly IEventStream<TTargetEvent> _target;

    /// <summary>
    /// Creates a new <see cref="EventListenerThrottleUntilDecorator{TEvent,TTargetEvent}"/> instance.
    /// </summary>
    /// <param name="target">Target event stream to wait for before emitting any subsequent events.</param>
    public EventListenerThrottleUntilDecorator(IEventStream<TTargetEvent> target)
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
        private Optional<TEvent> _event;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, IEventStream<TTargetEvent> target)
            : base( next )
        {
            _subscriber = subscriber;
            _target = target;
            _targetSubscriber = null;
            _event = Optional<TEvent>.Empty;

            if ( target.IsDisposed )
                DisposeSubscriber();
        }

        public override void React(TEvent @event)
        {
            if ( _targetSubscriber is not null )
                return;

            _event = new Optional<TEvent>( @event );
            Assume.IsNotNull( _target );
            _targetSubscriber = new LazyDisposable<IEventSubscriber>();
            var targetListener = new TargetEventListener( this );
            var actualTargetSubscriber = _target.Listen( targetListener );
            _targetSubscriber?.Assign( actualTargetSubscriber );
            _event = Optional<TEvent>.Empty;

            if ( _targetSubscriber is not null )
                Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _event = Optional<TEvent>.Empty;
            _target = null;
            _targetSubscriber?.Dispose();

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent()
        {
            Assume.IsNotNull( _targetSubscriber );
            _event.TryForward( Next );
            _event = Optional<TEvent>.Empty;
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

        internal TargetEventListener(EventListener sourceListener)
        {
            _sourceListener = sourceListener;
        }

        public override void React(TTargetEvent _)
        {
            Assume.IsNotNull( _sourceListener );
            _sourceListener.OnTargetEvent();
        }

        public override void OnDispose(DisposalSource source)
        {
            Assume.IsNotNull( _sourceListener );
            _sourceListener.ClearTargetReferences();

            if ( source == DisposalSource.EventSource )
                _sourceListener.DisposeSubscriber();

            _sourceListener = null;
        }
    }
}
