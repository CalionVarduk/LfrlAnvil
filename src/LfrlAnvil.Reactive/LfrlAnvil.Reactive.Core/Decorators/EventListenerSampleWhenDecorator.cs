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
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with the last emitted event when the target event stream publishes its own event,
/// unless no event has been emitted in the meantime.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TTargetEvent">Target event type.</typeparam>
public sealed class EventListenerSampleWhenDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly IEventStream<TTargetEvent> _target;

    /// <summary>
    /// Creates a new <see cref="EventListenerSampleWhenDecorator{TEvent,TTargetEvent}"/> instance.
    /// </summary>
    /// <param name="target">Target event stream to wait for before emitting the last stored event.</param>
    public EventListenerSampleWhenDecorator(IEventStream<TTargetEvent> target)
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
        private readonly IEventSubscriber _subscriber;
        private readonly IEventSubscriber _targetSubscriber;
        private readonly TargetEventListener _targetListener;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, IEventStream<TTargetEvent> target)
            : base( next )
        {
            _subscriber = subscriber;
            _targetListener = new TargetEventListener( this );
            _targetSubscriber = target.Listen( _targetListener );
        }

        public override void React(TEvent @event)
        {
            _targetListener.UpdateSample( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _targetSubscriber.Dispose();
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent(Optional<TEvent> sample)
        {
            sample.TryForward( Next );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void DisposeSubscriber()
        {
            _subscriber.Dispose();
        }
    }

    private sealed class TargetEventListener : EventListener<TTargetEvent>
    {
        private Optional<TEvent> _sample;
        private EventListener? _sourceListener;

        internal TargetEventListener(EventListener sourceListener)
        {
            _sample = Optional<TEvent>.Empty;
            _sourceListener = sourceListener;
        }

        public override void React(TTargetEvent _)
        {
            Assume.IsNotNull( _sourceListener );
            _sourceListener.OnTargetEvent( _sample );
            _sample = Optional<TEvent>.Empty;
        }

        public override void OnDispose(DisposalSource _)
        {
            Assume.IsNotNull( _sourceListener );
            _sample = Optional<TEvent>.Empty;
            _sourceListener.DisposeSubscriber();
            _sourceListener = null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void UpdateSample(TEvent @event)
        {
            _sample = new Optional<TEvent>( @event );
        }
    }
}
