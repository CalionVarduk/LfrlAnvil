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
/// Represents an exception thrown when <see cref="MessageBrokerStream"/> is in disposed state.
/// </summary>
public class MessageBrokerStreamDisposedException : OperationCanceledException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerChannelDisposedException"/> instance.
    /// </summary>
    /// <param name="stream">Disposed <see cref="MessageBrokerStream"/>.</param>
    public MessageBrokerStreamDisposedException(MessageBrokerStream stream)
        : base( Resources.StreamDisposed( stream.Id, stream.Name ), new CancellationToken( canceled: true ) )
    {
        Stream = stream;
    }

    /// <summary>
    /// Disposed <see cref="MessageBrokerStream"/>.
    /// </summary>
    public MessageBrokerStream Stream { get; }
}
