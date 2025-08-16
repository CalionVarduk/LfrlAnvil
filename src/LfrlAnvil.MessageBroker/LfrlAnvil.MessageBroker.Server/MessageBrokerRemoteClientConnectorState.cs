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

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Defines possible <see cref="MessageBrokerRemoteClientConnector"/> states.
/// </summary>
public enum MessageBrokerRemoteClientConnectorState : byte
{
    /// <summary>
    /// Specifies that the connector has been created but not started.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Specifies that the connector is currently performing a handshake with the remote client.
    /// </summary>
    Handshaking = 1,

    /// <summary>
    /// Specifies that the connector is being cancelled.
    /// </summary>
    Cancelling = 2,

    /// <summary>
    /// Specifies that the connector has completed successfully.
    /// </summary>
    Connected = 3,

    /// <summary>
    /// Specifies that the connector has completed due to cancellation.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Specifies that the connector has completed with failure.
    /// </summary>
    Failed = 5
}
