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
/// Represents an exception thrown when <see cref="MessageBrokerClient"/> has sent an invalid request to the server.
/// </summary>
public class MessageBrokerClientRequestException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerClientRequestException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerClient"/> that sent an invalid request.</param>
    /// <param name="endpoint">Server endpoint associated with sent request.</param>
    /// <param name="payload">Packet payload sent by the client.</param>
    /// <param name="errors">Collection of invalid request errors.</param>
    public MessageBrokerClientRequestException(
        MessageBrokerClient client,
        MessageBrokerServerEndpoint endpoint,
        uint payload,
        Chain<string> errors)
        : base( Resources.ClientPayloadRejected( client.Name, endpoint, payload, errors ) )
    {
        Client = client;
        Endpoint = endpoint;
        Payload = payload;
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> that sent an invalid request.
    /// </summary>
    public MessageBrokerClient Client { get; }

    /// <summary>
    /// Server endpoint associated with sent request.
    /// </summary>
    public MessageBrokerServerEndpoint Endpoint { get; }

    /// <summary>
    /// Packet payload sent by the client.
    /// </summary>
    public uint Payload { get; }
}
