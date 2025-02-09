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
/// Represents an exception thrown when <see cref="MessageBrokerRemoteClient"/> has sent an invalid request to the client.
/// </summary>
public class MessageBrokerServerRequestException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerServerRequestException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerRemoteClient"/> that sent an invalid request.</param>
    /// <param name="endpoint">Client endpoint associated with sent request.</param>
    /// <param name="payload">Packet payload sent by the server.</param>
    /// <param name="errors">Collection of invalid request errors.</param>
    public MessageBrokerServerRequestException(
        MessageBrokerRemoteClient client,
        MessageBrokerClientEndpoint endpoint,
        uint payload,
        Chain<string> errors)
        : base( Resources.ServerPayloadRejected( client.Id, client.Name, endpoint, payload, errors ) )
    {
        Client = client;
        Endpoint = endpoint;
        Payload = payload;
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> that sent an invalid request.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Client endpoint associated with sent request.
    /// </summary>
    public MessageBrokerClientEndpoint Endpoint { get; }

    /// <summary>
    /// Packet payload sent by the server.
    /// </summary>
    public uint Payload { get; }
}
