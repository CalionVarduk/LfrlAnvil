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

using System;

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

/// <summary>
/// Represents an exception thrown when something went wrong with message notification handling.
/// </summary>
public sealed class MessageBrokerRemoteClientMessageException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerRemoteClientMessageException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerRemoteClient"/> that encountered this error.</param>
    /// <param name="queue">Optional <see cref="MessageBrokerQueue"/> that encountered this error.</param>
    /// <param name="error">Encountered error.</param>
    public MessageBrokerRemoteClientMessageException(MessageBrokerRemoteClient client, MessageBrokerQueue? queue, string error)
        : base( error )
    {
        Client = client;
        Queue = queue;
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> that encountered this error.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Optional <see cref="MessageBrokerQueue"/> that encountered this error.
    /// </summary>
    public MessageBrokerQueue? Queue { get; }
}
