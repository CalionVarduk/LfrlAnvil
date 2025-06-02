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
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> when attempting to process
/// received system notification from the server.
/// </summary>
public readonly struct MessageBrokerClientProcessingSystemNotificationEvent
{
    private MessageBrokerClientProcessingSystemNotificationEvent(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerSystemNotificationType type)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Type = type;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// System notification's type.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerSystemNotificationType"/> for more information.</remarks>
    public MessageBrokerSystemNotificationType Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientProcessingSystemNotificationEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[ProcessingSystemNotification] {Source}, Type = {Resources.GetType( Type )}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProcessingSystemNotificationEvent Create(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerSystemNotificationType type)
    {
        return new MessageBrokerClientProcessingSystemNotificationEvent( client, traceId, type );
    }
}
