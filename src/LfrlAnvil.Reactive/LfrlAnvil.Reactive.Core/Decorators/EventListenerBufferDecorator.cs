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

using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Buffers emitted events until the buffer gets fully filled and then notifies the decorated event listener with that buffer,
/// and then repeats the process.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerBufferDecorator<TEvent> : IEventListenerDecorator<TEvent, ReadOnlyMemory<TEvent>>
{
    private readonly int _bufferLength;

    /// <summary>
    /// Creates a new <see cref="EventListenerBufferDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="bufferLength">Size of the underlying buffer.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="bufferLength"/> is less than <b>1</b>.</exception>
    public EventListenerBufferDecorator(int bufferLength)
    {
        Ensure.IsGreaterThan( bufferLength, 0 );
        _bufferLength = bufferLength;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<ReadOnlyMemory<TEvent>> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _bufferLength );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, ReadOnlyMemory<TEvent>>
    {
        private int _count;
        private readonly TEvent[] _buffer;

        internal EventListener(IEventListener<ReadOnlyMemory<TEvent>> next, int bufferLength)
            : base( next )
        {
            _count = 0;
            _buffer = new TEvent[bufferLength];
        }

        public override void React(TEvent @event)
        {
            _buffer[_count] = @event;

            if ( ++_count < _buffer.Length )
                return;

            Next.React( _buffer.AsMemory() );

            _count = 0;
            Array.Clear( _buffer, 0, _buffer.Length );
        }

        public override void OnDispose(DisposalSource source)
        {
            if ( _count > 0 )
            {
                Next.React( _buffer.AsMemory( 0, _count ) );

                _count = 0;
                Array.Clear( _buffer, 0, _buffer.Length );
            }

            base.OnDispose( source );
        }
    }
}
