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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace LfrlAnvil.Async;

/// <inheritdoc cref="IBatch{T}" />
public abstract class Batch<T> : IBatch<T>, IDisposable, IAsyncDisposable
{
    private readonly ManualResetValueTaskSource<bool> _flushContinuation;
    private QueueSlim<T> _items;
    private Task? _flushTask;

    /// <summary>
    /// Creates a new <see cref="Batch{T}"/> instance.
    /// </summary>
    /// <param name="queueOverflowStrategy">
    /// Specifies the maximum number of enqueued elements that, when exceeded while adding new elements,
    /// will cause this batch to react according to its <see cref="QueueOverflowStrategy"/>.
    /// Equal to <see cref="BatchQueueOverflowStrategy.DiscardLast"/> by default.
    /// </param>
    /// <param name="autoFlushCount">
    /// Specifies the number of enqueued elements, which acts as a threshold that, when reached while adding new elements,
    /// will cause this batch to automatically <see cref="Flush()"/> itself.
    /// Equal to <b>1 000</b> by default.
    /// </param>
    /// <param name="queueSizeLimitHint">
    /// Specifies the maximum number of enqueued elements that, when exceeded while adding new elements,
    /// will cause this batch to react according to its <see cref="QueueOverflowStrategy"/>.
    /// Equal to <b>100 000 000</b> by default.
    /// </param>
    /// <param name="minInitialCapacity">Specifies minimum initial capacity of the internal queue. Equal to <b>0</b> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="autoFlushCount"/> is less than <b>1</b>.</exception>
    protected Batch(
        BatchQueueOverflowStrategy queueOverflowStrategy = BatchQueueOverflowStrategy.DiscardLast,
        int autoFlushCount = 1000,
        int queueSizeLimitHint = 100000000,
        int minInitialCapacity = 0)
    {
        Ensure.IsDefined( queueOverflowStrategy );
        Ensure.IsGreaterThan( autoFlushCount, 0 );

        if ( queueSizeLimitHint < autoFlushCount )
            queueSizeLimitHint = autoFlushCount;

        _flushContinuation = new ManualResetValueTaskSource<bool>();
        _items = QueueSlim<T>.Create( minInitialCapacity );
        QueueOverflowStrategy = queueOverflowStrategy;
        AutoFlushCount = autoFlushCount;
        QueueSizeLimitHint = queueSizeLimitHint;
        _flushTask = RunFlushAsync();
    }

    /// <inheritdoc />
    public int Count
    {
        get
        {
            using ( AcquireLock() )
                return _items.Count;
        }
    }

    /// <inheritdoc />
    public BatchQueueOverflowStrategy QueueOverflowStrategy { get; }

    /// <inheritdoc />
    public int AutoFlushCount { get; }

    /// <inheritdoc />
    public int QueueSizeLimitHint { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        Task flushTask;
        using ( AcquireLock() )
        {
            if ( _flushTask is null )
                return;

            flushTask = _flushTask;
            _flushTask = null;
            SignalFlush( disposing: true );
        }

        await flushTask.ConfigureAwait( false );
    }

    /// <inheritdoc />
    public bool Add(T item)
    {
        using ( AcquireLock() )
        {
            if ( _flushTask is null )
                return false;

            var count = _items.Count;
            if ( count >= QueueSizeLimitHint )
            {
                switch ( QueueOverflowStrategy )
                {
                    case BatchQueueOverflowStrategy.DiscardLast:
                        return false;

                    case BatchQueueOverflowStrategy.DiscardFirst:
                        DiscardFirst( count - QueueSizeLimitHint + 1, disposing: false );
                        count = _items.Count;
                        break;
                }
            }

            _items.Enqueue( item );
            var shouldFlush = _items.Count == AutoFlushCount;
            NotifyEnqueued( count, shouldFlush );
            if ( shouldFlush )
                SignalFlush();

            return true;
        }
    }

