// Copyright 2025-2026 Łukasz Furlepa
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
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct EventScheduler
{
    private AsyncManualResetEvent _reset;
    private TimeoutEntry _writerCancellation;
    private TimeoutEntry _readerCancellation;
    private Timestamp _nextEventTimestamp;
    private Timestamp _nextSendPingTimestamp;
    private Task? _task;

    private EventScheduler(TimeoutEntry cancellation)
    {
        _reset = default;
        _writerCancellation = cancellation;
        _readerCancellation = _writerCancellation;
        _nextEventTimestamp = _writerCancellation.Timestamp;
        _nextSendPingTimestamp = _nextEventTimestamp;
        _task = null;
    }

    [Pure]
    internal static EventScheduler Create()
    {
        return new EventScheduler( TimeoutEntry.Empty() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerClient client)
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
            traceId = client.GetTraceId();

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.Unexpected ) )
        {
            if ( client.Logger.Error is { } error )
                error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

            await client.DisposeAsync( traceId, MessageBrokerClient.DeactivationSource.EventScheduler ).ConfigureAwait( false );
        }
    }

    internal void Dispose(ref Chain<Exception> exceptions)
    {
        try
        {
            _reset.Dispose();
            _reset = default;
        }
        catch ( Exception exc )
        {
            exceptions = exceptions.Extend( exc );
        }

        try
        {
            _writerCancellation = _writerCancellation.Cancel();
            _readerCancellation = _readerCancellation.Cancel();
            _nextEventTimestamp = _writerCancellation.Timestamp;
            _nextSendPingTimestamp = _nextEventTimestamp;
        }
        catch ( Exception exc )
        {
            exceptions = exceptions.Extend( exc );
        }
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
    internal CancellationToken ScheduleWriteTimeout(MessageBrokerClient client)
    {
        return ScheduleWriteTimeout( client, client.MessageTimeout );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal CancellationToken ScheduleConnectTimeout(MessageBrokerClient client)
    {
        return ScheduleWriteTimeout( client, client.ConnectionTimeout );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal CancellationToken ScheduleReadTimeout(MessageBrokerClient client)
    {
        var timestamp = client.GetTimestamp() + client.MessageTimeout;
        _readerCancellation = _readerCancellation.Prepare( timestamp );
        var token = _readerCancellation.GetPreparedToken();

        if ( _nextEventTimestamp > _readerCancellation.Timestamp )
            _reset.Set();

        return token;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ResetReadTimeout()
    {
        _readerCancellation = _readerCancellation.Reset();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal CancellationToken GetReadTimeoutToken()
    {
        return _readerCancellation.GetPreparedToken();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Timestamp GetPendingResponseTimeout(MessageBrokerClient client)
    {
        var result = client.GetTimestamp() + client.MessageTimeout;
        if ( _nextEventTimestamp > result )
            _reset.Set();

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SchedulePing(MessageBrokerClient client)
    {
        _nextSendPingTimestamp = client.GetTimestamp() + client.PingInterval;
        if ( _nextEventTimestamp > _nextSendPingTimestamp )
            _reset.Set();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void PausePing()
    {
        _nextSendPingTimestamp = TimeoutEntry.MaxTimestamp;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Duration GetPingDelay(MessageBrokerClient client)
    {
        var now = client.GetTimestamp();
        return _nextSendPingTimestamp - now;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask<Exception?> RunCore(MessageBrokerClient client)
    {
        Duration delay;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return null;

            delay = client.EventScheduler.UpdateNextEventTimestamp( client, client.GetTimestamp() );
        }

        while ( true )
        {
            var waitResult = await client.EventScheduler._reset.WaitAsync( delay ).ConfigureAwait( false );
            if ( waitResult == AsyncManualResetEventResult.Disposed )
                return client.State < MessageBrokerClientState.Disposing
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

                client.ResponseQueue.ProcessTimeouts( now );

                if ( client.EventScheduler._nextSendPingTimestamp <= now )
                    client.PingScheduler.SignalContinuation();

                delay = client.EventScheduler.UpdateNextEventTimestamp( client, now );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration UpdateNextEventTimestamp(MessageBrokerClient client, Timestamp now)
    {
        var next = _writerCancellation.Timestamp.Min( _readerCancellation.Timestamp );
        if ( client.PingScheduler.IsContinuationPending )
            next = next.Min( _nextSendPingTimestamp );

        var nextResponse = client.ResponseQueue.GetNext();
        if ( nextResponse.Source is not null )
            next = next.Min( nextResponse.Timeout );

        var delayUntilNextEvent = next - now;
        var delay = delayUntilNextEvent.Clamp( Duration.Zero, Defaults.Temporal.MaxTimeout );

        _nextEventTimestamp = now + delay;
        return delay;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private CancellationToken ScheduleWriteTimeout(MessageBrokerClient client, Duration delay)
    {
        var timestamp = client.GetTimestamp() + delay;
        _writerCancellation = _writerCancellation.Prepare( timestamp );
        var token = _writerCancellation.GetPreparedToken();

        if ( _nextEventTimestamp > _writerCancellation.Timestamp )
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
