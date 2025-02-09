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
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Exceptions;

/// <summary>
/// Represents an exception thrown when network protocol has been violated by message broker server.
/// </summary>
public class MessageBrokerClientProtocolException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerClientProtocolException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerClient"/> that encountered network protocol violation.</param>
    /// <param name="endpoint">Client endpoint associated with network protocol violation.</param>
    /// <param name="payload">Packet payload received by the client.</param>
    /// <param name="errors">Collection of network protocol errors.</param>
    public MessageBrokerClientProtocolException(
        MessageBrokerClient client,
        MessageBrokerClientEndpoint endpoint,
        uint payload,
        Chain<string> errors)
        : base( Resources.InvalidPayloadFromServer( client.Name, endpoint, payload, errors ) )
    {
        Client = client;
        Endpoint = endpoint;
        Payload = payload;
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> that encountered network protocol violation.
    /// </summary>
    public MessageBrokerClient Client { get; }

    /// <summary>
    /// Client endpoint associated with network protocol violation.
    /// </summary>
    public MessageBrokerClientEndpoint Endpoint { get; }

    /// <summary>
    /// Packet payload received by the client.
    /// </summary>
    public uint Payload { get; }
}
