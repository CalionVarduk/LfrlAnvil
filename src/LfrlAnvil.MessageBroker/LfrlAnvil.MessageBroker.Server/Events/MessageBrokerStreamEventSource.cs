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

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents a source of an event emitted by <see cref="MessageBrokerStream"/>.
/// </summary>
public readonly struct MessageBrokerStreamEventSource
{
    private MessageBrokerStreamEventSource(MessageBrokerStream stream, ulong traceId)
    {
        Stream = stream;
        TraceId = traceId;
    }

    /// <summary>
    /// <see cref="MessageBrokerStream"/> that emitted an event.
    /// </summary>
    public MessageBrokerStream Stream { get; }

    /// <summary>
    /// Identifier of an internal trace, with which an event is correlated to.
    /// </summary>
    /// <remarks>Can be used to correlate multiple events together, which are part of the same operation.</remarks>
    public ulong TraceId { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerStreamEventSource"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"Stream = [{Stream.Id}] '{Stream.Name}', TraceId = {TraceId}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamEventSource Create(MessageBrokerStream stream, ulong traceId)
    {
        return new MessageBrokerStreamEventSource( stream, traceId );
    }
}
