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
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a message broker channel linked to the client, which allows to publish messages.
/// </summary>
public sealed class MessageBrokerLinkedChannel
{
    private readonly object _sync = new object();
    private MessageBrokerLinkedChannelState _state;

    internal MessageBrokerLinkedChannel(MessageBrokerClient client, int id, string name)
    {
        Client = client;
        Id = id;
        Name = name;
        _state = MessageBrokerLinkedChannelState.Linked;
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> instance to which this channel is linked to.
    /// </summary>
    public MessageBrokerClient Client { get; }

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
    /// <remarks>See <see cref="MessageBrokerLinkedChannelState"/> for more information.</remarks>
    public MessageBrokerLinkedChannelState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask<Result<MessageBrokerChannelUnlinkResult>> UnlinkAsync(CancellationToken cancellationToken = default)
    {
        return LinkedChannelCollection.UnlinkAsync( this, cancellationToken );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void OnClientDisposed()
    {
        using ( AcquireLock() )
            _state = MessageBrokerLinkedChannelState.Unlinked;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool Unlink()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerLinkedChannelState.Unlinked )
                return false;

            _state = MessageBrokerLinkedChannelState.Unlinked;
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }
}
