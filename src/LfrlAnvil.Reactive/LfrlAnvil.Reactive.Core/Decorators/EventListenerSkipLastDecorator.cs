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
/// Skips the specified number of events at the end of the sequence. The decorated event listener will be notified with
/// a sequence of non-skipped events during its disposal.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerSkipLastDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _count;

    /// <summary>
    /// Creates a new <see cref="EventListenerSkipLastDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="count">Number of events at the end of the sequence to skip.</param>
    public EventListenerSkipLastDecorator(int count)
    {
        _count = count;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return _count <= 0 ? listener : new EventListener( listener, _count );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Lock _lock = new Lock();
        private readonly int _count;
        private ListSlim<TEvent> _buffer;
        private bool _isDisposed;

        internal EventListener(IEventListener<TEvent> next, int count)
            : base( next )
        {
            _buffer = ListSlim<TEvent>.Create();
            _count = count;
        }

        public override void React(TEvent @event)
        {
            using ( AcquireLock() )
            {
                if ( ! _isDisposed )
                    _buffer.Add( @event );
            }
        }

        public override void OnDispose(DisposalSource source)
        {
            ReadOnlySpan<TEvent> events;
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                _isDisposed = true;
                var count = _buffer.Count - _count;
                events = count > 0 ? _buffer.AsSpan().Slice( 0, count ) : ReadOnlySpan<TEvent>.Empty;
                _buffer = ListSlim<TEvent>.Create();
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
