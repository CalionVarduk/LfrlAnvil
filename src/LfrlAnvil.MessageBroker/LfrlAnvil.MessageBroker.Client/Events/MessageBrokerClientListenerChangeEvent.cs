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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to a listener state change.
/// </summary>
public readonly struct MessageBrokerClientListenerChangeEvent
{
    private MessageBrokerClientListenerChangeEvent(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerListener listener,
        MessageBrokerClientListenerChangeEventType type)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Listener = listener;
        Type = type;
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
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See<see cref="MessageBrokerClientListenerChangeEventType"/> for more information.</remarks>
    public MessageBrokerClientListenerChangeEventType Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientListenerChangeEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Type == MessageBrokerClientListenerChangeEventType.Bound
            ? $"[ListenerChange:{Type}] {Source}, Channel = [{Listener.ChannelId}] '{Listener.ChannelName}', Queue = [{Listener.QueueId}] '{Listener.QueueName}', PrefetchHint = {Listener.PrefetchHint}"
            : $"[ListenerChange:{Type}] {Source}, Channel = [{Listener.ChannelId}] '{Listener.ChannelName}', Queue = [{Listener.QueueId}] '{Listener.QueueName}'";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientListenerChangeEvent CreateBound(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerListener listener)
    {
        return new MessageBrokerClientListenerChangeEvent( client, traceId, listener, MessageBrokerClientListenerChangeEventType.Bound );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientListenerChangeEvent CreateUnbinding(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerListener listener)
    {
        return new MessageBrokerClientListenerChangeEvent(
            client,
            traceId,
            listener,
            MessageBrokerClientListenerChangeEventType.Unbinding );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientListenerChangeEvent CreateUnbound(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerListener listener)
    {
        return new MessageBrokerClientListenerChangeEvent( client, traceId, listener, MessageBrokerClientListenerChangeEventType.Unbound );
    }
}
