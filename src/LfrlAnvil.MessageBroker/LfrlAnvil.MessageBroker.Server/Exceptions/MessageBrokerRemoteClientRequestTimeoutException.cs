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
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

/// <summary>
/// Represents an exception thrown when message broker client failed to send a request to the server in the specified amount of time.
/// </summary>
public class MessageBrokerRemoteClientRequestTimeoutException : OperationCanceledException
{
    private readonly object _source;

    private MessageBrokerRemoteClientRequestTimeoutException(MessageBrokerRemoteClient client, Duration timeout)
        : base( Resources.RequestTimeout( client, timeout ), new CancellationToken( canceled: true ) )
    {
        _source = client;
    }

    /// <summary>
    /// Creates a new <see cref="MessageBrokerRemoteClientRequestTimeoutException"/> instance.
    /// </summary>
    /// <param name="connector"><see cref="MessageBrokerRemoteClientConnector"/> that encountered request timeout.</param>
    public MessageBrokerRemoteClientRequestTimeoutException(MessageBrokerRemoteClientConnector connector)
        : base( Resources.RequestTimeout( connector ), new CancellationToken( canceled: true ) )
    {
        _source = connector;
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> that encountered request timeout.
    /// </summary>
    public MessageBrokerRemoteClient? Client => _source as MessageBrokerRemoteClient;

    /// <summary>
    /// <see cref="MessageBrokerRemoteClientConnector"/> that encountered request timeout.
    /// </summary>
    public MessageBrokerRemoteClientConnector? Connector => _source as MessageBrokerRemoteClientConnector;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerRemoteClientRequestTimeoutException"/> instance for the handshake process.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerRemoteClient"/> that encountered handshake request timeout.</param>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MessageBrokerRemoteClientRequestTimeoutException CreateForHandshake(MessageBrokerRemoteClient client)
    {
        return new MessageBrokerRemoteClientRequestTimeoutException( client, client.MessageTimeout );
    }

    /// <summary>
    /// Creates a new <see cref="MessageBrokerRemoteClientRequestTimeoutException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerRemoteClient"/> that encountered request timeout.</param>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MessageBrokerRemoteClientRequestTimeoutException Create(MessageBrokerRemoteClient client)
    {
        return new MessageBrokerRemoteClientRequestTimeoutException( client, client.MaxReadTimeout );
    }
}
