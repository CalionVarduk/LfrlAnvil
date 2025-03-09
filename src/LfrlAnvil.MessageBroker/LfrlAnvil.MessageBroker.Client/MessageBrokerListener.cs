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

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a message broker listener, which allows the client to react to messages published to the related channel.
/// </summary>
public sealed class MessageBrokerListener
{
    private readonly object _sync = new object();
    private MessageBrokerListenerState _state;

    internal MessageBrokerListener(MessageBrokerClient client, int channelId, string channelName)
    {
        Client = client;
        ChannelId = channelId;
        ChannelName = channelName;
        _state = MessageBrokerListenerState.Listening;
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> instance that owns this subscription.
    /// </summary>
    public MessageBrokerClient Client { get; }

    /// <summary>
    /// Unique id of the channel to which this listener is related.
    /// </summary>
    public int ChannelId { get; }

    /// <summary>
    /// Unique name of the channel to which this listener is related.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Current listener's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerListenerState"/> for more information.</remarks>
    public MessageBrokerListenerState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Attempts to unsubscribe this listener from the channel.
    /// </summary>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerChannelUnsubscribeResult"/> instance.
    /// </returns>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <remarks>
    /// Unexpected errors encountered during unsubscribing will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the client has been successfully unsubscribed from the channel
    /// on the server side, or the listener is already locally unsubscribed from the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<MessageBrokerChannelUnsubscribeResult>> UnsubscribeAsync()
    {
        return ListenerCollection.UnsubscribeAsync( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void OnClientDisposed()
    {
        using ( AcquireLock() )
            _state = MessageBrokerListenerState.Disposed;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool Dispose()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerListenerState.Disposed )
                return false;

            _state = MessageBrokerListenerState.Disposed;
        }

        return true;
    }
}
