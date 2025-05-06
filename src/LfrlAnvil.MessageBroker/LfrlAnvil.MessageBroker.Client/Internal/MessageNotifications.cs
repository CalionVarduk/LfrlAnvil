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

internal struct MessageNotifications
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private QueueSlim<Message> _messages;
    private Task? _task;

    private MessageNotifications(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _messages = QueueSlim<Message>.Create();
        _task = null;
    }

    [Pure]
    internal static MessageNotifications Create()
    {
        return new MessageNotifications( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerClient client)
    {
        TaskStopReason stopReason;
        try
        {
            stopReason = await RunCore( client ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exc ) );
            stopReason = TaskStopReason.Error;
        }

        if ( stopReason == TaskStopReason.OwnerDisposed )
            return;

        using ( client.AcquireLock() )
            client.MessageNotifications._task = null;

        await client.DisposeAsync().ConfigureAwait( false );
    }

    internal void Dispose(MessageBrokerClient client)
    {
        // TODO: emit 'message-discarded' event (log refactor)
        foreach ( ref readonly var message in _messages )
            message.PoolToken.Return( client );

        _messages.Clear();

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

    internal void Enqueue(Protocol.PacketHeader header, Timestamp receivedAt, MemoryPoolToken<byte> poolToken, ReadOnlyMemory<byte> data)
    {
        _messages.Enqueue( new Message( header, receivedAt, poolToken, data ) );
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask<TaskStopReason> RunCore(MessageBrokerClient client)
    {
        bool reverseEndianness;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return TaskStopReason.OwnerDisposed;

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
        }

        while ( true )
        {
            var @continue = await client.MessageNotifications._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return TaskStopReason.OwnerDisposed;

            while ( true )
            {
                ulong contextId;
                Message message;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    if ( ! client.MessageNotifications._messages.TryDequeue( out message ) )
                    {
                        client.MessageNotifications._continuation.Reset();
                        break;
                    }

                    contextId = client.MessageContextQueue.AcquireContextId();
                }

                client.Emit( MessageBrokerClientEvent.MessageReceived( client, message.Header, contextId ) );

                var exc = Protocol.AssertMinPayload( client, message.Header, Protocol.MessageNotificationHeader.Length );
                if ( exc is not null )
                {
                    client.Emit( MessageBrokerClientEvent.MessageRejected( client, message.Header, exc, contextId ) );
                    message.PoolToken.Return( client );
                    return TaskStopReason.Error;
                }

                var request = Protocol.MessageNotificationHeader.Parse(
                    message.Data.Slice( 0, Protocol.MessageNotificationHeader.Length ),
                    reverseEndianness );

                var errors = request.StringifyErrors();
                if ( errors.Count > 0 )
                {
                    client.Emit(
                        MessageBrokerClientEvent.MessageRejected(
                            client,
                            message.Header,
                            Protocol.ProtocolException( client, message.Header, errors ),
                            contextId ) );

                    message.PoolToken.Return( client );
                    return TaskStopReason.Error;
                }

                var listener = ListenerCollection.TryGetByChannelId( client, request.ChannelId );
                if ( listener is null )
                {
                    client.Emit(
                        MessageBrokerClientEvent.MessageRejected(
                            client,
                            message.Header,
                            Protocol.ProtocolException(
                                client,
                                message.Header,
                                Chain.Create( Resources.ListenerDoesNotExist( request.ChannelId ) ) ),
                            contextId ) );

                    message.PoolToken.Return( client );
                    continue;
                }

                client.Emit( MessageBrokerClientEvent.MessageAccepted( client, message.Header, contextId ) );

                using ( listener.AcquireLock() )
                {
                    if ( ! listener.ShouldCancel )
                    {
                        MessageEmitter.Enqueue(
                            listener,
                            in request,
                            message.ReceivedAt,
                            message.PoolToken,
                            message.Data.Slice( Protocol.MessageNotificationHeader.Length ) );
                    }
                    else
                    {
                        // TODO: emit 'message-discarded' event (log refactor)
                        message.PoolToken.Return( client );
                    }
                }
            }
        }
    }

    private readonly struct Message
    {
        internal Message(Protocol.PacketHeader header, Timestamp receivedAt, MemoryPoolToken<byte> poolToken, ReadOnlyMemory<byte> data)
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
