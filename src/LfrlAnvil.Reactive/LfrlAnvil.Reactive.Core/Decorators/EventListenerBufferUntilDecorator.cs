using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Decorators
{
    public sealed class EventListenerBufferUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, ReadOnlyMemory<TEvent>>
    {
        private readonly IEventStream<TTargetEvent> _target;

        public EventListenerBufferUntilDecorator(IEventStream<TTargetEvent> target)
        {
            _target = target;
        }

        [Pure]
        public IEventListener<TEvent> Decorate(IEventListener<ReadOnlyMemory<TEvent>> listener, IEventSubscriber subscriber)
        {
            return new EventListener( listener, subscriber, _target );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, ReadOnlyMemory<TEvent>>
        {
            private readonly IEventSubscriber _targetSubscriber;
            private readonly IEventSubscriber _subscriber;
            private readonly GrowingBuffer<TEvent> _buffer;

            internal EventListener(
                IEventListener<ReadOnlyMemory<TEvent>> next,
                IEventSubscriber subscriber,
                IEventStream<TTargetEvent> target)
                : base( next )
            {
                _subscriber = subscriber;
                _buffer = new GrowingBuffer<TEvent>();
                var targetListener = new TargetEventListener( this );
                _targetSubscriber = target.Listen( targetListener );
            }

            public override void React(TEvent @event)
            {
                _buffer.Add( @event );
            }

            public override void OnDispose(DisposalSource source)
            {
                _targetSubscriber.Dispose();
                _buffer.Clear();

                base.OnDispose( source );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void OnTargetEvent()
            {
                Next.React( _buffer.AsMemory() );
                _buffer.RemoveAll();
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
                _sourceListener!.OnTargetEvent();
            }

            public override void OnDispose(DisposalSource _)
            {
                _sourceListener!.OnTargetEvent();
                _sourceListener!.DisposeSubscriber();
                _sourceListener = null;
            }
        }
    }
}
