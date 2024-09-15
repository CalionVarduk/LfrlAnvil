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
using System.Runtime.CompilerServices;

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
        private readonly IEventSubscriber _targetSubscriber;
        private readonly IEventSubscriber _subscriber;
        private ListSlim<TEvent> _buffer;

        internal EventListener(
            IEventListener<ReadOnlyMemory<TEvent>> next,
            IEventSubscriber subscriber,
            IEventStream<TTargetEvent> target)
            : base( next )
        {
            _subscriber = subscriber;
            _buffer = ListSlim<TEvent>.Create();
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
            _buffer.ResetCapacity();

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent()
        {
            Next.React( _buffer.AsMemory() );
            _buffer.Clear();
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
            Assume.IsNotNull( _sourceListener );
            _sourceListener.OnTargetEvent();
        }

        public override void OnDispose(DisposalSource _)
        {
            Assume.IsNotNull( _sourceListener );
            _sourceListener.OnTargetEvent();
            _sourceListener.DisposeSubscriber();
            _sourceListener = null;
        }
    }
}
