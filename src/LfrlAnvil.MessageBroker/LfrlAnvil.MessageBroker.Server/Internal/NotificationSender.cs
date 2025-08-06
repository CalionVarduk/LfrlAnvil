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
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct NotificationSender
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private QueueSlim<Notification> _notifications;
    private Task? _task;

    private NotificationSender(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _notifications = QueueSlim<Notification>.Create();
        _task = null;
    }

    [Pure]
    internal static NotificationSender Create()
    {
        return new NotificationSender( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerRemoteClient client)
    {
        try
        {
            await RunCore( client ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            ulong traceId;
            using ( client.AcquireLock() )
            {
                client.NotificationSender._task = null;
                traceId = client.GetTraceId();
            }

            using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Unexpected ) )
            {
                if ( client.Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );

                await client.DisposeAsync( traceId ).ConfigureAwait( false );
            }
        }

        Assume.IsGreaterThanOrEqualTo( client.State, MessageBrokerRemoteClientState.Disposing );
    }

    internal void BeginDispose()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( false );
    }

    internal (int DiscardedNotificationCount, Chain<Exception> Exceptions) EndDispose(bool extractExceptions)
    {
        var discardedNotificationCount = _notifications.Count;
        var exceptions = Chain<Exception>.Empty;

        foreach ( ref readonly var notification in _notifications )
        {
            var exc = notification.PoolToken.Return();
            if ( exc is not null && extractExceptions )
                exceptions = exceptions.Extend( exc );
        }

        _notifications = QueueSlim<Notification>.Create();
        return (discardedNotificationCount, exceptions);
    }

    internal void SetUnderlyingTask(Task? task)
    {
        _task = task;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
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
    internal static bool TryEnqueueSenderNameUnsafe(
        MessageBrokerRemoteClient client,
        in QueueMessage message,
        ref MemoryPoolToken<byte> poolToken,
        ReadOnlyMemory<byte> packet)
    {
        var sender = message.Publisher.Client;
        if ( ! client.ExternalNameCache.TryUpdate( client, sender ) )
            return false;

        var writerSource = client.WriterQueue.AcquireSource( packet, client.ClearBuffers );
        client.NotificationSender._notifications.Enqueue(
            new Notification( in message, default, default, default, default, NotificationType.SenderName, writerSource, poolToken ) );

        poolToken = MemoryPoolToken<byte>.Empty;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool TryEnqueueStreamNameUnsafe(
        MessageBrokerRemoteClient client,
        in QueueMessage message,
        ref MemoryPoolToken<byte> poolToken,
        ReadOnlyMemory<byte> packet)
    {
        var stream = message.Publisher.Stream;
        if ( ! client.ExternalNameCache.TryUpdate( stream ) )
            return false;

        var writerSource = client.WriterQueue.AcquireSource( packet, client.ClearBuffers );
        client.NotificationSender._notifications.Enqueue(
            new Notification( in message, default, default, default, default, NotificationType.StreamName, writerSource, poolToken ) );

        poolToken = MemoryPoolToken<byte>.Empty;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void EnqueueMessageUnsafe(
        MessageBrokerRemoteClient client,
        in QueueMessage message,
        Int31BoolPair retry,
        Int31BoolPair redelivery,
        ulong id,
        int ackId,
        MemoryPoolToken<byte> poolToken,
        ReadOnlyMemory<byte> packet)
    {
        var writerSource = client.WriterQueue.AcquireSource( packet, poolToken.Clear );
        client.NotificationSender._notifications.Enqueue(
            new Notification( in message, retry, redelivery, id, ackId, NotificationType.Message, writerSource, poolToken ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask RunCore(MessageBrokerRemoteClient client)
    {
        while ( true )
        {
            var @continue = await client.NotificationSender._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            while ( true )
            {
                ulong traceId;
                Notification notification;
                ReadOnlyMemory<byte> data;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return;

                    if ( client.NotificationSender._notifications.IsEmpty )
                    {
                        client.NotificationSender._continuation.Reset();
                        break;
                    }

                    notification = client.NotificationSender._notifications.First();
                    data = notification.WriterSource.Data;
                    traceId = client.GetTraceId();
                }

                Assume.Equals( notification.Listener.Client, client );
                if ( notification.Type == NotificationType.Message )
                {
                    using ( MessageBrokerRemoteClientTraceEvent.CreateScope(
                        client,
                        traceId,
                        MessageBrokerRemoteClientTraceEventType.MessageNotification ) )
                    {
                        try
                        {
                            if ( client.Logger.ProcessingMessage is { } processingMessage )
                                processingMessage.Emit(
                                    MessageBrokerRemoteClientProcessingMessageEvent.Create(
                                        notification.Listener,
                                        traceId,
                                        notification.Publisher,
                                        notification.MessageId,
                                        notification.AckId,
                                        notification.Retry,
                                        notification.Redelivery,
                                        GetMessageLength( data ) ) );

                            var packetHeader = GetPacketHeader( data, MessageBrokerClientEndpoint.MessageNotification );
                            var writerResult = await notification.WriterSource.GetTask().ConfigureAwait( false );
                            switch ( writerResult.Status )
                            {
                                case WriterSourceResultStatus.Ready:
                                {
                                    var (packetCount, exception) = await client
                                        .WritePotentialBatchAsync( packetHeader, data, traceId )
                                        .ConfigureAwait( false );

                                    if ( exception is not null )
                                    {
                                        await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                                        return;
                                    }

                                    if ( ! ReleaseWriter( client, notification.WriterSource, packetCount, traceId ) )
                                        return;

                                    break;
                                }
                                case WriterSourceResultStatus.Batched:
                                {
                                    if ( ! ReleaseBatchedWriter( client, notification.WriterSource, packetHeader, writerResult, traceId ) )
                                        return;

                                    break;
                                }
                                default:
                                {
                                    HandleDisposedWriter( client, traceId );
                                    return;
                                }
                            }

                            if ( notification.AckId <= 0 && notification.Listener.DecrementPrefetchCounter() )
                            {
                                using ( notification.Listener.Queue.AcquireLock() )
                                {
                                    if ( ! notification.Listener.Queue.ShouldCancel && ! notification.Listener.Queue.MessageStore.IsEmpty )
                                        notification.Listener.Queue.QueueProcessor.SignalContinuation();
                                }
                            }

                            if ( client.Logger.MessageProcessed is { } messageProcessed )
                                messageProcessed.Emit(
                                    MessageBrokerRemoteClientMessageProcessedEvent.Create(
                                        notification.Listener,
                                        traceId,
                                        notification.Publisher,
                                        notification.MessageId,
                                        notification.AckId,
                                        notification.Retry,
                                        notification.Redelivery ) );
                        }
                        finally
                        {
                            notification.PoolToken.Return( client, traceId );
                        }
                    }
                }
                else
                {
                    using ( MessageBrokerRemoteClientTraceEvent.CreateScope(
                        client,
                        traceId,
                        MessageBrokerRemoteClientTraceEventType.SystemNotification ) )
                    {
                        try
                        {
                            MessageBrokerSystemNotificationType type;
                            if ( notification.Type == NotificationType.SenderName )
                            {
                                type = MessageBrokerSystemNotificationType.SenderName;
                                if ( client.Logger.SendingSenderName is { } sendingSenderName )
                                    sendingSenderName.Emit(
                                        MessageBrokerRemoteClientSendingSenderNameEvent.Create(
                                            client,
                                            traceId,
                                            notification.Publisher.Client ) );
                            }
                            else
                            {
                                type = MessageBrokerSystemNotificationType.StreamName;
                                if ( client.Logger.SendingStreamName is { } sendingStreamName )
                                    sendingStreamName.Emit(
                                        MessageBrokerRemoteClientSendingStreamNameEvent.Create(
                                            client,
                                            traceId,
                                            notification.Publisher.Stream ) );
                            }

                            var packetHeader = GetPacketHeader( data, MessageBrokerClientEndpoint.SystemNotification );
                            var writerResult = await notification.WriterSource.GetTask().ConfigureAwait( false );
                            switch ( writerResult.Status )
                            {
                                case WriterSourceResultStatus.Ready:
                                {
                                    var (packetCount, exception) = await client
                                        .WritePotentialBatchAsync( packetHeader, data, traceId )
                                        .ConfigureAwait( false );

                                    if ( exception is not null )
                                    {
                                        await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                                        return;
                                    }

                                    if ( ! ReleaseWriter( client, notification.WriterSource, packetCount, traceId ) )
                                        return;

                                    break;
                                }
                                case WriterSourceResultStatus.Batched:
                                {
                                    if ( ! ReleaseBatchedWriter( client, notification.WriterSource, packetHeader, writerResult, traceId ) )
                                        return;

                                    break;
                                }
                                default:
                                {
                                    HandleDisposedWriter( client, traceId );
                                    return;
                                }
                            }

                            if ( client.Logger.SystemNotificationSent is { } systemNotificationSent )
                                systemNotificationSent.Emit(
                                    MessageBrokerRemoteClientSystemNotificationSentEvent.Create( client, traceId, type ) );
                        }
                        finally
                        {
                            notification.PoolToken.Return( client, traceId );
                        }
                    }
                }
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool ReleaseWriter(MessageBrokerRemoteClient client, WriterQueue.TaskSource writer, int packetCount, ulong traceId)
    {
        using ( client.AcquireActiveLock( traceId, out var exc ) )
        {
            if ( exc is not null )
                return false;

            client.NotificationSender._notifications.Dequeue();
            if ( packetCount > 1 )
                client.WriterQueue.ReleaseBatched( client, writer, packetCount, traceId );
            else
                client.WriterQueue.Release( client, writer );
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool ReleaseBatchedWriter(
        MessageBrokerRemoteClient client,
        WriterQueue.TaskSource writer,
        Protocol.PacketHeader header,
        WriterSourceResult writerResult,
        ulong traceId)
    {
        Assume.IsGreaterThan( client.MaxBatchPacketCount, 1 );
        Assume.Equals( writerResult.Status, WriterSourceResultStatus.Batched );

        if ( client.Logger.SendPacket is { } sendPacket )
            sendPacket.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateBatched( client, traceId, header, writerResult.BatchTraceId ) );

        using ( client.AcquireActiveLock( traceId, out var exc ) )
        {
            if ( exc is not null )
                return false;

            client.NotificationSender._notifications.Dequeue();
            client.WriterQueue.ReleaseBatched( client, writer, writerResult );
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void HandleDisposedWriter(MessageBrokerRemoteClient client, ulong traceId)
    {
        if ( client.Logger.Error is { } error )
            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ValueTask DisposeClientAsync(MessageBrokerRemoteClient client, ulong traceId)
    {
        using ( client.AcquireLock() )
            client.NotificationSender._task = null;

        return client.DisposeAsync( traceId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Protocol.PacketHeader GetPacketHeader(ReadOnlyMemory<byte> data, MessageBrokerClientEndpoint endpoint)
    {
        return Protocol.PacketHeader.Create( endpoint, unchecked( ( uint )(data.Length - Protocol.PacketHeader.Length) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int GetMessageLength(ReadOnlyMemory<byte> data)
    {
        return unchecked( data.Length - Protocol.PacketHeader.Length - Protocol.MessageNotificationHeader.Payload );
    }

    private enum NotificationType : byte
    {
        Message = 0,
        SenderName = 1,
        StreamName = 2
    }

    private readonly struct Notification
    {
        internal Notification(
            in QueueMessage message,
            Int31BoolPair retry,
            Int31BoolPair redelivery,
            ulong messageId,
            int ackId,
            NotificationType type,
            WriterQueue.TaskSource writerSource,
            MemoryPoolToken<byte> poolToken)
        {
            Publisher = message.Publisher;
            Listener = message.Listener;
            MessageId = messageId;
            Retry = retry;
            Redelivery = redelivery;
            AckId = ackId;
            Type = type;
            PoolToken = poolToken;
            WriterSource = writerSource;
        }

        internal readonly MessageBrokerChannelPublisherBinding Publisher;
        internal readonly MessageBrokerChannelListenerBinding Listener;
        internal readonly ulong MessageId;
        internal readonly Int31BoolPair Retry;
        internal readonly Int31BoolPair Redelivery;
        internal readonly int AckId;
        internal readonly NotificationType Type;
        internal readonly WriterQueue.TaskSource WriterSource;
        internal readonly MemoryPoolToken<byte> PoolToken;

        [Pure]
        public override string ToString()
        {
            return
                $"[{Type}] Publisher = ({Publisher}), Listener = ({Listener}), MessageId = {MessageId}, AckId = {AckId}, Retry = ({Retry}), Redelivery = ({Redelivery})";
        }
    }
}
