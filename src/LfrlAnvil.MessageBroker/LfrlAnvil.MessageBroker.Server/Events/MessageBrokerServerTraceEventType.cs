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
/// Represents the type of a <see cref="MessageBrokerServerTraceEvent"/>.
/// </summary>
public enum MessageBrokerServerTraceEventType : byte
{
    /// <summary>
    /// Specifies that trace is related to the server starting.
    /// </summary>
    Start = 0,

    /// <summary>
    /// Specifies that trace is related to accepting newly connected client.
    /// </summary>
    AcceptClient = 1,

    /// <summary>
    /// Specifies that trace is related to the server disposal.
    /// </summary>
    Dispose = 2,

    /// <summary>
    /// Specifies that trace is related to an unexpected occurrence in the server e.g. an error.
    /// </summary>
    Unexpected = 3
}
