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
/// Represents the type of a <see cref="MessageBrokerClientTraceEvent"/>.
/// </summary>
public enum MessageBrokerClientTraceEventType : byte
{
    /// <summary>
    /// Specifies that trace is related to the client starting.
    /// </summary>
    Start = 0,

    /// <summary>
    /// Specifies that trace is related to the client sending ping packet to the server.
    /// </summary>
    Ping = 1,

    /// <summary>
    /// Specifies that trace is related to the client listener binding.
    /// </summary>
    BindListener = 2,

    /// <summary>
    /// Specifies that trace is related to the client listener unbinding.
    /// </summary>
    UnbindListener = 3,

    /// <summary>
    /// Specifies that trace is related to the client publisher binding.
    /// </summary>
    BindPublisher = 4,

    /// <summary>
    /// Specifies that trace is related to the client publisher unbinding.
    /// </summary>
    UnbindPublisher = 5,

    /// <summary>
    /// Specifies that trace is related to the client pushing message packet to the server.
    /// </summary>
    PushMessage = 6,

    /// <summary>
    /// Specifies that trace is related to the client receiving message notification from the server.
    /// </summary>
    MessageNotification = 7,

    /// <summary>
    /// Specifies that trace is related to the client sending a message notification ACK to the server.
    /// </summary>
    Ack = 8,

    /// <summary>
    /// Specifies that trace is related to the client sending a negative message notification ACK to the server.
    /// </summary>
    NegativeAck = 9,

    /// <summary>
    /// Specifies that trace is related to the client receiving system notification from the server.
    /// </summary>
    SystemNotification = 10,

    /// <summary>
    /// Specifies that trace is related to the client disposal.
    /// </summary>
    Dispose = 11,

    /// <summary>
    /// Specifies that trace is related to an unexpected occurrence in the client e.g. an error.
    /// </summary>
    Unexpected = 12
}
