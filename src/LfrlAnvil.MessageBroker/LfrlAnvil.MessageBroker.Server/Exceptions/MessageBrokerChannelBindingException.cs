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
/// Represents an error related to a message broker channel binding.
/// </summary>
public class MessageBrokerChannelBindingException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerChannelBindingException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerServer"/> instance that encountered this error.</param>
    /// <param name="channel">Optional <see cref="MessageBrokerChannel"/> instance that encountered this error.</param>
    /// <param name="binding">Optional <see cref="MessageBrokerChannelBinding"/> instance that encountered this error.</param>
    /// <param name="message">Underlying error message.</param>
    public MessageBrokerChannelBindingException(
        MessageBrokerRemoteClient client,
        MessageBrokerChannel? channel,
        MessageBrokerChannelBinding? binding,
        string message)
        : base( message )
    {
        Client = client;
        Channel = channel;
        Binding = binding;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance that encountered this error.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Optional <see cref="MessageBrokerChannel"/> instance that encountered this error.
    /// </summary>
    public MessageBrokerChannel? Channel { get; }

    /// <summary>
    /// Optional <see cref="MessageBrokerChannelBinding"/> instance that encountered this error.
    /// </summary>
    public MessageBrokerChannelBinding? Binding { get; }
}
