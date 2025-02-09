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
using System.Threading;

namespace LfrlAnvil.MessageBroker.Client.Exceptions;

/// <summary>
/// Represents an exception thrown when <see cref="MessageBrokerClient"/> is in disposed state.
/// </summary>
public class MessageBrokerClientDisposedException : OperationCanceledException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerClientDisposedException"/> instance.
    /// </summary>
    /// <param name="client">Disposed <see cref="MessageBrokerClient"/>.</param>
    public MessageBrokerClientDisposedException(MessageBrokerClient client)
        : base( Resources.ClientDisposed( client.Name ), new CancellationToken( canceled: true ) )
    {
        Client = client;
    }

    /// <summary>
    /// Disposed <see cref="MessageBrokerClient"/>.
    /// </summary>
    public MessageBrokerClient Client { get; }
}
