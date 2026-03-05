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
using LfrlAnvil.Chrono;
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
    private readonly TaskCompletionSource _disposed;
    private MessageBrokerListenerState _state;
    private bool _autoDisposed;

    internal MessageBrokerListener(
        MessageBrokerClient client,
        int channelId,
        string channelName,
        int queueId,
        string queueName,
        short prefetchHint,
        int maxRetries,
        Duration retryDelay,
        int maxRedeliveries,
        Duration minAckTimeout,
        int deadLetterCapacityHint,
        Duration minDeadLetterRetention,
        string? filterExpression,
        bool isEphemeral,
        MessageBrokerListenerCallback callback)
    {
        Client = client;
        ChannelId = channelId;
        ChannelName = channelName;
        QueueId = queueId;
        QueueName = queueName;
        PrefetchHint = prefetchHint;
        MaxRetries = maxRetries;
        RetryDelay = retryDelay;
        MaxRedeliveries = maxRedeliveries;
        MinAckTimeout = minAckTimeout;
        DeadLetterCapacityHint = deadLetterCapacityHint;
        MinDeadLetterRetention = minDeadLetterRetention;
        FilterExpression = filterExpression;
        IsEphemeral = isEphemeral;
        Callback = callback;
        _state = MessageBrokerListenerState.Bound;
        _autoDisposed = false;
        CancellationSource = new CancellationTokenSource();
        _disposed = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
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
    /// <remarks>
    /// This is a max potential value. Actual value is dependant on all listeners attached to the queue
    /// and all of its currently pending messages.
    /// </remarks>
    public short PrefetchHint { get; }

    /// <summary>
    /// Specifies how many times the server will attempt to automatically send a message notification retry
    /// when the <see cref="Client"/> responds with a negative ACK, before giving up.
    /// </summary>
    /// <remarks>Retries are disabled when value is equal <b>0</b>.</remarks>
    public int MaxRetries { get; }

    /// <summary>
    /// Specifies the delay between the server successfully processing negative ACK sent by the <see cref="Client"/>
    /// and the server sending a message notification retry.
    /// </summary>
    public Duration RetryDelay { get; }

    /// <summary>
    /// Specifies how many times the server will attempt to automatically send a message notification redelivery
    /// when the <see cref="Client"/> fails to respond with either an ACK or a negative ACK in time (see <see cref="MinAckTimeout"/>),
    /// before giving up.
    /// </summary>
    /// <remarks>Redelivery are disabled when value is equal <b>0</b>.</remarks>
    public int MaxRedeliveries { get; }

    /// <summary>
    /// Specifies the minimum amount of time that the server will wait for the <see cref="Client"/> to send either an ACK or a negative ACK
    /// before attempting a message notification redelivery.
    /// Actual ACK timeout may be different due to the state of the queue and other listeners bound to it.
    /// </summary>
    public Duration MinAckTimeout { get; }

    /// <summary>
    /// Specifies how many messages will be stored at most by the dead letter.
    /// </summary>
    /// <remarks>
    /// This is a min value. Actual value is dependant on all listeners attached to the queue and the state of the queue's dead letter.
    /// </remarks>
    public int DeadLetterCapacityHint { get; }

    /// <summary>
    /// Specifies retention period for messages stored in the dead letter.
    /// </summary>
    /// <remarks>
    /// This is a min value. Actual value is dependant on all listeners attached to the queue and the state of the queue's dead letter.
    /// </remarks>
    public Duration MinDeadLetterRetention { get; }

    /// <summary>
    /// Specifies server-side message filter expression.
    /// </summary>
    public string? FilterExpression { get; }

    /// <summary>
    /// Specifies whether the listener is ephemeral.
    /// </summary>
    /// <remarks>Ephemeral listeners will be removed by the server when client disconnects.</remarks>
    public bool IsEphemeral { get; }

    /// <summary>
    /// Specifies whether the <see cref="Client"/> is expected to send ACK or negative ACK to the server
    /// in order to confirm message notification.
    /// </summary>
    public bool AreAcksEnabled => MinAckTimeout > Duration.Zero;

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

    /// <summary>
    /// Attempts to send a message notification ACK.
    /// </summary>
    /// <param name="ackId">Id of the pending ACK associated with the message.</param>
    /// <param name="streamId">Unique id of the server-side stream that handled the message.</param>
    /// <param name="messageId">Unique message id.</param>
    /// <param name="retry">Retry attempt of the message.</param>
    /// <param name="redelivery">Redelivery number of the message.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="ackId"/> is less than or equal to <b>0</b>
    /// or when <paramref name="streamId"/> is less than or equal to <b>0</b>
    /// or when <paramref name="retry"/> is less than <b>0</b> or greater than the listener's <see cref="MaxRetries"/>
    /// or when <paramref name="redelivery"/> is less than <b>0</b> or greater than the listener's <see cref="MaxRedeliveries"/>.
    /// </exception>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <exception cref="MessageBrokerClientMessageException">When ACKs are not enabled for this listener.</exception>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="bool"/> result. If the result is equal to <b>true</b>, then ACK sending was successful,
    /// otherwise the listener was no longer bound to the channel.
    /// </returns>
    /// <remarks>
    /// Unexpected errors encountered during ACK sending attempt will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the client has successfully sent the ACK to the server,
    /// or the listener is already locally ubound from the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<bool>> SendMessageAckAsync(int ackId, int streamId, ulong messageId, int retry, int redelivery)
    {
        return ListenerCollection.SendMessageAckAsync( this, ackId, streamId, messageId, retry, redelivery, null );
    }

    /// <summary>
    /// Attempts to send a negative message notification ACK.
    /// </summary>
    /// <param name="ackId">Id of the pending ACK associated with the message.</param>
    /// <param name="streamId">Unique id of the server-side stream that handled the message.</param>
    /// <param name="messageId">Unique message id.</param>
    /// <param name="retry">Retry attempt of the message.</param>
    /// <param name="redelivery">Redelivery attempt of the message.</param>
    /// <param name="nack">
    /// Optional <see cref="MessageBrokerNegativeAck"/> instance that allows to modify the ACK.
    /// Equal to <see cref="MessageBrokerNegativeAck.Default"/> by default.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="ackId"/> is less than or equal to <b>0</b>
    /// or when <paramref name="streamId"/> is less than or equal to <b>0</b>
    /// or when <paramref name="retry"/> is less than <b>0</b> or greater than the listener's <see cref="MaxRetries"/>
    /// or when <paramref name="redelivery"/> is less than <b>0</b> or greater than the listener's <see cref="MaxRedeliveries"/>.
    /// </exception>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <exception cref="MessageBrokerClientMessageException">When ACKs are not enabled for this listener.</exception>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="bool"/> result. If the result is equal to <b>true</b>, then ACK sending was successful,
    /// otherwise the listener was no longer bound to the channel.
    /// </returns>
    /// <remarks>
    /// Unexpected errors encountered during ACK sending attempt will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the client has successfully sent the ACK to the server,
    /// or the listener is already locally ubound from the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<bool>> SendNegativeMessageAckAsync(
        int ackId,
        int streamId,
        ulong messageId,
        int retry,
        int redelivery,
        MessageBrokerNegativeAck nack = default)
    {
        return ListenerCollection.SendNegativeMessageAckAsync(
            this,
            ackId,
            streamId,
            messageId,
            retry,
            redelivery,
            null,
            nack,
            false );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool BeginUnbinding()
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
    internal void OnClientDisposing(ulong traceId)
    {
        var exceptions = Chain<Exception>.Empty;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _autoDisposed = true;
            _state = MessageBrokerListenerState.Disposing;
            MessageEmitter.BeginDispose( ref exceptions );
        }

        Client.EmitErrors( ref exceptions, traceId );
        Client.EmitError( CancellationSource.TryCancel(), traceId );
        Client.EmitError( CancellationSource.TryDispose(), traceId );
    }

    internal async ValueTask EndUnbindingAsync(ulong traceId)
    {
        try
        {
            Task? messageEmitterTask;
            var exceptions = Chain<Exception>.Empty;

            using ( AcquireLock() )
            {
                Assume.Equals( _autoDisposed, false );
                Assume.Equals( _state, MessageBrokerListenerState.Disposing );
                messageEmitterTask = MessageEmitter.DiscardUnderlyingTask();
                MessageEmitter.BeginDispose( ref exceptions );
            }

            Client.EmitErrors( ref exceptions, traceId );
            Client.EmitError( CancellationSource.TryCancel(), traceId );
            Client.EmitError( CancellationSource.TryDispose(), traceId );
            Client.EmitError(
                await messageEmitterTask.AsSafeCancellable( Client.ListenerDisposalTimeout ).ConfigureAwait( false ),
                traceId );

            MessageEmitter.DiscardedNotification[] discardedMessages;
            using ( AcquireLock() )
                discardedMessages = MessageEmitter.EndDispose();

            EndDiscardedMessages( discardedMessages, traceId );
            using ( AcquireLock() )
                _state = MessageBrokerListenerState.Disposed;
        }
        finally
        {
            _disposed.TrySetResult();
        }
    }

    internal async ValueTask OnClientDisposedAsync(ulong traceId)
    {
        bool autoDisposed;
        MessageBrokerListenerState state;
        Task? messageEmitterTask = null;

        using ( AcquireLock() )
        {
            Assume.IsGreaterThanOrEqualTo( _state, MessageBrokerListenerState.Disposing );
            autoDisposed = _autoDisposed;
            state = _state;
            if ( autoDisposed )
                messageEmitterTask = MessageEmitter.DiscardUnderlyingTask();
        }

        if ( ! autoDisposed )
        {
            if ( state == MessageBrokerListenerState.Disposing )
                await _disposed.Task.ConfigureAwait( false );

            return;
        }

        try
        {
            Client.EmitError(
                await messageEmitterTask.AsSafeCancellable( Client.ListenerDisposalTimeout ).ConfigureAwait( false ),
                traceId );

            MessageEmitter.DiscardedNotification[] discardedMessages;
            using ( AcquireLock() )
                discardedMessages = MessageEmitter.EndDispose();

            EndDiscardedMessages( discardedMessages, traceId );
            using ( AcquireLock() )
                _state = MessageBrokerListenerState.Disposed;
        }
        finally
        {
            _disposed.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _disposed );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EndDiscardedMessages(MessageEmitter.DiscardedNotification[] discardedMessages, ulong traceId)
    {
        var error = Client.Logger.Error;
        var traceEnd = Client.Logger.TraceEnd;
        foreach ( var m in discardedMessages )
        {
            m.PoolToken.Return( Client, traceId );
            if ( error is not null )
            {
                var exc = Client.DisposedException();
                error.Emit( MessageBrokerClientErrorEvent.Create( Client, m.TraceId, exc ) );
            }

            traceEnd?.Emit(
                MessageBrokerClientTraceEvent.Create( Client, m.TraceId, MessageBrokerClientTraceEventType.MessageNotification ) );
        }

        if ( discardedMessages.Length > 0 && error is not null )
        {
            var exc = Client.MessageException( this, Resources.MessagesDiscarded( ChannelId, ChannelName, discardedMessages.Length ) );
            error.Emit( MessageBrokerClientErrorEvent.Create( Client, traceId, exc ) );
        }
    }
}
