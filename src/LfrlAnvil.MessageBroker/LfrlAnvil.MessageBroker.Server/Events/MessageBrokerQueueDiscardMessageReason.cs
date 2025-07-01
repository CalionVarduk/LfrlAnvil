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
/// Represents the reason behind discarding an enqueued message.
/// </summary>
public enum MessageBrokerQueueDiscardMessageReason : byte
{
    /// <summary>
    /// Specifies that max redelivery attempts have been reached for the listener.
    /// </summary>
    MaxRedeliveriesReached = 0,

    /// <summary>
    /// Specifies that max retry attempts have been reached for the listener.
    /// </summary>
    MaxRetriesReached = 1,

    /// <summary>
    /// Specifies that a negative ACK caused an explicit stop to any further retry attempts.
    /// </summary>
    ExplicitNoRetry = 2,

    /// <summary>
    /// Specifies that a listener has been disposed for a pending message.
    /// </summary>
    DisposedPending = 3,

    /// <summary>
    /// Specifies that a listener has been disposed for an unacked message.
    /// </summary>
    DisposedUnacked = 4,

    /// <summary>
    /// Specifies that a listener has been disposed for a scheduled retry.
    /// </summary>
    DisposedRetry = 5
}
