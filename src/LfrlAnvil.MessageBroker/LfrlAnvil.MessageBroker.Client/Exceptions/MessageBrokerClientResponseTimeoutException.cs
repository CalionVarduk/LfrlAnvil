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
using System.Threading;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Exceptions;

/// <summary>
/// Represents an exception thrown when message broker server failed to send a response to client's request in the specified amount of time.
/// </summary>
public class MessageBrokerClientResponseTimeoutException : OperationCanceledException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerClientResponseTimeoutException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerClient"/> that encountered response timeout.</param>
    /// <param name="requestEndpoint">Request endpoint.</param>
    public MessageBrokerClientResponseTimeoutException(MessageBrokerClient client, MessageBrokerServerEndpoint requestEndpoint)
        : base(
            Resources.ResponseTimeout( client.Name, client.MessageTimeout, requestEndpoint ),
            new CancellationToken( canceled: true ) )
    {
        Client = client;
        RequestEndpoint = requestEndpoint;
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> that encountered response timeout.
    /// </summary>
    public MessageBrokerClient Client { get; }

    /// <summary>
    /// Request endpoint.
    /// </summary>
    public MessageBrokerServerEndpoint RequestEndpoint { get; }
}
