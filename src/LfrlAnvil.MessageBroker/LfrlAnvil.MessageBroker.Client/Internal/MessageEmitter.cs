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

                var cancellationToken = message.Args.Listener.CancellationSource.Token;
                try
                {
                    await message.Args.Listener.Callback( message.Args, cancellationToken ).ConfigureAwait( false );
                }
                catch ( OperationCanceledException exc ) when ( exc.CancellationToken == cancellationToken )
                {
                    // TODO: emit 'message-cancelled' event (log refactor)
                }
                catch ( Exception exc )
                {
                    message.Args.Listener.Client.Emit( MessageBrokerClientEvent.Unexpected( message.Args.Listener.Client, exc ) );
                    // TODO: send NACK to server
                }
                finally
                {
                    message.PoolToken.Return( message.Args.Listener.Client );
                }
            }
        }
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

    internal static void Enqueue(
        MessageBrokerListener listener,
        in Protocol.MessageNotificationHeader request,
        Timestamp receivedAt,
        MemoryPoolToken<byte> poolToken,
        ReadOnlyMemory<byte> data)
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
                poolToken ) );

        if ( listener.MessageEmitter._continuation.Status == ValueTaskSourceStatus.Pending )
            listener.MessageEmitter._continuation.SetResult( true );
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
            MemoryPoolToken<byte> poolToken)
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
                data );

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
