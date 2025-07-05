// Copyright 2025 Łukasz Furlepa
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
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct EventScheduler
{
    private AsyncManualResetEvent _reset;
    private TimeoutEntry _writerCancellation;
    private TimeoutEntry _readerCancellation;
    private Timestamp _nextEventTimestamp;
    private QueueEventHeap _queueHeap;
    private Task? _task;

    private EventScheduler(TimeoutEntry cancellation)
    {
        _reset = default;
        _writerCancellation = cancellation;
        _readerCancellation = _writerCancellation;
        _nextEventTimestamp = _writerCancellation.Timestamp;
        _queueHeap = QueueEventHeap.Create();
        _task = null;
    }

    [Pure]
    internal static EventScheduler Create()
    {
        return new EventScheduler( TimeoutEntry.Empty() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerRemoteClient client)
    {
        Exception? exception;
        try
        {
            exception = await RunCore( client ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        if ( exception is null )
            return;

        ulong traceId;
        using ( client.AcquireLock() )
        {
            client.EventScheduler._task = null;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Unexpected ) )
        {
            if ( client.Logger.Error is { } error )
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exception ) );

            await client.DisposeAsync( traceId ).ConfigureAwait( false );
        }
    }

    internal void Dispose()
    {
        _reset.Dispose();
        _writerCancellation = _writerCancellation.Cancel();
        _readerCancellation = _readerCancellation.Cancel();
        _nextEventTimestamp = _writerCancellation.Timestamp;
        _queueHeap.Clear();
    }

    internal void SetUnderlyingTask(Task task)
    {
        Assume.IsNull( _task );
        _task = task;
    }

    internal Task? DiscardUnderlyingTask()
    {
        var result = _task;
        _task = null;
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void InitializeResetEvent(ValueTaskDelaySource source)
    {
        _reset = source.GetResetEvent();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ResetWriteTimeout()
    {
        _writerCancellation = _writerCancellation.Reset();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ResetReadTimeout()
    {
        _readerCancellation = _readerCancellation.Prepare( TimeoutEntry.MaxTimestamp );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal CancellationToken ScheduleWriteTimeout(MessageBrokerRemoteClient client)
    {
        var timestamp = client.GetTimestamp() + client.MessageTimeout;
        _writerCancellation = _writerCancellation.Prepare( timestamp );
        var token = _writerCancellation.GetPreparedToken();

        if ( _nextEventTimestamp > _writerCancellation.Timestamp )
            _reset.Set();

        return token;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal CancellationToken ScheduleReadTimeout(MessageBrokerRemoteClient client)
    {
        return ScheduleReadTimeout( client, client.MessageTimeout );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal CancellationToken ScheduleMaxReadTimeout(MessageBrokerRemoteClient client)
    {
        return ScheduleReadTimeout( client, client.MaxReadTimeout );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddQueue(MessageBrokerQueue queue)
    {
        _queueHeap.Add( queue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void UpdateQueue(MessageBrokerQueue queue)
    {
        var timestamp = _queueHeap.Update( queue );
        if ( _nextEventTimestamp > timestamp )
            _reset.Set();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RemoveQueue(MessageBrokerQueue queue)
    {
        _queueHeap.Remove( queue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask<Exception?> RunCore(MessageBrokerRemoteClient client)
    {
        Duration delay;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return null;

            delay = client.EventScheduler.UpdateNextEventTimestamp( client.GetTimestamp() );
        }

        while ( true )
        {
            var waitResult = await client.EventScheduler._reset.WaitAsync( delay ).ConfigureAwait( false );
            if ( waitResult == AsyncManualResetEventResult.Disposed )
                return client.State < MessageBrokerRemoteClientState.Disposing
                    ? new OperationCanceledException( Resources.ExternalDelaySourceHasBeenDisposed )
                    : null;

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return null;

                client.EventScheduler._reset.Reset();
                var now = client.GetTimestamp();

                if ( client.EventScheduler._writerCancellation.IsOverdue( now ) )
                    client.EventScheduler.InvokeWriterCancellation();

                if ( client.EventScheduler._readerCancellation.IsOverdue( now ) )
                    client.EventScheduler.InvokeReaderCancellation();

                client.EventScheduler._queueHeap.Process( now );
                delay = client.EventScheduler.UpdateNextEventTimestamp( now );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration UpdateNextEventTimestamp(Timestamp now)
    {
        var next = _writerCancellation.Timestamp.Min( _readerCancellation.Timestamp );
        if ( _queueHeap.TryGetNextTimestamp( out var timestamp ) )
            next = next.Min( timestamp );

        var delayUntilNextEvent = next - now;
        var delay = delayUntilNextEvent.Clamp( Duration.Zero, Defaults.Temporal.MaxTimeout );

        _nextEventTimestamp = now + delay;
        return delay;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private CancellationToken ScheduleReadTimeout(MessageBrokerRemoteClient client, Duration delay)
    {
        var timestamp = client.GetTimestamp() + delay;
        _readerCancellation = _readerCancellation.Prepare( timestamp );
        var token = _readerCancellation.GetPreparedToken();

        if ( _nextEventTimestamp > _readerCancellation.Timestamp )
            _reset.Set();

        return token;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void InvokeWriterCancellation()
    {
        _writerCancellation = _writerCancellation.Cancel();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void InvokeReaderCancellation()
    {
        _readerCancellation = _readerCancellation.Cancel().Prepare( TimeoutEntry.MaxTimestamp );
    }
}
