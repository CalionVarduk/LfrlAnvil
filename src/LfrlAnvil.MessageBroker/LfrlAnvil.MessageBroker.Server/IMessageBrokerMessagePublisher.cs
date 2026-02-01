// Copyright 2026 Łukasz Furlepa
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

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message publisher.
/// </summary>
public interface IMessageBrokerMessagePublisher
{
    /// <summary>
    /// Client's unique identifier assigned by the server.
    /// </summary>
    int ClientId { get; }

    /// <summary>
    /// Client's unique name.
    /// </summary>
    string ClientName { get; }

    /// <summary>
    /// Specifies whether or not the client is ephemeral.
    /// </summary>
    bool IsClientEphemeral { get; }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> instance to which this publisher belongs to.
    /// </summary>
    MessageBrokerRemoteClient? Client { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannel"/> instance to which the client is bound to as a publisher.
    /// </summary>
    MessageBrokerChannel Channel { get; }

    /// <summary>
    /// <see cref="MessageBrokerStream"/> instance through which this publisher will push messages to subscribers.
    /// </summary>
    MessageBrokerStream Stream { get; }
}
