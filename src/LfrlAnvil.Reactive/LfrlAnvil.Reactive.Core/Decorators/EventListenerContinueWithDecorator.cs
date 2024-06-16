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
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Invokes the provided continuation factory delegate with the last emitted event as the parameter and attaches
/// the decorated event listener to the result of that invocation.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TNextEvent">Next event type.</typeparam>
public sealed class EventListenerContinueWithDecorator<TEvent, TNextEvent> : IEventListenerDecorator<TEvent, TNextEvent>
{
    private readonly Func<TEvent, IEventStream<TNextEvent>> _continuationFactory;

    /// <summary>
    /// Creates a new <see cref="EventListenerContinueWithDecorator{TEvent,TNextEvent}"/> instance.
    /// </summary>
    /// <param name="continuationFactory">Delegate that creates the continuation event stream based on the last emitted event.</param>
    public EventListenerContinueWithDecorator(Func<TEvent, IEventStream<TNextEvent>> continuationFactory)
    {
        _continuationFactory = continuationFactory;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TNextEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _continuationFactory );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TNextEvent>
    {
        private readonly Func<TEvent, IEventStream<TNextEvent>> _continuationFactory;
        private Optional<TEvent> _argument;

        internal EventListener(
            IEventListener<TNextEvent> next,
            Func<TEvent, IEventStream<TNextEvent>> continuationFactory)
            : base( next )
        {
            _continuationFactory = continuationFactory;
            _argument = Optional<TEvent>.Empty;
        }

        public override void React(TEvent @event)
        {
            _argument = new Optional<TEvent>( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            var argument = _argument;
            _argument = Optional<TEvent>.Empty;

            if ( ! argument.HasValue )
            {
                base.OnDispose( source );
                return;
            }

            var continuationStream = _continuationFactory( argument.Event! );
            continuationStream.Listen( Next );
        }
    }
}
