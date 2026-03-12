// Copyright 2025-2026 Łukasz Furlepa
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

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Defines possible <see cref="MessageBrokerChannelPublisherBinding"/> states.
/// </summary>
public enum MessageBrokerChannelPublisherBindingState : byte
{
    /// <summary>
    /// Specifies that the publisher is in the process of being bound.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Specifies that the publisher binding is currently running.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Specifies that the publisher binding is currently being deactivated.
    /// </summary>
    Deactivating = 2,

    /// <summary>
    /// Specifies that the publisher binding is currently inactive.
    /// </summary>
    Inactive = 3,

    /// <summary>
    /// Specifies that the publisher binding is currently being disposed.
    /// </summary>
    Disposing = 4,

    /// <summary>
    /// Specifies that the publisher binding has been disposed.
    /// </summary>
    Disposed = 5
}
