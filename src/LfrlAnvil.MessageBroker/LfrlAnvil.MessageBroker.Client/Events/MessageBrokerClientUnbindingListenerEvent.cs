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

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to unbinding a listener.
/// </summary>
public readonly struct MessageBrokerClientUnbindingListenerEvent
{
    private MessageBrokerClientUnbindingListenerEvent(MessageBrokerListener listener, ulong traceId)
    {
        Source = MessageBrokerClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerListener"/> related to this event.
    /// </summary>
    public MessageBrokerListener Listener { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientUnbindingListenerEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[UnbindingListener] {Source}, Channel = [{Listener.ChannelId}] '{Listener.ChannelName}', Queue = [{Listener.QueueId}] '{Listener.QueueName}'";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientUnbindingListenerEvent Create(MessageBrokerListener listener, ulong traceId)
    {
        return new MessageBrokerClientUnbindingListenerEvent( listener, traceId );
    }
}
