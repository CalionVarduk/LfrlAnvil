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
        var buffer = ListSlim<StreamMessage>.Create( minCapacity: 16 );
        while ( true )
        {
            var @continue = await stream.StreamProcessor._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            var disposed = false;
            while ( true )
            {
                ulong traceId;
                MessageBrokerChannel? channel;
                using ( stream.AcquireLock() )
                {
                    if ( stream.ShouldCancel )
                        return;

                    channel = stream.CopyMessagesIntoUnsafe( ref buffer );
                    if ( channel is null )
                    {
                        if ( stream.TryDisposeDueToEmptyQueueUnsafe() )
                            disposed = true;
                        else
                            stream.StreamProcessor._continuation.Reset();

                        break;
                    }

                    traceId = stream.GetTraceId();
                }

                using ( MessageBrokerStreamTraceEvent.CreateScope( stream, traceId, MessageBrokerStreamTraceEventType.ProcessMessages ) )
                {
                    MessageBrokerStreamProcessingMessagesEvent.Create( stream, traceId, channel, buffer.Count )
                        .Emit( stream.Logger.ProcessingMessages );

                    var listeners = channel.Listeners.GetAll();
                    foreach ( var listener in listeners )
                    {
                        try
                        {
                            listener.Queue.PushMessages( listener, in buffer, stream, traceId );
                        }
                        catch ( Exception exc )
                        {
                            MessageBrokerStreamErrorEvent.Create( stream, traceId, exc ).Emit( stream.Logger.Error );
                        }
                    }

                    using ( stream.AcquireLock() )
                        stream.DequeueMessagesUnsafe( buffer.Count );

                    ClearBuffer( stream, ref buffer, traceId );
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ClearBuffer(MessageBrokerStream stream, ref ListSlim<StreamMessage> messages, ulong traceId)
    {
        foreach ( ref readonly var message in messages )
        {
            MessageBrokerStreamMessageProcessedEvent.Create( message.Publisher, traceId, message.Id, message.Data.Length )
                .Emit( stream.Logger.MessageProcessed );

            var exc = message.Return();
            if ( exc is not null )
                MessageBrokerStreamErrorEvent.Create( stream, traceId, exc ).Emit( stream.Logger.Error );
        }

        messages.Clear();
    }
}
