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
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

/// <summary>
/// Represents an exception thrown when network protocol has been violated by message broker client.
/// </summary>
public class MessageBrokerServerProtocolException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerServerProtocolException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerRemoteClient"/> that encountered network protocol violation.</param>
    /// <param name="endpoint">Server endpoint associated with network protocol violation.</param>
    /// <param name="payload">Packet payload received by the server.</param>
    /// <param name="errors">Collection of network protocol errors.</param>
    public MessageBrokerServerProtocolException(
        MessageBrokerRemoteClient client,
        MessageBrokerServerEndpoint endpoint,
        uint payload,
        Chain<string> errors)
        : base( Resources.InvalidPayloadFromClient( client.Id, client.Name, endpoint, payload, errors ) )
    {
        Client = client;
        Endpoint = endpoint;
        Payload = payload;
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> that encountered network protocol violation.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Server endpoint associated with network protocol violation.
    /// </summary>
    public MessageBrokerServerEndpoint Endpoint { get; }

    /// <summary>
    /// Packet payload received by the server.
    /// </summary>
    public uint Payload { get; }
}
