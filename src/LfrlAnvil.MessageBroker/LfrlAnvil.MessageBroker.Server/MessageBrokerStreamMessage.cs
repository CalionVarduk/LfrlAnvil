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

using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a snapshot of a message stored by a <see cref="MessageBrokerStream"/>.
/// </summary>
public readonly struct MessageBrokerStreamMessage
{
    internal MessageBrokerStreamMessage(
        MessageBrokerChannelPublisherBinding publisher,
        MemorySize length,
        ulong id,
        Timestamp pushedAt,
        int storeKey,
        int refCount)
    {
        Publisher = publisher;
        Length = length;
        Id = id;
        PushedAt = pushedAt;
        StoreKey = storeKey;
        RefCount = refCount;
    }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that pushed this message.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Length of this message.
    /// </summary>
    public MemorySize Length { get; }

    /// <summary>
    /// Unique id of this message.
    /// </summary>
    public ulong Id { get; }

    /// <summary>
    /// Moment in time when this message has been pushed.
    /// </summary>
    public Timestamp PushedAt { get; }

    /// <summary>
    /// Store key of this message.
    /// </summary>
    public int StoreKey { get; }

    /// <summary>
    /// Number of references to this message.
    /// </summary>
    public int RefCount { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerStreamMessage"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"Publisher = ({Publisher}), Length = {Length}, Id = {Id}, PushedAt = {PushedAt}, StoreKey = {StoreKey}, RefCount = {RefCount}";
    }
}
