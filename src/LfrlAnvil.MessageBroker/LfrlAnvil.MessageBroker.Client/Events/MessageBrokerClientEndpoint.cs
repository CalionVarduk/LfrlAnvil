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
    /// Represents server's response to client's ping.
    /// </summary>
    Pong = 1,

    /// <summary>
    /// Represents message notification sent by server.
    /// </summary>
    MessageNotification = 2,

    /// <summary>
    /// Represents server's response to client's valid push message request.
    /// </summary>
    MessageAcceptedResponse = 4,

    /// <summary>
    /// Represents server's response to client's invalid push message request.
    /// </summary>
    MessageRejectedResponse = 5,

    /// <summary>
    /// Represents server's response to client's valid bind publisher request.
    /// </summary>
    PublisherBoundResponse = 6,

    /// <summary>
    /// Represents server's response to client's invalid bind publisher request.
    /// </summary>
    BindPublisherFailureResponse = 7,

    /// <summary>
    /// Represents server's response to client's valid unbind publisher request.
    /// </summary>
    PublisherUnboundResponse = 8,

    /// <summary>
    /// Represents server's response to client's invalid unbind publisher request.
    /// </summary>
    UnbindPublisherFailureResponse = 9,

    /// <summary>
    /// Represents server's response to client's valid bind listener request.
    /// </summary>
    ListenerBoundResponse = 10,

    /// <summary>
    /// Represents server's response to client's invalid bind listener request.
    /// </summary>
    BindListenerFailureResponse = 11,

    /// <summary>
    /// Represents server's response to client's valid unbind listener request.
    /// </summary>
    ListenerUnboundResponse = 12,

    /// <summary>
    /// Represents server's response to client's invalid unbind listener request.
    /// </summary>
    UnbindListenerFailureResponse = 13,

    /// <summary>
    /// Represents server's response to client's valid handshake request.
    /// </summary>
    HandshakeAcceptedResponse = 254,

    /// <summary>
    /// Represents server's response to client's invalid handshake request.
    /// </summary>
    HandshakeRejectedResponse = 255
}
