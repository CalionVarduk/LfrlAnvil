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
using System.Collections.Generic;
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
    /// with underlying <see cref="MessageBrokerUnbindPublisherResult"/> instance.
    /// </returns>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <remarks>
    /// Unexpected errors encountered during publisher unbinding will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the publisher has been successfully unbound from the channel
    /// on the server side, or the publisher is already locally unbound from the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<MessageBrokerUnbindPublisherResult>> UnbindAsync()
    {
        return PublisherCollection.UnbindAsync( this );
    }

    /// <summary>
    /// Pushes a message to the bound channel.
    /// </summary>
    /// <param name="data">Message to push.</param>
    /// <param name="targets">Optional collection of explicit routing targets. Equal to <b>null</b> by default.</param>
    /// <param name="confirm">
    /// Specifies whether or not the server should send confirmation that it received the message. Equal to <b>true</b> by default.
    /// </param>
    /// <param name="clearBuffer">
    /// Specifies whether or not to clear the internal memory buffer when it's returned to the pool. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerPushResult"/> instance.
    /// </returns>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When one of routing <paramref name="targets"/> contains client id that is less than or equal to <b>0</b>
    /// or when one of routing <paramref name="targets"/> contains client name whose length is
    /// less than <b>1</b> or greater than <b>512</b>.
    /// </exception>
    /// <exception cref="InvalidOperationException">When routing target count limit of <b>32767</b> has been exceeded.</exception>
    /// <remarks>
    /// Unexpected errors encountered during pushing will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the message has been successfully enqueued on the server side,
    /// or the publisher is already locally unbound from the channel, which will cancel the request to the server.
    /// </remarks>
    public async ValueTask<Result<MessageBrokerPushResult>> PushAsync(
        ReadOnlyMemory<byte> data,
        IEnumerable<MessageBrokerClientRoutingTarget>? targets = null,
        bool confirm = true,
        bool clearBuffer = false)
    {
        using var context = GetPushContext( MemorySize.FromBytes( data.Length ), clearBuffer );
        if ( targets is not null )
        {
            foreach ( var target in targets )
            {
                if ( target.IsFromName )
                    context.AddTarget( target.Name );
                else
                    context.AddTarget( target.Id );
            }
        }

        return await context.Append( data.Span ).PushAsync( confirm ).ConfigureAwait( false );
    }

    /// <summary>
    /// Acquires a <see cref="MessageBrokerPushContext"/> instance which gives access to the internal memory pool
    /// and allows to push a message to the bound channel.
    /// </summary>
    /// <param name="minCapacity">
    /// Specifies minimum initial capacity of the allocated memory buffer.
    /// Equal to <b>null</b> by default, which will cause the minimum of <b>1KB</b> to be used.
    /// </param>
    /// <param name="clearBufferOnDispose">
    /// Specifies whether or not to clear the internal memory buffer when it's returned to the pool. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>Pooled <see cref="MessageBrokerPushContext"/> instance.</returns>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    public MessageBrokerPushContext GetPushContext(MemorySize? minCapacity = null, bool clearBufferOnDispose = false)
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
