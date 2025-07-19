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
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents the result of a dead letter query.
/// </summary>
public readonly struct MessageBrokerDeadLetterQueryResult
{
    private MessageBrokerDeadLetterQueryResult(int totalCount, int maxReadCount, Timestamp nextExpirationAt)
    {
        TotalCount = totalCount;
        MaxReadCount = maxReadCount;
        NextExpirationAt = nextExpirationAt;
    }

    /// <summary>
    /// Specifies the current total number of messages in the dead letter.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Specifies the max number of dead letter messages that will be asynchronously consumed.
    /// </summary>
    public int MaxReadCount { get; }

    /// <summary>
    /// Specifies the moment in time when the next dead letter message expires.
    /// </summary>
    public Timestamp NextExpirationAt { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerDeadLetterQueryResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"TotalCount = {TotalCount}{(TotalCount > 0 ? $", MaxReadCount = {MaxReadCount}, NextExpirationAt = {NextExpirationAt}" : string.Empty)}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerDeadLetterQueryResult Create(int totalCount, int maxReadCount, Timestamp nextExpirationAt)
    {
        return new MessageBrokerDeadLetterQueryResult( totalCount, maxReadCount, nextExpirationAt );
    }
}
