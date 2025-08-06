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
/// Represents the type of a <see cref="MessageBrokerRemoteClientTraceEvent"/>.
/// </summary>
public enum MessageBrokerRemoteClientTraceEventType : byte
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
    /// Specifies that trace is related to the client pushing message routing packet to the server.
    /// </summary>
    PushMessageRouting = 6,

    /// <summary>
    /// Specifies that trace is related to the client pushing message packet to the server.
    /// </summary>
    PushMessage = 7,

    /// <summary>
    /// Specifies that trace is related to the client's dead letter query.
    /// </summary>
    DeadLetterQuery = 8,

    /// <summary>
    /// Specifies that trace is related to the server sending message notification to the client.
    /// </summary>
    MessageNotification = 9,

    /// <summary>
    /// Specifies that trace is related to the client sending a message notification ACK to the server.
    /// </summary>
    Ack = 10,

    /// <summary>
    /// Specifies that trace is related to the client sending a negative message notification ACK to the server.
    /// </summary>
    NegativeAck = 11,

    /// <summary>
    /// Specifies that trace is related to the server sending system notification to the client.
    /// </summary>
    SystemNotification = 12,

    /// <summary>
    /// Specifies that trace is related to the client disposal.
    /// </summary>
    Dispose = 13,

    /// <summary>
    /// Specifies that trace is related to an unexpected occurrence in the client e.g. an error.
    /// </summary>
    Unexpected = 14
}
