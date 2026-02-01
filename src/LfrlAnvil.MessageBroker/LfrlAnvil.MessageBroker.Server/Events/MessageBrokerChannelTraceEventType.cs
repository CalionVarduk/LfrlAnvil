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
/// Represents the type of a <see cref="MessageBrokerChannelTraceEvent"/>.
/// </summary>
public enum MessageBrokerChannelTraceEventType : byte
{
    /// <summary>
    /// Specifies that trace is related to the channel being recreated.
    /// </summary>
    Recreated = 0,

    /// <summary>
    /// Specifies that trace is related to the channel listener binding.
    /// </summary>
    BindListener = 1,

    /// <summary>
    /// Specifies that trace is related to the channel listener unbinding.
    /// </summary>
    UnbindListener = 2,

    /// <summary>
    /// Specifies that trace is related to the channel publisher binding.
    /// </summary>
    BindPublisher = 3,

    /// <summary>
    /// Specifies that trace is related to the channel publisher unbinding.
    /// </summary>
    UnbindPublisher = 4,

    /// <summary>
    /// Specifies that trace is related to the channel disposal.
    /// </summary>
    Dispose = 5
}
