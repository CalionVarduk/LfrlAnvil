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

    private readonly object _sync = new object();
    private readonly MessageBrokerStreamEventHandler? _eventHandler;
    private QueueSlim<StreamMessage> _messages;
    private ulong _nextMessageId;
    private MessageBrokerStreamState _state;

    internal MessageBrokerStream(MessageBrokerServer server, int id, string name)
    {
        Server = server;
        Id = id;
        Name = name;
        _nextMessageId = 0;
        _state = MessageBrokerStreamState.Running;
        _messages = QueueSlim<StreamMessage>.Create();
        PublishersByClientChannelIdPair = ReferenceStore<Pair<int, int>, MessageBrokerChannelPublisherBinding>.Create();
        StreamProcessor = StreamProcessor.Create();
        _eventHandler = Server.StreamEventHandlerFactory?.Invoke( this );
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
        ulong traceId,
        ref ulong? messageId)
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
            Emit( MessageBrokerStreamEvent.MessageEnqueued( this, publisher, id, traceId ) );
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
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            if ( ! PublishersByClientChannelIdPair.Remove( new Pair<int, int>( client.Id, channel.Id ) )
                || PublishersByClientChannelIdPair.Count > 0
                || ! _messages.IsEmpty )
                return;

            _state = MessageBrokerStreamState.Disposing;
        }

        await DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false ).ConfigureAwait( false );
    }

    internal async ValueTask OnServerDisposedAsync()
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerStreamState.Disposing;
        }

        Emit( MessageBrokerStreamEvent.Disposing( this ) );

        Task? processorTask;
        using ( AcquireLock() )
        {
            PublishersByClientChannelIdPair.Clear();
            ClearMessages();
            processorTask = StreamProcessor.DiscardUnderlyingTask();
            StreamProcessor.Dispose();
        }

        if ( processorTask is not null )
            await processorTask.ConfigureAwait( false );

        using ( AcquireLock() )
            _state = MessageBrokerStreamState.Disposed;

        Emit( MessageBrokerStreamEvent.Disposed( this ) );
    }

    internal async ValueTask DisposeDueToLackOfReferencesAsync(bool ignoreProcessorTask)
    {
        Assume.Equals( State, MessageBrokerStreamState.Disposing );
        Emit( MessageBrokerStreamEvent.Disposing( this ) );

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
            Emit( MessageBrokerStreamEvent.Unexpected( this, exc ) );

        using ( AcquireLock() )
            _state = MessageBrokerStreamState.Disposed;

        Emit( MessageBrokerStreamEvent.Disposed( this ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Emit(MessageBrokerStreamEvent e)
    {
        if ( _eventHandler is null )
            return;

        try
        {
            _eventHandler( e );
        }
        catch
        {
            // NOTE: do nothing
        }
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
    private void ClearMessages()
    {
        // TODO: emit 'message-discarded' (log refactor)
        foreach ( ref readonly var message in _messages )
            message.Return();

        _messages = QueueSlim<StreamMessage>.Create();
    }
}
