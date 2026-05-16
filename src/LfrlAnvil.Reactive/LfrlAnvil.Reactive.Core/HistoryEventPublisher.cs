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

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Reactive;

/// <inheritdoc cref="IHistoryEventPublisher{TEvent}" />
public class HistoryEventPublisher<TEvent> : EventPublisher<TEvent>, IHistoryEventPublisher<TEvent>
{
    private QueueSlim<TEvent> _history;

    /// <summary>
    /// Creates a new <see cref="HistoryEventPublisher{TEvent}"/> instance.
    /// </summary>
    /// <param name="capacity">Specifies the maximum number of events this event publisher can record.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="capacity"/> is less than <b>1</b>.</exception>
    public HistoryEventPublisher(int capacity)
    {
        Ensure.IsGreaterThan( capacity, 0 );
        Capacity = capacity;
        _history = QueueSlim<TEvent>.Create();
    }

    /// <inheritdoc />
    public int Capacity { get; }

    /// <inheritdoc cref="IHistoryEventPublisher{TEvent}.History" />
    public TEvent[] History
    {
        get
        {
            using ( AcquireLock() )
            {
                if ( _history.IsEmpty )
                    return [ ];

                var i = 0;
                var result = new TEvent[_history.Count];
                foreach ( var e in _history )
                    result[i++] = e;

                return result;
            }
        }
    }

    IReadOnlyCollection<TEvent> IHistoryEventPublisher<TEvent>.History => History;

    /// <inheritdoc />
    public void ClearHistory()
    {
        using ( AcquireLock() )
            _history.Clear();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        if ( DisposeCore( out var exceptions ) )
        {
            using ( AcquireLock() )
                _history = QueueSlim<TEvent>.Create();
        }

        if ( exceptions.Count > 0 )
            exceptions.Consolidate()?.Rethrow();
    }

    /// <inheritdoc />
    protected override void OnPublish(TEvent @event)
    {
        using ( AcquireLock() )
        {
            if ( ! IsDisposedUnsafe() )
            {
                if ( _history.Count == Capacity )
                    _history.Dequeue();

                _history.Enqueue( @event );
            }
        }

        base.OnPublish( @event );
    }

    /// <inheritdoc />
    protected override IEventListener<TEvent> OnSubscriberRegistered(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        ArrayPoolToken<TEvent> historyToken = default;
        if ( ! _history.IsEmpty )
        {
            historyToken = ArrayPool<TEvent>.Shared.RentToken( _history.Count, clearArray: true );
            var history = historyToken.AsSpan();

            var i = 0;
            foreach ( var e in _history )
                history[i++] = e;
        }

        return new HistoryListener( listener, historyToken );
    }

    /// <inheritdoc />
    protected override void OnSubscriberAddedUnsafe(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        base.OnSubscriberAddedUnsafe( subscriber, listener );
        StartHistoryListener( listener );
    }

    /// <summary>
    /// Starts an internal history listener.
    /// </summary>
    /// <param name="listener">Listener to start.</param>
    /// <exception cref="InvalidCastException">
    /// When <paramref name="listener"/> is not a history listener returned by <see cref="OnSubscriberRegistered"/> method.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void StartHistoryListener(IEventListener<TEvent> listener)
    {
        (( HistoryListener )listener).Start();
    }

    private sealed class HistoryListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly object _sync = new object();
        private ArrayPoolToken<TEvent> _historyToken;
        private QueueSlim<TEvent> _buffer;
        private State _state;

        public HistoryListener(IEventListener<TEvent> next, ArrayPoolToken<TEvent> historyToken)
            : base( next )
        {
            _historyToken = historyToken;
            _buffer = QueueSlim<TEvent>.Create();
            _state = State.Replaying;
        }

        public override void React(TEvent @event)
        {
            using ( AcquireLock() )
            {
                switch ( _state )
                {
                    case State.Replaying:
                        _buffer.Enqueue( @event );
                        return;

                    case State.Disposed:
                        return;
                }
            }

            Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            ArrayPoolToken<TEvent> historyToken = default;
            try
            {
                using ( AcquireLock() )
                {
                    if ( _state == State.Disposed )
                        return;

                    _state = State.Disposed;
                    historyToken = _historyToken;
                    _historyToken = default;
                    _buffer = QueueSlim<TEvent>.Create();
                }
            }
            finally
            {
                historyToken.Dispose();
            }

            base.OnDispose( source );
        }

        internal void Start()
        {
            ArrayPoolToken<TEvent> historyToken = default;
            try
            {
                using ( AcquireLock() )
                {
                    if ( _state == State.Disposed )
                        return;

                    historyToken = _historyToken;
                    _historyToken = default;
                }

                var history = historyToken.AsSpan();
                foreach ( var e in history )
                {
                    using ( AcquireLock() )
                    {
                        if ( _state == State.Disposed )
                            return;
                    }

                    Next.React( e );
                }
            }
            finally
            {
                historyToken.Dispose();
            }

            using ( AcquireLock() )
            {
                if ( _state == State.Disposed )
                    return;

                if ( _buffer.IsEmpty )
                {
                    _state = State.Forwarding;
                    return;
                }
            }

            while ( true )
            {
                TEvent e;
                using ( AcquireLock() )
                {
                    if ( _state == State.Disposed )
                        return;

                    Assume.False( _buffer.IsEmpty );
                    e = _buffer.First();
                    _buffer.Dequeue();
                }

                Next.React( e );

                using ( AcquireLock() )
                {
                    if ( _state == State.Disposed )
                        return;

                    if ( _buffer.IsEmpty )
                    {
                        _state = State.Forwarding;
                        _buffer = QueueSlim<TEvent>.Create();
                        return;
                    }
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ExclusiveLock AcquireLock()
        {
            return ExclusiveLock.Enter( _sync );
        }

        private enum State : byte
        {
            Replaying = 0,
            Forwarding = 1,
            Disposed = 2
        }
    }
}
