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
/// Represents an exception thrown when <see cref="MessageBrokerChannelListenerBinding"/> encountered an error
/// or when <see cref="MessageBrokerRemoteClient"/> encountered an error related to listener binding.
/// </summary>
public sealed class MessageBrokerChannelListenerBindingException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerChannelListenerBindingException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerServer"/> instance that encountered this error.</param>
    /// <param name="listener">Optional <see cref="MessageBrokerChannelListenerBinding"/> instance that encountered this error.</param>
    /// <param name="error">Encountered error.</param>
    public MessageBrokerChannelListenerBindingException(
        MessageBrokerRemoteClient client,
        MessageBrokerChannelListenerBinding? listener,
        string error)
        : base( error )
    {
        Client = client;
        Listener = listener;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance that encountered this error.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Optional <see cref="MessageBrokerChannelListenerBinding"/> instance that encountered this error.
    /// </summary>
    public MessageBrokerChannelListenerBinding? Listener { get; }
}
