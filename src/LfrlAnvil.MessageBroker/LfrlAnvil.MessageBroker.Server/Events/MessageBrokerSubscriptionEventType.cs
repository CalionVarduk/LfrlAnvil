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
/// Defines available <see cref="MessageBrokerSubscriptionEvent"/> types.
/// </summary>
public enum MessageBrokerSubscriptionEventType : byte
{
    /// <summary>
    /// Specifies that the subscription has encountered an unexpected error.
    /// </summary>
    Unexpected,

    /// <summary>
    /// Specifies that the subscription instance has been created.
    /// </summary>
    Created,

    /// <summary>
    /// Specifies that the subscription is about to be disposed.
    /// </summary>
    Disposing,

    /// <summary>
    /// Specifies that the subscription has been disposed.
    /// </summary>
    Disposed
}
