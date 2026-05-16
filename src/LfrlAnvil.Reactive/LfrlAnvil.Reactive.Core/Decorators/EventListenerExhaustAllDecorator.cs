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

using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Subscribes to the first emitted inner event stream and ignores all subsequent emitted inner event streams
/// until the active one is disposed. The decorated event listener is notified with all events emitted by the active inner stream.
/// </summary>
/// <typeparam name="TEvent">Inner event type.</typeparam>
public sealed class EventListenerExhaustAllDecorator<TEvent> : IEventListenerDecorator<IEventStream<TEvent>, TEvent>
{
    /// <inheritdoc />
    public IEventListener<IEventStream<TEvent>> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener );
    }

    private sealed class EventListener : DecoratedEventListener<IEventStream<TEvent>, TEvent>
    {
        private readonly Lock _lock = new Lock();
        private LazyDisposable<IEventSubscriber>? _innerSubscriber;
        private InnerEventListener? _innerListener;
        private State _state;

        internal EventListener(IEventListener<TEvent> next)
            : base( next )
        {
            _state = State.Idle;
        }

        public override void React(IEventStream<TEvent> @event)
        {
            LazyDisposable<IEventSubscriber> innerSubscriber;
            InnerEventListener innerListener;
            using ( AcquireLock() )
            {
                if ( _state != State.Idle )
                    return;

                Assume.IsNull( _innerSubscriber );
                _state = State.Active;
                _innerSubscriber = new LazyDisposable<IEventSubscriber>();
                innerSubscriber = _innerSubscriber;
                _innerListener = new InnerEventListener( this, _innerSubscriber );
                innerListener = _innerListener;
            }

            innerSubscriber.Assign( @event.Listen( innerListener ) );
        }

        public override void OnDispose(DisposalSource source)
        {
            LazyDisposable<IEventSubscriber>? innerSubscriber;
            using ( AcquireLock() )
            {
                if ( _state == State.Disposed )
                    return;

                _state = State.Disposed;
                innerSubscriber = _innerSubscriber;
                _innerSubscriber = null;
                _innerListener = null;
            }

            innerSubscriber?.Dispose();
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerEvent(TEvent @event)
        {
            Next.React( @event );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerDisposed(InnerEventListener listener)
        {
            LazyDisposable<IEventSubscriber>? innerSubscriber;
            using ( AcquireLock() )
            {
                if ( _state == State.Disposed || ! ReferenceEquals( _innerListener, listener ) )
                    return;

                Assume.Equals( _state, State.Active );
                _state = State.Idle;
                innerSubscriber = _innerSubscriber;
                _innerSubscriber = null;
                _innerListener = null;
            }

            innerSubscriber?.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private Lock.Scope AcquireLock()
        {
            return _lock.EnterScope();
        }

        private enum State : byte
        {
            Idle = 0,
            Active = 1,
            Disposed = 2
        }
    }

    private sealed class InnerEventListener : EventListener<TEvent>
    {
        private readonly EventListener _outerListener;
        private readonly LazyDisposable<IEventSubscriber> _subscriber;

        internal InnerEventListener(EventListener outerListener, LazyDisposable<IEventSubscriber> subscriber)
        {
            _outerListener = outerListener;
            _subscriber = subscriber;
        }

        public override void React(TEvent @event)
        {
            _outerListener.OnInnerEvent( @event );
        }

        public override void OnDispose(DisposalSource _)
        {
            _subscriber.Dispose();
            _outerListener.OnInnerDisposed( this );
        }
    }
}
