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

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

/// <summary>
/// Represents an exception thrown when message broker client failed to send a request to the server in the specified amount of time.
/// </summary>
public class MessageBrokerRemoteClientRequestTimeoutException : OperationCanceledException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerRemoteClientRequestTimeoutException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerRemoteClient"/> that encountered request timeout.</param>
    public MessageBrokerRemoteClientRequestTimeoutException(MessageBrokerRemoteClient client)
        : base( Resources.RequestTimeout( client.Id, client.Name, client.MessageTimeout ), new CancellationToken( canceled: true ) )
    {
        Client = client;
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> that encountered request timeout.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }
}
