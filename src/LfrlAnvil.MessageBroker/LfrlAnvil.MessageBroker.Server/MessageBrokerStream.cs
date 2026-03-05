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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;
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
    internal readonly ServerStorage.Stream Storage;

    private readonly TaskCompletionSource _disposed;
    private MessageBrokerStreamState _state;
    private ulong _nextTraceId;
    private ulong? _autoDisposalTraceId;

    internal MessageBrokerStream(MessageBrokerServer server, int id, string name, ulong nextTraceId = 0)
    {
        Storage = server.Storage.CreateForStream();
        Server = server;
        Id = id;
        Name = name;
        _nextTraceId = nextTraceId;
        _state = MessageBrokerStreamState.Running;
        _autoDisposalTraceId = null;
        _disposed = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        PublishersByClientChannelIdPair = ReferenceStore<Pair<int, int>, MessageBrokerChannelPublisherBinding>.Create();
        StreamProcessor = StreamProcessor.Create();
        MessageStore = StreamMessageStore.Create();
        Logger = Server.StreamLoggerFactory?.Invoke( this ) ?? default;
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

    internal bool IsDisposed => _state >= MessageBrokerStreamState.Disposing;

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
    internal void FinalizeMessageReferences(ulong serverTraceId)
    {
        Chain<Exception> exceptions;
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return;

            exceptions = MessageStore.FinalizeMessageReferences();
        }

        Server.EmitErrors( ref exceptions, serverTraceId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void StartProcessor()
    {
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return;

            if ( StreamProcessor.GetUnderlyingTask() is null )
                StreamProcessor.SetUnderlyingTask( StreamProcessor.StartUnderlyingTask( this ) );

            StreamProcessor.SignalContinuation();
        }
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
            if ( IsDisposed )
                return PushMessageResult.StreamDisposed;

            using ( publisher.AcquireLock() )
            {
                if ( publisher.IsInactive )
                    return publisher.IsDisposed ? PushMessageResult.BindingDisposed : PushMessageResult.BindingInactive;

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
        PublishersByClientChannelIdPair.Remove( Pair.Create( clientId, channelId ) );
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
            if ( IsDisposed )
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

                if ( PublishersByClientChannelIdPair.Remove( Pair.Create( client.Id, channel.Id ), out publisher )
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

    internal async ValueTask OnServerDisposingAsync(ulong serverTraceId)
    {
        var traceId = 0UL;
        MessageBrokerStreamState state;
        var result = Result.Valid;
        Task? processorTask = null;

        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerStreamState.Disposed )
                return;

            state = _state;
            if ( _state != MessageBrokerStreamState.Disposing )
            {
                _state = MessageBrokerStreamState.Disposing;
                traceId = GetTraceId();
                _autoDisposalTraceId = traceId;
            }
            else
            {
                processorTask = StreamProcessor.DiscardUnderlyingTask();
                result = StreamProcessor.BeginDispose();
            }
        }

        if ( state == MessageBrokerStreamState.Disposing )
        {
            Server.EmitError( result, serverTraceId );
            Server.EmitError( await processorTask.AsSafeNonCancellable().ConfigureAwait( false ), serverTraceId );
            return;
        }

        if ( Logger.TraceStart is { } traceStart )
            traceStart.Emit( MessageBrokerStreamTraceEvent.Create( this, traceId, MessageBrokerStreamTraceEventType.Dispose ) );

        if ( Logger.ServerTrace is { } serverTrace )
            serverTrace.Emit( MessageBrokerStreamServerTraceEvent.Create( this, traceId, serverTraceId ) );

        if ( Logger.Disposing is { } disposing )
            disposing.Emit( MessageBrokerStreamDisposingEvent.Create( this, traceId ) );

        using ( AcquireLock() )
        {
            processorTask = StreamProcessor.DiscardUnderlyingTask();
            result = StreamProcessor.BeginDispose();
        }

        EmitError( result, traceId );
        EmitError( await processorTask.AsSafeNonCancellable().ConfigureAwait( false ), traceId );
    }

    internal async ValueTask OnServerDisposedAsync(ulong serverTraceId, bool storageLoaded)
    {
        ulong? autoDisposalTraceId;
        MessageBrokerStreamState state;

        using ( AcquireLock() )
        {
            Assume.IsGreaterThanOrEqualTo( _state, MessageBrokerStreamState.Disposing );
            state = _state;
            autoDisposalTraceId = _autoDisposalTraceId;
        }

        if ( autoDisposalTraceId is null )
        {
            if ( state == MessageBrokerStreamState.Disposing )
            {
                if ( storageLoaded )
                    Server.EmitError( await Storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), serverTraceId );

                Server.EmitError( await _disposed.Task.AsSafeCancellable().ConfigureAwait( false ), serverTraceId );
            }

            return;
        }

        try
        {
            var exceptions = Chain<Exception>.Empty;

            if ( Server.RootStorageDirectoryPath is not null )
            {
                bool hasPublishers;
                NullableIndex nextPendingNodeId;
                ulong nextMessageId;
                ListSlim<KeyValuePair<int, StreamMessage>> messages;
                ListSlim<KeyValuePair<ulong, ReadOnlyMemory<byte>>> routings;

                using ( AcquireLock() )
                {
                    hasPublishers = PublishersByClientChannelIdPair.Count > 0;
                    nextPendingNodeId = MessageStore.GetNextPendingNodeId();
                    nextMessageId = MessageStore.GetNextMessageId();
                    messages = MessageStore.GetMessages();
                    routings = MessageStore.GetRoutings();
                }

                if ( ! hasPublishers && messages.IsEmpty && storageLoaded )
                    EmitError( await Storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), autoDisposalTraceId.Value );
                else
                {
                    EmitError(
                        await Storage.SaveMetadataAsync( this, autoDisposalTraceId.Value ).AsSafe().ConfigureAwait( false ),
                        autoDisposalTraceId.Value );

                    if ( storageLoaded )
                        EmitError(
                            await Storage.SaveAsync( this, nextPendingNodeId, nextMessageId, messages, routings, autoDisposalTraceId.Value )
                                .AsSafe()
                                .ConfigureAwait( false ),
                            autoDisposalTraceId.Value );
                }
            }
            else
            {
                int discardedMessageCount;
                using ( AcquireLock() )
                    discardedMessageCount = MessageStore.ClearPending( ref exceptions );

                EmitErrors( ref exceptions, autoDisposalTraceId.Value );
                if ( discardedMessageCount > 0 && Logger.Error is { } error )
                {
                    var exc = this.Exception( Resources.StreamMessagesDiscarded( discardedMessageCount ) );
                    error.Emit( MessageBrokerStreamErrorEvent.Create( this, autoDisposalTraceId.Value, exc ) );
                }
            }

            using ( AcquireLock() )
            {
                PublishersByClientChannelIdPair.Clear();
                exceptions = MessageStore.Clear();
                _state = MessageBrokerStreamState.Disposed;
            }

            EmitErrors( ref exceptions, autoDisposalTraceId.Value );
            if ( Logger.Disposed is { } disposed )
                disposed.Emit( MessageBrokerStreamDisposedEvent.Create( this, autoDisposalTraceId.Value ) );
        }
        finally
        {
            if ( Logger.TraceEnd is { } traceEnd )
                traceEnd.Emit(
                    MessageBrokerStreamTraceEvent.Create( this, autoDisposalTraceId.Value, MessageBrokerStreamTraceEventType.Dispose ) );

            _disposed.TrySetResult();
        }
    }

    internal async ValueTask DisposeDueToLackOfReferencesAsync(bool ignoreProcessorTask, ulong traceId)
    {
        try
        {
            Assume.Equals( State, MessageBrokerStreamState.Disposing );
            if ( Logger.Disposing is { } disposing )
                disposing.Emit( MessageBrokerStreamDisposingEvent.Create( this, traceId ) );

            Result result;
            Task? processorTask;
            using ( AcquireLock() )
            {
                Assume.Equals( PublishersByClientChannelIdPair.Count, 0 );
                Assume.True( MessageStore.IsEmpty );

                processorTask = StreamProcessor.GetUnderlyingTask();
                if ( ignoreProcessorTask )
                    processorTask = null;

                result = StreamProcessor.BeginDispose();
                MessageStore.Clear();
            }

            EmitError( result, traceId );
            EmitError( await processorTask.AsSafeNonCancellable().ConfigureAwait( false ), traceId );
            EmitError( await Storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), traceId );
            EmitError( StreamCollection.Remove( this ), traceId );

            using ( AcquireLock() )
            {
                _ = StreamProcessor.DiscardUnderlyingTask();
                _state = MessageBrokerStreamState.Disposed;
            }

            if ( Logger.Disposed is { } disposed )
                disposed.Emit( MessageBrokerStreamDisposedEvent.Create( this, traceId ) );
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
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerStreamDisposedException? exception)
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
            error.Emit( MessageBrokerStreamErrorEvent.Create( this, traceId, exception ) );

        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EmitError(Result result, ulong traceId)
    {
        if ( result.Exception is not null && Logger.Error is { } error )
            error.Emit( MessageBrokerStreamErrorEvent.Create( this, traceId, result.Exception ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EmitErrors(ref Chain<Exception> exceptions, ulong traceId)
    {
        if ( exceptions.Count > 0 && Logger.Error is { } error )
        {
            foreach ( var exc in exceptions )
                error.Emit( MessageBrokerStreamErrorEvent.Create( this, traceId, exc ) );
        }

        exceptions = Chain<Exception>.Empty;
    }
}
