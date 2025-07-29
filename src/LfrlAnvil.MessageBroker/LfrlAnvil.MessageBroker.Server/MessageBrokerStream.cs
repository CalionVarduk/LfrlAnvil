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
using LfrlAnvil.Async;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker stream, which allows <see cref="MessageBrokerChannelPublisherBinding"/> instances to push messages
/// in order to be processed and moved to relevant <see cref="MessageBrokerChannelListenerBinding"/> instances.
/// </summary>
public sealed class MessageBrokerStream
{
    internal ReferenceStore<Pair<int, int>, MessageBrokerChannelPublisherBinding> PublishersByClientChannelIdPair;
    internal StreamProcessor StreamProcessor;
    internal StreamMessageStore MessageStore;
    internal readonly MessageBrokerStreamLogger Logger;

    private readonly object _sync = new object();
    private MessageBrokerStreamState _state;
    private ulong _nextTraceId;

    internal MessageBrokerStream(MessageBrokerServer server, int id, string name)
    {
        Server = server;
        Id = id;
        Name = name;
        _nextTraceId = 0;
        _state = MessageBrokerStreamState.Running;
        PublishersByClientChannelIdPair = ReferenceStore<Pair<int, int>, MessageBrokerChannelPublisherBinding>.Create();
        StreamProcessor = StreamProcessor.Create();
        MessageStore = StreamMessageStore.Create();
        Logger = server.StreamLoggerFactory?.Invoke( this ) ?? default;
        StreamProcessor.SetUnderlyingTask( StreamProcessor.StartUnderlyingTask( this ) );
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance that owns this stream.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Stream's unique identifier assigned by the server.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Stream's unique name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Current stream's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerStreamState"/> for more information.</remarks>
    public MessageBrokerStreamState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelPublisherBinding"/> instances attached to this stream,
    /// identified by (client-id, channel-id) tuples.
    /// </summary>
    public MessageBrokerStreamPublisherCollection Publishers => new MessageBrokerStreamPublisherCollection( this );

    /// <summary>
    /// Collection of messages stored in this stream.
    /// </summary>
    public MessageBrokerStreamMessageCollection Messages => new MessageBrokerStreamMessageCollection( this );

    internal bool ShouldCancel => _state >= MessageBrokerStreamState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerStream"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Id}] '{Name}' stream ({State})";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal PushMessageResult PushMessage(
        MessageBrokerChannelPublisherBinding publisher,
        MemoryPoolToken<byte> token,
        ReadOnlyMemory<byte> data,
        in MessageRouting routing,
        ref ulong messageId,
        ref int storeKey,
        ref ulong streamTraceId)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return PushMessageResult.StreamDisposed;

            using ( publisher.AcquireLock() )
            {
                if ( publisher.ShouldCancel )
                    return PushMessageResult.BindingDisposed;

                messageId = MessageStore.Add( publisher, token, data, in routing, out storeKey );
            }

            streamTraceId = GetTraceId();
            StreamProcessor.SignalContinuation();
        }

