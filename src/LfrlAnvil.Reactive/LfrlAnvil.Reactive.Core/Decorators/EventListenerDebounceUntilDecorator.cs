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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Creates a new target event stream subscription on each emitted event, and drops the current one if it exists,
/// and keeps the event until the target stream emits its own event, which then notifies the decorated
/// event listener with the stored event, drops the target event stream subscription and repeats the process.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TTargetEvent">Target event type.</typeparam>
public sealed class EventListenerDebounceUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly IEventStream<TTargetEvent> _target;

    /// <summary>
    /// Creates a new <see cref="EventListenerDebounceUntilDecorator{TEvent,TTargetEvent}"/> instance.
    /// </summary>
    /// <param name="target">Target event stream to wait for before emitting the stored event.</param>
    public EventListenerDebounceUntilDecorator(IEventStream<TTargetEvent> target)
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
        private readonly object _sync = new object();
        private readonly IEventStream<TTargetEvent> _target;
        private readonly IEventSubscriber _subscriber;
        private LazyDisposable<IEventSubscriber>? _targetSubscriber;
        private TargetEventListener? _targetListener;
        private bool _isDisposing;
        private bool _isDisposed;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, IEventStream<TTargetEvent> target)
            : base( next )
        {
            _subscriber = subscriber;
            _target = target;
            _targetSubscriber = null;
            _targetListener = null;

            if ( target.IsDisposed )
                _subscriber.Dispose();
        }

        public override void React(TEvent @event)
        {
            LazyDisposable<IEventSubscriber>? oldTargetSubscriber;
            LazyDisposable<IEventSubscriber> targetSubscriber;
            TargetEventListener targetListener;
            using ( AcquireLock() )
            {
                if ( _isDisposing )
                    return;

                oldTargetSubscriber = _targetSubscriber;
                _targetSubscriber = new LazyDisposable<IEventSubscriber>();
                targetSubscriber = _targetSubscriber;
                _targetListener = new TargetEventListener( this, _targetSubscriber, @event );
                targetListener = _targetListener;
            }

            oldTargetSubscriber?.Dispose();
            var actualTargetSubscriber = _target.Listen( targetListener );
            targetSubscriber.Assign( actualTargetSubscriber );
        }

        public override void OnDispose(DisposalSource source)
        {
            LazyDisposable<IEventSubscriber>? targetSubscriber;
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                _isDisposing = true;
                _isDisposed = true;
                targetSubscriber = _targetSubscriber;
                _targetSubscriber = null;
                _targetListener = null;
            }

            targetSubscriber?.Dispose();
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent(TargetEventListener listener)
        {
            LazyDisposable<IEventSubscriber> targetSubscriber;
            using ( AcquireLock() )
            {
                if ( _isDisposing || ! ReferenceEquals( _targetListener, listener ) )
                    return;

                Assume.IsNotNull( _targetSubscriber );
                targetSubscriber = _targetSubscriber;
                _targetSubscriber = null;
                _targetListener = null;
            }

            targetSubscriber.Dispose();
            Next.React( listener.Event );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetDisposed(TargetEventListener listener, DisposalSource source)
        {
            using ( AcquireLock() )
            {
                if ( _isDisposing || ! ReferenceEquals( _targetListener, listener ) )
                    return;

                _targetSubscriber = null;
                _targetListener = null;
                if ( source == DisposalSource.Subscriber )
                    return;

                _isDisposing = true;
            }

            _subscriber.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ExclusiveLock AcquireLock()
        {
            return ExclusiveLock.Enter( _sync );
        }
    }

    private sealed class TargetEventListener : EventListener<TTargetEvent>
    {
        private readonly EventListener _sourceListener;
        private readonly LazyDisposable<IEventSubscriber> _subscriber;
        internal readonly TEvent Event;

        internal TargetEventListener(EventListener sourceListener, LazyDisposable<IEventSubscriber> subscriber, TEvent @event)
        {
            _sourceListener = sourceListener;
            _subscriber = subscriber;
            Event = @event;
        }

        public override void React(TTargetEvent _)
        {
            _sourceListener.OnTargetEvent( this );
        }

        public override void OnDispose(DisposalSource source)
        {
            _subscriber.Dispose();
            _sourceListener.OnTargetDisposed( this, source );
        }
    }
}
