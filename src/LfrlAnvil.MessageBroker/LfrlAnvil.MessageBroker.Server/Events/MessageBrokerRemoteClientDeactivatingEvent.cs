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

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> when it's started to be deactivated.
/// </summary>
public readonly struct MessageBrokerRemoteClientDeactivatingEvent
{
    private MessageBrokerRemoteClientDeactivatingEvent(MessageBrokerRemoteClient client, ulong traceId, bool isAlive)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        IsAlive = isAlive;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Specifies whether or not the client should remain active after deactivation is finished.
    /// </summary>
    public bool IsAlive { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientDeactivatingEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Deactivating] {Source}, IsAlive = {IsAlive}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientDeactivatingEvent Create(MessageBrokerRemoteClient client, ulong traceId, bool isAlive)
    {
        return new MessageBrokerRemoteClientDeactivatingEvent( client, traceId, isAlive );
    }
}
