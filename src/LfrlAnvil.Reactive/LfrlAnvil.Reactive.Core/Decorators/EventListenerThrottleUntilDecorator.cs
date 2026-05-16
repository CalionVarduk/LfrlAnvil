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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

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
        private readonly Lock _lock = new Lock();
        private readonly IEventStream<TTargetEvent> _target;
        private readonly IEventSubscriber _subscriber;
        private LazyDisposable<IEventSubscriber>? _targetSubscriber;
        private TargetEventListener? _targetListener;
        private State _state;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, IEventStream<TTargetEvent> target)
            : base( next )
        {
            _subscriber = subscriber;
            _target = target;
            _targetSubscriber = null;
            _targetListener = null;
            _state = State.Idle;

            if ( target.IsDisposed )
                _subscriber.Dispose();
        }

        public override void React(TEvent @event)
        {
            LazyDisposable<IEventSubscriber> targetSubscriber;
            TargetEventListener targetListener;
            using ( AcquireLock() )
            {
                if ( _state != State.Idle )
                    return;

                Assume.IsNull( _targetSubscriber );
                Assume.IsNull( _targetListener );
                _state = State.WaitingForTarget;
                _targetSubscriber = new LazyDisposable<IEventSubscriber>();
                targetSubscriber = _targetSubscriber;
                _targetListener = new TargetEventListener( this, _targetSubscriber );
                targetListener = _targetListener;
            }

            try
            {
                Next.React( @event );
            }
            finally
            {
                targetSubscriber.Assign( _target.Listen( targetListener ) );
            }
        }

        public override void OnDispose(DisposalSource source)
        {
            LazyDisposable<IEventSubscriber>? targetSubscriber;
            using ( AcquireLock() )
            {
                if ( _state == State.Disposed )
                    return;

                _state = State.Disposed;
                targetSubscriber = _targetSubscriber;
                _targetSubscriber = null;
                _targetListener = null;
            }

            targetSubscriber?.Dispose();
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent(TargetEventListener listener)
        {
            LazyDisposable<IEventSubscriber>? targetSubscriber;
            using ( AcquireLock() )
            {
                if ( _state >= State.Disposing || ! ReferenceEquals( _targetListener, listener ) )
                    return;

                Assume.Equals( _state, State.WaitingForTarget );
                _state = State.Idle;
                targetSubscriber = _targetSubscriber;
                _targetSubscriber = null;
                _targetListener = null;
            }

            targetSubscriber?.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetDisposed(TargetEventListener listener, DisposalSource source)
        {
            using ( AcquireLock() )
            {
                if ( _state >= State.Disposing || ! ReferenceEquals( _targetListener, listener ) )
                    return;

                Assume.Equals( _state, State.WaitingForTarget );
                _targetSubscriber = null;
                _targetListener = null;
                _state = State.Idle;
                if ( source == DisposalSource.Subscriber )
                    return;

                _state = State.Disposing;
            }

            _subscriber.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private Lock.Scope AcquireLock()
        {
            return _lock.EnterScope();
        }

        private enum State : byte
        {
            Idle = 0,
            WaitingForTarget = 1,
            Disposing = 2,
            Disposed = 3
        }
    }

    private sealed class TargetEventListener : EventListener<TTargetEvent>
    {
        private readonly EventListener _sourceListener;
        private readonly LazyDisposable<IEventSubscriber> _subscriber;

        internal TargetEventListener(EventListener sourceListener, LazyDisposable<IEventSubscriber> subscriber)
        {
            _sourceListener = sourceListener;
            _subscriber = subscriber;
        }

        public override void React(TTargetEvent _)
        {
            _sourceListener.OnTargetEvent( this );
        }

        public override void OnDispose(DisposalSource source)
        {
            _subscriber.Dispose();
            _sourceListener.OnTargetDisposed( this, source );
        }
    }
}
