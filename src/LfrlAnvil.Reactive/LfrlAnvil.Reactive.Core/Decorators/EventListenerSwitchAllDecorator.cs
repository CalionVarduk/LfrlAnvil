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
/// Subscribes to the first emitted inner event stream. If another inner event stream is emitted,
/// then the active inner event stream subscription is disposed and the new event stream becomes the active one.
/// The decorated event listener is notified with events emitted by the active inner stream.
/// </summary>
/// <typeparam name="TEvent">Inner event type.</typeparam>
public sealed class EventListenerSwitchAllDecorator<TEvent> : IEventListenerDecorator<IEventStream<TEvent>, TEvent>
{
    /// <inheritdoc />
    public IEventListener<IEventStream<TEvent>> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener );
    }

    private sealed class EventListener : DecoratedEventListener<IEventStream<TEvent>, TEvent>
    {
        private readonly Lock _lock = new Lock();
        private LazyDisposable<IEventSubscriber>? _activeSubscriber;
        private InnerEventListener? _activeListener;
        private IEventStream<TEvent>? _nextStream;
        private State _state;

        internal EventListener(IEventListener<TEvent> next)
            : base( next )
        {
            _state = State.Idle;
        }

        public override void React(IEventStream<TEvent> @event)
        {
            LazyDisposable<IEventSubscriber>? activeSubscriber = null;
            var activate = false;
            using ( AcquireLock() )
            {
                if ( _state == State.Disposed )
                    return;

                if ( _state == State.Idle )
                {
                    Assume.IsNull( _activeSubscriber );
                    Assume.IsNull( _activeListener );
                    _state = State.Active;
                    activate = true;
                }
                else
                {
                    _nextStream = @event;
                    activeSubscriber = _activeSubscriber;
                }
            }

            activeSubscriber?.Dispose();
            if ( activate )
                StartListeningToNextInnerStream( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            LazyDisposable<IEventSubscriber>? activeSubscriber;
            using ( AcquireLock() )
            {
                if ( _state == State.Disposed )
                    return;

                _state = State.Disposed;
                activeSubscriber = _activeSubscriber;
                _activeSubscriber = null;
                _activeListener = null;
                _nextStream = null;
            }

            activeSubscriber?.Dispose();
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
            IEventStream<TEvent> nextStream;
            using ( AcquireLock() )
            {
                if ( _state == State.Disposed || ! ReferenceEquals( _activeListener, listener ) )
                    return;

                Assume.Equals( _state, State.Active );
                _activeSubscriber = null;
                _activeListener = null;
                if ( _nextStream is null )
                {
                    _state = State.Idle;
                    return;
                }

                nextStream = _nextStream;
                _nextStream = null;
            }

            StartListeningToNextInnerStream( nextStream );
        }

        private void StartListeningToNextInnerStream(IEventStream<TEvent> stream)
        {
            var innerSubscriber = new LazyDisposable<IEventSubscriber>();
            var innerListener = new InnerEventListener( this, innerSubscriber );

            while ( true )
            {
                IEventStream<TEvent>? nextStream = null;
                using ( AcquireLock() )
                {
                    if ( _state == State.Disposed )
                        return;

                    if ( _nextStream is null )
                    {
                        _activeSubscriber = innerSubscriber;
                        _activeListener = innerListener;
                    }
                    else
                    {
                        nextStream = _nextStream;
                        _nextStream = null;
                    }
                }

                if ( nextStream is null )
                {
                    innerSubscriber.Assign( stream.Listen( innerListener ) );
                    return;
                }

                stream = nextStream;
            }
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
