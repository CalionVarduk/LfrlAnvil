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
/// Defines endpoints acceptable by message broker server.
/// </summary>
public enum MessageBrokerServerEndpoint : byte
{
    /// <summary>
    /// Represents ping request sent by client.
    /// </summary>
    PingRequest = 1,

    /// <summary>
    /// Represents client's confirmation of server's handshake acceptance response.
    /// </summary>
    ConfirmHandshakeResponse = 2,

    /// <summary>
    /// Represents handshake request sent by client.
    /// </summary>
    HandshakeRequest = 3,

    /// <summary>
    /// Represents link channel request sent by client.
    /// </summary>
    LinkChannelRequest = 5,

    /// <summary>
    /// Represents unlink channel request sent by client.
    /// </summary>
    UnlinkChannelRequest = 7
}
