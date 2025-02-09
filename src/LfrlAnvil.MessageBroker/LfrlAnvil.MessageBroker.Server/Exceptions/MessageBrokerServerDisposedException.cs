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
/// Represents an exception thrown when <see cref="MessageBrokerServer"/> is in disposed state.
/// </summary>
public class MessageBrokerServerDisposedException : OperationCanceledException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerServerDisposedException"/> instance.
    /// </summary>
    /// <param name="server">Disposed <see cref="MessageBrokerServer"/>.</param>
    public MessageBrokerServerDisposedException(MessageBrokerServer server)
        : base( Resources.ServerDisposed, new CancellationToken( canceled: true ) )
    {
        Server = server;
    }

    /// <summary>
    /// Disposed <see cref="MessageBrokerServer"/>.
    /// </summary>
    public MessageBrokerServer Server { get; }
}
