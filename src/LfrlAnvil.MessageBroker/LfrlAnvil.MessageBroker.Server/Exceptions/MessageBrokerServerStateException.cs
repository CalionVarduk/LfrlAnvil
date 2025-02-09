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
/// Represents an exception thrown when <see cref="MessageBrokerServer"/> is in an invalid state.
/// </summary>
public class MessageBrokerServerStateException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerServerStateException"/> instance.
    /// </summary>
    /// <param name="server"><see cref="MessageBrokerServer"/> in an invalid state.</param>
    /// <param name="actual">Current invalid client state.</param>
    /// <param name="expected">Expected client state.</param>
    public MessageBrokerServerStateException(MessageBrokerServer server, MessageBrokerServerState actual, MessageBrokerServerState expected)
        : base( Resources.InvalidServerState( actual, expected ) )
    {
        Server = server;
        Actual = actual;
        Expected = expected;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> in an invalid state.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Current invalid server state.
    /// </summary>
    public MessageBrokerServerState Actual { get; }

    /// <summary>
    /// Expected server state.
    /// </summary>
    public MessageBrokerServerState Expected { get; }
}
