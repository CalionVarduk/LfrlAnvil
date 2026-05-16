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
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener only with the specified number of events at the end of the sequence, during its disposal.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerTakeLastDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _count;

    /// <summary>
    /// Creates a new <see cref="EventListenerTakeLastDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="count">Number of events at the end of the sequence to take.</param>
    public EventListenerTakeLastDecorator(int count)
    {
        _count = Math.Max( count, 0 );
    }

    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _count );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Lock _lock = new Lock();
        private readonly int _count;
        private QueueSlim<TEvent> _last;
        private bool _isDisposed;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, int count)
            : base( next )
        {
            _last = QueueSlim<TEvent>.Create();
            _count = count;

            if ( _count == 0 )
                subscriber.Dispose();
        }

        public override void React(TEvent @event)
        {
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                if ( _last.Count == _count )
                    _last.Dequeue();

                _last.Enqueue( @event );
            }
        }

        public override void OnDispose(DisposalSource source)
        {
            QueueSlimMemory<TEvent> events;
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                _isDisposed = true;
                events = _last.AsMemory();
                _last = QueueSlim<TEvent>.Create();
            }

            foreach ( var e in events )
                Next.React( e );

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private Lock.Scope AcquireLock()
        {
            return _lock.EnterScope();
        }
    }
}
