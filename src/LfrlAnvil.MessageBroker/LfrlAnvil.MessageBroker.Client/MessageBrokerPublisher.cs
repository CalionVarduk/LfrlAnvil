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
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a message broker listener, which allows the client to publish messages to the related channel.
/// </summary>
public sealed class MessageBrokerPublisher
{
    private readonly object _sync = new object();
    private MessageBrokerPublisherState _state;

    internal MessageBrokerPublisher(MessageBrokerClient client, int channelId, string channelName, int streamId, string streamName)
    {
        Client = client;
        ChannelId = channelId;
        ChannelName = channelName;
        StreamId = streamId;
        StreamName = streamName;
        _state = MessageBrokerPublisherState.Bound;
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> instance that owns this publisher.
    /// </summary>
    public MessageBrokerClient Client { get; }

    /// <summary>
    /// Unique id of the channel to which this publisher is related.
    /// </summary>
    public int ChannelId { get; }

    /// <summary>
    /// Unique name of the channel to which this publisher is related.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Unique id of the stream to which this publisher is pushing messages.
    /// </summary>
    public int StreamId { get; }

    /// <summary>
    /// Unique name of the stream to which this publisher is pushing messages.
    /// </summary>
    public string StreamName { get; }

    /// <summary>
    /// Current publisher's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerPublisherState"/> for more information.</remarks>
    public MessageBrokerPublisherState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerPublisher"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[{Client.Id}] '{Client.Name}' => [{ChannelId}] '{ChannelName}' publisher (using [{StreamId}] '{StreamName}' stream) ({State})";
    }

    /// <summary>
    /// Attempts to unbind this publisher from the channel.
    /// </summary>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerUnbindResult"/> instance.
    /// </returns>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <remarks>
    /// Unexpected errors encountered during unbinding will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the client has been successfully unbound from the channel
    /// on the server side, or the publisher is already locally unbound from the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<MessageBrokerUnbindResult>> UnbindAsync()
    {
        return PublisherCollection.UnbindAsync( this );
    }

    /// <summary>
    /// Sends a message to the bound channel.
    /// </summary>
    /// <param name="data">Message to send.</param>
    /// <param name="clearBufferOnDispose">
    /// Specifies whether or not to clear the internal memory buffer when it's returned to the pool. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MesageBrokerSendResult"/> instance.
    /// </returns>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <remarks>
    /// Unexpected errors encountered during sending will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the message has been successfully enqueued on the server side,
    /// or the publisher is already locally unbound from the channel, which will cancel the request to the server.
    /// </remarks>
    public async ValueTask<Result<MesageBrokerSendResult>> SendAsync(ReadOnlyMemory<byte> data, bool clearBufferOnDispose = false)
    {
        using var context = GetSendContext( MemorySize.FromBytes( data.Length ), clearBufferOnDispose );
        return await context.Append( data.Span ).SendAsync().ConfigureAwait( false );
    }

    /// <summary>
    /// Acquires a <see cref="MessageBrokerSendContext"/> instance which gives access to the internal memory pool
    /// and allows to send a message to the bound channel.
    /// </summary>
    /// <param name="minCapacity">
    /// Specifies minimum initial capacity of the allocated memory buffer.
    /// Equal to <b>null</b> by default, which will cause the minimum of <b>1KB</b> to be used.
    /// </param>
    /// <param name="clearBufferOnDispose">
    /// Specifies whether or not to clear the internal memory buffer when it's returned to the pool. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>Pooled <see cref="MessageBrokerSendContext"/> instance.</returns>
    public MessageBrokerSendContext GetSendContext(MemorySize? minCapacity = null, bool clearBufferOnDispose = false)
    {
        return Client.RentMessageContext( this, minCapacity ?? MemorySize.Zero, clearBufferOnDispose );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool Dispose()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerPublisherState.Disposed )
                return false;

            _state = MessageBrokerPublisherState.Disposed;
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void OnClientDisposed()
    {
        using ( AcquireLock() )
            _state = MessageBrokerPublisherState.Disposed;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }
}
