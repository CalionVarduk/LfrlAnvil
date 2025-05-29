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
    internal readonly MessageBrokerStreamLogger Logger;

    private readonly object _sync = new object();
    private QueueSlim<StreamMessage> _messages;
    private MessageBrokerStreamState _state;
    private ulong _nextMessageId;
    private ulong _nextTraceId;

    internal MessageBrokerStream(MessageBrokerServer server, int id, string name)
    {
        Server = server;
        Id = id;
        Name = name;
        _nextMessageId = 0;
        _nextTraceId = 0;
        _state = MessageBrokerStreamState.Running;
        _messages = QueueSlim<StreamMessage>.Create();
        PublishersByClientChannelIdPair = ReferenceStore<Pair<int, int>, MessageBrokerChannelPublisherBinding>.Create();
        StreamProcessor = StreamProcessor.Create();
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
    internal Protocol.MessageRejectedResponse.Reasons PushMessage(
        MessageBrokerChannelPublisherBinding publisher,
        MemoryPoolToken<byte> token,
        ReadOnlyMemory<byte> data,
        ref ulong? messageId,
        ref ulong streamTraceId)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return Protocol.MessageRejectedResponse.Reasons.Cancelled;

            ulong id;
            using ( publisher.AcquireLock() )
            {
                if ( publisher.ShouldCancel )
                    return Protocol.MessageRejectedResponse.Reasons.Cancelled;

                id = unchecked( _nextMessageId++ );
                _messages.Enqueue( new StreamMessage( id, publisher.Client.GetTimestamp(), publisher, token, data ) );
            }

            messageId = id;
            streamTraceId = GetTraceId();
            StreamProcessor.SignalContinuation();
        }

        return Protocol.MessageRejectedResponse.Reasons.None;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MessageBrokerChannel? CopyMessagesIntoUnsafe(ref ListSlim<StreamMessage> buffer)
    {
        Assume.True( buffer.IsEmpty );
        Assume.IsGreaterThan( buffer.Capacity, 0 );
        if ( _messages.IsEmpty )
            return null;

        var queueSlice = _messages.AsMemory();
        if ( queueSlice.Length > buffer.Capacity )
            queueSlice = queueSlice.Slice( 0, buffer.Capacity );

        var firstSlice = queueSlice.First.Span;
        var firstMessage = firstSlice[0];
        var channel = firstMessage.Publisher.Channel;
        buffer.Add( firstMessage );

        if ( ! CopyMessagesInto( firstSlice.Slice( 1 ), ref buffer, channel ) )
            CopyMessagesInto( queueSlice.Second.Span, ref buffer, channel );

        return channel;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void DequeueMessagesUnsafe(int count)
    {
        _messages.DequeueRange( count );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingPublisherUnsafe(int clientId, int channelId)
    {
        PublishersByClientChannelIdPair.Remove( new Pair<int, int>( clientId, channelId ) );
        if ( PublishersByClientChannelIdPair.Count > 0 || ! _messages.IsEmpty )
            return false;

        _state = MessageBrokerStreamState.Disposing;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeDueToEmptyQueueUnsafe()
    {
        Assume.True( _messages.IsEmpty );
        if ( PublishersByClientChannelIdPair.Count > 0 )
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
            MessageBrokerStreamClientTraceEvent.Create( this, traceId, client, clientTraceId ).Emit( Logger.ClientTrace );

            var dispose = false;
            MessageBrokerChannelPublisherBinding? publisher;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                if ( PublishersByClientChannelIdPair.Remove( new Pair<int, int>( client.Id, channel.Id ), out publisher )
                    && PublishersByClientChannelIdPair.Count == 0
                    && _messages.IsEmpty )
                {
                    dispose = true;
                    _state = MessageBrokerStreamState.Disposing;
                }
            }

            if ( publisher is null )
            {
                var error = new MessageBrokerStreamException( this, Resources.NotBoundAsPublisher( this, client, channel ) );
                MessageBrokerStreamErrorEvent.Create( this, traceId, error ).Emit( Logger.Error );
                return;
            }

            MessageBrokerStreamPublisherUnboundEvent.Create( publisher, traceId, channelRemoved: false ).Emit( Logger.PublisherUnbound );
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
            MessageBrokerStreamServerTraceEvent.Create( this, traceId, serverTraceId ).Emit( Logger.ServerTrace );
            MessageBrokerStreamDisposingEvent.Create( this, traceId ).Emit( Logger.Disposing );

            Task? processorTask;
            using ( AcquireLock() )
            {
                PublishersByClientChannelIdPair.Clear();
                processorTask = StreamProcessor.DiscardUnderlyingTask();
                StreamProcessor.Dispose();
            }

            if ( processorTask is not null )
                await processorTask.ConfigureAwait( false );

            int discardedMessageCount;
            Chain<Exception> exceptions;
            using ( AcquireLock() )
                (discardedMessageCount, exceptions) = ClearMessages();

            foreach ( var exc in exceptions )
                MessageBrokerStreamErrorEvent.Create( this, traceId, exc ).Emit( Logger.Error );

            if ( discardedMessageCount > 0 )
            {
                var error = new MessageBrokerStreamException( this, Resources.StreamMessagesDiscarded( discardedMessageCount ) );
                MessageBrokerStreamErrorEvent.Create( this, traceId, error ).Emit( Logger.Error );
            }

            using ( AcquireLock() )
                _state = MessageBrokerStreamState.Disposed;

            MessageBrokerStreamDisposedEvent.Create( this, traceId ).Emit( Logger.Disposed );
        }
    }

    internal async ValueTask DisposeDueToLackOfReferencesAsync(bool ignoreProcessorTask, ulong traceId)
    {
        Assume.Equals( State, MessageBrokerStreamState.Disposing );
        MessageBrokerStreamDisposingEvent.Create( this, traceId ).Emit( Logger.Disposing );

        Task? processorTask;
        using ( AcquireLock() )
        {
            Assume.Equals( PublishersByClientChannelIdPair.Count, 0 );
            Assume.True( _messages.IsEmpty );

            processorTask = StreamProcessor.DiscardUnderlyingTask();
            if ( ignoreProcessorTask )
                processorTask = null;

            StreamProcessor.Dispose();
        }

        if ( processorTask is not null )
            await processorTask.ConfigureAwait( false );

        var exc = StreamCollection.Remove( this ).Exception;
        if ( exc is not null )
            MessageBrokerStreamErrorEvent.Create( this, traceId, exc ).Emit( Logger.Error );

        using ( AcquireLock() )
            _state = MessageBrokerStreamState.Disposed;

        MessageBrokerStreamDisposedEvent.Create( this, traceId ).Emit( Logger.Disposed );
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
        exception = new MessageBrokerStreamDisposedException( this );
        MessageBrokerStreamErrorEvent.Create( this, traceId, exception ).Emit( Logger.Error );
        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool CopyMessagesInto(
        ReadOnlySpan<StreamMessage> source,
        ref ListSlim<StreamMessage> target,
        MessageBrokerChannel channel)
    {
        foreach ( ref readonly var message in source )
        {
            Assume.IsLessThan( target.Count, target.Capacity );
            if ( ! ReferenceEquals( channel, message.Publisher.Channel ) )
                return true;

            target.Add( message );
        }

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (int DiscardedMessageCount, Chain<Exception> Exceptions) ClearMessages()
    {
        var discardedMessageCount = _messages.Count;
        var exceptions = Chain<Exception>.Empty;

        foreach ( ref readonly var message in _messages )
        {
            var exc = message.Return();
            if ( exc is not null )
                exceptions = exceptions.Extend( exc );
        }

        _messages = QueueSlim<StreamMessage>.Create();
        return (discardedMessageCount, exceptions);
    }
}
