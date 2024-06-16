// Copyright 2024 Łukasz Furlepa
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

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Encapsulates decorated event listener's reactions to events and disposal in an exclusive lock.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerConcurrentDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly object? _sync;

    /// <summary>
    /// Creates a new <see cref="EventListenerConcurrentDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="sync">Optional synchronization object.</param>
    public EventListenerConcurrentDecorator(object? sync)
    {
        _sync = sync;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _sync );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly object _sync;
        private bool _disposed;

        internal EventListener(IEventListener<TEvent> next, object? sync)
            : base( next )
        {
            _sync = sync ?? new object();
            _disposed = false;
        }

        public override void React(TEvent @event)
        {
            lock ( _sync )
            {
                if ( _disposed )
                    return;

                Next.React( @event );
            }
        }

        public override void OnDispose(DisposalSource source)
        {
            lock ( _sync )
            {
                if ( _disposed )
                    return;

                _disposed = true;
                base.OnDispose( source );
            }
        }
    }
}
