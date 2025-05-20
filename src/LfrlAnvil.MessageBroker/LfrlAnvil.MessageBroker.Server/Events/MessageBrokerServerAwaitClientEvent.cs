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
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerServer"/> related to waiting for a <see cref="TcpClient"/> to connect.
/// </summary>
public readonly struct MessageBrokerServerAwaitClientEvent
{
    private MessageBrokerServerAwaitClientEvent(MessageBrokerServer server, EndPoint? endPoint, Exception? exception)
    {
        Server = server;
        EndPoint = endPoint;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> that emitted an event.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Incoming client's endpoint.
    /// </summary>
    public EndPoint? EndPoint { get; }

    /// <summary>
    /// Encountered error.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServerAwaitClientEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var endpoint = EndPoint is not null ? $", EndPoint = {EndPoint}" : string.Empty;
        var result = $"[AwaitClient] Server = {Server.LocalEndPoint}{endpoint}";
        return Exception is null ? result : $"{result}{Environment.NewLine}{Exception}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerAwaitClientEvent Create(MessageBrokerServer server, Exception? exception = null)
    {
        return new MessageBrokerServerAwaitClientEvent( server, null, exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerAwaitClientEvent Create(MessageBrokerServer server, EndPoint endpoint)
    {
        return new MessageBrokerServerAwaitClientEvent( server, endpoint, null );
    }
}
