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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> due to an error.
/// </summary>
public readonly struct MessageBrokerQueueErrorEvent
{
    private MessageBrokerQueueErrorEvent(MessageBrokerQueue queue, ulong traceId, Exception exception)
    {
        Source = MessageBrokerQueueEventSource.Create( queue, traceId );
        Exception = exception;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// Encountered error.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueErrorEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Error] {Source}{Environment.NewLine}{Exception}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueErrorEvent Create(MessageBrokerQueue queue, ulong traceId, Exception exception)
    {
        return new MessageBrokerQueueErrorEvent( queue, traceId, exception );
    }
}
