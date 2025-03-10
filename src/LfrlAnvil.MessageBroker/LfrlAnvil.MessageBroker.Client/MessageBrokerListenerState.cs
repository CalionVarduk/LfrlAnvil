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
/// Defines possible <see cref="MessageBrokerListener"/> states.
/// </summary>
public enum MessageBrokerListenerState : byte
{
    /// <summary>
    /// Specifies that the listener is subscribed to the channel and listens to messages.
    /// </summary>
    Subscribed = 0,

    /// <summary>
    /// Specifies that the listener has been disposed.
    /// </summary>
    Disposed = 1
}
