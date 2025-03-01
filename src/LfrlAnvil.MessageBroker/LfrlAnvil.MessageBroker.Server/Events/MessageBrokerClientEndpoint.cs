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

namespace LfrlAnvil.MessageBroker.Server.Events;

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
    /// Represents server's response to client's valid link channel request.
    /// </summary>
    ChannelLinkedResponse = 8,

    /// <summary>
    /// Represents server's response to client's invalid link channel request.
    /// </summary>
    LinkChannelFailureResponse = 10
}
