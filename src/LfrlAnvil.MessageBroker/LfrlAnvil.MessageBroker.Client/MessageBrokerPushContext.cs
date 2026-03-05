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
/// Represents an object that can push a single message to the server, with access to the client's internal memory pool,
/// which allows to efficiently buffer data to be sent.
/// </summary>
/// <remarks>
/// Make sure to dispose and discard an instance after pushing a message, otherwise issues like memory leaks may be encountered.
/// </remarks>
public sealed class MessageBrokerPushContext : IBufferWriter<byte>, IDisposable
{
    private readonly Memory.MemoryPool<byte> _pool;
    private MessageBrokerPublisher? _publisher;
    private InterlockedBoolean _disposed;
    private MemoryPoolToken<byte> _token;
    private Memory<byte> _buffer;
    private int _written;
    internal MessageRouting Routing;

    internal MessageBrokerPushContext(Memory.MemoryPool<byte> pool)
    {
        _pool = pool;
        _publisher = null;
        _disposed = new InterlockedBoolean( true );
        _token = MemoryPoolToken<byte>.Empty;
        _buffer = Memory<byte>.Empty;
        _written = 0;
        Routing = default;
    }

    /// <summary>
    /// Returns remaining available routing network packet length based on
    /// currently written routing data and <see cref="MessageBrokerClient.MaxNetworkPacketLength"/>.
    /// </summary>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    public MemorySize RemainingRoutingPacketLength
    {
        get
        {
            ObjectDisposedException.ThrowIf( _disposed.Value, this );
            return MemorySize.FromBytes( Routing.GetRemainingBytes( Publisher.Client ) );
        }
    }

    /// <summary>
    /// Returns remaining available network packet length based on
    /// currently written data and <see cref="MessageBrokerClient.MaxNetworkMessagePacketLength"/> (reduced by necessary packet headers).
    /// </summary>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    public MemorySize RemainingPacketLength
    {
        get
        {
            ObjectDisposedException.ThrowIf( _disposed.Value, this );
            return MemorySize.FromBytes( unchecked( Publisher.Client.MaxNetworkPushMessagePacketBytes - _written ) );
        }
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
        var routingToken = Routing.Token;
        _publisher = null;
        _token = MemoryPoolToken<byte>.Empty;
        _buffer = Memory<byte>.Empty;
        _written = 0;
        Routing = default;

        Assume.IsNotNull( publisher );
        publisher.Client.ReturnMessageContext( this, token, routingToken );
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
    public MessageBrokerPushContext Append(ReadOnlySpan<byte> data)
    {
        data.CopyTo( GetSpan( data.Length ) );
        _written = unchecked( _written + data.Length );
        return this;
    }

    /// <summary>
    /// Pushes the buffered message to the publisher's bound channel.
    /// </summary>
    /// <param name="confirm">
    /// Specifies whether the server should send confirmation that it received the message. Equal to <b>true</b> by default.
    /// </param>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerPushResult"/> instance.
    /// </returns>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <remarks>
    /// Unexpected errors encountered during pushing will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the message has been successfully enqueued on the server side,
    /// or the publisher is already locally unbound from the channel, which will cancel the request to the server.
    /// </remarks>
    public async ValueTask<Result<MessageBrokerPushResult>> PushAsync(bool confirm = true)
    {
        var finalizer = await EnqueueAsync( confirm ).ConfigureAwait( false );
        if ( finalizer.Exception is not null )
            return finalizer.Exception;

        return await finalizer.Value.PushAsync().ConfigureAwait( false );
    }

    /// <summary>
    /// Enqueues the buffered message for sending and returns an object capable of finalizing the process.
    /// </summary>
    /// <param name="confirm">
    /// Specifies whether the server should send confirmation that it received the message. Equal to <b>true</b> by default.
    /// </param>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerPushMessageFinalizer"/> instance.
    /// </returns>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <remarks>
    /// Unexpected errors encountered during enqueueing will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the message has been successfully enqueued for sending,
    /// or the publisher is already locally unbound from the channel, which will cancel the enqueueing.
    /// <b>Make sure to always invoke returned finalizer's <see cref="MessageBrokerPushMessageFinalizer.PushAsync"/> method,
    /// otherwise the client may get deadlocked and may no longer be able to send any further requests to the server!</b>
    /// </remarks>
    public ValueTask<Result<MessageBrokerPushMessageFinalizer>> EnqueueAsync(bool confirm = true)
    {
        ObjectDisposedException.ThrowIf( _disposed.Value, this );
        return PublisherCollection.EnqueueAsync( this, confirm );
    }

    /// <summary>
    /// Adds routing target to the message in the form of a client id.
    /// </summary>
    /// <param name="clientId">Id of the target client.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="clientId"/> is less than or equal to <b>0</b>.</exception>
    /// <exception cref="InvalidOperationException">When routing target count limit of <b>32767</b> has already been reached.</exception>
    public MessageBrokerPushContext AddTarget(int clientId)
    {
        ObjectDisposedException.ThrowIf( _disposed.Value, this );
        Routing.Add( Publisher.Client, clientId );
        return this;
    }

    /// <summary>
    /// Adds routing target to the message in the form of a client name.
    /// </summary>
    /// <param name="clientName">Name of the target client.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="ObjectDisposedException">When this context has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="clientName"/>'s length is less than <b>1</b> or greater than <b>512</b>.
    /// </exception>
    /// <exception cref="InvalidOperationException">When routing target count limit of <b>32767</b> has already been reached.</exception>
    public MessageBrokerPushContext AddTarget(string clientName)
    {
        ObjectDisposedException.ThrowIf( _disposed.Value, this );
        Routing.Add( Publisher.Client, clientName );
        return this;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Initialize(MessageBrokerPublisher publisher, MemorySize minCapacity)
    {
        Assume.True( _disposed.Value );
        Assume.Equals( _written, 0 );
        Assume.IsNull( _publisher );

        var capacity = Defaults.Memory.GetInitialBufferCapacity( checked( minCapacity.Bytes + Protocol.PushMessageHeader.Length ) );
        _token = _pool.Rent( capacity, publisher.Client.ClearBuffers, out _buffer );
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
        _token.IncreaseLength( desired, out _buffer );
    }
}
