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
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct SynchronousScheduler
{
    private readonly ManualResetEventSlim _reset;
    private TimeoutEntry _writerCancellation;
    private TimeoutEntry _readerCancellation;
    private Timestamp _nextEventTimestamp;
    private Timestamp _nextSendPingTimestamp;
    private Task? _task;

    private SynchronousScheduler(ManualResetEventSlim reset)
    {
        _reset = reset;
        _writerCancellation = TimeoutEntry.Empty();
        _readerCancellation = _writerCancellation;
        _nextEventTimestamp = _writerCancellation.Timestamp;
        _nextSendPingTimestamp = _nextEventTimestamp;
        _task = null;
    }

    [Pure]
    internal static SynchronousScheduler Create()
    {
        return new SynchronousScheduler( new ManualResetEventSlim( false ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Task StartUnderlyingTask(MessageBrokerClient client)
    {
        return Task.Factory.StartNew(
            static state =>
            {
                Assume.IsNotNull( state );
                RunSynchronousScheduler( ReinterpretCast.To<MessageBrokerClient>( state ) );
            },
            client,
            TaskCreationOptions.LongRunning );
    }

    internal Exception? BeginDispose()
    {
        Exception? exception = null;
        try
        {
            _reset.Set();
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        _writerCancellation = _writerCancellation.Cancel();
        _readerCancellation = _readerCancellation.Cancel();
        return exception;
    }

    internal Exception? EndDispose()
    {
        _nextEventTimestamp = _writerCancellation.Timestamp;
        _nextSendPingTimestamp = _nextEventTimestamp;
        return _reset.TryDispose().Exception;
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
        var timestamp = client.GetFutureTimestamp( client.MessageTimeout );
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
        var result = client.GetFutureTimestamp( client.MessageTimeout );
        if ( _nextEventTimestamp > result )
            _reset.Set();

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SchedulePing(MessageBrokerClient client)
    {
        _nextSendPingTimestamp = client.GetFutureTimestamp( client.PingInterval );
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

    [MethodImpl( MethodImplOptions.NoInlining )]
    private static void RunSynchronousScheduler(MessageBrokerClient client)
    {
        try
        {
            client.SynchronousScheduler.RunCore( client );
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exc ) );

            using ( client.AcquireLock() )
                client.SynchronousScheduler._task = null;

            client.Dispose();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void RunCore(MessageBrokerClient client)
    {
        Duration delay;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return;

            delay = UpdateNextEventTimestamp( client, client.GetTimestamp() );
        }

        while ( true )
        {
            try
            {
                _reset.Wait( delay );
            }
            catch ( ObjectDisposedException )
            {
                return;
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return;

                _reset.Reset();
                var now = client.GetTimestamp();

                if ( _writerCancellation.IsOverdue( now ) )
                    _writerCancellation = _writerCancellation.Cancel();

                if ( _readerCancellation.IsOverdue( now ) )
                    _readerCancellation = _readerCancellation.Cancel().Prepare( TimeoutEntry.MaxTimestamp );

                client.MessageContextQueue.ProcessPendingResponseTimeouts( now );

                if ( _nextSendPingTimestamp <= now )
                    client.PingScheduler.SignalContinuation();

                delay = UpdateNextEventTimestamp( client, now );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration UpdateNextEventTimestamp(MessageBrokerClient client, Timestamp now)
    {
        var next = _writerCancellation.Timestamp.Min( _readerCancellation.Timestamp );
        if ( client.PingScheduler.IsContinuationPending )
            next = next.Min( _nextSendPingTimestamp );

        var nextResponseSource = client.MessageContextQueue.GetNextPendingResponse();
        if ( nextResponseSource.Source is not null )
            next = next.Min( nextResponseSource.Timeout );

        var delayUntilNextEvent = next - now;
        var delay = delayUntilNextEvent.Clamp( Duration.Zero, Defaults.Temporal.MaxTimeout );

        _nextEventTimestamp = now + delay;
        return delay;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private CancellationToken ScheduleWriteTimeout(MessageBrokerClient client, Duration delay)
    {
        var timestamp = client.GetFutureTimestamp( delay );
        _writerCancellation = _writerCancellation.Prepare( timestamp );
        var token = _writerCancellation.GetPreparedToken();

        if ( _nextEventTimestamp > _writerCancellation.Timestamp )
            _reset.Set();

        return token;
    }
}
