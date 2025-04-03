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

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Defines endpoints acceptable by message broker client.
/// </summary>
public enum MessageBrokerClientEndpoint : byte
{
    /// <summary>
    /// Represents server's response to client's ping request.
    /// </summary>
    PingResponse = 2,

    /// <summary>
    /// Represents server's response to client's invalid handshake request.
    /// </summary>
    HandshakeRejectedResponse = 4,

    /// <summary>
    /// Represents server's response to client's valid handshake request.
    /// </summary>
    HandshakeAcceptedResponse = 6,

    /// <summary>
    /// Represents server's response to client's valid bind request.
    /// </summary>
    BoundResponse = 8,

    /// <summary>
    /// Represents server's response to client's invalid bind request.
    /// </summary>
    BindFailureResponse = 10,

    /// <summary>
    /// Represents server's response to client's valid unbind request.
    /// </summary>
    UnboundResponse = 12,

    /// <summary>
    /// Represents server's response to client's invalid unbind request.
    /// </summary>
    UnbindFailureResponse = 14,

    /// <summary>
    /// Represents server's response to client's valid subscribe request.
    /// </summary>
    SubscribedResponse = 16,

    /// <summary>
    /// Represents server's response to client's invalid subscribe request.
    /// </summary>
    SubscribeFailureResponse = 18,

    /// <summary>
    /// Represents server's response to client's valid unsubscribe request.
    /// </summary>
    UnsubscribedResponse = 20,

    /// <summary>
    /// Represents server's response to client's invalid unsubscribe request.
    /// </summary>
    UnsubscribeFailureResponse = 22,

    /// <summary>
    /// Represents server's response to client's valid message request.
    /// </summary>
    MessageAcceptedResponse = 24,

    /// <summary>
    /// Represents server's response to client's invalid message request.
    /// </summary>
    MessageRejectedResponse = 26
}
