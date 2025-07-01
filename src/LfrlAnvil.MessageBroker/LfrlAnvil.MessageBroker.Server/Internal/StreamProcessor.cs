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
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct StreamProcessor
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private Task? _task;

    private StreamProcessor(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _task = null;
    }

    [Pure]
    internal static StreamProcessor Create()
    {
        return new StreamProcessor( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerStream stream)
    {
        while ( true )
        {
            try
            {
                await RunCore( stream ).ConfigureAwait( false );
                break;
            }
            catch ( Exception exc )
            {
                ulong traceId;
                using ( stream.AcquireLock() )
                    traceId = stream.GetTraceId();

                using ( MessageBrokerStreamTraceEvent.CreateScope( stream, traceId, MessageBrokerStreamTraceEventType.Unexpected ) )
                {
                    MessageBrokerStreamErrorEvent.Create( stream, traceId, exc ).Emit( stream.Logger.Error );
                    try
                    {
                        using ( stream.AcquireLock() )
                        {
                            if ( stream.ShouldCancel )
                                break;

                            stream.StreamProcessor.SignalContinuation();
                        }

                        await Task.Delay( TimeSpan.FromSeconds( 1 ) ).ConfigureAwait( false );
                    }
                    catch ( Exception exc2 )
                    {
                        MessageBrokerStreamErrorEvent.Create( stream, traceId, exc2 ).Emit( stream.Logger.Error );
                    }
                }
            }
        }
    }

    internal void Dispose()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( false );
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
    internal void SignalContinuation()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask RunCore(MessageBrokerStream stream)
    {
        while ( true )
        {
            var @continue = await stream.StreamProcessor._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            var disposed = false;
            while ( true )
            {
                int key;
                StreamMessage message;
                ulong traceId;
                using ( stream.AcquireLock() )
                {
                    if ( stream.ShouldCancel )
                        return;

                    if ( ! stream.MessageStore.TryPeekPending( out key, out message ) )
                    {
                        if ( stream.TryDisposeDueToEmptyMessageStoreUnsafe() )
                            disposed = true;
                        else
                            stream.StreamProcessor._continuation.Reset();

                        break;
                    }

                    traceId = stream.GetTraceId();
                }

                using ( MessageBrokerStreamTraceEvent.CreateScope( stream, traceId, MessageBrokerStreamTraceEventType.ProcessMessage ) )
                {
                    var failures = 0;
                    var listeners = message.Publisher.Channel.Listeners.GetAll();
                    MessageBrokerStreamProcessingMessageEvent.Create(
                            message.Publisher,
                            traceId,
                            message.Id,
                            message.Data.Length,
                            listeners )
                        .Emit( stream.Logger.ProcessingMessage );

                    if ( listeners.Count > 0 )
                    {
                        using ( stream.AcquireLock() )
                            stream.MessageStore.IncreaseRefCount( key, listeners.Count );

                        foreach ( var listener in listeners )
                        {
                            try
                            {
                                if ( ! listener.Queue.PushMessage( listener, key, in message, stream, traceId ) )
                                    ++failures;
                            }
                            catch ( Exception exc )
                            {
                                ++failures;
                                MessageBrokerStreamErrorEvent.Create( stream, traceId, exc ).Emit( stream.Logger.Error );
                            }
                        }
                    }

                    Result<bool> result;
                    using ( stream.AcquireLock() )
                        result = stream.MessageStore.DequeuePending( failures );

                    if ( result.Exception is not null )
                        MessageBrokerStreamErrorEvent.Create( stream, traceId, result.Exception ).Emit( stream.Logger.Error );

                    MessageBrokerStreamMessageProcessedEvent.Create(
                            message.Publisher,
                            traceId,
                            message.Id,
                            message.Data.Length,
                            failures,
                            result.Value )
                        .Emit( stream.Logger.MessageProcessed );
                }
            }

            if ( disposed )
            {
                ulong traceId;
                using ( stream.AcquireLock() )
                    traceId = stream.GetTraceId();

                using ( MessageBrokerStreamTraceEvent.CreateScope( stream, traceId, MessageBrokerStreamTraceEventType.Dispose ) )
                    await stream.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: true, traceId ).ConfigureAwait( false );

                return;
            }
        }
    }
}
