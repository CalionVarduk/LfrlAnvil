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
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents an object that can send a single message to the server, with access to the client's internal memory pool,
/// which allows to efficiently buffer data to be sent.
/// </summary>
/// <remarks>
/// Make sure to dispose and discard an instance after sending a message, otherwise issues like memory leaks may be encountered.
/// </remarks>
public sealed class MessageBrokerSendContext : IBufferWriter<byte>, IDisposable
{
    private readonly Memory.MemoryPool<byte> _pool;
    private MessageBrokerPublisher? _publisher;
    private InterlockedBoolean _disposed;
    private MemoryPoolToken<byte> _token;
    private Memory<byte> _buffer;
    private int _written;

    internal MessageBrokerSendContext(Memory.MemoryPool<byte> pool)
    {
        _pool = pool;
        _publisher = null;
        _disposed = new InterlockedBoolean( true );
        _token = MemoryPoolToken<byte>.Empty;
        _buffer = Memory<byte>.Empty;
        _written = 0;
    }

    internal MessageBrokerPublisher Publisher
    {
        get
        {
            Assume.IsNotNull( _publisher );
            return _publisher;
        }
    }

    internal Memory<byte> Data => _buffer.Slice( 0, _written );

    /// <inheritdoc/>
    public void Dispose()
    {
        if ( ! _disposed.WriteTrue() )
            return;

        var publisher = _publisher;
        var token = _token;
        _publisher = null;
        _token = MemoryPoolToken<byte>.Empty;
        _buffer = Memory<byte>.Empty;
        _written = 0;

        Assume.IsNotNull( publisher );
        publisher.Client.ReturnMessageContext( this, token );
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    public void Advance(int count)
    {
        ObjectDisposedException.ThrowIf( _disposed.Value, this );
        Ensure.IsInRange( count, 0, unchecked( _buffer.Length - _written ) );
        _written = unchecked( _written + count );
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    public Memory<byte> GetMemory(int sizeHint)
    {
        EnsureRemainingCapacity( sizeHint );
        return _buffer.Slice( _written );
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    public Span<byte> GetSpan(int sizeHint)
    {
        return GetMemory( sizeHint ).Span;
    }

    /// <summary>
    /// Appends provided <paramref name="data"/> to the end of the buffer.
    /// </summary>
    /// <param name="data">Data to append to the end of the buffer.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    public MessageBrokerSendContext Append(ReadOnlySpan<byte> data)
    {
        data.CopyTo( GetSpan( data.Length ) );
        _written = unchecked( _written + data.Length );
        return this;
    }

    /// <summary>
    /// Sends the buffered message to the publisher's bound channel.
    /// </summary>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MesageBrokerSendResult"/> instance.
    /// </returns>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <remarks>
    /// Unexpected errors encountered during sending will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the message has been successfully enqueued on the server side,
    /// or the publisher is already locally unbound from the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<MesageBrokerSendResult>> SendAsync()
    {
        ObjectDisposedException.ThrowIf( _disposed.Value, this );
        return PublisherCollection.SendAsync( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Initialize(MessageBrokerPublisher publisher, MemorySize minCapacity, bool clearOnDispose)
    {
        Assume.True( _disposed.Value );
        Assume.Equals( _written, 0 );
        Assume.IsNull( _publisher );

        var capacity = Defaults.Memory.GetInitialBufferCapacity( checked( minCapacity.Bytes + Protocol.PushMessageHeader.Length ) );
        _token = _pool.Rent( capacity, out _buffer ).EnableClearing( clearOnDispose );
        _publisher = publisher;
        _written = Protocol.PushMessageHeader.Length;
        _disposed.WriteFalse();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EnsureRemainingCapacity(int desired)
    {
        ObjectDisposedException.ThrowIf( _disposed.Value, this );
        var remaining = unchecked( _buffer.Length - _written );
        if ( desired <= remaining )
            return;

        desired = Defaults.Memory.GetBufferCapacity( checked( _written + desired ) );
        _token.SetLength( desired, out _buffer );
    }
}
