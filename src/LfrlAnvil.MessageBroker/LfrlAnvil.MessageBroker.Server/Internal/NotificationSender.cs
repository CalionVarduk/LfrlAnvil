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
    private QueueSlim<Message> _messages;
    private Task? _task;

    private NotificationSender(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _messages = QueueSlim<Message>.Create();
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

    internal (int DiscardedMessageCount, Chain<Exception> Exceptions) EndDispose(bool extractExceptions)
    {
        var discardedMessageCount = _messages.Count;
        var exceptions = Chain<Exception>.Empty;

        foreach ( ref readonly var message in _messages )
        {
            var exc = message.PoolToken.Return();
            if ( exc is not null && extractExceptions )
                exceptions = exceptions.Extend( exc );
        }

        _messages.Clear();
        return (discardedMessageCount, exceptions);
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
        var senderName = NameNotification.Empty;
        var streamName = NameNotification.Empty;
        if ( client.SynchronizeExternalObjectNames )
        {
            var sender = message.Publisher.Client;
            if ( client.ExternalNameCache.TryUpdate( client, sender ) )
                senderName = new NameNotification( client.WriterQueue.AcquireSource(), sender.Id, sender.Name );

            var stream = message.Publisher.Stream;
            if ( client.ExternalNameCache.TryUpdate( stream ) )
                streamName = new NameNotification( client.WriterQueue.AcquireSource(), stream.Id, stream.Name );
        }

        var writerSource = client.WriterQueue.AcquireSource();
        client.NotificationSender._messages.Enqueue(
            new Message( in message, retry, redelivery, id, ackId, poolToken, packet, writerSource, senderName, streamName ) );
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
                Message message;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return;

                    if ( client.NotificationSender._messages.IsEmpty )
                    {
                        client.NotificationSender._continuation.Reset();
                        break;
                    }

                    message = client.NotificationSender._messages.First();
                    traceId = client.GetTraceId();
                }

                Assume.Equals( message.Listener.Client, client );
                using ( MessageBrokerRemoteClientTraceEvent.CreateScope(
                    client,
                    traceId,
                    MessageBrokerRemoteClientTraceEventType.MessageNotification ) )
                {
                    if ( client.Logger.ProcessingMessage is { } processingMessage )
                        processingMessage.Emit(
                            MessageBrokerRemoteClientProcessingMessageEvent.Create(
                                message.Listener,
                                traceId,
                                message.Publisher,
                                message.MessageId,
                                message.AckId,
                                message.Retry,
                                message.Redelivery,
                                message.Length ) );

                    try
                    {
                        if ( message.SenderName.WriterSource is not null )
                        {
                            if ( client.Logger.SendingSenderName is { } sendingSenderName )
                                sendingSenderName.Emit(
                                    MessageBrokerRemoteClientSendingSenderNameEvent.Create(
                                        client,
                                        traceId,
                                        message.SenderName.Id,
                                        message.SenderName.Name ) );

                            if ( ! await SendObjectNameNotificationAsync(
                                    client,
                                    MessageBrokerSystemNotificationType.SenderName,
                                    message.SenderName,
                                    traceId )
                                .ConfigureAwait( false ) )
                                return;
                        }

                        if ( message.StreamName.WriterSource is not null )
                        {
                            if ( client.Logger.SendingStreamName is { } sendingStreamName )
                                sendingStreamName.Emit(
                                    MessageBrokerRemoteClientSendingStreamNameEvent.Create(
                                        client,
                                        traceId,
                                        message.StreamName.Id,
                                        message.StreamName.Name ) );

                            if ( ! await SendObjectNameNotificationAsync(
                                    client,
                                    MessageBrokerSystemNotificationType.StreamName,
                                    message.StreamName,
                                    traceId )
                                .ConfigureAwait( false ) )
                                return;
                        }

                        if ( ! await message.WriterSource.GetTask().ConfigureAwait( false ) )
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ) );

                            return;
                        }

                        var writeResult = await client.WriteAsync( message.PacketHeader, message.Packet, traceId )
                            .ConfigureAwait( false );

                        if ( writeResult.Exception is not null )
                        {
                            using ( client.AcquireLock() )
                                client.NotificationSender._task = null;

                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return;
                        }

                        if ( client.Logger.MessageProcessed is { } messageProcessed )
                            messageProcessed.Emit(
                                MessageBrokerRemoteClientMessageProcessedEvent.Create(
                                    message.Listener,
                                    traceId,
                                    message.Publisher,
                                    message.MessageId,
                                    message.AckId,
                                    message.Retry,
                                    message.Redelivery ) );

                        if ( message.AckId <= 0 && message.Listener.DecrementPrefetchCounter() )
                        {
                            using ( message.Listener.Queue.AcquireLock() )
                            {
                                if ( ! message.Listener.Queue.ShouldCancel && ! message.Listener.Queue.MessageStore.IsEmpty )
                                    message.Listener.Queue.QueueProcessor.SignalContinuation();
                            }
                        }

                        using ( client.AcquireActiveLock( traceId, out var exc ) )
                        {
                            if ( exc is not null )
                                return;

                            client.NotificationSender._messages.Dequeue();
                            client.WriterQueue.Release( client, message.WriterSource );
                        }
                    }
                    finally
                    {
                        message.PoolToken.Return( client, traceId );
                    }
                }
            }
        }
    }

    private static async ValueTask<bool> SendObjectNameNotificationAsync(
        MessageBrokerRemoteClient client,
        MessageBrokerSystemNotificationType type,
        NameNotification data,
        ulong traceId)
    {
        Assume.IsNotNull( data.WriterSource );
        var notification = new Protocol.ObjectNameNotification( type, data.Id, data.Name );
        var token = client.MemoryPool.Rent( notification.Length, out var buffer ).EnableClearing();
        try
        {
            notification.Serialize( buffer );
            if ( ! await data.WriterSource.GetTask().ConfigureAwait( false ) )
            {
                if ( client.Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ) );

                return false;
            }

            var writeResult = await client.WriteAsync( notification.Header, buffer, traceId ).ConfigureAwait( false );
            if ( writeResult.Exception is not null )
            {
                using ( client.AcquireLock() )
                    client.NotificationSender._task = null;

                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                return false;
            }

            if ( client.Logger.SystemNotificationSent is { } systemNotificationSent )
                systemNotificationSent.Emit( MessageBrokerRemoteClientSystemNotificationSentEvent.Create( client, traceId, type ) );

            using ( client.AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return false;

                client.WriterQueue.Release( client, data.WriterSource );
            }
        }
        finally
        {
            token.Return( client, traceId );
        }

        return true;
    }

    private readonly struct Message
    {
        internal Message(
            in QueueMessage message,
            Int31BoolPair retry,
            Int31BoolPair redelivery,
            ulong messageId,
            int ackId,
            MemoryPoolToken<byte> poolToken,
            ReadOnlyMemory<byte> packet,
            ManualResetValueTaskSource<bool> writerSource,
            NameNotification senderName,
            NameNotification streamName)
        {
            Publisher = message.Publisher;
            Listener = message.Listener;
            MessageId = messageId;
            Retry = retry;
            Redelivery = redelivery;
            AckId = ackId;
            WriterSource = writerSource;
            SenderName = senderName;
            StreamName = streamName;
            PoolToken = poolToken;
            Packet = packet;
        }

        internal readonly MessageBrokerChannelPublisherBinding Publisher;
        internal readonly MessageBrokerChannelListenerBinding Listener;
        internal readonly ulong MessageId;
        internal readonly Int31BoolPair Retry;
        internal readonly Int31BoolPair Redelivery;
        internal readonly int AckId;
        internal readonly ManualResetValueTaskSource<bool> WriterSource;
        internal readonly NameNotification SenderName;
        internal readonly NameNotification StreamName;
        internal readonly MemoryPoolToken<byte> PoolToken;
        internal readonly ReadOnlyMemory<byte> Packet;

        internal Protocol.PacketHeader PacketHeader => Protocol.PacketHeader.Create(
            MessageBrokerClientEndpoint.MessageNotification,
            ( uint )Packet.Length - Protocol.PacketHeader.Length );

        internal int Length => unchecked( Packet.Length - Protocol.PacketHeader.Length - Protocol.MessageNotificationHeader.Payload );

        [Pure]
        public override string ToString()
        {
            return
                $"Header = ({PacketHeader}), Publisher = ({Publisher}), Listener = ({Listener}), MessageId = {MessageId}, AckId = {AckId}, Retry = ({Retry}), Redelivery = ({Redelivery})";
        }
    }

    private readonly struct NameNotification
    {
        internal static NameNotification Empty => new NameNotification( null, 0, string.Empty );

        internal NameNotification(ManualResetValueTaskSource<bool>? writerSource, int id, string name)
        {
            WriterSource = writerSource;
            Id = id;
            Name = name;
        }

        internal readonly ManualResetValueTaskSource<bool>? WriterSource;
        internal readonly int Id;
        internal readonly string Name;

        [Pure]
        public override string ToString()
        {
            return $"Id = {Id}, Name = '{Name}'";
        }
    }
}
