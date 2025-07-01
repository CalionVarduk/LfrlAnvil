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
/// Represents an exception thrown when <see cref="MessageBrokerChannelPublisherBinding"/> is in disposed state.
/// </summary>
public class MessageBrokerChannelPublisherBindingDisposedException : OperationCanceledException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerChannelPublisherBindingDisposedException"/> instance.
    /// </summary>
    /// <param name="publisher">Disposed <see cref="MessageBrokerChannelPublisherBinding"/>.</param>
    public MessageBrokerChannelPublisherBindingDisposedException(MessageBrokerChannelPublisherBinding publisher)
        : base( Resources.PublisherDisposed( publisher.Client, publisher.Channel ), new CancellationToken( canceled: true ) )
    {
        Publisher = publisher;
    }

    /// <summary>
    /// Disposed <see cref="MessageBrokerChannelPublisherBinding"/>.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }
}
