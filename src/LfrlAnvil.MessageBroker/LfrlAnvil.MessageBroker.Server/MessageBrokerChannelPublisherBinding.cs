// Copyright 2025-2026 Łukasz Furlepa
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
using LfrlAnvil.Async;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker channel binding for a publisher, which allows clients to publish messages through channels.
/// </summary>
public sealed class MessageBrokerChannelPublisherBinding : IMessageBrokerMessagePublisher
{
    private readonly object _sync = new object();
    private MessageBrokerChannelPublisherBindingState _state;
    private TaskCompletionSource? _deactivated;
    private bool _isEphemeral;
    private bool _autoDisposed;

    private MessageBrokerChannelPublisherBinding(
        MessageBrokerRemoteClient client,
        MessageBrokerChannel channel,
        MessageBrokerStream stream,
        bool isEphemeral,
        MessageBrokerChannelPublisherBindingState state)
    {
        Client = client;
        Channel = channel;
        Stream = stream;
        _isEphemeral = isEphemeral;
        _state = state;
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> instance to which this publisher belongs to.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannel"/> instance to which the <see cref="Client"/> is bound to as a publisher.
    /// </summary>
    public MessageBrokerChannel Channel { get; }

    /// <summary>
    /// <see cref="MessageBrokerStream"/> instance through which this publisher will push messages to subscribers.
    /// </summary>
    public MessageBrokerStream Stream { get; }

    /// <summary>
    /// Specifies whether the publisher is ephemeral.
    /// </summary>
    public bool IsEphemeral
    {
        get
        {
            using ( AcquireLock() )
                return _isEphemeral;
        }
    }

    /// <summary>
    /// Current publisher's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerChannelPublisherBindingState"/> for more information.</remarks>
    public MessageBrokerChannelPublisherBindingState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    internal bool IsInactive => _state >= MessageBrokerChannelPublisherBindingState.Deactivating;
    internal bool IsDisposed => _state >= MessageBrokerChannelPublisherBindingState.Disposing;

    int IMessageBrokerMessagePublisher.ClientId => Client.Id;
    string IMessageBrokerMessagePublisher.ClientName => Client.Name;
    bool IMessageBrokerMessagePublisher.IsClientEphemeral => Client.IsEphemeral;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherBinding Create(
        MessageBrokerRemoteClient client,
        MessageBrokerChannel channel,
        MessageBrokerStream stream,
        bool isEphemeral)
    {
        return new MessageBrokerChannelPublisherBinding(
            client,
            channel,
            stream,
            isEphemeral,
            MessageBrokerChannelPublisherBindingState.Created );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherBinding CreateInactive(
        MessageBrokerRemoteClient client,
        MessageBrokerChannel channel,
        MessageBrokerStream stream)
    {
        return new MessageBrokerChannelPublisherBinding(
            client,
            channel,
            stream,
            isEphemeral: false,
            MessageBrokerChannelPublisherBindingState.Inactive );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherBinding CreateDisposed(
        MessageBrokerRemoteClient client,
        MessageBrokerChannel channel,
        MessageBrokerStream stream)
    {
        return new MessageBrokerChannelPublisherBinding(
            client,
            channel,
            stream,
            isEphemeral: true,
            MessageBrokerChannelPublisherBindingState.Disposed );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelPublisherBinding"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[{Client.Id}] '{Client.Name}' => [{Channel.Id}] '{Channel.Name}' publisher binding (using [{Stream.Id}] '{Stream.Name}' stream) ({State})";
    }

    /// <summary>
    /// Deletes this publisher from the server.
    /// </summary>
    /// <exception cref="MessageBrokerServerException">
    /// When server is in <see cref="MessageBrokerServerState.Created"/> or <see cref="MessageBrokerServerState.Starting"/> state.
    /// </exception>
    /// <exception cref="MessageBrokerRemoteClientException">
    /// When this publisher is in <see cref="MessageBrokerChannelPublisherBindingState.Created"/> state.
    /// </exception>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async ValueTask DeleteAsync()
    {
        if ( Client.Server.State < MessageBrokerServerState.Running )
            ExceptionThrower.Throw( Client.Server.Exception( Resources.ServerIsNotRunning ) );

        TaskCompletionSource? deactivated;
        while ( true )
        {
            MessageBrokerChannelPublisherBindingState state;

            using ( AcquireLock() )
            {
                if ( _state == MessageBrokerChannelPublisherBindingState.Created )
                    ExceptionThrower.Throw( Client.Exception( Resources.PublisherIsBeingBound ) );

                if ( ! IsInactive )
                {
                    _state = MessageBrokerChannelPublisherBindingState.Disposing;
                    _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
                    deactivated = _deactivated;
                    break;
                }

                state = _state;
                deactivated = _deactivated;
            }

            if ( state is MessageBrokerChannelPublisherBindingState.Deactivating or MessageBrokerChannelPublisherBindingState.Disposing
                && deactivated is not null )
                await deactivated.Task.ConfigureAwait( false );

            if ( state >= MessageBrokerChannelPublisherBindingState.Disposing )
                return;

            using ( AcquireLock() )
            {
                if ( _state == MessageBrokerChannelPublisherBindingState.Disposed )
                    return;

                if ( _state == MessageBrokerChannelPublisherBindingState.Inactive )
                {
                    _state = MessageBrokerChannelPublisherBindingState.Disposing;
                    _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
                    deactivated = _deactivated;
                    break;
                }
            }
        }

        try
        {
            ulong? traceId = null;
            ServerStorage.Client clientStorage = default;

            using ( Client.AcquireLock() )
            {
                if ( Client.IsDisposed )
                    clientStorage = Client.GetStorage();
                else
                    traceId = Client.GetTraceId();
            }

            if ( traceId is null )
            {
                await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false );
                return;
            }

            await DeleteAsyncCore( traceId.Value ).ConfigureAwait( false );
        }
        finally
        {
            using ( AcquireLock() )
            {
                _state = MessageBrokerChannelPublisherBindingState.Disposed;
                _deactivated = null;
            }

            deactivated.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReactivationResult TryReactivate(string streamName, bool isEphemeral)
    {
        using ( AcquireLock() )
        {
            if ( _state != MessageBrokerChannelPublisherBindingState.Inactive )
                return ReactivationResult.AlreadyBound;

            if ( ! Stream.Name.Equals( streamName, StringComparison.OrdinalIgnoreCase ) )
            {
                _state = MessageBrokerChannelPublisherBindingState.Created;
                return ReactivationResult.Rebinding;
            }

            _isEphemeral = isEphemeral;
            _state = MessageBrokerChannelPublisherBindingState.Created;
        }

        return ReactivationResult.Reactivated;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RevertRebinding()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerChannelPublisherBindingState.Created )
                _state = MessageBrokerChannelPublisherBindingState.Inactive;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsEphemeral()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerChannelPublisherBindingState.Inactive )
                _isEphemeral = true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsRunning()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerChannelPublisherBindingState.Created )
                _state = MessageBrokerChannelPublisherBindingState.Running;
        }
    }

