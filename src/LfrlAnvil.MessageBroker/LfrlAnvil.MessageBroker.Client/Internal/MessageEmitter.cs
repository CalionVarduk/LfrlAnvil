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
using LfrlAnvil.Chrono;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct MessageEmitter
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private QueueSlim<Message> _messages;
    private Task? _task;

    private MessageEmitter(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _messages = QueueSlim<Message>.Create();
    }

    [Pure]
    internal static MessageEmitter Create()
    {
        return new MessageEmitter( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerListener listener)
    {
        while ( true )
        {
            try
            {
                await RunCore( listener ).ConfigureAwait( false );
                break;
            }
            catch ( Exception exc )
            {
                ulong traceId;
                using ( listener.Client.AcquireLock() )
                    traceId = listener.Client.GetTraceId();

                using ( MessageBrokerClientTraceEvent.CreateScope(
                    listener.Client,
                    traceId,
                    MessageBrokerClientTraceEventType.Unexpected ) )
                {
                    MessageBrokerClientErrorEvent.Create( listener.Client, traceId, exc ).Emit( listener.Client.Logger.Error );
                    try
                    {
                        using ( listener.AcquireLock() )
                        {
                            if ( listener.ShouldCancel )
                                break;

                            SignalContinuation( listener );
                        }

                        await Task.Delay( TimeSpan.FromSeconds( 1 ) ).ConfigureAwait( false );
                    }
                    catch ( Exception exc2 )
                    {
                        MessageBrokerClientErrorEvent.Create( listener.Client, traceId, exc2 ).Emit( listener.Client.Logger.Error );
                    }
                }
            }
        }

        Assume.IsGreaterThanOrEqualTo( listener.State, MessageBrokerListenerState.Disposing );
    }

    internal (int DiscardedMessageCount, Chain<Exception> Exceptions) Dispose()
    {
        var discardedMessageCount = _messages.Count;
        var exceptions = Chain<Exception>.Empty;

        foreach ( ref readonly var message in _messages )
        {
            var exc = message.PoolToken.Return();
            if ( exc is not null )
                exceptions = exceptions.Extend( exc );
        }

        _messages.Clear();

        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( false );

        return (discardedMessageCount, exceptions);
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
    internal static void SignalContinuation(MessageBrokerListener listener)
    {
        if ( listener.MessageEmitter._continuation.Status == ValueTaskSourceStatus.Pending )
            listener.MessageEmitter._continuation.SetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void Enqueue(
        MessageBrokerListener listener,
        in Protocol.MessageNotificationHeader request,
        Timestamp receivedAt,
        MemoryPoolToken<byte> poolToken,
        ReadOnlyMemory<byte> data,
        ulong traceId)
    {
        listener.MessageEmitter._messages.Enqueue(
            new Message(
                listener,
                request.MessageId,
                request.EnqueuedAt,
                receivedAt,
                request.SenderId,
                request.StreamId,
                request.RetryAttempt,
                request.RedeliveryAttempt,
                data,
                poolToken,
                traceId ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask RunCore(MessageBrokerListener listener)
    {
        while ( true )
        {
            var @continue = await listener.MessageEmitter._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            while ( true )
            {
                Message message;
                using ( listener.AcquireLock() )
                {
                    if ( listener.ShouldCancel )
                        return;

                    if ( ! listener.MessageEmitter._messages.TryDequeue( out message ) )
                    {
                        listener.MessageEmitter._continuation.Reset();
                        break;
                    }
                }

                Assume.Equals( listener, message.Args.Listener );
                var cancellationToken = listener.CancellationSource.Token;
                try
                {
                    await listener.Callback( message.Args, cancellationToken ).ConfigureAwait( false );
                }
                catch ( Exception exc )
                {
                    MessageBrokerClientErrorEvent.Create( listener.Client, message.Args.TraceId, exc ).Emit( listener.Client.Logger.Error );
                    if ( exc is not OperationCanceledException cancelExc || cancelExc.CancellationToken != cancellationToken )
                    {
                        // TODO: send NACK to server
                    }
                }
                finally
                {
                    MessageBrokerClientMessageProcessedEvent.Create(
                            listener,
                            message.Args.TraceId,
                            message.Args.MessageId,
                            message.Args.Data.Length )
                        .Emit( listener.Client.Logger.MessageProcessed );

                    message.PoolToken.Return( listener.Client, message.Args.TraceId );
                    MessageBrokerClientTraceEvent
                        .Create( listener.Client, message.Args.TraceId, MessageBrokerClientTraceEventType.MessageNotification )
                        .Emit( listener.Client.Logger.TraceEnd );
                }
            }
        }
    }

    private readonly struct Message
    {
        internal Message(
            MessageBrokerListener listener,
            ulong messageId,
            Timestamp enqueuedAt,
            Timestamp receivedAt,
            int senderId,
            int streamId,
            int retryAttempt,
            int redeliveryAttempt,
            ReadOnlyMemory<byte> data,
            MemoryPoolToken<byte> poolToken,
            ulong traceId)
        {
            Args = new MessageBrokerListenerCallbackArgs(
                listener,
                messageId,
                enqueuedAt,
                receivedAt,
                senderId,
                streamId,
                retryAttempt,
                redeliveryAttempt,
                data,
                traceId );

            PoolToken = poolToken;
        }

        internal readonly MessageBrokerListenerCallbackArgs Args;
        internal readonly MemoryPoolToken<byte> PoolToken;

        [Pure]
        public override string ToString()
        {
            return $"Args = ({Args})";
        }
    }
}
