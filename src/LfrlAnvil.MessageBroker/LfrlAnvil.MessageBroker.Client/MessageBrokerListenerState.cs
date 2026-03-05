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

namespace LfrlAnvil.MessageBroker.Client;

// TODO
// when server gets its own Delete listener/publisher, some additional changes will have to be made:
// - client-side bindings need Created state, and they have to be added to collections before request is sent
// - publisher cannot send messages in Created state, listener CAN receive messages in Created state (to make it easier)
// - client changes binding state to Bound after successful response, only if in Created state
// - server-side bindings get a new Created state, move to Running state after response is enqueued
// - server delete sends system notification to client, cannot delete Created binding
// - client cannot unbind Created binding
// - client can only delete/move to Disposing when binding is in Created or Bound state
// - Created bindings will be persisted by the server

/// <summary>
/// Defines possible <see cref="MessageBrokerListener"/> states.
/// </summary>
public enum MessageBrokerListenerState : byte
{
    /// <summary>
    /// Specifies that the listener is bound to the channel and listens to messages.
    /// </summary>
    Bound = 0,

    /// <summary>
    /// Specifies the listener is currently being disposed.
    /// </summary>
    Disposing = 1,

    /// <summary>
    /// Specifies that the listener has been disposed.
    /// </summary>
    Disposed = 2
}
