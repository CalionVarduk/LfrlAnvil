// Copyright 2024-2026 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with the result of (source, target) pair mapping, where source and target event
/// are at the same position in a sequence of emitted events by their respective emitters.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TTargetEvent">Target event type.</typeparam>
/// <typeparam name="TNextEvent">Next event type.</typeparam>
public sealed class EventListenerZipDecorator<TEvent, TTargetEvent, TNextEvent> : IEventListenerDecorator<TEvent, TNextEvent>
{
    private readonly IEventStream<TTargetEvent> _target;
    private readonly Func<TEvent, TTargetEvent, TNextEvent> _selector;

    /// <summary>
    /// Creates a new <see cref="EventListenerZipDecorator{TEvent,TTargetEvent,TNextEvent}"/> instance.
    /// </summary>
    /// <param name="target">Target event stream.</param>
    /// <param name="selector">Next event selector.</param>
    public EventListenerZipDecorator(IEventStream<TTargetEvent> target, Func<TEvent, TTargetEvent, TNextEvent> selector)
    {
        _target = target;
        _selector = selector;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TNextEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _target, _selector );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TNextEvent>
    {
        private readonly Lock _lock = new Lock();
        private readonly IEventSubscriber _subscriber;
        private readonly LazyDisposable<IEventSubscriber> _targetSubscriber;
        private readonly Func<TEvent, TTargetEvent, TNextEvent> _selector;
        private QueueSlim<TEvent> _sourceEvents;
        private QueueSlim<TTargetEvent> _targetEvents;
        private bool _isDisposed;

        internal EventListener(
            IEventListener<TNextEvent> next,
            IEventSubscriber subscriber,
            IEventStream<TTargetEvent> target,
            Func<TEvent, TTargetEvent, TNextEvent> selector)
            : base( next )
        {
            _subscriber = subscriber;
            _selector = selector;
            _sourceEvents = QueueSlim<TEvent>.Create();
            _targetEvents = QueueSlim<TTargetEvent>.Create();

            _targetSubscriber = new LazyDisposable<IEventSubscriber>();
            var targetListener = new TargetEventListener( this );
            _targetSubscriber.Assign( target.Listen( targetListener ) );
        }

        public override void React(TEvent @event)
        {
            TTargetEvent? targetEvent;
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                if ( ! _targetEvents.TryDequeue( out targetEvent ) )
                {
                    _sourceEvents.Enqueue( @event );
                    return;
                }
            }

            Next.React( _selector( @event, targetEvent ) );
        }

        public override void OnDispose(DisposalSource source)
        {
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                _isDisposed = true;
                _sourceEvents = QueueSlim<TEvent>.Create();
                _targetEvents = QueueSlim<TTargetEvent>.Create();
            }

            _targetSubscriber.Dispose();
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent(TTargetEvent @event)
        {
            TEvent? sourceEvent;
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                if ( ! _sourceEvents.TryDequeue( out sourceEvent ) )
                {
                    _targetEvents.Enqueue( @event );
                    return;
                }
            }

            Next.React( _selector( sourceEvent, @event ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetDisposed()
        {
            _targetSubscriber.Dispose();
            _subscriber.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private Lock.Scope AcquireLock()
        {
            return _lock.EnterScope();
        }
    }

    private sealed class TargetEventListener : EventListener<TTargetEvent>
    {
        private readonly EventListener _parent;

        internal TargetEventListener(EventListener parent)
        {
            _parent = parent;
        }

        public override void React(TTargetEvent @event)
        {
            _parent.OnTargetEvent( @event );
        }

        public override void OnDispose(DisposalSource _)
        {
            _parent.OnTargetDisposed();
        }
    }
}
