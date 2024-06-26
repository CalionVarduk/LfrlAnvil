﻿// Copyright 2024 Łukasz Furlepa
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

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Skips events at the beginning of the sequence until the target event stream emits its own event,
/// before starting to notify the decorated event listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TTargetEvent">Target event type.</typeparam>
public sealed class EventListenerSkipUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly IEventStream<TTargetEvent> _target;

    /// <summary>
    /// Creates a new <see cref="EventListenerSkipUntilDecorator{TEvent,TTargetEvent}"/> instance.
    /// </summary>
    /// <param name="target">Target event stream to wait for before starting to notify the decorated event listener.</param>
    public EventListenerSkipUntilDecorator(IEventStream<TTargetEvent> target)
    {
        _target = target;
    }

    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _target );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly LazyDisposable<IEventSubscriber> _targetSubscriber;

        internal EventListener(IEventListener<TEvent> next, IEventStream<TTargetEvent> target)
            : base( next )
        {
            _targetSubscriber = new LazyDisposable<IEventSubscriber>();
            var targetListener = new TargetEventListener( this );
            var actualTargetSubscriber = target.Listen( targetListener );
            _targetSubscriber.Assign( actualTargetSubscriber );
        }

        public override void React(TEvent @event)
        {
            if ( _targetSubscriber.IsDisposed )
                Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _targetSubscriber.Dispose();
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent()
        {
            _targetSubscriber.Dispose();
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
            _sourceListener = null;
        }
    }
}
