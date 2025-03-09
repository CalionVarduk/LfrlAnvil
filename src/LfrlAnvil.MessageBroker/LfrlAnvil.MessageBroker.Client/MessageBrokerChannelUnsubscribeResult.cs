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

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents the result of <see cref="MessageBrokerListener"/> unsubscribe operation.
/// </summary>
public enum MessageBrokerChannelUnsubscribeResult : byte
{
    /// <summary>
    /// Specifies that the client was not subscribed to the channel.
    /// </summary>
    NotSubscribed = 0,

    /// <summary>
    /// Specifies that the client has been successfully unsubscribed from the channel.
    /// </summary>
    Unsubscribed = 1,

    /// <summary>
    /// Specifies that the client has been successfully unsubscribed from the channel and that channel has been removed by the server.
    /// </summary>
    UnsubscribedAndChannelRemoved = 2
}
