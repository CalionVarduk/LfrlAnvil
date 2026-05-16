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
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Buffers emitted events and notifies the decorated event listener with that buffer once the target event stream emits its own event,
/// which then repeats the process.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TTargetEvent">Target event type.</typeparam>
public sealed class EventListenerBufferUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, ReadOnlyMemory<TEvent>>
{
    private readonly IEventStream<TTargetEvent> _target;

    /// <summary>
    /// Creates a new <see cref="EventListenerBufferUntilDecorator{TEvent,TTargetEvent}"/> instance.
    /// </summary>
    /// <param name="target">Target event stream to wait for before emitting the underlying buffer.</param>
    public EventListenerBufferUntilDecorator(IEventStream<TTargetEvent> target)
    {
        _target = target;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<ReadOnlyMemory<TEvent>> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _target );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, ReadOnlyMemory<TEvent>>
    {
        internal readonly LazyDisposable<IEventSubscriber> TargetSubscriber;
        private readonly Lock _lock = new Lock();
        private readonly IEventSubscriber _subscriber;
        private ListSlim<TEvent> _buffer;
        private bool _isDisposing;
        private bool _isDisposed;

        internal EventListener(
            IEventListener<ReadOnlyMemory<TEvent>> next,
            IEventSubscriber subscriber,
            IEventStream<TTargetEvent> target)
            : base( next )
        {
            _subscriber = subscriber;
            _buffer = ListSlim<TEvent>.Create();
            TargetSubscriber = new LazyDisposable<IEventSubscriber>();
            var targetListener = new TargetEventListener( this );
            TargetSubscriber.Assign( target.Listen( targetListener ) );
        }

        public override void React(TEvent @event)
        {
            using ( AcquireLock() )
            {
                if ( ! _isDisposing )
                    _buffer.Add( @event );
            }
        }

        public override void OnDispose(DisposalSource source)
        {
            ReadOnlyMemory<TEvent> events;
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                _isDisposing = true;
                _isDisposed = true;
                events = _buffer.AsMemory();
                _buffer = ListSlim<TEvent>.Create();
            }

            if ( events.Length > 0 )
                Next.React( events );

            TargetSubscriber.Dispose();
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent()
        {
            ArrayPoolToken<TEvent> poolToken = default;
            try
            {
                var events = Memory<TEvent>.Empty;
                using ( AcquireLock() )
                {
                    if ( _isDisposing )
                        return;

                    if ( ! _buffer.IsEmpty )
                    {
                        poolToken = ArrayPool<TEvent>.Shared.RentToken( _buffer.Count, clearArray: true );
                        events = poolToken.AsMemory();
                        _buffer.AsMemory().CopyTo( events );
                        _buffer.Clear();
                    }
                }

                Next.React( events );
            }
            finally
            {
                poolToken.Dispose();
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetDisposed()
        {
            ReadOnlyMemory<TEvent> events;
            using ( AcquireLock() )
            {
                if ( _isDisposing )
                    return;

                _isDisposing = true;
                events = _buffer.AsMemory();
                _buffer = ListSlim<TEvent>.Create();
            }

            Next.React( events );
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
        private readonly EventListener _sourceListener;

        internal TargetEventListener(EventListener sourceListener)
        {
            _sourceListener = sourceListener;
        }

        public override void React(TTargetEvent _)
        {
            _sourceListener.OnTargetEvent();
        }

        public override void OnDispose(DisposalSource _)
        {
            _sourceListener.TargetSubscriber.Dispose();
            _sourceListener.OnTargetDisposed();
        }
    }
}
