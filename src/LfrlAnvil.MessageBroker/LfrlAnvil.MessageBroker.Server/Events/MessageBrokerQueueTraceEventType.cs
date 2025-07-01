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
/// Represents the type of a <see cref="MessageBrokerQueueTraceEvent"/>.
/// </summary>
public enum MessageBrokerQueueTraceEventType : byte
{
    /// <summary>
    /// Specifies that trace is related to the queue listener binding.
    /// </summary>
    BindListener = 0,

    /// <summary>
    /// Specifies that trace is related to the queue listener unbinding.
    /// </summary>
    UnbindListener = 1,

    /// <summary>
    /// Specifies that trace is related to a message being enqueued by a stream.
    /// </summary>
    EnqueueMessage = 2,

    /// <summary>
    /// Specifies that trace is related to processing of an enqueued message.
    /// </summary>
    ProcessMessage = 3,

    /// <summary>
    /// Specifies that trace is related to a message notification ACK.
    /// </summary>
    Ack = 4,

    /// <summary>
    /// Specifies that trace is related to a negative message notification ACK.
    /// </summary>
    NegativeAck = 5,

    /// <summary>
    /// Specifies that trace is related to the queue disposal.
    /// </summary>
    Dispose = 6,

    /// <summary>
    /// Specifies that trace is related to an unexpected occurrence in the queue e.g. an error.
    /// </summary>
    Unexpected = 7
}
