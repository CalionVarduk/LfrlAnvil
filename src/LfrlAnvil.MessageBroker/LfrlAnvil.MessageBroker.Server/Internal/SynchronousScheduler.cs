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
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct SynchronousScheduler
{
    private readonly ManualResetEventSlim _reset;
    private TimeoutEntry _writerCancellation;
    private TimeoutEntry _readerCancellation;
    private Timestamp _nextEventTimestamp;
    private Task? _task;

    private SynchronousScheduler(ManualResetEventSlim reset)
    {
        _reset = reset;
        _writerCancellation = TimeoutEntry.Empty();
        _readerCancellation = _writerCancellation;
        _nextEventTimestamp = _writerCancellation.Timestamp;
        _task = null;
    }

    [Pure]
    internal static SynchronousScheduler Create()
    {
        return new SynchronousScheduler( new ManualResetEventSlim( false ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Task StartUnderlyingTask(MessageBrokerRemoteClient client)
    {
        return Task.Factory.StartNew(
            static state =>
            {
                Assume.IsNotNull( state );
                RunSynchronousScheduler( ReinterpretCast.To<MessageBrokerRemoteClient>( state ) );
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
    internal void ResetReadTimeout()
    {
        _readerCancellation = _readerCancellation.Prepare( TimeoutEntry.MaxTimestamp );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal CancellationToken ScheduleWriteTimeout(MessageBrokerRemoteClient client)
    {
        var timestamp = client.GetFutureTimestamp( client.MessageTimeout );
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

    [MethodImpl( MethodImplOptions.NoInlining )]
    private static void RunSynchronousScheduler(MessageBrokerRemoteClient client)
    {
        try
        {
            client.SynchronousScheduler.RunCore( client );
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerRemoteClientEvent.Unexpected( client, exc ) );

            using ( client.AcquireLock() )
                client.SynchronousScheduler._task = null;

            client.DisconnectAsync().AsTask().Wait();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void RunCore(MessageBrokerRemoteClient client)
    {
        Duration delay;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return;

            delay = UpdateNextEventTimestamp( client.GetTimestamp() );
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

                delay = UpdateNextEventTimestamp( now );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration UpdateNextEventTimestamp(Timestamp now)
    {
        var next = _writerCancellation.Timestamp.Min( _readerCancellation.Timestamp );

        var delayUntilNextEvent = next - now;
        var delay = delayUntilNextEvent.Clamp( Duration.Zero, Defaults.Temporal.MaxTimeout );

        _nextEventTimestamp = now + delay;
        return delay;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private CancellationToken ScheduleReadTimeout(MessageBrokerRemoteClient client, Duration delay)
    {
        var timestamp = client.GetFutureTimestamp( delay );
        _readerCancellation = _readerCancellation.Prepare( timestamp );
        var token = _readerCancellation.GetPreparedToken();

        if ( _nextEventTimestamp > _readerCancellation.Timestamp )
            _reset.Set();

        return token;
    }
}
