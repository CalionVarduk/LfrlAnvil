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
    /// Represents ping sent by client.
    /// </summary>
    Ping = 1,

    /// <summary>
    /// Represents push message routing sent by client.
    /// </summary>
    PushMessageRouting = 2,

    /// <summary>
    /// Represents push message sent by client.
    /// </summary>
    PushMessage = 3,

    /// <summary>
    /// Represents bind publisher request sent by client.
    /// </summary>
    BindPublisherRequest = 4,

    /// <summary>
    /// Represents unbind publisher request sent by client.
    /// </summary>
    UnbindPublisherRequest = 5,

    /// <summary>
    /// Represents bind listener request sent by client.
    /// </summary>
    BindListenerRequest = 6,

    /// <summary>
    /// Represents unbind listener request sent by client.
    /// </summary>
    UnbindListenerRequest = 7,

    /// <summary>
    /// Represents message notification ACK sent by client.
    /// </summary>
    MessageNotificationAck = 8,

    /// <summary>
    /// Represents message notification negative ACK sent by client.
    /// </summary>
    MessageNotificationNack = 9,

    /// <summary>
    /// Represents handshake request sent by client.
    /// </summary>
    HandshakeRequest = 254,

    /// <summary>
    /// Represents client's confirmation of server's handshake acceptance response.
    /// </summary>
    ConfirmHandshakeResponse = 255
}
