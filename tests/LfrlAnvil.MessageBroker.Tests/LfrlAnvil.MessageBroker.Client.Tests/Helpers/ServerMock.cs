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
            _client.ReceiveTimeout = ChronoConstants.MillisecondsPerSecond;
            _client.SendTimeout = ChronoConstants.MillisecondsPerSecond;
            _client.NoDelay = true;
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
                _client.Client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );
        }
    }

    internal byte[] ReadConfirmHandshakeResponse()
    {
        return AssertEndpoint( Read( Protocol.PacketHeader.Length ), MessageBrokerServerEndpoint.ConfirmHandshakeResponse );
    }

    internal byte[] ReadPingRequest()
    {
        return AssertEndpoint( Read( Protocol.PacketHeader.Length ), MessageBrokerServerEndpoint.PingRequest );
    }

    internal byte[] ReadUnbindRequest()
    {
        return AssertEndpoint( Read( Protocol.UnbindRequest.Length ), MessageBrokerServerEndpoint.UnbindRequest );
    }

    internal byte[] ReadUnsubscribeRequest()
    {
        return AssertEndpoint( Read( Protocol.UnsubscribeRequest.Length ), MessageBrokerServerEndpoint.UnsubscribeRequest );
    }

    internal byte[] ReadMessageRequest(int length)
    {
        return AssertEndpoint( Read( Protocol.MessageRequestHeader.Length + length ), MessageBrokerServerEndpoint.MessageRequest );
    }

    internal byte[] Read(Protocol.HandshakeRequest request)
    {
        return AssertEndpoint( Read( request.Length ), MessageBrokerServerEndpoint.HandshakeRequest );
    }

    internal byte[] Read(Protocol.BindRequest request)
    {
        return AssertEndpoint( Read( request.Length ), MessageBrokerServerEndpoint.BindRequest );
    }

    internal byte[] Read(Protocol.SubscribeRequest request)
    {
        return AssertEndpoint( Read( request.Length ), MessageBrokerServerEndpoint.SubscribeRequest );
    }

    internal byte[] Read(Protocol.MessageRequestHeader request)
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + ( int )request.Header.Payload ),
            MessageBrokerServerEndpoint.MessageRequest );
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

    internal void SendPing(uint? endiannessPayload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.PingResponse );
        writer.Write( endiannessPayload ?? Protocol.Endianness.VerificationPayload );
        Send( buffer );
    }

    internal void SendBoundResponse(bool channelCreated, bool queueCreated, int channelId, int queueId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.BoundResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.BoundResponse );
        writer.MoveWrite( payload ?? Protocol.BoundResponse.Length );
        writer.MoveWrite( ( byte )((channelCreated ? 1 : 0) | (queueCreated ? 2 : 0)) );
        writer.MoveWrite( ( uint )channelId );
        writer.Write( ( uint )queueId );
        Send( buffer );
    }

    internal void SendBindFailureResponse(bool clientAlreadyLinkedToChannel, bool cancelled, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.BindFailureResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.BindFailureResponse );
        writer.MoveWrite( payload ?? Protocol.BindFailureResponse.Length );
        writer.Write( ( byte )((clientAlreadyLinkedToChannel ? 1 : 0) | (cancelled ? 2 : 0)) );
        Send( buffer );
    }

    internal void SendUnboundResponse(bool channelRemoved, bool queueRemoved, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnboundResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.UnboundResponse );
        writer.MoveWrite( payload ?? Protocol.UnboundResponse.Length );
        writer.Write( ( byte )((channelRemoved ? 1 : 0) | (queueRemoved ? 2 : 0)) );
        Send( buffer );
    }

    internal void SendUnbindFailureResponse(bool clientNotLinked, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnbindFailureResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.UnbindFailureResponse );
        writer.MoveWrite( payload ?? Protocol.UnbindFailureResponse.Length );
        writer.Write( ( byte )(clientNotLinked ? 1 : 0) );
        Send( buffer );
    }

    internal void SendSubscribedResponse(bool channelCreated, int channelId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.SubscribedResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.SubscribedResponse );
        writer.MoveWrite( payload ?? Protocol.SubscribedResponse.Length );
        writer.MoveWrite( ( byte )(channelCreated ? 1 : 0) );
        writer.Write( ( uint )channelId );
        Send( buffer );
    }

    internal void SendSubscribeFailureResponse(
        bool channelDoesNotExist,
        bool clientAlreadySubscribedToChannel,
        bool cancelled,
        uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.SubscribeFailureResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.SubscribeFailureResponse );
        writer.MoveWrite( payload ?? Protocol.SubscribeFailureResponse.Length );
        writer.Write( ( byte )((channelDoesNotExist ? 1 : 0) | (clientAlreadySubscribedToChannel ? 2 : 0) | (cancelled ? 4 : 0)) );

        Send( buffer );
    }

    internal void SendUnsubscribedResponse(bool channelRemoved, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnsubscribedResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.UnsubscribedResponse );
        writer.MoveWrite( payload ?? Protocol.UnsubscribedResponse.Length );
        writer.Write( ( byte )(channelRemoved ? 1 : 0) );
        Send( buffer );
    }

    internal void SendUnsubscribeFailureResponse(bool clientNotSubscribed, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnsubscribeFailureResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.UnsubscribeFailureResponse );
        writer.MoveWrite( payload ?? Protocol.UnsubscribeFailureResponse.Length );
        writer.Write( ( byte )(clientNotSubscribed ? 1 : 0) );
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