    /// <inheritdoc />
    public bool AddRange(IEnumerable<T> items)
    {
        if ( items is T[] arr )
            return AddRange( arr.AsSpan() );

        using ( AcquireLock() )
        {
            if ( _flushTask is null )
                return false;

            var added = 0;
            var count = _items.Count;
            switch ( QueueOverflowStrategy )
            {
                case BatchQueueOverflowStrategy.DiscardLast:
                {
                    if ( count >= QueueSizeLimitHint )
                        return false;

                    foreach ( var item in items )
                    {
                        _items.Enqueue( item );
                        ++added;
                    }

                    break;
                }
                case BatchQueueOverflowStrategy.DiscardFirst:
                {
                    using var enumerator = items.GetEnumerator();
                    if ( ! enumerator.MoveNext() )
                        return true;

                    if ( count >= QueueSizeLimitHint )
                    {
                        var discarded = count - QueueSizeLimitHint + 1;
                        DiscardFirst( discarded, disposing: false );

                        do
                        {
                            _items.Enqueue( enumerator.Current );
                            ++added;
                        }
                        while ( enumerator.MoveNext() );

                        count -= discarded;
                        discarded = Math.Min( added - 1, count );
                        if ( discarded > 0 )
                        {
                            DiscardFirst( discarded, disposing: false );
                            count -= discarded;
                        }
                    }
                    else
                    {
                        do
                        {
                            _items.Enqueue( enumerator.Current );
                            ++added;
                        }
                        while ( enumerator.MoveNext() );
                    }

                    break;
                }
                default:
                {
                    foreach ( var item in items )
                    {
                        _items.Enqueue( item );
                        ++added;
                    }

                    break;
                }
            }

            if ( added > 0 )
            {
                var shouldFlush = count < AutoFlushCount && _items.Count >= AutoFlushCount;
                NotifyEnqueued( count, shouldFlush );
                if ( shouldFlush )
                    SignalFlush();
            }

            return true;
        }
    }

    /// <inheritdoc />
    public bool AddRange(ReadOnlySpan<T> items)
    {
        if ( items.Length == 0 )
            return true;

        using ( AcquireLock() )
        {
            if ( _flushTask is null )
                return false;

            var count = _items.Count;
            if ( count >= QueueSizeLimitHint )
            {
                switch ( QueueOverflowStrategy )
                {
                    case BatchQueueOverflowStrategy.DiscardLast:
                        return false;

                    case BatchQueueOverflowStrategy.DiscardFirst:
                    {
                        var toDiscard = unchecked( ( uint )count - ( uint )QueueSizeLimitHint + ( uint )items.Length );
                        toDiscard = Math.Min( toDiscard, unchecked( ( uint )count ) );
                        DiscardFirst( unchecked( ( int )toDiscard ), disposing: false );
                        count = _items.Count;
                        break;
                    }
                }
            }

            _items.EnqueueRange( items );
            var shouldFlush = count < AutoFlushCount && _items.Count >= AutoFlushCount;
            NotifyEnqueued( count, shouldFlush );
            if ( shouldFlush )
                SignalFlush();

            return true;
        }
    }

    /// <inheritdoc />
    public bool Flush()
    {
        using ( AcquireLock() )
        {
            if ( _flushTask is null )
                return false;

            SignalFlush();
            return true;
        }
    }

    /// <summary>
    /// Allows to react to a non-empty range of elements being discarded due to <see cref="QueueOverflowStrategy"/>
    /// or due to this batch being disposed.
    /// </summary>
    /// <param name="items">Range of elements to be discarded.</param>
    /// <param name="disposing">Specifies whether or not this batch is in the process of being disposed.</param>
    /// <remarks>
    /// Exceptions thrown by this method will be completely ignored.
    /// </remarks>
    protected virtual void OnDiscarding(QueueSlimMemory<T> items, bool disposing) { }

    /// <summary>
    /// Allows to react to a non-empty range of elements being enqueued.
    /// </summary>
    /// <param name="items">Range of enqueued elements.</param>
    /// <param name="autoFlushing">
    /// Specifies whether or not this batch will be automatically flushed
    /// due to the combination of its <see cref="AutoFlushCount"/> and new elements being added.
    /// </param>
    /// <remarks>
    /// Exceptions thrown by this method will be completely ignored.
    /// </remarks>
    protected virtual void OnEnqueued(QueueSlimMemory<T> items, bool autoFlushing) { }

    /// <summary>
    /// Allows to react to a non-empty range of elements being dequeued and made ready to process.
    /// </summary>
    /// <param name="items">Range of dequeued elements.</param>
    /// <param name="disposing">Specifies whether or not this batch is in the process of being disposed.</param>
    /// <remarks>
    /// Exceptions thrown by this method will be completely ignored.
    /// Disposing the batch from inside this method may cause a deadlock.
    /// </remarks>
    protected virtual void OnDequeued(ReadOnlyMemory<T> items, bool disposing) { }

    /// <summary>
    /// Allows to react to the batch being disposed.
    /// </summary>
    /// <remarks>
    /// Exceptions thrown by this method will be completely ignored.
    /// The batch will not have any enqueued elements at the moment of invocation of this method.
    /// </remarks>
    protected virtual void OnDisposed() { }

