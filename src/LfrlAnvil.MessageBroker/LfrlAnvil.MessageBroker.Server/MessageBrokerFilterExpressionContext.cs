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
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a context for a single message filter predicate applied by a <see cref="MessageBrokerChannelListenerBinding"/>.
/// </summary>
public readonly struct MessageBrokerFilterExpressionContext
{
    internal MessageBrokerFilterExpressionContext(MessageBrokerChannelListenerBinding listener, in StreamMessage message)
    {
        Listener = listener;
        Publisher = message.Publisher;
        Id = message.Id;
        PushedAt = message.PushedAt;
        Data = message.Data;
    }

    /// <summary>
    /// <see cref="MessageBrokerChannelListenerBinding"/> that filters the message.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that pushed the message.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Unique message id.
    /// </summary>
    public ulong Id { get; }

    /// <summary>
    /// Moment of registration of this message in the <see cref="MessageBrokerStream"/>.
    /// </summary>
    public Timestamp PushedAt { get; }

    /// <summary>
    /// Binary message data.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }
}
