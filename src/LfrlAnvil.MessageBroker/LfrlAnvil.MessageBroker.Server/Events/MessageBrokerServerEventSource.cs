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
/// Represents a source of an event emitted by <see cref="MessageBrokerServer"/>.
/// </summary>
public readonly struct MessageBrokerServerEventSource
{
    private MessageBrokerServerEventSource(MessageBrokerServer server, ulong traceId)
    {
        Server = server;
        TraceId = traceId;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> that emitted an event.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Identifier of an internal trace, with which an event is correlated to.
    /// </summary>
    /// <remarks>Can be used to correlate multiple events together, which are part of the same operation.</remarks>
    public ulong TraceId { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServerEventSource"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"Server = {Server.LocalEndPoint}, TraceId = {TraceId}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerEventSource Create(MessageBrokerServer server, ulong traceId)
    {
        return new MessageBrokerServerEventSource( server, traceId );
    }
}
