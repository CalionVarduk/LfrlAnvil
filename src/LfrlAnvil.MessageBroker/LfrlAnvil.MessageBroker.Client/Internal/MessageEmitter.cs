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
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Internal;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct MessageEmitter
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private QueueSlim<Notification> _messages;
    private Task? _task;

    private MessageEmitter(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _messages = QueueSlim<Notification>.Create();
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
                    var error = listener.Client.Logger.Error;
                    error?.Emit( MessageBrokerClientErrorEvent.Create( listener.Client, traceId, exc ) );

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
                        error?.Emit( MessageBrokerClientErrorEvent.Create( listener.Client, traceId, exc2 ) );
                    }
                }
            }
        }

        Assume.IsGreaterThanOrEqualTo( listener.State, MessageBrokerListenerState.Disposing );
    }

    internal void BeginDispose(ref Chain<Exception> exceptions)
    {
        try
        {
            _continuation.TrySetResult( false );
        }
        catch ( Exception exc )
        {
            exceptions = exceptions.Extend( exc );
        }
    }

    internal DiscardedNotification[] EndDispose()
    {
        var i = 0;
        var result = new DiscardedNotification[_messages.Count];
        foreach ( ref readonly var message in _messages )
            result[i++] = new DiscardedNotification( message.PoolToken, message.Args.TraceId );

        _messages = QueueSlim<Notification>.Create();
        return result;
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
        listener.MessageEmitter._continuation.TrySetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void Enqueue(
        MessageBrokerListener listener,
        in Protocol.MessageNotificationHeader request,
        in MessageBrokerExternalObject sender,
        in MessageBrokerExternalObject stream,
        Timestamp receivedAt,
        MemoryPoolToken<byte> poolToken,
        ReadOnlyMemory<byte> data,
        ulong traceId)
    {
        listener.MessageEmitter._messages.Enqueue(
            new Notification(
                listener,
                request.QueueId,
                request.AckId,
                request.MessageId,
                request.PushedAt,
                receivedAt,
                sender,
                stream,
                request.Retry,
                request.Redelivery,
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
                Notification notification;
                using ( listener.AcquireLock() )
                {
                    if ( listener.ShouldCancel )
                        return;

                    if ( ! listener.MessageEmitter._messages.TryDequeue( out notification ) )
                    {
                        listener.MessageEmitter._continuation.Reset();
                        break;
                    }
                }

                Assume.Equals( listener, notification.Args.Listener );
                var cancellationToken = listener.CancellationSource.Token;
                try
                {
                    await listener.Callback( notification.Args, cancellationToken ).ConfigureAwait( false );
                }
                catch ( Exception exc )
                {
                    if ( listener.Client.Logger.Error is { } error )
                        error.Emit( MessageBrokerClientErrorEvent.Create( listener.Client, notification.Args.TraceId, exc ) );

                    if ( (exc is not OperationCanceledException cancelExc || cancelExc.CancellationToken != cancellationToken)
                        && notification.Args.AckId > 0 )
                    {
                        await ListenerCollection.SendNegativeMessageAckAsync(
                                listener,
                                notification.Args.QueueId,
                                notification.Args.AckId,
                                notification.Args.Stream.Id,
                                notification.Args.MessageId,
                                notification.Args.Retry,
                                notification.Args.Redelivery,
                                notification.Args.TraceId,
                                MessageBrokerNegativeAck.Default,
                                true )
                            .ConfigureAwait( false );
                    }
                }
                finally
                {
                    if ( listener.Client.Logger.MessageProcessed is { } messageProcessed )
                        messageProcessed.Emit(
                            MessageBrokerClientMessageProcessedEvent.Create(
                                listener,
                                notification.Args.TraceId,
                                notification.Args.QueueId,
                                notification.Args.Stream.Id,
                                notification.Args.MessageId,
                                notification.Args.Retry,
                                notification.Args.Redelivery ) );

                    notification.PoolToken.Return( listener.Client, notification.Args.TraceId );
                    if ( listener.Client.Logger.TraceEnd is { } traceEnd )
                        traceEnd.Emit(
                            MessageBrokerClientTraceEvent.Create(
                                listener.Client,
                                notification.Args.TraceId,
                                MessageBrokerClientTraceEventType.MessageNotification ) );
                }
            }
        }
    }

    internal readonly struct DiscardedNotification
    {
        internal readonly MemoryPoolToken<byte> PoolToken;
        internal readonly ulong TraceId;

        internal DiscardedNotification(MemoryPoolToken<byte> poolToken, ulong traceId)
        {
            PoolToken = poolToken;
            TraceId = traceId;
        }
    }

    private readonly struct Notification
    {
        internal Notification(
            MessageBrokerListener listener,
            int queueId,
            int ackId,
            ulong messageId,
            Timestamp pushedAt,
            Timestamp receivedAt,
            MessageBrokerExternalObject sender,
            MessageBrokerExternalObject stream,
            Int31BoolPair retry,
            Int31BoolPair redelivery,
            ReadOnlyMemory<byte> data,
            MemoryPoolToken<byte> poolToken,
            ulong traceId)
        {
            Args = new MessageBrokerListenerCallbackArgs(
                listener,
                queueId,
                ackId,
                messageId,
                pushedAt,
                receivedAt,
                sender,
                stream,
                retry,
                redelivery,
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