    internal void OnServerDisposing()
    {
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return;

            _state = MessageBrokerChannelPublisherBindingState.Disposing;
            _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            _autoDisposed = true;
        }
    }

    internal void OnClientDeactivating(bool keepAlive)
    {
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return;

            if ( keepAlive && ! _isEphemeral )
            {
                if ( IsInactive )
                    return;

                _state = MessageBrokerChannelPublisherBindingState.Deactivating;
            }
            else
                _state = MessageBrokerChannelPublisherBindingState.Disposing;

            _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            _autoDisposed = true;
        }
    }

    internal async ValueTask OnServerDisposedAsync(
        ServerStorage.Client clientStorage,
        bool clearBuffers,
        bool storageLoaded,
        ulong clientTraceId)
    {
        bool isEphemeral;
        bool autoDisposed;
        MessageBrokerChannelPublisherBindingState state;
        TaskCompletionSource? deactivated;

        using ( AcquireLock() )
        {
            Assume.IsGreaterThanOrEqualTo( _state, MessageBrokerChannelPublisherBindingState.Disposing );
            state = _state;
            autoDisposed = _autoDisposed;
            isEphemeral = _isEphemeral;
            deactivated = _deactivated;
        }

        if ( ! autoDisposed )
        {
            if ( state == MessageBrokerChannelPublisherBindingState.Disposing )
            {
                if ( storageLoaded )
                    Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );

                Client.EmitError( await (deactivated?.Task).AsSafeCancellable().ConfigureAwait( false ), clientTraceId );
            }

            return;
        }

        try
        {
            if ( isEphemeral )
            {
                using ( Stream.AcquireLock() )
                    Stream.PublishersByClientChannelIdPair.Remove( Pair.Create( Client.Id, Channel.Id ) );

                using ( Channel.AcquireLock() )
                    Channel.PublishersByClientId.Remove( Client.Id );
            }
            else
                Client.EmitError(
                    await clientStorage.SaveMetadataAsync( this, clearBuffers, clientTraceId ).AsSafe().ConfigureAwait( false ),
                    clientTraceId );

            using ( AcquireLock() )
            {
                _state = MessageBrokerChannelPublisherBindingState.Disposed;
                _autoDisposed = false;
                _deactivated = null;
            }
        }
        finally
        {
            deactivated?.TrySetResult();
        }
    }

    internal async ValueTask OnClientDeactivatedAsync(ServerStorage.Client clientStorage, bool keepAlive, ulong clientTraceId)
    {
        bool dispose;
        bool autoDisposed;
        MessageBrokerChannelPublisherBindingState state;
        TaskCompletionSource? deactivated;

        using ( AcquireLock() )
        {
            state = _state;
            Assume.IsGreaterThanOrEqualTo( _state, MessageBrokerChannelPublisherBindingState.Deactivating );
            if ( state == MessageBrokerChannelPublisherBindingState.Inactive && keepAlive )
                return;

            autoDisposed = _autoDisposed;
            deactivated = _deactivated;
            dispose = _isEphemeral || ! keepAlive;
        }

        if ( ! autoDisposed )
        {
            if ( state == MessageBrokerChannelPublisherBindingState.Disposing )
            {
                Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );
                Client.EmitError( await (deactivated?.Task).AsSafeCancellable().ConfigureAwait( false ), clientTraceId );
            }

            return;
        }

        try
        {
            if ( dispose )
            {
                if ( keepAlive )
                {
                    using ( Client.AcquireLock() )
                        Client.PublishersByChannelId.Remove( Channel.Id );
                }

                await Stream.OnPublisherDisposingAsync( Client, Channel, clientTraceId ).ConfigureAwait( false );
                await Channel.OnPublisherDisposingAsync( Client, clientTraceId ).ConfigureAwait( false );
                Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );

                using ( AcquireLock() )
                {
                    _state = MessageBrokerChannelPublisherBindingState.Disposed;
                    _autoDisposed = false;
                    _deactivated = null;
                }
            }
            else
            {
                using ( AcquireLock() )
                {
                    _state = MessageBrokerChannelPublisherBindingState.Inactive;
                    _autoDisposed = false;
                    _deactivated = null;
                }
            }
        }
        finally
        {
            deactivated?.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryBeginDisposingUnsafe()
    {
        if ( IsInactive )
        {
            if ( _state != MessageBrokerChannelPublisherBindingState.Inactive )
                return false;
        }

        _state = MessageBrokerChannelPublisherBindingState.Disposing;
        _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal async ValueTask EndDisposingAsync(ServerStorage.Client clientStorage, ulong clientTraceId)
    {
        TaskCompletionSource? deactivated = null;
        try
        {
            Assume.Equals( State, MessageBrokerChannelPublisherBindingState.Disposing );
            Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );

            using ( Client.AcquireLock() )
            {
                if ( ! Client.IsDisposed )
                    Client.PublishersByChannelId.Remove( Channel.Id );
            }

            using ( AcquireLock() )
            {
                _state = MessageBrokerChannelPublisherBindingState.Disposed;
                deactivated = _deactivated;
                _deactivated = null;
            }
        }
        finally
        {
            deactivated?.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void EndDisposingDueToRebind()
    {
        TaskCompletionSource? deactivated = null;
        try
        {
            Assume.Equals( State, MessageBrokerChannelPublisherBindingState.Disposing );
            using ( AcquireLock() )
            {
                _state = MessageBrokerChannelPublisherBindingState.Disposed;
                deactivated = _deactivated;
                _deactivated = null;
            }
        }
        finally
        {
            deactivated?.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }

    private async ValueTask DeleteAsyncCore(ulong clientTraceId)
    {
        var notificationEnqueued = false;
        var notificationPoolToken = MemoryPoolToken<byte>.Empty;
        if ( Client.Logger.TraceStart is { } traceStart )
            traceStart.Emit(
                MessageBrokerRemoteClientTraceEvent.Create(
                    Client,
                    clientTraceId,
                    MessageBrokerRemoteClientTraceEventType.UnbindPublisher ) );

        try
        {
            var channelTraceId = 0UL;
            var streamTraceId = 0UL;
            var disposingChannel = false;
            var disposingStream = false;
            var clearBuffers = true;
            ServerStorage.Client clientStorage;
            Exception? exception = null;

            using ( Client.AcquireLock() )
            {
                clientStorage = Client.GetStorage();
                if ( Client.IsDisposed )
                    exception = Client.DeactivatedException( disposed: true );
                else
                {
                    clearBuffers = Client.GetClearBuffersOption();

                    using ( Channel.AcquireLock() )
                    {
                        if ( Channel.IsDisposed )
                            exception = Channel.DisposedException();
                        else
                        {
                            using ( Stream.AcquireLock() )
                            {
                                if ( Stream.IsDisposed )
                                    exception = Stream.DisposedException();
                                else
                                {
                                    using ( AcquireLock() )
                                    {
                                        Assume.Equals( _state, MessageBrokerChannelPublisherBindingState.Disposing );
                                        disposingChannel = Channel.TryDisposeByRemovingPublisherUnsafe( Client.Id );
                                        disposingStream = Stream.TryDisposeByRemovingPublisherUnsafe( Client.Id, Channel.Id );
                                        channelTraceId = Channel.GetTraceId();
                                        streamTraceId = Stream.GetTraceId();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );
            if ( exception is not null )
            {
                if ( Client.Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( Client, clientTraceId, exception ) );

                return;
            }

            using ( MessageBrokerStreamTraceEvent.CreateScope(
                Stream,
                streamTraceId,
                MessageBrokerStreamTraceEventType.UnbindPublisher ) )
            {
                if ( Stream.Logger.ClientTrace is { } clientTrace )
                    clientTrace.Emit( MessageBrokerStreamClientTraceEvent.Create( Stream, streamTraceId, Client, clientTraceId ) );

                if ( Stream.Logger.PublisherUnbound is { } streamPublisherUnbound )
                    streamPublisherUnbound.Emit( MessageBrokerStreamPublisherUnboundEvent.Create( this, streamTraceId, disposingChannel ) );

                if ( disposingStream )
                    await Stream.DisposeDueToLackOfReferencesAsync( streamTraceId ).ConfigureAwait( false );
            }

            using ( MessageBrokerChannelTraceEvent.CreateScope(
                Channel,
                channelTraceId,
                MessageBrokerChannelTraceEventType.UnbindPublisher ) )
            {
                if ( Channel.Logger.ClientTrace is { } clientTrace )
                    clientTrace.Emit( MessageBrokerChannelClientTraceEvent.Create( Channel, channelTraceId, Client, clientTraceId ) );

                if ( Channel.Logger.PublisherUnbound is { } channelPublisherUnbound )
                    channelPublisherUnbound.Emit(
                        MessageBrokerChannelPublisherUnboundEvent.Create( this, channelTraceId, disposingStream ) );

                if ( disposingChannel )
                    await Channel.DisposeDueToLackOfReferencesAsync( channelTraceId ).ConfigureAwait( false );
            }

            using ( Client.AcquireLock() )
            {
                if ( ! Client.IsDisposed )
                    Client.PublishersByChannelId.Remove( Channel.Id );
            }

            using ( AcquireLock() )
                _state = MessageBrokerChannelPublisherBindingState.Disposed;

            if ( Client.Logger.PublisherUnbound is { } publisherUnbound )
                publisherUnbound.Emit(
                    MessageBrokerRemoteClientPublisherUnboundEvent.Create( this, clientTraceId, disposingChannel, disposingStream ) );

            var notification = new Protocol.ChannelBindingDeletedNotification(
                MessageBrokerSystemNotificationType.PublisherDeleted,
                Channel.Name );

            var notificationLength = notification.Length;
            notificationPoolToken = Client.MemoryPool.Rent( notificationLength, clearBuffers, out var responseData );
            notification.Serialize( responseData );

            using ( Client.AcquireLock() )
            {
                if ( Client.IsInactive )
                    exception = Client.IsDisposed ? Client.DeactivatedException( disposed: true ) : null;
                else
                {
                    var writerSource = Client.WriterQueue.AcquireSource( responseData, clearBuffers );
                    ResponseSender.EnqueueUnsafe(
                        Client,
                        notification.Header,
                        writerSource,
                        notificationPoolToken,
                        MessageBrokerRemoteClientTraceEventType.UnbindPublisher,
                        clientTraceId );

                    notificationEnqueued = true;
                    Client.ResponseSender.SignalContinuation();
                }
            }

            if ( exception is not null )
            {
                if ( Client.Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( Client, clientTraceId, exception ) );
            }
        }
        finally
        {
            if ( ! notificationEnqueued )
            {
                notificationPoolToken.Return( Client, clientTraceId );
                if ( Client.Logger.TraceEnd is { } traceEnd )
                    traceEnd.Emit(
                        MessageBrokerRemoteClientTraceEvent.Create(
                            Client,
                            clientTraceId,
                            MessageBrokerRemoteClientTraceEventType.UnbindPublisher ) );
            }
        }
    }
}
