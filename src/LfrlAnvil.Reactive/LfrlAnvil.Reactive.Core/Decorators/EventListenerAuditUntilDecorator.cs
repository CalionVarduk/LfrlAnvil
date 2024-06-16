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

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Creates a new target event stream subscription on each emitted event, unless an active one already exists,
/// and keeps updating the last emitted event until the target stream emits its own event, which then notifies the decorated
/// event listener with the stored event, drops the target event stream subscription and repeats the process.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TTargetEvent">Target event type.</typeparam>
public sealed class EventListenerAuditUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly IEventStream<TTargetEvent> _target;

    /// <summary>
    /// Creates a new <see cref="EventListenerAuditUntilDecorator{TEvent,TTargetEvent}"/> instance.
    /// </summary>
    /// <param name="target">Target event stream to wait for before emitting the last emitted event.</param>
    public EventListenerAuditUntilDecorator(IEventStream<TTargetEvent> target)
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
        private LazyDisposable<IEventSubscriber>? _targetSubscriber;
        private TargetEventListener? _targetListener;
        private IEventStream<TTargetEvent>? _target;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, IEventStream<TTargetEvent> target)
            : base( next )
        {
            _subscriber = subscriber;
            _target = target;
            _targetSubscriber = null;
            _targetListener = null;

            if ( target.IsDisposed )
                DisposeSubscriber();
        }

        public override void React(TEvent @event)
        {
            if ( _targetListener is not null )
            {
                _targetListener.UpdateEvent( @event );
                return;
            }

            Assume.IsNotNull( _target );
            _targetSubscriber = new LazyDisposable<IEventSubscriber>();
            _targetListener = new TargetEventListener( this, @event );
            var actualTargetSubscriber = _target.Listen( _targetListener );
            _targetSubscriber?.Assign( actualTargetSubscriber );
        }

        public override void OnDispose(DisposalSource source)
        {
            _target = null;
            _targetSubscriber?.Dispose();

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent(TEvent @event)
        {
            Next.React( @event );
            Assume.IsNotNull( _targetSubscriber );
            _targetSubscriber.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ClearTargetReferences()
        {
            _targetSubscriber = null;
            _targetListener = null;
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
        private TEvent? _sourceEvent;

        internal TargetEventListener(EventListener sourceListener, TEvent sourceEvent)
        {
            _sourceListener = sourceListener;
            _sourceEvent = sourceEvent;
        }

        public override void React(TTargetEvent _)
        {
            Assume.IsNotNull( _sourceListener );
            _sourceListener.OnTargetEvent( _sourceEvent! );
        }

        public override void OnDispose(DisposalSource source)
        {
            _sourceEvent = default;
            Assume.IsNotNull( _sourceListener );
            _sourceListener.ClearTargetReferences();

            if ( source == DisposalSource.EventSource )
                _sourceListener.DisposeSubscriber();

            _sourceListener = null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void UpdateEvent(TEvent @event)
        {
            _sourceEvent = @event;
        }
    }
}
