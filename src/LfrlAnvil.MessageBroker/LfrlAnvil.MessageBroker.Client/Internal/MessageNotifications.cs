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
        try
        {
            await RunCore( client ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            ulong traceId;
            using ( client.AcquireLock() )
            {
                client.MessageNotifications._task = null;
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
        var discardedMessageCount = _messages.Count;
        var exceptions = Chain<Exception>.Empty;

        foreach ( ref readonly var message in _messages )
        {
            var exc = message.PoolToken.Return();
            if ( exc is not null )
                exceptions = exceptions.Extend( exc );
        }

        _messages.Clear();
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
        _messages.Enqueue( new Message( header, receivedAt, poolToken, data ) );
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
            var @continue = await client.MessageNotifications._continuation.GetTask().ConfigureAwait( false );
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

                    if ( ! client.MessageNotifications._messages.TryDequeue( out message ) )
                    {
                        client.MessageNotifications._continuation.Reset();
                        break;
                    }

                    traceId = client.GetTraceId();
                }

                var failed = true;
                MessageBrokerClientTraceEvent.Create( client, traceId, MessageBrokerClientTraceEventType.MessageNotification )
                    .Emit( client.Logger.TraceStart );

                try
                {
                    MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, message.Header ).Emit( client.Logger.ReadPacket );

                    var exception = Protocol.AssertMinPayload( client, message.Header, Protocol.MessageNotificationHeader.Length );
                    if ( exception is not null )
                    {
                        MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                        await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                        return;
                    }

                    var request = Protocol.MessageNotificationHeader.Parse(
                        message.Data.Slice( 0, Protocol.MessageNotificationHeader.Length ),
                        reverseEndianness );

                    var errors = request.StringifyErrors();
                    if ( errors.Count > 0 )
                    {
                        var error = Protocol.ProtocolException( client, message.Header, errors );
                        MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                        await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                        return;
                    }

                    var data = message.Data.Slice( Protocol.MessageNotificationHeader.Length );
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

                    var listener = ListenerCollection.TryGetByChannelId( client, request.ChannelId );
                    if ( listener is null )
                    {
                        var error = new MessageBrokerClientMessageException(
                            client,
                            null,
                            Resources.ListenerDoesNotExist( request.ChannelId ) );

                        MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                        continue;
                    }

                    MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, message.Header ).Emit( client.Logger.ReadPacket );

                    bool cancel;
                    using ( listener.AcquireLock() )
                    {
                        cancel = listener.ShouldCancel;
                        if ( ! cancel )
                        {
                            MessageEmitter.Enqueue( listener, in request, message.ReceivedAt, message.PoolToken, data, traceId );
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
                        message.PoolToken.Return( client, traceId );
                        MessageBrokerClientTraceEvent.Create( client, traceId, MessageBrokerClientTraceEventType.MessageNotification )
                            .Emit( client.Logger.TraceEnd );
                    }
                }
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ValueTask DisposeClientAsync(MessageBrokerClient client, ulong traceId)
    {
        using ( client.AcquireLock() )
            client.MessageNotifications._task = null;

        return client.DisposeAsync( traceId );
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
