using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client.Tests.Helpers;

internal sealed class ServerMock : IDisposable
{
    private readonly TcpListener _listener = new TcpListener( IPAddress.Loopback, 0 );
    private readonly List<byte[]> _received = new List<byte[]>();
    private TcpClient? _client;

    public void Dispose()
    {
        lock ( _listener )
        {
            _client?.TryDispose();
            _client = null;
            _listener.Stop();
        }
    }

    internal IPEndPoint Start()
    {
        lock ( _listener )
        {
            _listener.Start();
            return ( IPEndPoint )_listener.LocalEndpoint;
        }
    }

    internal void WaitForClient()
    {
        lock ( _listener )
        {
            _client = _listener.AcceptTcpClient();
            _client.ReceiveTimeout = ChronoConstants.MillisecondsPerSecond * 15;
            _client.SendTimeout = ChronoConstants.MillisecondsPerSecond * 15;
            _client.NoDelay = true;
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
                _client.Client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );
        }
    }

    internal byte[] ReadConfirmHandshakeResponse()
    {
        return AssertEndpoint( Read( Protocol.PacketHeader.Length ), MessageBrokerServerEndpoint.ConfirmHandshakeResponse );
    }

    internal byte[] ReadPing()
    {
        return AssertEndpoint( Read( Protocol.PacketHeader.Length ), MessageBrokerServerEndpoint.Ping );
    }

    internal byte[] ReadUnbindPublisherRequest()
    {
        return AssertEndpoint( Read( Protocol.UnbindPublisherRequest.Length ), MessageBrokerServerEndpoint.UnbindPublisherRequest );
    }

    internal byte[] ReadUnbindListenerRequest()
    {
        return AssertEndpoint( Read( Protocol.UnbindListenerRequest.Length ), MessageBrokerServerEndpoint.UnbindListenerRequest );
    }

    internal byte[] ReadMessageNotificationAck()
    {
        return AssertEndpoint( Read( Protocol.MessageNotificationAck.Length ), MessageBrokerServerEndpoint.MessageNotificationAck );
    }

    internal byte[] ReadMessageNotificationNegativeAck()
    {
        return AssertEndpoint(
            Read( Protocol.MessageNotificationNegativeAck.Length ),
            MessageBrokerServerEndpoint.MessageNotificationNack );
    }

    internal byte[] ReadPushMessageRouting(int length)
    {
        return AssertEndpoint( Read( Protocol.PushMessageRoutingHeader.Length + length ), MessageBrokerServerEndpoint.PushMessageRouting );
    }

    internal byte[] ReadPushMessage(int length)
    {
        return AssertEndpoint( Read( Protocol.PushMessageHeader.Length + length ), MessageBrokerServerEndpoint.PushMessage );
    }

    internal byte[] ReadDeadLetterQuery()
    {
        return AssertEndpoint( Read( Protocol.DeadLetterQuery.Length ), MessageBrokerServerEndpoint.DeadLetterQuery );
    }

    internal byte[] Read(Protocol.HandshakeRequest request)
    {
        return AssertEndpoint( Read( request.Length ), MessageBrokerServerEndpoint.HandshakeRequest );
    }

    internal byte[] Read(Protocol.BindPublisherRequest request)
    {
        return AssertEndpoint( Read( request.Length ), MessageBrokerServerEndpoint.BindPublisherRequest );
    }

    internal byte[] Read(Protocol.BindListenerRequest request)
    {
        return AssertEndpoint( Read( request.Length ), MessageBrokerServerEndpoint.BindListenerRequest );
    }

    internal byte[] Read(Protocol.PushMessageHeader request)
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + ( int )request.Header.Payload ),
            MessageBrokerServerEndpoint.PushMessage );
    }

    internal byte[] Read(int length)
    {
        lock ( _listener )
        {
            Assume.IsNotNull( _client );
            var buffer = new byte[length];
            _client.GetStream().ReadExactly( buffer );
            _received.Add( buffer );
            return buffer;
        }
    }

    internal void SendHandshakeAccepted(int id, Duration messageTimeout, Duration pingInterval, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.HandshakeAcceptedResponse );
        writer.MoveWrite( payload ?? Protocol.HandshakeAcceptedResponse.Length );
        writer.MoveWrite( ( byte )(BitConverter.IsLittleEndian ? 1 : 0) );
        writer.MoveWrite( ( uint )id );
        writer.MoveWrite( ( uint )messageTimeout.FullMilliseconds );
        writer.Write( ( uint )pingInterval.FullMilliseconds );
        Send( buffer );
    }

    internal void SendHandshakeRejected(bool invalidNameLength, bool nameDecodingFailure, bool nameAlreadyExists, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.HandshakeRejectedResponse );
        writer.MoveWrite( payload ?? Protocol.HandshakeRejectedResponse.Length );
        writer.Write( ( byte )((invalidNameLength ? 1 : 0) | (nameDecodingFailure ? 2 : 0) | (nameAlreadyExists ? 4 : 0)) );
        Send( buffer );
    }

    internal void SendPong(uint? endiannessPayload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.Pong );
        writer.Write( endiannessPayload ?? Protocol.Endianness.VerificationPayload );
        Send( buffer );
    }

    internal void SendPublisherBoundResponse(bool channelCreated, bool streamCreated, int channelId, int streamId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.PublisherBoundResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.PublisherBoundResponse );
        writer.MoveWrite( payload ?? Protocol.PublisherBoundResponse.Length );
        writer.MoveWrite( ( byte )((channelCreated ? 1 : 0) | (streamCreated ? 2 : 0)) );
        writer.MoveWrite( ( uint )channelId );
        writer.Write( ( uint )streamId );
        Send( buffer );
    }

    internal void SendBindPublisherFailureResponse(bool clientAlreadyBound, bool cancelled, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.BindPublisherFailureResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.BindPublisherFailureResponse );
        writer.MoveWrite( payload ?? Protocol.BindPublisherFailureResponse.Length );
        writer.Write( ( byte )((clientAlreadyBound ? 1 : 0) | (cancelled ? 2 : 0)) );
        Send( buffer );
    }

    internal void SendPublisherUnboundResponse(bool channelRemoved, bool streamRemoved, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.PublisherUnboundResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.PublisherUnboundResponse );
        writer.MoveWrite( payload ?? Protocol.PublisherUnboundResponse.Length );
        writer.Write( ( byte )((channelRemoved ? 1 : 0) | (streamRemoved ? 2 : 0)) );
        Send( buffer );
    }

    internal void SendUnbindPublisherFailureResponse(bool clientNotBound, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnbindPublisherFailureResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.UnbindPublisherFailureResponse );
        writer.MoveWrite( payload ?? Protocol.UnbindPublisherFailureResponse.Length );
        writer.Write( ( byte )(clientNotBound ? 1 : 0) );
        Send( buffer );
    }

    internal void SendListenerBoundResponse(bool channelCreated, bool queueCreated, int channelId, int queueId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.ListenerBoundResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.ListenerBoundResponse );
        writer.MoveWrite( payload ?? Protocol.ListenerBoundResponse.Length );
        writer.MoveWrite( ( byte )((channelCreated ? 1 : 0) | (queueCreated ? 2 : 0)) );
        writer.MoveWrite( ( uint )channelId );
        writer.Write( ( uint )queueId );
        Send( buffer );
    }

    internal void SendBindListenerFailureResponse(
        bool channelDoesNotExist,
        bool clientAlreadyBound,
        bool cancelled,
        bool unexpectedFilterExpression,
        bool invalidFilterExpression,
        uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.BindListenerFailureResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.BindListenerFailureResponse );
        writer.MoveWrite( payload ?? Protocol.BindListenerFailureResponse.Length );
        writer.Write(
            ( byte )((clientAlreadyBound ? 1 : 0)
                | (cancelled ? 2 : 0)
                | (channelDoesNotExist ? 4 : 0)
                | (unexpectedFilterExpression ? 8 : 0)
                | (invalidFilterExpression ? 16 : 0)) );

        Send( buffer );
    }

    internal void SendListenerUnboundResponse(bool channelRemoved, bool queueRemoved, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.ListenerUnboundResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.ListenerUnboundResponse );
        writer.MoveWrite( payload ?? Protocol.ListenerUnboundResponse.Length );
        writer.Write( ( byte )((channelRemoved ? 1 : 0) | (queueRemoved ? 2 : 0)) );
        Send( buffer );
    }

    internal void SendUnbindListenerFailureResponse(bool clientNotBound, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnbindListenerFailureResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.UnbindListenerFailureResponse );
        writer.MoveWrite( payload ?? Protocol.UnbindListenerFailureResponse.Length );
        writer.Write( ( byte )(clientNotBound ? 1 : 0) );
        Send( buffer );
    }

    internal void SendDeadLetterQueryResponse(int totalCount, int maxReadCount, Timestamp nextExpirationAt, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.DeadLetterQueryResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.DeadLetterQueryResponse );
        writer.MoveWrite( payload ?? Protocol.DeadLetterQueryResponse.Length );
        writer.MoveWrite( ( uint )totalCount );
        writer.MoveWrite( ( uint )maxReadCount );
        writer.Write( ( ulong )nextExpirationAt.UnixEpochTicks );
        Send( buffer );
    }

    internal void SendMessageAcceptedResponse(ulong messageId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.MessageAcceptedResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.MessageAcceptedResponse );
        writer.MoveWrite( payload ?? Protocol.MessageAcceptedResponse.Length );
        writer.Write( messageId );
        Send( buffer );
    }

    internal void SendMessageRejectedResponse(bool notBound, bool cancelled, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.MessageRejectedResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.MessageRejectedResponse );
        writer.MoveWrite( payload ?? Protocol.MessageRejectedResponse.Length );
        writer.Write( ( byte )((notBound ? 1 : 0) | (cancelled ? 2 : 0)) );
        Send( buffer );
    }

    internal void SendMessageNotification(
        int ackId,
        ulong messageId,
        int senderId,
        int channelId,
        int streamId,
        byte[] data,
        Timestamp? pushedAt = null,
        bool isRetry = false,
        int? retry = null,
        bool isRedelivery = false,
        int? redelivery = null,
        uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Length + data.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.MessageNotification );
        writer.MoveWrite( payload ?? ( uint )(Protocol.MessageNotificationHeader.Length + data.Length) );
        writer.MoveWrite( ( uint )ackId );
        writer.MoveWrite( ( uint )streamId );
        writer.MoveWrite( messageId );
        writer.MoveWrite( ( uint )(retry ?? 0) | (isRetry ? 1U << 31 : 0) );
        writer.MoveWrite( ( uint )(redelivery ?? 0) | (isRedelivery ? 1U << 31 : 0) );
        writer.MoveWrite( ( uint )channelId );
        writer.MoveWrite( ( uint )senderId );
        writer.MoveWrite( ( ulong )(pushedAt ?? new Timestamp( DateTime.UtcNow )).UnixEpochTicks );
        data.AsSpan().CopyTo( writer.GetSpan( data.Length ) );
        Send( buffer );
    }

    internal void SendObjectNameNotification(MessageBrokerSystemNotificationType type, int objectId, string name, uint? payload = null)
    {
        var encodedName = TextEncoding.Prepare( name ).GetValueOrThrow();
        var buffer = new byte[Protocol.PacketHeader.Length
            + Protocol.SystemNotificationHeader.Length
            + Protocol.ObjectNameNotificationHeader.Length
            + encodedName.ByteCount];

        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.SystemNotification );
        writer.MoveWrite(
            payload
            ?? ( uint )(Protocol.SystemNotificationHeader.Length + Protocol.ObjectNameNotificationHeader.Length + encodedName.ByteCount) );

        writer.MoveWrite( ( byte )type );
        writer.MoveWrite( ( uint )objectId );
        encodedName.Encode( writer.GetSpan( encodedName.ByteCount ) ).ThrowIfError();
        Send( buffer );
    }

    internal void SendHeader(MessageBrokerClientEndpoint endpoint, uint payload, bool reverseEndianness = false)
    {
        var buffer = new byte[Protocol.PacketHeader.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )endpoint );
        writer.Write( reverseEndianness ? BinaryPrimitives.ReverseEndianness( payload ) : payload );
        Send( buffer );
    }

    internal void Send(byte[] data)
    {
        lock ( _listener )
        {
            Assume.IsNotNull( _client );
            Thread.Sleep( 15 );
            _client.GetStream().Write( data );
        }
    }

    internal async Task<Protocol.HandshakeRequest> EstablishHandshake(
        MessageBrokerClient client,
        int? id = null,
        Duration? messageTimeout = null,
        Duration? pingInterval = null)
    {
        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            () =>
            {
                WaitForClient();
                Read( handshakeRequest );
                SendHandshakeAccepted( id ?? 1, messageTimeout ?? Duration.FromSeconds( 1 ), pingInterval ?? Duration.FromSeconds( 10 ) );
                ReadConfirmHandshakeResponse();
            } );

        await client.StartAsync();
        await serverTask;
        return handshakeRequest;
    }

    internal Task GetTask(Action<ServerMock> action)
    {
        return Task.Factory.StartNew( () => action( this ) );
    }

    [Pure]
    internal byte[][] GetAllReceived()
    {
        lock ( _listener )
            return _received.ToArray();
    }

    private static byte[] AssertEndpoint(byte[] data, MessageBrokerServerEndpoint endpoint)
    {
        (( MessageBrokerServerEndpoint )data[0]).TestEquals( endpoint ).Go();
        return data;
    }
}
