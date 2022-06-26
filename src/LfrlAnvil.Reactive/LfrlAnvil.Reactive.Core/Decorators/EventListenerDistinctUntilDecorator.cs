using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Decorators;

public sealed class EventListenerDistinctUntilDecorator<TEvent, TKey, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Func<TEvent, TKey> _keySelector;
    private readonly IEqualityComparer<TKey> _equalityComparer;
    private readonly IEventStream<TTargetEvent> _target;

    public EventListenerDistinctUntilDecorator(
        Func<TEvent, TKey> keySelector,
        IEqualityComparer<TKey> equalityComparer,
        IEventStream<TTargetEvent> target)
    {
        _keySelector = keySelector;
        _equalityComparer = equalityComparer;
        _target = target;
    }

    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _keySelector, _equalityComparer, _target );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly IEventSubscriber _targetSubscriber;
        private readonly Func<TEvent, TKey> _keySelector;
        private readonly HashSet<TKey> _keySet;

        internal EventListener(
            IEventListener<TEvent> next,
            Func<TEvent, TKey> keySelector,
            IEqualityComparer<TKey> equalityComparer,
            IEventStream<TTargetEvent> target)
            : base( next )
        {
            _keySelector = keySelector;
            _keySet = new HashSet<TKey>( equalityComparer );
            var targetListener = new TargetEventListener( this );
            _targetSubscriber = target.Listen( targetListener );
        }

        public override void React(TEvent @event)
        {
            if ( _keySet.Add( _keySelector( @event ) ) )
                Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _targetSubscriber.Dispose();
            _keySet.Clear();
            _keySet.TrimExcess();

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent()
        {
            _keySet.Clear();
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
            _sourceListener!.OnTargetEvent();
        }

        public override void OnDispose(DisposalSource _)
        {
            _sourceListener!.OnTargetEvent();
            _sourceListener = null;
        }
    }
}
