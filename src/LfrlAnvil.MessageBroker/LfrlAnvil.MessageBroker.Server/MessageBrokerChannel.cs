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
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker channel, which allows clients to bind to it as either a publisher or a listener.
/// </summary>
public sealed class MessageBrokerChannel
{
    internal ReferenceStore<int, MessageBrokerChannelPublisherBinding> PublishersByClientId;
    internal ReferenceStore<int, MessageBrokerChannelListenerBinding> ListenersByClientId;
    internal readonly MessageBrokerChannelLogger Logger;
    internal readonly ServerStorage.Channel Storage;

    private readonly TaskCompletionSource _disposed;
    private MessageBrokerChannelState _state;
    private ulong _nextTraceId;
    private ulong? _autoDisposalTraceId;

    internal MessageBrokerChannel(MessageBrokerServer server, int id, string name, ulong nextTraceId = 0)
    {
        Storage = server.Storage.CreateForChannel();
        Server = server;
        Id = id;
        Name = name;
        _state = MessageBrokerChannelState.Running;
        _autoDisposalTraceId = null;
        _nextTraceId = nextTraceId;
        _disposed = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        PublishersByClientId = ReferenceStore<int, MessageBrokerChannelPublisherBinding>.Create();
        ListenersByClientId = ReferenceStore<int, MessageBrokerChannelListenerBinding>.Create();
        Logger = Server.ChannelLoggerFactory?.Invoke( this ) ?? default;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance that owns this channel.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Channel's unique identifier assigned by the server.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Channel's unique name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Current channel's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerChannelState"/> for more information.</remarks>
    public MessageBrokerChannelState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelPublisherBinding"/> instances attached to this channel, identified by client ids.
    /// </summary>
    public MessageBrokerChannelPublisherBindingCollection Publishers => new MessageBrokerChannelPublisherBindingCollection( this );

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelListenerBinding"/> instances attached to this channel, identified by client ids.
    /// </summary>
    public MessageBrokerChannelListenerBindingCollection Listeners => new MessageBrokerChannelListenerBindingCollection( this );

    internal bool IsDisposed => _state >= MessageBrokerChannelState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannel"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Id}] '{Name}' channel ({State})";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingPublisherUnsafe(int clientId)
    {
        PublishersByClientId.Remove( clientId );
        if ( ListenersByClientId.Count > 0 || PublishersByClientId.Count > 0 )
            return false;

        _state = MessageBrokerChannelState.Disposing;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingListenerUnsafe(int clientId)
    {
        ListenersByClientId.Remove( clientId );
        if ( ListenersByClientId.Count > 0 || PublishersByClientId.Count > 0 )
            return false;

        _state = MessageBrokerChannelState.Disposing;
        return true;
    }

    internal async ValueTask TryDisposeDueToLackOfReferencesAsync(ulong serverTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( IsDisposed || ListenersByClientId.Count > 0 || PublishersByClientId.Count > 0 )
                return;

            _state = MessageBrokerChannelState.Disposing;
            traceId = GetTraceId();
        }

        try
        {
            using ( MessageBrokerChannelTraceEvent.CreateScope( this, traceId, MessageBrokerChannelTraceEventType.Dispose ) )
            {
                if ( Logger.ServerTrace is { } serverTrace )
                    serverTrace.Emit( MessageBrokerChannelServerTraceEvent.Create( this, traceId, serverTraceId ) );

                if ( Logger.Disposing is { } disposing )
                    disposing.Emit( MessageBrokerChannelDisposingEvent.Create( this, traceId ) );

                EmitError( await Storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), traceId );
                EmitError( ChannelCollection.Remove( this ), traceId );
                using ( AcquireLock() )
                    _state = MessageBrokerChannelState.Disposed;

                if ( Logger.Disposed is { } disposed )
                    disposed.Emit( MessageBrokerChannelDisposedEvent.Create( this, traceId ) );
            }
        }
        finally
        {
            _disposed.TrySetResult();
        }
    }

    internal async ValueTask OnPublisherDisposingAsync(MessageBrokerRemoteClient client, ulong clientTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return;

            traceId = GetTraceId();
        }

        using ( MessageBrokerChannelTraceEvent.CreateScope( this, traceId, MessageBrokerChannelTraceEventType.UnbindPublisher ) )
        {
            if ( Logger.ClientTrace is { } clientTrace )
                clientTrace.Emit( MessageBrokerChannelClientTraceEvent.Create( this, traceId, client, clientTraceId ) );

            var dispose = false;
            MessageBrokerChannelPublisherBinding? publisher;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                if ( PublishersByClientId.Remove( client.Id, out publisher )
                    && PublishersByClientId.Count == 0
                    && ListenersByClientId.Count == 0 )
                {
                    dispose = true;
                    _state = MessageBrokerChannelState.Disposing;
                }
            }

            if ( publisher is null )
            {
                if ( Logger.Error is { } error )
                {
                    var exc = this.Exception( Resources.NotBoundAsPublisher( this, client ) );
                    error.Emit( MessageBrokerChannelErrorEvent.Create( this, traceId, exc ) );
                }

                return;
            }

            if ( Logger.PublisherUnbound is { } publisherUnbound )
                publisherUnbound.Emit( MessageBrokerChannelPublisherUnboundEvent.Create( publisher, traceId, streamRemoved: false ) );

