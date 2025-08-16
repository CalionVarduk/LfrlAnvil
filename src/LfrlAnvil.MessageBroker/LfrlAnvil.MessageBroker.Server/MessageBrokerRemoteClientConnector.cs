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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a remote message broker client connector.
/// </summary>
public sealed class MessageBrokerRemoteClientConnector
{
    private readonly TcpClient _tcp;
    private readonly TaskCompletionSource _completed;
    private readonly MemoryPool<byte> _memoryPool;
    private CancellationTokenSource _cancellationSource;
    private Task? _handshakeTask;
    private Stream _stream;
    private MessageBrokerRemoteClientConnectorState _state;

    internal MessageBrokerRemoteClientConnector(
        int id,
        MessageBrokerServer server,
        TcpClient tcp,
        CancellationTokenSource cancellationSource)
    {
        _tcp = tcp;
        _stream = _tcp.GetStream();
        Server = server;
        Id = id;
        _handshakeTask = null;
        _state = MessageBrokerRemoteClientConnectorState.Created;
        _memoryPool = new MemoryPool<byte>( unchecked( ( int )server.MaxNetworkPacketLength.Bytes ) );
        _completed = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        _cancellationSource = cancellationSource;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance to which this client belongs to.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Connector's unique identifier assigned by the server.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The remote <see cref="IPEndPoint"/> of the remote client to which this connector connects to.
    /// </summary>
    public EndPoint? RemoteEndPoint
    {
        get
        {
            using ( AcquireLock() )
            {
                try
                {
                    return _tcp.Client.RemoteEndPoint;
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// The local <see cref="EndPoint"/> that this connector is using for communications with the remote client.
    /// </summary>
    public EndPoint? LocalEndPoint
    {
        get
        {
            using ( AcquireLock() )
            {
                try
                {
                    return _tcp.Client.LocalEndPoint;
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// Current connector's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerRemoteClientConnectorState"/> for more information.</remarks>
    public MessageBrokerRemoteClientConnectorState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    internal bool ShouldCancel => _state >= MessageBrokerRemoteClientConnectorState.Cancelling;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientConnector"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Id}] connector ({State})";
    }

    /// <summary>
    /// Requests this connector to cancel operations.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous disconnect operation.
    /// The underlying <see cref="Result{T}"/> returns <b>true</b> when cancellation was successfully requested,
    /// otherwise <b>false</b>.
    /// </returns>
    public async ValueTask<Result<bool>> CancelAsync()
    {
        var cancelled = false;
        Exception? exception = null;
        using ( AcquireLock() )
        {
            if ( ! ShouldCancel )
            {
                cancelled = true;
                _state = MessageBrokerRemoteClientConnectorState.Cancelling;
                exception = _tcp.TryDispose().Exception;
            }
        }

        await _completed.Task.ConfigureAwait( false );
        return exception is not null ? Result.Error( exception, cancelled ) : Result.Create( cancelled );
    }

    internal async ValueTask StartAsync(ulong serverTraceId)
    {
        var failed = true;
        try
        {
            if ( Server.Logger.ConnectorStarted is { } connectorStarted )
                connectorStarted.Emit( MessageBrokerServerConnectorStartedEvent.Create( this, serverTraceId ) );

            using ( AcquireActiveLock( serverTraceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                Assume.Equals( _state, MessageBrokerRemoteClientConnectorState.Created );
                _state = MessageBrokerRemoteClientConnectorState.Handshaking;
                _handshakeTask = StartHandshakeTask( serverTraceId, _stream );
            }

            failed = false;
        }
        catch ( Exception exc )
        {
            if ( Server.Logger.Error is { } error )
                error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, exc ) );
        }
        finally
        {
            if ( failed )
            {
                await FailAsync( serverTraceId ).ConfigureAwait( false );
                if ( Server.Logger.TraceEnd is { } traceEnd )
                    traceEnd.Emit(
                        MessageBrokerServerTraceEvent.Create( Server, serverTraceId, MessageBrokerServerTraceEventType.AcceptClient ) );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _tcp, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireActiveLock(ulong serverTraceId, out MessageBrokerRemoteClientConnectorDisposedException? exception)
    {
        var @lock = AcquireLock();
        if ( ! ShouldCancel )
        {
            exception = null;
            return @lock;
        }

        @lock.Dispose();
        exception = this.DisposedException();
        if ( Server.Logger.Error is { } error )
            error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, exception ) );

        return default;
    }

    private Task StartHandshakeTask(ulong serverTraceId, Stream stream)
    {
        return Task.Run(
            async () =>
            {
                var failed = true;
                try
                {
                    if ( Server.StreamDecorator is not null )
                    {
                        stream = await Server.StreamDecorator( this, ReinterpretCast.To<NetworkStream>( stream ) ).ConfigureAwait( false );
                        using ( AcquireActiveLock( serverTraceId, out var exc ) )
                        {
                            if ( exc is not null )
                                return;

                            _stream = stream;
                        }
                    }

                    var handshake = await ReadHandshakeRequestAsync( serverTraceId, stream ).ConfigureAwait( false );
                    if ( handshake.Exception is not null )
                    {
                        if ( handshake.Value.RejectionReason != Protocol.HandshakeRejectedResponse.Reasons.None )
                            await SendHandshakeRejectionAsync(
                                    serverTraceId,
                                    stream,
                                    handshake.Value.RejectionReason,
                                    handshake.Value.IsClientLittleEndian )
                                .ConfigureAwait( false );

                        return;
                    }

                    Assume.IsNotNull( handshake.Value.Client );
                    failed = false;
                    await handshake.Value.Client.StartAsync( serverTraceId ).ConfigureAwait( false );
                }
                catch ( Exception exc )
                {
                    if ( Server.Logger.Error is { } error )
                        error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, exc ) );
                }
                finally
                {
                    if ( failed )
                        await FailAsync( serverTraceId, ignoreTask: true ).ConfigureAwait( false );
                    else
                        _completed.TrySetResult();

                    if ( Server.Logger.TraceEnd is { } traceEnd )
                        traceEnd.Emit(
                            MessageBrokerServerTraceEvent.Create( Server, serverTraceId, MessageBrokerServerTraceEventType.AcceptClient ) );
                }
            } );
    }

    private async ValueTask<Result<HandshakeResult>> ReadHandshakeRequestAsync(ulong serverTraceId, Stream stream)
    {
        var poolToken = MemoryPoolToken<byte>.Empty;
        var timeoutToken = default( CancellationToken );
        try
        {
            poolToken = _memoryPool.Rent( Protocol.HandshakeRequestHeader.Length, clear: true, out var buffer );
            using ( AcquireActiveLock( serverTraceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                timeoutToken = _cancellationSource.Token;
                _cancellationSource.CancelAfter( Server.HandshakeTimeout );
            }

            var data = buffer.Slice( 0, Protocol.PacketHeader.Length );
            await stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            var header = Protocol.PacketHeader.Parse( data );
            if ( BitConverter.IsLittleEndian )
                header = header.ReverseEndianness();

            var readPacket = Server.Logger.ReadPacket;
            readPacket?.Emit( MessageBrokerServerReadPacketEvent.CreateReceived( this, serverTraceId, header ) );

            if ( header.GetServerEndpoint() != MessageBrokerServerEndpoint.HandshakeRequest )
            {
                var exc = this.ProtocolException( header, Resources.UnexpectedServerEndpoint );
                if ( Server.Logger.Error is { } error )
                    error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, exc ) );

                return exc;
            }

            var packetLength = header.AssertPacketLength( this, Defaults.Memory.DefaultNetworkPacketLength );
            if ( packetLength.Exception is not null )
            {
                if ( Server.Logger.Error is { } error )
                    error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, packetLength.Exception ) );

                return packetLength.Exception;
            }

            var exception = header.AssertMinPayload( this, Protocol.HandshakeRequestHeader.Length );
            if ( exception is not null )
            {
                if ( Server.Logger.Error is { } error )
                    error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, exception ) );

                return exception;
            }

            if ( packetLength.Value > buffer.Length )
                poolToken.IncreaseLength( packetLength.Value, out buffer );

            data = buffer.Slice( 0, packetLength.Value );
            await stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            var handshakeHeader = Protocol.HandshakeRequestHeader.Parse( data );

            var name = TextEncoding.Parse( data.Slice( Protocol.HandshakeRequestHeader.Length ) );
            if ( name.Exception is not null )
            {
                if ( Server.Logger.Error is { } error )
                    error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, name.Exception ) );

                return name.Exception;
            }

            Assume.IsNotNull( name.Value );
            if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
            {
                var exc = this.ProtocolException( header, Resources.InvalidNameLength( name.Value.Length ) );
                if ( Server.Logger.Error is { } error )
                    error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, exc ) );

