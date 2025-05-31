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
                client.MessageNotifications._task = null;
                traceId = client.GetTraceId();
            }

            using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Unexpected ) )
            {
                MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
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

    internal static void EnqueueMessagesUnsafe(MessageBrokerRemoteClient client, in ListSlim<QueueMessage> messages)
    {
        foreach ( ref readonly var message in messages )
        {
            var writerSource = client.WriterQueue.AcquireSource();
            client.MessageNotifications._messages.Enqueue( new Message( in message, writerSource ) );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask RunCore(MessageBrokerRemoteClient client)
    {
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

                    // TODO: don't dequeue immediately
                    // only when msg was actually sent
                    // fix it when implementing acks, since msg after sending will have to be added to unacked msgs
                    if ( ! client.MessageNotifications._messages.TryDequeue( out message ) )
                    {
                        client.MessageNotifications._continuation.Reset();
                        break;
                    }

                    traceId = client.GetTraceId();
                }

                Assume.Equals( message.Listener.Client, client );
                using ( MessageBrokerRemoteClientTraceEvent.CreateScope(
                    client,
                    traceId,
                    MessageBrokerRemoteClientTraceEventType.MessageNotification ) )
                {
                    MessageBrokerRemoteClientProcessingMessageEvent.Create(
                            message.Listener,
                            traceId,
                            message.Publisher,
                            message.Id,
                            0,
                            0,
                            message.Length )
                        .Emit( client.Logger.ProcessingMessage );

                    try
                    {
                        if ( ! await message.WriterSource.GetTask().ConfigureAwait( false ) )
                        {
                            MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() )
                                .Emit( client.Logger.Error );

                            return;
                        }

                        var writeResult = await client.WriteAsync( message.PacketHeader, message.Packet, traceId )
                            .ConfigureAwait( false );

                        if ( writeResult.Exception is not null )
                        {
                            using ( client.AcquireLock() )
                                client.MessageNotifications._task = null;

                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return;
                        }

                        MessageBrokerRemoteClientMessageProcessedEvent.Create( message.Listener, traceId, message.Publisher, message.Id )
                            .Emit( client.Logger.MessageProcessed );

                        if ( message.Listener.DecrementPrefetchCounter() )
                        {
                            using ( message.Listener.Queue.AcquireLock() )
                            {
                                if ( ! message.Listener.Queue.ShouldCancel )
                                    message.Listener.Queue.QueueProcessor.SignalContinuation();
                            }
                        }

                        using ( client.AcquireActiveLock( traceId, out var exc ) )
                        {
                            if ( exc is not null )
                                return;

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

    private readonly struct Message
    {
        internal Message(in QueueMessage message, ManualResetValueTaskSource<bool> writerSource)
        {
            PacketHeader = message.PacketHeader;
            Publisher = message.Publisher;
            Listener = message.Listener;
            Id = message.Id;
            WriterSource = writerSource;
            PoolToken = message.PoolToken;
            Packet = message.Packet;
        }

        internal readonly Protocol.PacketHeader PacketHeader;
        internal readonly MessageBrokerChannelPublisherBinding Publisher;
        internal readonly MessageBrokerChannelListenerBinding Listener;
        internal readonly ulong Id;
        internal readonly ManualResetValueTaskSource<bool> WriterSource;
        internal readonly MemoryPoolToken<byte> PoolToken;
        internal readonly ReadOnlyMemory<byte> Packet;

        internal int Length => unchecked( Packet.Length - Protocol.PacketHeader.Length - Protocol.MessageNotificationHeader.Payload );

        [Pure]
        public override string ToString()
        {
            return $"Header = ({PacketHeader}), Id = {Id}, Publisher = ({Publisher}), Listener = ({Listener})";
        }
    }
}
