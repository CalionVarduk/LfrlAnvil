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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
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
    private InterlockedBoolean _isEphemeral;
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
        _isEphemeral = new InterlockedBoolean( isEphemeral );
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
    public bool IsEphemeral => _isEphemeral.Value;

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
            MessageBrokerChannelPublisherBindingState.Running );
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsEphemeral()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerChannelPublisherBindingState.Inactive )
                _isEphemeral.WriteTrue();
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
            if ( _state == MessageBrokerChannelPublisherBindingState.Disposed )
                return;

            if ( keepAlive && ! IsEphemeral )
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
            isEphemeral = IsEphemeral;
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
            state = State;
            Assume.IsGreaterThanOrEqualTo( _state, MessageBrokerChannelPublisherBindingState.Deactivating );
            if ( state == MessageBrokerChannelPublisherBindingState.Inactive && keepAlive )
                return;

            autoDisposed = _autoDisposed;
            deactivated = _deactivated;
            dispose = IsEphemeral || ! keepAlive;
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
    internal void BeginDisposingUnsafe()
    {
        Assume.Equals( _state, MessageBrokerChannelPublisherBindingState.Running );
        _state = MessageBrokerChannelPublisherBindingState.Disposing;
        _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
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
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }
}