            if ( dispose )
                await DisposeDueToLackOfReferencesAsync( traceId ).ConfigureAwait( false );
        }
    }

    internal async ValueTask OnListenerDisposingAsync(MessageBrokerRemoteClient client, ulong clientTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return;

            traceId = GetTraceId();
        }

        using ( MessageBrokerChannelTraceEvent.CreateScope( this, traceId, MessageBrokerChannelTraceEventType.UnbindListener ) )
        {
            if ( Logger.ClientTrace is { } clientTrace )
                clientTrace.Emit( MessageBrokerChannelClientTraceEvent.Create( this, traceId, client, clientTraceId ) );

            var dispose = false;
            MessageBrokerChannelListenerBinding? listener;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                if ( ListenersByClientId.Remove( client.Id, out listener )
                    && PublishersByClientId.Count == 0
                    && ListenersByClientId.Count == 0 )
                {
                    dispose = true;
                    _state = MessageBrokerChannelState.Disposing;
                }
            }

            if ( listener is null )
            {
                if ( Logger.Error is { } error )
                {
                    var exc = this.Exception( Resources.NotBoundAsListener( this, client ) );
                    error.Emit( MessageBrokerChannelErrorEvent.Create( this, traceId, exc ) );
                }

                return;
            }

            if ( Logger.ListenerUnbound is { } listenerUnbound )
                listenerUnbound.Emit( MessageBrokerChannelListenerUnboundEvent.Create( listener, traceId, queueRemoved: false ) );

            if ( dispose )
                await DisposeDueToLackOfReferencesAsync( traceId ).ConfigureAwait( false );
        }
    }

    internal void OnServerDisposing(ulong serverTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return;

            _state = MessageBrokerChannelState.Disposing;
            traceId = GetTraceId();
            _autoDisposalTraceId = traceId;
        }

        if ( Logger.TraceStart is { } traceStart )
            traceStart.Emit( MessageBrokerChannelTraceEvent.Create( this, traceId, MessageBrokerChannelTraceEventType.Dispose ) );

        if ( Logger.ServerTrace is { } serverTrace )
            serverTrace.Emit( MessageBrokerChannelServerTraceEvent.Create( this, traceId, serverTraceId ) );

        if ( Logger.Disposing is { } disposing )
            disposing.Emit( MessageBrokerChannelDisposingEvent.Create( this, traceId ) );
    }

    internal async ValueTask OnServerDisposedAsync(ulong serverTraceId, bool storageLoaded)
    {
        ulong? autoDisposalTraceId;
        MessageBrokerChannelState state;

        using ( AcquireLock() )
        {
            Assume.IsGreaterThanOrEqualTo( _state, MessageBrokerChannelState.Disposing );
            state = _state;
            autoDisposalTraceId = _autoDisposalTraceId;
        }

        if ( autoDisposalTraceId is null )
        {
            if ( state == MessageBrokerChannelState.Disposing )
            {
                if ( storageLoaded )
                    Server.EmitError( await Storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), serverTraceId );

                Server.EmitError( await _disposed.Task.AsSafeCancellable().ConfigureAwait( false ), serverTraceId );
            }

            return;
        }

        try
        {
            if ( Server.RootStorageDirectoryPath is not null )
            {
                bool isReferenced;
                using ( AcquireLock() )
                    isReferenced = PublishersByClientId.Count > 0 || ListenersByClientId.Count > 0;

                if ( ! isReferenced && storageLoaded )
                    EmitError( await Storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), autoDisposalTraceId.Value );
                else
                    EmitError(
                        await Storage.SaveMetadataAsync( this, autoDisposalTraceId.Value ).AsSafe().ConfigureAwait( false ),
                        autoDisposalTraceId.Value );
            }

            using ( AcquireLock() )
            {
                PublishersByClientId.Clear();
                ListenersByClientId.Clear();
                _state = MessageBrokerChannelState.Disposed;
            }

            if ( Logger.Disposed is { } disposed )
                disposed.Emit( MessageBrokerChannelDisposedEvent.Create( this, autoDisposalTraceId.Value ) );
        }
        finally
        {
            if ( Logger.TraceEnd is { } traceEnd )
                traceEnd.Emit(
                    MessageBrokerChannelTraceEvent.Create( this, autoDisposalTraceId.Value, MessageBrokerChannelTraceEventType.Dispose ) );

            _disposed.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _disposed );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }

    internal async ValueTask DisposeDueToLackOfReferencesAsync(ulong traceId)
    {
        try
        {
            Assume.Equals( State, MessageBrokerChannelState.Disposing );
            if ( Logger.Disposing is { } disposing )
                disposing.Emit( MessageBrokerChannelDisposingEvent.Create( this, traceId ) );

            EmitError( await Storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), traceId );
            EmitError( ChannelCollection.Remove( this ), traceId );

            using ( AcquireLock() )
            {
                Assume.Equals( PublishersByClientId.Count, 0 );
                Assume.Equals( ListenersByClientId.Count, 0 );
                _state = MessageBrokerChannelState.Disposed;
            }

            if ( Logger.Disposed is { } disposed )
                disposed.Emit( MessageBrokerChannelDisposedEvent.Create( this, traceId ) );
        }
        finally
        {
            _disposed.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerChannelDisposedException? exception)
    {
        var @lock = AcquireLock();
        if ( ! IsDisposed )
        {
            exception = null;
            return @lock;
        }

        @lock.Dispose();
        exception = this.DisposedException();
        if ( Logger.Error is { } error )
            error.Emit( MessageBrokerChannelErrorEvent.Create( this, traceId, exception ) );

        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EmitError(Result result, ulong traceId)
    {
        if ( result.Exception is not null && Logger.Error is { } error )
            error.Emit( MessageBrokerChannelErrorEvent.Create( this, traceId, result.Exception ) );
    }
}
