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
/// Represents an event emitted by <see cref="MessageBrokerStream"/> due to an error.
/// </summary>
public readonly struct MessageBrokerStreamErrorEvent
{
    private MessageBrokerStreamErrorEvent(MessageBrokerStream stream, ulong traceId, Exception exception)
    {
        Source = MessageBrokerStreamEventSource.Create( stream, traceId );
        Exception = exception;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerStreamEventSource Source { get; }

    /// <summary>
    /// Encountered error.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerStreamErrorEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Error] {Source}{Environment.NewLine}{Exception}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamErrorEvent Create(MessageBrokerStream stream, ulong traceId, Exception exception)
    {
        return new MessageBrokerStreamErrorEvent( stream, traceId, exception );
    }
}
