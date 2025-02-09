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

namespace LfrlAnvil.MessageBroker.Client.Exceptions;

/// <summary>
/// Represents an exception thrown when <see cref="MessageBrokerClient"/> is in an invalid state.
/// </summary>
public class MessageBrokerClientStateException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerClientStateException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerClient"/> in an invalid state.</param>
    /// <param name="actual">Current invalid client state.</param>
    /// <param name="expected">Expected client state.</param>
    public MessageBrokerClientStateException(MessageBrokerClient client, MessageBrokerClientState actual, MessageBrokerClientState expected)
        : base( Resources.InvalidClientState( client.Name, actual, expected ) )
    {
        Client = client;
        Actual = actual;
        Expected = expected;
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> in an invalid state.
    /// </summary>
    public MessageBrokerClient Client { get; }

    /// <summary>
    /// Current invalid client state.
    /// </summary>
    public MessageBrokerClientState Actual { get; }

    /// <summary>
    /// Expected client state.
    /// </summary>
    public MessageBrokerClientState Expected { get; }
}
