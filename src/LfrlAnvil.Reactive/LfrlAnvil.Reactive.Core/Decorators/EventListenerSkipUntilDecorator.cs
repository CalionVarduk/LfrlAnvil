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

using System.Runtime.CompilerServices;
using LfrlAnvil.Async;

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
        private InterlockedInt32 _state;

        internal EventListener(IEventListener<TEvent> next, IEventStream<TTargetEvent> target)
            : base( next )
        {
            _state = new InterlockedInt32( ( int )State.Skipping );
            _targetSubscriber = new LazyDisposable<IEventSubscriber>();
            var targetListener = new TargetEventListener( this );
            _targetSubscriber.Assign( target.Listen( targetListener ) );
        }

        public override void React(TEvent @event)
        {
            if ( ( State )_state.Value == State.Forwarding )
                Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            if ( ! _state.Write( ( int )State.Disposed ) )
                return;

            _targetSubscriber.Dispose();
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent()
        {
            _targetSubscriber.Dispose();
            _state.Write( ( int )State.Forwarding, ( int )State.Skipping );
        }

        private enum State : byte
        {
            Skipping = 0,
            Forwarding = 1,
            Disposed = 2
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
            _sourceListener.OnTargetEvent();
        }
    }
}