    /// <summary>
    /// Asynchronously processes provided non-empty range of dequeued elements.
    /// </summary>
    /// <param name="items">Range of elements to process. Number of elements will not exceed <see cref="AutoFlushCount"/>.</param>
    /// <param name="disposing">Specifies whether or not this batch is in the process of being disposed.</param>
    /// <returns>Task that returns the number of successfully processed elements.</returns>
    /// <remarks>
    /// Exceptions thrown by this method will be completely ignored.
    /// If the returned number of successfully processed elements is less than <b>1</b>,
    /// then the batch will stop attempting to process enqueued elements further, until the next <see cref="Flush()"/> invocation
    /// or automatic flushing due to <see cref="AutoFlushCount"/>.
    /// If the returned number of successfully processed elements is greater than <b>0</b>
    /// but less than the number of provided elements to process,
    /// then the batch will treat elements at the start of the provided range as processed and the remaining elements
    /// will stay in the buffer.
    /// Disposing the batch from inside this method may cause a deadlock.
    /// </remarks>
    protected abstract ValueTask<int> ProcessAsync(ReadOnlyMemory<T> items, bool disposing);

    private async Task RunFlushAsync()
    {
        var buffer = ListSlim<T>.Create( Math.Min( AutoFlushCount, 64 ) );

        while ( true )
        {
            var disposing = await _flushContinuation.GetTask().ConfigureAwait( false );
            if ( ! disposing )
            {
                using ( AcquireLock() )
                    _flushContinuation.Reset();
            }

            while ( true )
            {
                using ( AcquireLock() )
                {
                    if ( ! disposing && _flushTask is null )
                        disposing = true;

                    var oldCount = buffer.Count;
                    var maxToMove = AutoFlushCount - oldCount;
                    if ( maxToMove > 0 )
                    {
                        var toDequeue = _items.AsMemory();
                        var length = toDequeue.Length;
                        if ( length > maxToMove )
                        {
                            toDequeue = toDequeue.Slice( 0, maxToMove );
                            length = maxToMove;
                        }

                        if ( length > 0 )
                        {
                            buffer.AddRange( toDequeue.First.Span );
                            buffer.AddRange( toDequeue.Second.Span );
                            _items.DequeueRange( length );

                            try
                            {
                                OnDequeued( buffer.AsMemory().Slice( oldCount ), disposing );
                            }
                            catch
                            {
                                // NOTE: do nothing
                            }
                        }
                    }

                    if ( buffer.Count == 0 )
                    {
                        if ( _flushTask is null )
                        {
                            InvokeOnDisposed();
                            return;
                        }

                        break;
                    }
                }

                var package = buffer.AsMemory();
                Assume.IsLessThanOrEqualTo( package.Length, AutoFlushCount );

                int processed;
                try
                {
                    processed = await ProcessAsync( package, disposing ).ConfigureAwait( false );
                }
                catch
                {
                    processed = 0;
                }

                if ( processed <= 0 )
                    break;

                if ( processed >= package.Length )
                    buffer.Clear();
                else
                    buffer.RemoveRangeAt( 0, processed );
            }

            using ( AcquireLock() )
            {
                if ( _flushTask is null )
                {
                    if ( buffer.Count > 0 )
                    {
                        InvokeOnDiscarding( QueueSlimMemory<T>.From( buffer.AsMemory() ), disposing: true );
                        buffer.Clear();
                    }

                    if ( _items.Count > 0 )
                        DiscardFirst( _items.Count, disposing: true );

                    InvokeOnDisposed();
                    return;
                }
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void DiscardFirst(int count, bool disposing)
    {
        var discarded = _items.AsMemory().Slice( 0, count );
        InvokeOnDiscarding( discarded, disposing );
        _items.DequeueRange( discarded.Length );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void NotifyEnqueued(int oldCount, bool autoFlushing)
    {
        Assume.IsGreaterThanOrEqualTo( oldCount, 0 );
        var enqueued = _items.AsMemory().Slice( oldCount );
        Assume.IsGreaterThan( enqueued.Length, 0 );

        try
        {
            OnEnqueued( enqueued, autoFlushing );
        }
        catch
        {
            // NOTE: do nothing
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void InvokeOnDiscarding(QueueSlimMemory<T> items, bool disposing)
    {
        Assume.IsGreaterThan( items.Length, 0 );
        try
        {
            OnDiscarding( items, disposing );
        }
        catch
        {
            // NOTE: do nothing
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void InvokeOnDisposed()
    {
        Assume.True( _items.IsEmpty );
        try
        {
            OnDisposed();
        }
        catch
        {
            // NOTE: do nothing
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SignalFlush(bool disposing = false)
    {
        if ( _flushContinuation.Status == ValueTaskSourceStatus.Pending )
            _flushContinuation.SetResult( disposing );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _flushContinuation );
    }
}
