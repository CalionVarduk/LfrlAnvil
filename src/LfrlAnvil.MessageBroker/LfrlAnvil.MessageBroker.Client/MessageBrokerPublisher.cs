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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
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

    internal MessageBrokerPublisher(MessageBrokerClient client, int channelId, string channelName)
    {
        Client = client;
        ChannelId = channelId;
        ChannelName = channelName;
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
        return $"[{Client.Id}] '{Client.Name}' => [{ChannelId}] '{ChannelName}' publisher ({State})";
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
        return ExclusiveLock.Enter( _sync );
    }
}
