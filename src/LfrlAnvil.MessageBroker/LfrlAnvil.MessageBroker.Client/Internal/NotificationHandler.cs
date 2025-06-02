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
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct NotificationHandler
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private QueueSlim<Notification> _notifications;
    private Task? _task;

    private NotificationHandler(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _notifications = QueueSlim<Notification>.Create();
        _task = null;
    }

    [Pure]
    internal static NotificationHandler Create()
    {
        return new NotificationHandler( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerClient client)
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
                client.NotificationHandler._task = null;
                traceId = client.GetTraceId();
            }

            using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.Unexpected ) )
            {
                MessageBrokerClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
                await client.DisposeAsync( traceId ).ConfigureAwait( false );
            }
        }

        Assume.IsGreaterThanOrEqualTo( client.State, MessageBrokerClientState.Disposing );
    }

    internal void BeginDispose()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( false );
    }

    internal (int DiscardedMessageCount, Chain<Exception> Exceptions) EndDispose()
    {
        var discardedMessageCount = _notifications.Count;
        var exceptions = Chain<Exception>.Empty;

        foreach ( ref readonly var message in _notifications )
        {
            if ( message.Header.GetClientEndpoint() == MessageBrokerClientEndpoint.SystemNotification )
                --discardedMessageCount;

            var exc = message.PoolToken.Return();
            if ( exc is not null )
                exceptions = exceptions.Extend( exc );
        }

        _notifications = QueueSlim<Notification>.Create();
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
    internal void Enqueue(Protocol.PacketHeader header, Timestamp receivedAt, MemoryPoolToken<byte> poolToken, ReadOnlyMemory<byte> data)
    {
        _notifications.Enqueue( new Notification( header, receivedAt, poolToken, data ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SignalContinuation()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask RunCore(MessageBrokerClient client)
    {
        bool reverseEndianness;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return;

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
        }

        while ( true )
        {
            var @continue = await client.NotificationHandler._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            while ( true )
            {
                ulong traceId;
                Notification notification;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return;

                    if ( ! client.NotificationHandler._notifications.TryDequeue( out notification ) )
                    {
                        client.NotificationHandler._continuation.Reset();
                        break;
                    }

                    traceId = client.GetTraceId();
                }

                if ( notification.Header.GetClientEndpoint() == MessageBrokerClientEndpoint.SystemNotification )
                {
                    try
                    {
                        using ( MessageBrokerClientTraceEvent.CreateScope(
                            client,
                            traceId,
                            MessageBrokerClientTraceEventType.SystemNotification ) )
                        {
                            MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, notification.Header )
                                .Emit( client.Logger.ReadPacket );

                            var exception = Protocol.AssertMinPayload(
                                client,
                                notification.Header,
                                Protocol.SystemNotificationHeader.Length );

                            if ( exception is not null )
                            {
                                MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                                await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                                return;
                            }

                            var requestHeader = Protocol.SystemNotificationHeader.Parse(
                                notification.Data.Slice( 0, Protocol.SystemNotificationHeader.Length ) );

                            MessageBrokerClientProcessingSystemNotificationEvent.Create( client, traceId, requestHeader.Type )
                                .Emit( client.Logger.ProcessingSystemNotification );

                            var valid = requestHeader.Type switch
                            {
                                MessageBrokerSystemNotificationType.SenderName => await HandleSenderNameNotificationAsync(
                                        client,
                                        notification,
                                        reverseEndianness,
                                        traceId )
                                    .ConfigureAwait( false ),
                                MessageBrokerSystemNotificationType.StreamName => await HandleStreamNameNotificationAsync(
                                        client,
                                        notification,
                                        reverseEndianness,
                                        traceId )
                                    .ConfigureAwait( false ),
                                _ => await HandleInvalidSystemNotificationAsync( client, notification.Header, requestHeader.Type, traceId )
                                    .ConfigureAwait( false )
                            };

                            if ( ! valid )
                                return;
                        }
                    }
                    finally
                    {
                        notification.PoolToken.Return( client, traceId );
                    }

                    continue;
                }

                Assume.Equals( notification.Header.GetClientEndpoint(), MessageBrokerClientEndpoint.MessageNotification );

                var failed = true;
                MessageBrokerClientTraceEvent.Create( client, traceId, MessageBrokerClientTraceEventType.MessageNotification )
                    .Emit( client.Logger.TraceStart );

                try
                {
                    MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, notification.Header )
                        .Emit( client.Logger.ReadPacket );

                    var exception = Protocol.AssertMinPayload( client, notification.Header, Protocol.MessageNotificationHeader.Length );
                    if ( exception is not null )
                    {
                        MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                        await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                        return;
                    }

                    var request = Protocol.MessageNotificationHeader.Parse(
                        notification.Data.Slice( 0, Protocol.MessageNotificationHeader.Length ),
                        reverseEndianness );

                    var errors = request.StringifyErrors();
                    if ( errors.Count > 0 )
                    {
                        var error = Protocol.ProtocolException( client, notification.Header, errors );
                        MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                        await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                        return;
                    }

                    var data = notification.Data.Slice( Protocol.MessageNotificationHeader.Length );
                    MessageBrokerClientProcessingMessageEvent.Create(
                            client,
                            traceId,
                            request.SenderId,
                            request.StreamId,
                            request.MessageId,
                            request.ChannelId,
                            request.RetryAttempt,
                            request.RedeliveryAttempt,
                            data.Length )
                        .Emit( client.Logger.ProcessingMessage );

                    MessageBrokerListener? listener;
                    var sender = new MessageBrokerExternalObject( request.SenderId );
                    var stream = new MessageBrokerExternalObject( request.StreamId );
                    using ( client.AcquireLock() )
                    {
                        listener = client.ListenerCollection.TryGetByChannelIdUnsafe( request.ChannelId );
                        if ( listener is not null )
                        {
                            sender = client.ExternalNameCache.GetSender( client, request.SenderId );
                            stream = client.ExternalNameCache.GetStream( request.StreamId );
                        }
                    }

                    if ( listener is null )
                    {
                        var error = new MessageBrokerClientMessageException(
                            client,
                            null,
                            Resources.ListenerDoesNotExist( request.ChannelId ) );

                        MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                        continue;
                    }

                    MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, notification.Header )
                        .Emit( client.Logger.ReadPacket );

                    bool cancel;
                    using ( listener.AcquireLock() )
                    {
                        cancel = listener.ShouldCancel;
                        if ( ! cancel )
                        {
                            MessageEmitter.Enqueue(
                                listener,
                                in request,
                                in sender,
                                in stream,
                                notification.ReceivedAt,
                                notification.PoolToken,
                                data,
                                traceId );

                            failed = false;
                            MessageEmitter.SignalContinuation( listener );
                        }
                    }

                    if ( cancel )
                    {
                        var error = new MessageBrokerClientMessageException(
                            client,
                            listener,
                            Resources.ListenerDoesNotExist( request.ChannelId ) );

                        MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                    }
                }
                finally
                {
                    if ( failed )
                    {
                        notification.PoolToken.Return( client, traceId );
                        MessageBrokerClientTraceEvent.Create( client, traceId, MessageBrokerClientTraceEventType.MessageNotification )
                            .Emit( client.Logger.TraceEnd );
                    }
                }
            }
        }
    }

    private static async ValueTask<bool> HandleSenderNameNotificationAsync(
        MessageBrokerClient client,
        Notification notification,
        bool reverseEndianness,
        ulong traceId)
    {
        if ( ! client.SynchronizeExternalObjectNames )
        {
            var error = Protocol.ProtocolException(
                client,
                notification.Header,
                Chain.Create( Resources.ExternalObjectNameSynchronizationIsDisabled ) );

            MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return false;
        }

        var exception = Protocol.AssertMinPayload(
            client,
            notification.Header,
            Protocol.SystemNotificationHeader.Length + Protocol.ObjectNameNotificationHeader.Length );

        if ( exception is not null )
        {
            MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return false;
        }

        var data = notification.Data.Slice( Protocol.SystemNotificationHeader.Length );
        var parsedRequest = Protocol.ObjectNameNotificationHeader.Parse(
            data.Slice( 0, Protocol.ObjectNameNotificationHeader.Length ),
            reverseEndianness );

        var requestErrors = parsedRequest.StringifySenderErrors( client.Id );
        if ( requestErrors.Count > 0 )
        {
            var error = Protocol.ProtocolException( client, notification.Header, requestErrors );
            MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return false;
        }

        var name = TextEncoding.Parse( data.Slice( Protocol.ObjectNameNotificationHeader.Length ) );
        if ( name.Exception is not null )
        {
            MessageBrokerClientErrorEvent.Create( client, traceId, name.Exception ).Emit( client.Logger.Error );
            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return false;
        }

        Assume.IsNotNull( name.Value );
        if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
        {
            var error = Protocol.InvalidSenderNameLengthException( client, notification.Header, name.Value.Length );
            MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return false;
        }

        string? prev;
        using ( client.AcquireActiveLock( traceId, out var exc ) )
        {
            if ( exc is not null )
                return false;

            prev = client.ExternalNameCache.SetSender( parsedRequest.Id, name.Value );
        }

        MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, notification.Header ).Emit( client.Logger.ReadPacket );
        MessageBrokerClientSenderNameProcessedEvent.Create( client, traceId, parsedRequest.Id, prev, name.Value )
            .Emit( client.Logger.SenderNameProcessed );

        return true;
    }

    private static async ValueTask<bool> HandleStreamNameNotificationAsync(
        MessageBrokerClient client,
        Notification notification,
        bool reverseEndianness,
        ulong traceId)
    {
        if ( ! client.SynchronizeExternalObjectNames )
        {
            var error = Protocol.ProtocolException(
                client,
                notification.Header,
                Chain.Create( Resources.ExternalObjectNameSynchronizationIsDisabled ) );

            MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return false;
        }

        var exception = Protocol.AssertMinPayload(
            client,
            notification.Header,
            Protocol.SystemNotificationHeader.Length + Protocol.ObjectNameNotificationHeader.Length );

        if ( exception is not null )
        {
            MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return false;
        }

        var data = notification.Data.Slice( Protocol.SystemNotificationHeader.Length );
        var parsedRequest = Protocol.ObjectNameNotificationHeader.Parse(
            data.Slice( 0, Protocol.ObjectNameNotificationHeader.Length ),
            reverseEndianness );

        var requestErrors = parsedRequest.StringifyStreamErrors();
        if ( requestErrors.Count > 0 )
        {
            var error = Protocol.ProtocolException( client, notification.Header, requestErrors );
            MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return false;
        }

        var name = TextEncoding.Parse( data.Slice( Protocol.ObjectNameNotificationHeader.Length ) );
        if ( name.Exception is not null )
        {
            MessageBrokerClientErrorEvent.Create( client, traceId, name.Exception ).Emit( client.Logger.Error );
            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return false;
        }

        Assume.IsNotNull( name.Value );
        if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
        {
            var error = Protocol.InvalidStreamNameLengthException( client, notification.Header, name.Value.Length );
            MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return false;
        }

        string? prev;
        using ( client.AcquireActiveLock( traceId, out var exc ) )
        {
            if ( exc is not null )
                return false;

            prev = client.ExternalNameCache.SetStream( parsedRequest.Id, name.Value );
        }

        MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, notification.Header ).Emit( client.Logger.ReadPacket );
        MessageBrokerClientStreamNameProcessedEvent.Create( client, traceId, parsedRequest.Id, prev, name.Value )
            .Emit( client.Logger.StreamNameProcessed );

        return true;
    }

    private static async ValueTask<bool> HandleInvalidSystemNotificationAsync(
        MessageBrokerClient client,
        Protocol.PacketHeader header,
        MessageBrokerSystemNotificationType type,
        ulong traceId)
    {
        var error = Protocol.ProtocolException( client, header, Chain.Create( Resources.UnexpectedSystemNotificationType( type ) ) );
        MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
        await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ValueTask DisposeClientAsync(MessageBrokerClient client, ulong traceId)
    {
        using ( client.AcquireLock() )
            client.NotificationHandler._task = null;

        return client.DisposeAsync( traceId );
    }

    private readonly struct Notification
    {
        internal Notification(
            Protocol.PacketHeader header,
            Timestamp receivedAt,
            MemoryPoolToken<byte> poolToken,
            ReadOnlyMemory<byte> data)
        {
            Header = header;
            ReceivedAt = receivedAt;
            PoolToken = poolToken;
            Data = data;
        }

        internal readonly Protocol.PacketHeader Header;
        internal readonly Timestamp ReceivedAt;
        internal readonly MemoryPoolToken<byte> PoolToken;
        internal readonly ReadOnlyMemory<byte> Data;

        [Pure]
        public override string ToString()
        {
            return $"Header = ({Header}), ReceivedAt = {ReceivedAt}, Length = {Data.Length}";
        }
    }
}
