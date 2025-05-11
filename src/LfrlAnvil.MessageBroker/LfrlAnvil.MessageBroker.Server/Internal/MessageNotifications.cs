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
    private QueueSlim<Message> _pending;
    private Task? _task;

    private MessageNotifications(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _pending = QueueSlim<Message>.Create();
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
        TaskStopReason stopReason;
        try
        {
            stopReason = await RunCore( client ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerRemoteClientEvent.Unexpected( client, exc ) );
            stopReason = TaskStopReason.Error;
        }

        if ( stopReason == TaskStopReason.OwnerDisposed )
            return;

        using ( client.AcquireLock() )
            client.MessageNotifications._task = null;

        await client.DisconnectAsync().ConfigureAwait( false );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Dispose()
    {
        // TODO: emit 'message-discarded' (log refactor & permanence)
        foreach ( ref readonly var message in _pending )
            message.Return();

        _pending.Clear();

        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( false );
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

    internal static void SendMessages(MessageBrokerRemoteClient client, in ListSlim<QueueMessage> messages)
    {
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
            {
                // TODO: emit 'message-discarded' (log refactor & permanence)
                foreach ( ref readonly var message in messages )
                    message.Return();

                return;
            }

            foreach ( ref readonly var message in messages )
            {
                var contextId = client.MessageContextQueue.AcquireContextId();
                var writerSource = client.MessageContextQueue.AcquireWriterSource();
                client.MessageNotifications._pending.Enqueue( new Message( in message, contextId, writerSource ) );
            }

            client.MessageNotifications.SignalContinuation();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask<TaskStopReason> RunCore(MessageBrokerRemoteClient client)
    {
        var buffer = ListSlim<Message>.Create( minCapacity: 16 );
        while ( true )
        {
            var @continue = await client.MessageNotifications._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return TaskStopReason.OwnerDisposed;

            while ( true )
            {
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    client.MessageNotifications.CopyMessagesInto( ref buffer );
                    if ( buffer.Count == 0 )
                    {
                        client.MessageNotifications._continuation.Reset();
                        break;
                    }
                }

                TaskStopReason? stopReason = null;
                for ( var i = 0; i < buffer.Count; ++i )
                {
                    var message = buffer[i];
                    try
                    {
                        if ( stopReason is not null )
                            continue;

                        if ( ! await message.WriterSource.GetTask().ConfigureAwait( false ) )
                        {
                            stopReason = TaskStopReason.OwnerDisposed;
                            continue;
                        }

                        var writeResult = await client.WriteAsync( message.PacketHeader, message.Packet, message.ContextId )
                            .ConfigureAwait( false );

                        if ( writeResult.Exception is not null )
                        {
                            stopReason = TaskStopReason.Error;
                            continue;
                        }

                        if ( message.Subscription.DecrementPrefetchCounter() )
                        {
                            using ( message.Subscription.Queue.AcquireLock() )
                            {
                                if ( ! message.Subscription.Queue.ShouldCancel )
                                    message.Subscription.Queue.QueueProcessor.SignalContinuation();
                            }
                        }

                        using ( client.AcquireLock() )
                        {
                            if ( client.ShouldCancel )
                            {
                                stopReason = TaskStopReason.OwnerDisposed;
                                continue;
                            }

                            client.MessageContextQueue.ResetOutgoingWriter( client, message.WriterSource );
                        }
                    }
                    catch ( Exception exc )
                    {
                        client.Emit( MessageBrokerRemoteClientEvent.Unexpected( client, exc ) );
                        stopReason = TaskStopReason.Error;
                    }
                    finally
                    {
                        message.Return();
                    }
                }

                if ( stopReason is not null )
                    return stopReason.Value;

                using ( client.AcquireLock() )
                    client.MessageNotifications._pending.DequeueRange( buffer.Count );

                buffer.Clear();
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void CopyMessagesInto(ref ListSlim<Message> buffer)
    {
        Assume.True( buffer.IsEmpty );
        Assume.IsGreaterThan( buffer.Capacity, 0 );
        if ( _pending.IsEmpty )
            return;

        var queueSlice = _pending.AsMemory();
        if ( queueSlice.Length > buffer.Capacity )
            queueSlice = queueSlice.Slice( 0, buffer.Capacity );

        CopyMessagesInto( queueSlice.First.Span, ref buffer );
        CopyMessagesInto( queueSlice.Second.Span, ref buffer );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void CopyMessagesInto(ReadOnlySpan<Message> source, ref ListSlim<Message> target)
    {
        Assume.IsLessThanOrEqualTo( target.Count + source.Length, target.Capacity );
        target.AddRange( source );
    }

    private readonly struct Message
    {
        private readonly MemoryPoolToken<byte> _poolToken;

        internal Message(in QueueMessage message, ulong contextId, ManualResetValueTaskSource<bool> writerSource)
        {
            PacketHeader = message.PacketHeader;
            Subscription = message.Listener;
            ContextId = contextId;
            WriterSource = writerSource;
            _poolToken = message.PoolToken;
            Packet = message.Packet;
        }

        internal readonly Protocol.PacketHeader PacketHeader;
        internal readonly MessageBrokerChannelListenerBinding Subscription;
        internal readonly ulong ContextId;
        internal readonly ManualResetValueTaskSource<bool> WriterSource;
        internal readonly ReadOnlyMemory<byte> Packet;

        [Pure]
        public override string ToString()
        {
            return $"Header = ({PacketHeader}), Subscription = ({Subscription}), ContextId = {ContextId}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Return()
        {
            _poolToken.Return( Subscription.Client );
        }
    }
}
