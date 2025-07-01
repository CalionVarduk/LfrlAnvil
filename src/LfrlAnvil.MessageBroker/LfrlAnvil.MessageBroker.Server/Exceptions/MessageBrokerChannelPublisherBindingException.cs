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
/// Represents an exception thrown when <see cref="MessageBrokerChannelPublisherBinding"/> encountered an error
/// or when <see cref="MessageBrokerRemoteClient"/> encountered an error related to publisher binding.
/// </summary>
public class MessageBrokerChannelPublisherBindingException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerChannelPublisherBindingException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerServer"/> instance that encountered this error.</param>
    /// <param name="publisher">Optional <see cref="MessageBrokerChannelPublisherBinding"/> instance that encountered this error.</param>
    /// <param name="error">Encountered error.</param>
    public MessageBrokerChannelPublisherBindingException(
        MessageBrokerRemoteClient client,
        MessageBrokerChannelPublisherBinding? publisher,
        string error)
        : base( error )
    {
        Client = client;
        Publisher = publisher;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance that encountered this error.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Optional <see cref="MessageBrokerChannelPublisherBinding"/> instance that encountered this error.
    /// </summary>
    public MessageBrokerChannelPublisherBinding? Publisher { get; }
}
