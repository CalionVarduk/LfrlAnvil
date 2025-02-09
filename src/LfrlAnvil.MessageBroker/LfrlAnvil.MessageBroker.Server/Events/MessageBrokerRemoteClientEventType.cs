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
/// Defines available <see cref="MessageBrokerRemoteClientEvent"/> types.
/// </summary>
public enum MessageBrokerRemoteClientEventType : byte
{
    /// <summary>
    /// Specifies that the client has encountered an unexpected error.
    /// </summary>
    Unexpected,

    /// <summary>
    /// Specifies that the client instance has been created and is about to initiate handshaking.
    /// </summary>
    Created,

    /// <summary>
    /// Specifies that the client is waiting for message from the remote client.
    /// </summary>
    WaitingForMessage,

    /// <summary>
    /// Specifies that the client has received a message from the remote client.
    /// </summary>
    MessageReceived,

    /// <summary>
    /// Specifies that the client has accepted a message received from the remote client.
    /// </summary>
    MessageAccepted,

    /// <summary>
    /// Specifies that the client has rejected a message received from the remote client.
    /// </summary>
    MessageRejected,

    /// <summary>
    /// Specifies that the client is about to send a message to the remote client.
    /// </summary>
    SendingMessage,

    /// <summary>
    /// Specifies that the client has sent a message to the remote client.
    /// </summary>
    MessageSent,

    /// <summary>
    /// Specifies that the client is about to be disposed.
    /// </summary>
    Disposing,

    /// <summary>
    /// Specifies that the client has been disposed.
    /// </summary>
    Disposed
}
