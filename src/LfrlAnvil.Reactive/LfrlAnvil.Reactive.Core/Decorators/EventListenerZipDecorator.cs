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
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with the result of (source, target) pair mapping, where source and target event
/// are at the same position in a sequence of emitted events by their respective emitters.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TTargetEvent">Target event type.</typeparam>
/// <typeparam name="TNextEvent">Next event type.</typeparam>
public sealed class EventListenerZipDecorator<TEvent, TTargetEvent, TNextEvent> : IEventListenerDecorator<TEvent, TNextEvent>
{
    private readonly IEventStream<TTargetEvent> _target;
    private readonly Func<TEvent, TTargetEvent, TNextEvent> _selector;

    /// <summary>
    /// Creates a new <see cref="EventListenerZipDecorator{TEvent,TTargetEvent,TNextEvent}"/> instance.
    /// </summary>
    /// <param name="target">Target event stream.</param>
    /// <param name="selector">Next event selector.</param>
    public EventListenerZipDecorator(IEventStream<TTargetEvent> target, Func<TEvent, TTargetEvent, TNextEvent> selector)
    {
        _target = target;
        _selector = selector;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TNextEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _target, _selector );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TNextEvent>
    {
        private readonly Queue<TEvent> _sourceEvents;
        private readonly Queue<TTargetEvent> _targetEvents;
        private readonly IEventSubscriber _subscriber;
        private readonly IEventSubscriber _targetSubscriber;
        private readonly Func<TEvent, TTargetEvent, TNextEvent> _selector;

        internal EventListener(
            IEventListener<TNextEvent> next,
            IEventSubscriber subscriber,
            IEventStream<TTargetEvent> target,
            Func<TEvent, TTargetEvent, TNextEvent> selector)
            : base( next )
        {
            _sourceEvents = new Queue<TEvent>();
            _targetEvents = new Queue<TTargetEvent>();
            _subscriber = subscriber;
            _selector = selector;

            _targetSubscriber = target.Listen(
                Reactive.EventListener.Create<TTargetEvent>(
                    e =>
                    {
                        if ( _sourceEvents.TryDequeue( out var sourceEvent ) )
                        {
                            Next.React( _selector( sourceEvent, e ) );
                            return;
                        }

                        _targetEvents.Enqueue( e );
                    },
                    _ => _subscriber.Dispose() ) );
        }

        public override void React(TEvent @event)
        {
            if ( _targetEvents.TryDequeue( out var targetEvent ) )
            {
                Next.React( _selector( @event, targetEvent ) );
                return;
            }

            _sourceEvents.Enqueue( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _targetSubscriber.Dispose();
            _sourceEvents.Clear();
            _sourceEvents.TrimExcess();
            _targetEvents.Clear();
            _targetEvents.TrimExcess();

            base.OnDispose( source );
        }
    }
}