                return Result.Error(
                    exc,
                    new HandshakeResult(
                        null,
                        Protocol.HandshakeRejectedResponse.Reasons.InvalidNameLength,
                        handshakeHeader.IsClientLittleEndian ) );
            }

            if ( Server.Logger.HandshakeReceived is { } handshakeReceived )
                handshakeReceived.Emit(
                    MessageBrokerServerHandshakeReceivedEvent.Create(
                        this,
                        serverTraceId,
                        name.Value,
                        handshakeHeader.MessageTimeout,
                        handshakeHeader.PingInterval,
                        handshakeHeader.MaxBatchPacketCount,
                        handshakeHeader.MaxNetworkBatchPacketLength,
                        handshakeHeader.SynchronizeExternalObjectNames,
                        handshakeHeader.ClearBuffers,
                        handshakeHeader.IsClientLittleEndian ) );

            var exceptions = Chain<Exception>.Empty;
            Result<MessageBrokerRemoteClient> client;
            bool alreadyConnected;
            using ( Server.AcquireActiveLock( serverTraceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                using ( AcquireActiveLock( serverTraceId, out var connExc ) )
                {
                    if ( connExc is not null )
                        return connExc;

                    client = RemoteClientCollection.TryRegisterUnsafe(
                        Server,
                        _tcp,
                        stream,
                        _memoryPool,
                        name.Value,
                        in handshakeHeader,
                        out alreadyConnected );

                    if ( client.Exception is null )
                    {
                        _state = MessageBrokerRemoteClientConnectorState.Connected;
                        _handshakeTask = null;
                        var cancellationSource = TryResetCancellationSource( ref exceptions );
                        RemoteClientConnectorCollection.RemoveUnsafe( this, cancellationSource );
                    }
                    else if ( ! _cancellationSource.TryReset() )
                    {
                        _cancellationSource.TryCleanUp( ref exceptions );
                        _cancellationSource = RemoteClientConnectorCollection.GetCancellationSourceUnsafe( Server );
                    }
                }
            }

            Server.TryEmitErrors( serverTraceId, exceptions );
            if ( client.Exception is not null )
            {
                if ( Server.Logger.Error is { } error )
                    error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, client.Exception ) );

                return Result.Error(
                    client.Exception,
                    alreadyConnected
                        ? new HandshakeResult(
                            null,
                            Protocol.HandshakeRejectedResponse.Reasons.AlreadyConnected,
                            handshakeHeader.IsClientLittleEndian )
                        : default );
            }

            Assume.IsNotNull( client.Value );
            readPacket?.Emit( MessageBrokerServerReadPacketEvent.CreateAccepted( this, serverTraceId, header ) );
            return new HandshakeResult( client.Value, Protocol.HandshakeRejectedResponse.Reasons.None, client.Value.IsLittleEndian );
        }
        catch ( Exception exc )
        {
            if ( exc is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken )
                exc = this.RequestTimeoutException();

            if ( Server.Logger.Error is { } error )
                error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, exc ) );

            return exc;
        }
        finally
        {
            var exc = poolToken.Return();
            if ( exc is not null && Server.Logger.Error is { } error )
                error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, exc ) );
        }
    }

    private async ValueTask SendHandshakeRejectionAsync(
        ulong serverTraceId,
        Stream stream,
        Protocol.HandshakeRejectedResponse.Reasons reason,
        bool isClientLittleEndian)
    {
        var response = new Protocol.HandshakeRejectedResponse( reason );

        var sendPacket = Server.Logger.SendPacket;
        sendPacket?.Emit( MessageBrokerServerSendPacketEvent.CreateSending( this, serverTraceId, response.Header ) );

        var poolToken = MemoryPoolToken<byte>.Empty;
        try
        {
            poolToken = _memoryPool.Rent(
                Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload,
                clear: true,
                out var data );

            response.Serialize( data, isClientLittleEndian != BitConverter.IsLittleEndian );

            CancellationToken timeoutToken;
            using ( AcquireActiveLock( serverTraceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                timeoutToken = _cancellationSource.Token;
                _cancellationSource.CancelAfter( Server.HandshakeTimeout );
            }

            await stream.WriteAsync( data, timeoutToken ).ConfigureAwait( false );

            sendPacket?.Emit( MessageBrokerServerSendPacketEvent.CreateSent( this, serverTraceId, response.Header ) );
        }
        catch ( Exception exc )
        {
            if ( Server.Logger.Error is { } error )
                error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, exc ) );
        }
        finally
        {
            var exc = poolToken.Return();
            if ( exc is not null && Server.Logger.Error is { } error )
                error.Emit( MessageBrokerServerErrorEvent.Create( Server, serverTraceId, exc ) );
        }
    }

    private async ValueTask FailAsync(ulong serverTraceId, bool ignoreTask = false)
    {
        using ( AcquireLock() )
        {
            if ( _state >= MessageBrokerRemoteClientConnectorState.Connected )
                return;

            _state = _state == MessageBrokerRemoteClientConnectorState.Cancelling
                ? MessageBrokerRemoteClientConnectorState.Cancelled
                : MessageBrokerRemoteClientConnectorState.Failed;
        }

        try
        {
            Task? handshakeTask = null;
            var exceptions = Chain<Exception>.Empty;
            CancellationTokenSource? cancellationSource;
            using ( AcquireLock() )
            {
                if ( ! ignoreTask )
                    handshakeTask = _handshakeTask;

                _handshakeTask = null;
                _tcp.TryDispose().TryAddExceptionTo( ref exceptions );
                cancellationSource = TryResetCancellationSource( ref exceptions );
            }

            var taskResult = await handshakeTask.SafeWaitAsync().ConfigureAwait( false );
            taskResult.TryAddExceptionTo( ref exceptions );

            using ( Server.AcquireActiveLock( serverTraceId, out var exc ) )
            {
                if ( exc is not null )
                    cancellationSource?.TryCleanUp( ref exceptions );
                else
                    RemoteClientConnectorCollection.RemoveUnsafe( this, cancellationSource );
            }

            Server.TryEmitErrors( serverTraceId, exceptions );
        }
        finally
        {
            _completed.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private CancellationTokenSource? TryResetCancellationSource(ref Chain<Exception> exceptions)
    {
        if ( _cancellationSource.TryReset() )
            return _cancellationSource;

        _cancellationSource.TryCleanUp( ref exceptions );
        return null;
    }

    private readonly struct HandshakeResult
    {
        internal readonly MessageBrokerRemoteClient? Client;
        internal readonly Protocol.HandshakeRejectedResponse.Reasons RejectionReason;
        internal readonly bool IsClientLittleEndian;

        internal HandshakeResult(
            MessageBrokerRemoteClient? client,
            Protocol.HandshakeRejectedResponse.Reasons rejectionReason,
            bool isClientLittleEndian)
        {
            Client = client;
            RejectionReason = rejectionReason;
            IsClientLittleEndian = isClientLittleEndian;
        }
    }
}
