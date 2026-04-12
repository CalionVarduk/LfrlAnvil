// Copyright 2025-2026 Łukasz Furlepa
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
using System.Runtime.InteropServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a context for a single message filter predicate applied by a <see cref="MessageBrokerQueueListenerBinding"/>.
/// </summary>
public readonly struct MessageBrokerFilterExpressionContext
{
    private readonly byte[] _source;
    private readonly int _startIndex;
    private readonly int _length;

    internal MessageBrokerFilterExpressionContext(MessageBrokerQueueListenerBinding listener, in StreamMessage message)
    {
        MemoryMarshal.TryGetArray( message.Data, out var segment );
        _source = segment.Array ?? Array.Empty<byte>();
        _startIndex = segment.Offset;
        _length = segment.Count;
        Listener = listener;
        Publisher = message.Publisher;
        MessageId = message.Id;
        PushedAt = message.PushedAt;
    }

    /// <summary>
    /// <see cref="MessageBrokerQueueListenerBinding"/> that filters the message.
    /// </summary>
    public MessageBrokerQueueListenerBinding Listener { get; }

    /// <summary>
    /// <see cref="IMessageBrokerMessagePublisher"/> that pushed the message.
    /// </summary>
    public IMessageBrokerMessagePublisher Publisher { get; }

    /// <summary>
    /// Unique message id.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Moment of registration of this message in the <see cref="MessageBrokerStream"/>.
    /// </summary>
    public Timestamp PushedAt { get; }

    /// <summary>
    /// Binary message data.
    /// </summary>
    public Span Data => new Span( _source, _startIndex, _length );

    /// <summary>
    /// Converts binary message data to <see cref="ReadOnlyMemory{T}"/>.
    /// </summary>
    /// <returns>New <see cref="ReadOnlyMemory{T}"/> instance.</returns>
    [Pure]
    public ReadOnlyMemory<byte> AsMemory()
    {
        return _source.AsMemory( _startIndex, _length );
    }

    /// <summary>
    /// Converts binary message data to <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <returns>New <see cref="ReadOnlySpan{T}"/> instance.</returns>
    [Pure]
    public ReadOnlySpan<byte> AsSpan()
    {
        return _source.AsSpan( _startIndex, _length );
    }

    /// <summary>
    /// Represents binary message data.
    /// </summary>
    public readonly ref struct Span
    {
        private readonly ReadOnlySpan<byte> _data;

        internal Span(byte[] source, int startIndex, int length)
        {
            ref var first = ref Unsafe.Add( ref MemoryMarshal.GetArrayDataReference( source ), startIndex );
            _data = MemoryMarshal.CreateReadOnlySpan( ref first, length );
        }

        /// <summary>
        /// Binary message length.
        /// </summary>
        public int Length => _data.Length;

        /// <summary>
        /// Gets the byte at the provided 0-based <paramref name="index"/>.
        /// </summary>
        /// <param name="index">0-based index of the byte to get.</param>
        /// <exception cref="IndexOutOfRangeException">When <paramref name="index"/> is out of range.</exception>
        public byte this[int index] => _data[index];
    }
}
