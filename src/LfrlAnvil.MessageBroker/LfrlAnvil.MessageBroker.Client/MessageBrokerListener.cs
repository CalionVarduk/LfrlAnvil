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
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a message broker listener, which allows the client to react to messages published to the related channel.
/// </summary>
public sealed class MessageBrokerListener
{
    internal readonly CancellationTokenSource CancellationSource;
    internal MessageEmitter MessageEmitter;
    private MessageBrokerListenerState _state;

    internal MessageBrokerListener(
        MessageBrokerClient client,
        int channelId,
        string channelName,
        int queueId,
        string queueName,
        int prefetchHint,
        MessageBrokerListenerCallback callback)
    {
        Client = client;
        ChannelId = channelId;
        ChannelName = channelName;
        QueueId = queueId;
        QueueName = queueName;
        PrefetchHint = prefetchHint;
        Callback = callback;
        _state = MessageBrokerListenerState.Bound;
        CancellationSource = new CancellationTokenSource();
        MessageEmitter = MessageEmitter.Create();
        MessageEmitter.SetUnderlyingTask( MessageEmitter.StartUnderlyingTask( this ) );
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> instance that owns this listener.
    /// </summary>
    public MessageBrokerClient Client { get; }

    /// <summary>
    /// Unique id of the channel to which this listener is related.
    /// </summary>
    public int ChannelId { get; }

    /// <summary>
    /// Unique name of the channel to which this listener is related.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Unique id of the queue to which this listener is related.
    /// </summary>
    public int QueueId { get; }

    /// <summary>
    /// Unique name of the queue to which this listener is related.
    /// </summary>
    public string QueueName { get; }

    /// <summary>
    /// Specifies how many messages intended for this listener can be sent by the server to the <see cref="Client"/> at the same time.
    /// </summary>
    public int PrefetchHint { get; }

    /// <summary>
    /// Callback invoked when this listener receives a message from the server.
    /// </summary>
    public MessageBrokerListenerCallback Callback { get; }

    /// <summary>
    /// Current listener's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerListenerState"/> for more information.</remarks>
    public MessageBrokerListenerState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    internal bool ShouldCancel => _state >= MessageBrokerListenerState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerListener"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Client.Id}] '{Client.Name}' => [{ChannelId}] '{ChannelName}' listener (using [{QueueId}] '{QueueName}' queue) ({State})";
    }

    /// <summary>
    /// Attempts to unbind this listener from the channel.
    /// </summary>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerUnbindListenerResult"/> instance.
    /// </returns>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <remarks>
    /// This operation will cause all pending messages for this listener to be discarded.
    /// Unexpected errors encountered during listener unbinding will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the listener has been successfully unbound from the channel
    /// on the server side, or the listener is already locally unbound from the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<MessageBrokerUnbindListenerResult>> UnbindAsync()
    {
        return ListenerCollection.UnbindAsync( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool BeginDispose()
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return false;

            _state = MessageBrokerListenerState.Disposing;
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ValueTask EndDisposingAsync(ulong traceId)
    {
        return DisposeAsync( MessageBrokerListenerState.Disposing, traceId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ValueTask OnClientDisposedAsync(ulong traceId)
    {
        return DisposeAsync( MessageBrokerListenerState.Bound, traceId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( CancellationSource, spinWaitMultiplier: 4 );
    }

    private async ValueTask DisposeAsync(MessageBrokerListenerState expectedState, ulong traceId)
    {
        Task? messageEmitterTask;
        int discardedMessageCount;
        Chain<Exception> exceptions;
        using ( AcquireLock() )
        {
            if ( _state != expectedState )
                return;

            _state = MessageBrokerListenerState.Disposing;
            messageEmitterTask = MessageEmitter.DiscardUnderlyingTask();
            (discardedMessageCount, exceptions) = MessageEmitter.Dispose();
        }

        foreach ( var exc in exceptions )
            MessageBrokerClientErrorEvent.Create( Client, traceId, exc ).Emit( Client.Logger.Error );

        if ( discardedMessageCount > 0 )
        {
            var error = new MessageBrokerClientMessageException(
                Client,
                this,
                Resources.MessagesDiscarded( ChannelId, ChannelName, discardedMessageCount ) );

            MessageBrokerClientErrorEvent.Create( Client, traceId, error ).Emit( Client.Logger.Error );
        }

        try
        {
            await CancellationSource.CancelAsync().ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            MessageBrokerClientErrorEvent.Create( Client, traceId, exc ).Emit( Client.Logger.Error );
        }

        CancellationSource.TryDispose();

        if ( messageEmitterTask is not null )
        {
            try
            {
                await messageEmitterTask.WaitAsync( Client.ListenerDisposalTimeout ).ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                MessageBrokerClientErrorEvent.Create( Client, traceId, exc ).Emit( Client.Logger.Error );
            }
        }

        using ( AcquireLock() )
            _state = MessageBrokerListenerState.Disposed;
    }
}
