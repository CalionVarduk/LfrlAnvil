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

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

/// <summary>
/// Represents an exception thrown when <see cref="MessageBrokerRemoteClient"/> is in a deactivated state.
/// </summary>
public class MessageBrokerRemoteClientDeactivatedException : OperationCanceledException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerRemoteClientDeactivatedException"/> instance.
    /// </summary>
    /// <param name="client">Deactivated <see cref="MessageBrokerRemoteClient"/>.</param>
    /// <param name="disposed">Specifies whether or not the client has been disposed.</param>
    public MessageBrokerRemoteClientDeactivatedException(MessageBrokerRemoteClient client, bool disposed)
        : base( Resources.ClientDeactivated( client.Id, client.Name, disposed ), new CancellationToken( canceled: true ) )
    {
        Client = client;
        Disposed = disposed;
    }

    /// <summary>
    /// Deactivated <see cref="MessageBrokerRemoteClient"/>.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Specifies whether or not the <see cref="Client"/> has been disposed.
    /// </summary>
    public bool Disposed { get; }
}