        return PushMessageResult.Success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingPublisherUnsafe(int clientId, int channelId)
    {
        PublishersByClientChannelIdPair.Remove( new Pair<int, int>( clientId, channelId ) );
        if ( PublishersByClientChannelIdPair.Count > 0 || ! MessageStore.IsEmpty )
            return false;

        _state = MessageBrokerStreamState.Disposing;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeDueToEmptyMessageStoreUnsafe()
    {
        if ( ! MessageStore.IsEmpty || PublishersByClientChannelIdPair.Count > 0 )
            return false;

        _state = MessageBrokerStreamState.Disposing;
        return true;
    }

    internal async ValueTask OnPublisherDisposingAsync(MessageBrokerRemoteClient client, MessageBrokerChannel channel, ulong clientTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            traceId = GetTraceId();
        }

        using ( MessageBrokerStreamTraceEvent.CreateScope( this, traceId, MessageBrokerStreamTraceEventType.UnbindPublisher ) )
        {
            if ( Logger.ClientTrace is { } clientTrace )
                clientTrace.Emit( MessageBrokerStreamClientTraceEvent.Create( this, traceId, client, clientTraceId ) );

            var dispose = false;
            MessageBrokerChannelPublisherBinding? publisher;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                if ( PublishersByClientChannelIdPair.Remove( new Pair<int, int>( client.Id, channel.Id ), out publisher )
                    && PublishersByClientChannelIdPair.Count == 0
                    && MessageStore.IsEmpty )
                {
                    dispose = true;
                    _state = MessageBrokerStreamState.Disposing;
                }
            }

            if ( publisher is null )
            {
                if ( Logger.Error is { } error )
                {
                    var exc = this.Exception( Resources.NotBoundAsPublisher( this, client, channel ) );
                    error.Emit( MessageBrokerStreamErrorEvent.Create( this, traceId, exc ) );
                }

                return;
            }

            if ( Logger.PublisherUnbound is { } publisherUnbound )
                publisherUnbound.Emit( MessageBrokerStreamPublisherUnboundEvent.Create( publisher, traceId, channelRemoved: false ) );

            if ( dispose )
                await DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false, traceId ).ConfigureAwait( false );
        }
    }

    internal async ValueTask OnServerDisposedAsync(ulong serverTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerStreamState.Disposing;
            traceId = GetTraceId();
        }

        using ( MessageBrokerStreamTraceEvent.CreateScope( this, traceId, MessageBrokerStreamTraceEventType.Dispose ) )
        {
            if ( Logger.ServerTrace is { } serverTrace )
                serverTrace.Emit( MessageBrokerStreamServerTraceEvent.Create( this, traceId, serverTraceId ) );

            if ( Logger.Disposing is { } disposing )
                disposing.Emit( MessageBrokerStreamDisposingEvent.Create( this, traceId ) );

            Task? processorTask;
            using ( AcquireLock() )
            {
                PublishersByClientChannelIdPair.Clear();
                processorTask = StreamProcessor.DiscardUnderlyingTask();
                StreamProcessor.Dispose();
            }

            if ( processorTask is not null )
                await processorTask.ConfigureAwait( false );

            var error = Logger.Error;
            int discardedMessageCount;
            Chain<Exception> exceptions;
            using ( AcquireLock() )
                (discardedMessageCount, exceptions) = MessageStore.ClearPending( error is not null );

            foreach ( var exc in exceptions )
            {
                Assume.IsNotNull( error );
                error.Emit( MessageBrokerStreamErrorEvent.Create( this, traceId, exc ) );
            }

            if ( discardedMessageCount > 0 && error is not null )
            {
                var exc = this.Exception( Resources.StreamMessagesDiscarded( discardedMessageCount ) );
                error.Emit( MessageBrokerStreamErrorEvent.Create( this, traceId, exc ) );
            }

            using ( AcquireLock() )
                _state = MessageBrokerStreamState.Disposed;

            if ( Logger.Disposed is { } disposed )
                disposed.Emit( MessageBrokerStreamDisposedEvent.Create( this, traceId ) );
        }
    }

    internal async ValueTask DisposeDueToLackOfReferencesAsync(bool ignoreProcessorTask, ulong traceId)
    {
        Assume.Equals( State, MessageBrokerStreamState.Disposing );
        if ( Logger.Disposing is { } disposing )
            disposing.Emit( MessageBrokerStreamDisposingEvent.Create( this, traceId ) );

        Task? processorTask;
        using ( AcquireLock() )
        {
            Assume.Equals( PublishersByClientChannelIdPair.Count, 0 );
            Assume.True( MessageStore.IsEmpty );

            processorTask = StreamProcessor.DiscardUnderlyingTask();
            if ( ignoreProcessorTask )
                processorTask = null;

            StreamProcessor.Dispose();
            MessageStore.Clear();
        }

        if ( processorTask is not null )
            await processorTask.ConfigureAwait( false );

        var exc = StreamCollection.Remove( this ).Exception;
        if ( exc is not null && Logger.Error is { } error )
            error.Emit( MessageBrokerStreamErrorEvent.Create( this, traceId, exc ) );

        using ( AcquireLock() )
            _state = MessageBrokerStreamState.Disposed;

        if ( Logger.Disposed is { } disposed )
            disposed.Emit( MessageBrokerStreamDisposedEvent.Create( this, traceId ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ClearMessageStore()
    {
        Assume.IsGreaterThanOrEqualTo( State, MessageBrokerStreamState.Disposing );

        Chain<Exception> exceptions;
        using ( AcquireLock() )
            exceptions = MessageStore.Clear();

        if ( exceptions.Count == 0 )
            return;

        ulong traceId;
        using ( AcquireLock() )
            traceId = GetTraceId();

        using ( MessageBrokerStreamTraceEvent.CreateScope( this, traceId, MessageBrokerStreamTraceEventType.Unexpected ) )
        {
            if ( Logger.Error is { } error )
            {
                foreach ( var exc in exceptions )
                    error.Emit( MessageBrokerStreamErrorEvent.Create( this, traceId, exc ) );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerStreamDisposedException? exception)
    {
        var @lock = AcquireLock();
        if ( ! ShouldCancel )
        {
            exception = null;
            return @lock;
        }

        @lock.Dispose();
        exception = this.DisposedException();
        if ( Logger.Error is { } error )
            error.Emit( MessageBrokerStreamErrorEvent.Create( this, traceId, exception ) );

        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }
}
