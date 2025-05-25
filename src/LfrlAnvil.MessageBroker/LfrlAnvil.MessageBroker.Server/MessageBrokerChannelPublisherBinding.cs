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

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker channel binding for a publisher, which allows clients to publish messages through channels.
/// </summary>
public sealed class MessageBrokerChannelPublisherBinding
{
    private readonly object _sync = new object();
    private MessageBrokerChannelPublisherBindingState _state;

    internal MessageBrokerChannelPublisherBinding(
        MessageBrokerRemoteClient client,
        MessageBrokerChannel channel,
        MessageBrokerStream stream)
    {
        Client = client;
        Channel = channel;
        Stream = stream;
        _state = MessageBrokerChannelPublisherBindingState.Running;
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

    internal bool ShouldCancel => _state >= MessageBrokerChannelPublisherBindingState.Disposing;

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

    internal void OnServerDisposed()
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerChannelPublisherBindingState.Disposed;
        }
    }

    internal async ValueTask OnClientDisconnectedAsync(ulong traceId)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerChannelPublisherBindingState.Disposing;
        }

        await Stream.OnPublisherDisposingAsync( Client, Channel, traceId ).ConfigureAwait( false );
        Channel.OnPublisherDisposing( Client, traceId );

        using ( AcquireLock() )
            _state = MessageBrokerChannelPublisherBindingState.Disposed;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void BeginDisposingUnsafe()
    {
        Assume.Equals( _state, MessageBrokerChannelPublisherBindingState.Running );
        _state = MessageBrokerChannelPublisherBindingState.Disposing;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void EndDisposing()
    {
        using ( AcquireLock() )
        {
            Assume.Equals( _state, MessageBrokerChannelPublisherBindingState.Disposing );
            _state = MessageBrokerChannelPublisherBindingState.Disposed;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }
}
