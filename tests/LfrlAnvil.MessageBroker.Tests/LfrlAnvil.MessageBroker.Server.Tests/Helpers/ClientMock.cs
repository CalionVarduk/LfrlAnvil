using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

internal sealed class ClientMock : IDisposable
{
    private readonly TcpClient _client = new TcpClient();
    private readonly List<byte[]> _received = new List<byte[]>();

    internal ClientMock()
    {
        _client.NoDelay = true;
        _client.SendTimeout = ChronoConstants.MillisecondsPerSecond;
        _client.ReceiveTimeout = ChronoConstants.MillisecondsPerSecond;
    }

    public void Dispose()
    {
        lock ( _client )
        {
            _client.TryDispose();
        }
    }

    internal void Connect(IPEndPoint endPoint)
    {
        lock ( _client )
        {
            _client.Connect( endPoint );
        }
    }

    internal byte[] ReadHandshakeAcceptedResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload ),
            MessageBrokerClientEndpoint.HandshakeAcceptedResponse );
    }

    internal byte[] ReadHandshakeRejectedResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload ),
            MessageBrokerClientEndpoint.HandshakeRejectedResponse );
    }

    internal byte[] ReadPingResponse()
    {
        return AssertEndpoint( Read( Protocol.PacketHeader.Length ), MessageBrokerClientEndpoint.PingResponse );
    }

    internal byte[] ReadBoundResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.BoundResponse.Payload ),
            MessageBrokerClientEndpoint.BoundResponse );
    }

    internal byte[] ReadBindFailureResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.BindFailureResponse.Payload ),
            MessageBrokerClientEndpoint.BindFailureResponse );
    }

    internal byte[] ReadUnboundResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.UnboundResponse.Payload ),
            MessageBrokerClientEndpoint.UnboundResponse );
    }

    internal byte[] ReadUnbindFailureResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.UnbindFailureResponse.Payload ),
            MessageBrokerClientEndpoint.UnbindFailureResponse );
    }

    internal byte[] ReadSubscribedResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.SubscribedResponse.Payload ),
            MessageBrokerClientEndpoint.SubscribedResponse );
    }

    internal byte[] ReadSubscribeFailureResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.SubscribeFailureResponse.Payload ),
            MessageBrokerClientEndpoint.SubscribeFailureResponse );
    }

    internal byte[] ReadUnsubscribedResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.UnsubscribedResponse.Payload ),
            MessageBrokerClientEndpoint.UnsubscribedResponse );
    }

    internal byte[] ReadUnsubscribeFailureResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.UnsubscribeFailureResponse.Payload ),
            MessageBrokerClientEndpoint.UnsubscribeFailureResponse );
    }

    internal byte[] ReadMessageAcceptedResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.MessageAcceptedResponse.Payload ),
            MessageBrokerClientEndpoint.MessageAcceptedResponse );
    }

    internal byte[] ReadMessageRejectedResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.MessageRejectedResponse.Payload ),
            MessageBrokerClientEndpoint.MessageRejectedResponse );
    }

    internal byte[] Read(int length)
    {
        lock ( _client )
        {
            var buffer = new byte[length];
            _client.GetStream().ReadExactly( buffer );
            _received.Add( buffer );
            return buffer;
        }
    }

    internal void SendHandshake(string name, Duration messageTimeout, Duration pingInterval, uint? payload = null)
    {
        var preparedName = EncodeableText.Create( TextEncoding.Instance, name ).GetValueOrThrow();
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.HandshakeRequestHeader.Length + preparedName.ByteCount];

        var payloadToSend = payload ?? Protocol.HandshakeRequestHeader.Length + ( uint )preparedName.ByteCount;
        var messageTimeoutMs = ( uint )messageTimeout.FullMilliseconds;
        var pingIntervalMs = ( uint )pingInterval.FullMilliseconds;
        if ( BitConverter.IsLittleEndian )
        {
            payloadToSend = BinaryPrimitives.ReverseEndianness( payloadToSend );
            messageTimeoutMs = BinaryPrimitives.ReverseEndianness( messageTimeoutMs );
            pingIntervalMs = BinaryPrimitives.ReverseEndianness( pingIntervalMs );
        }

        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.HandshakeRequest );
        writer.MoveWrite( payloadToSend );
        writer.MoveWrite( ( byte )(BitConverter.IsLittleEndian ? 2 : 0) );
        writer.MoveWrite( messageTimeoutMs );
        writer.MoveWrite( pingIntervalMs );
        preparedName.Encode( writer.GetSpan( preparedName.ByteCount ) ).ThrowIfError();
        Send( buffer );
    }

    internal void SendConfirmHandshakeResponse(uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.ConfirmHandshakeResponse );
        writer.Write( payload ?? Protocol.Endianness.VerificationPayload );
        Send( buffer );
    }

    internal void SendPing(uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.PingRequest );
        writer.Write( payload ?? Protocol.Endianness.VerificationPayload );
        Send( buffer );
    }

    internal void SendBindRequest(string channelName, string? queueName = null, int? channelNameLength = null, uint? payload = null)
    {
        var preparedChannelName = EncodeableText.Create( TextEncoding.Instance, channelName ).GetValueOrThrow();
        var preparedQueueName = EncodeableText.Create( TextEncoding.Instance, queueName ?? string.Empty ).GetValueOrThrow();
        var buffer = new byte[Protocol.PacketHeader.Length
            + Protocol.BindRequestHeader.Length
            + preparedChannelName.ByteCount
            + preparedQueueName.ByteCount];

        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.BindRequest );
        writer.MoveWrite(
            payload ?? ( uint )(Protocol.BindRequestHeader.Length + preparedChannelName.ByteCount + preparedQueueName.ByteCount) );

        writer.MoveWrite( 0 );
        writer.MoveWrite( unchecked( ( uint )(channelNameLength ?? preparedChannelName.ByteCount) ) );
        preparedChannelName.Encode( writer.GetSpan( preparedChannelName.ByteCount ) ).ThrowIfError();
        writer.Move( preparedChannelName.ByteCount );
        preparedQueueName.Encode( writer.GetSpan( preparedQueueName.ByteCount ) ).ThrowIfError();

        Send( buffer );
    }

    internal void SendUnbindRequest(int channelId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnbindRequest.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.UnbindRequest );
        writer.MoveWrite( payload ?? Protocol.UnbindRequest.Length );
        writer.Write( ( uint )channelId );
        Send( buffer );
    }

    internal void SendSubscribeRequest(string channelName, bool createChannelIfNotExists, uint? payload = null)
    {
        var preparedName = EncodeableText.Create( TextEncoding.Instance, channelName ).GetValueOrThrow();
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.SubscribeRequestHeader.Length + preparedName.ByteCount];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.SubscribeRequest );
        writer.MoveWrite( payload ?? ( uint )(Protocol.SubscribeRequestHeader.Length + preparedName.ByteCount) );
        writer.MoveWrite( ( byte )(createChannelIfNotExists ? 1 : 0) );
        preparedName.Encode( writer.GetSpan( preparedName.ByteCount ) ).ThrowIfError();
        Send( buffer );
    }

    internal void SendUnsubscribeRequest(int channelId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnsubscribeRequest.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.UnsubscribeRequest );
        writer.MoveWrite( payload ?? Protocol.UnsubscribeRequest.Length );
        writer.Write( ( uint )channelId );
        Send( buffer );
    }

    internal void SendMessageRequest(int channelId, byte[] data, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.MessageRequestHeader.Length + data.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.MessageRequest );
        writer.MoveWrite( payload ?? ( uint )(Protocol.MessageRequestHeader.Length + data.Length) );
        writer.MoveWrite( ( uint )channelId );
        data.AsSpan().CopyTo( writer.GetSpan( data.Length ) );
        Send( buffer );
    }

    internal void Send(byte[] data)
    {
        lock ( _client )
        {
            _client.GetStream().Write( data );
        }
    }

    internal Task EstablishHandshake(
        MessageBrokerServer server,
        string? name = null,
        Duration? messageTimeout = null,
        Duration? pingInterval = null)
    {
        return Task.Factory.StartNew(
            () =>
            {
                Connect( server.LocalEndPoint );
                SendHandshake( name ?? "test", messageTimeout ?? Duration.FromSeconds( 1 ), pingInterval ?? Duration.FromSeconds( 10 ) );
                ReadHandshakeAcceptedResponse();
                SendConfirmHandshakeResponse();
            } );
    }

    internal Task GetTask(Action<ClientMock> action)
    {
        return Task.Factory.StartNew( () => action( this ) );
    }

    [Pure]
    internal byte[][] GetAllReceived()
    {
        lock ( _client )
            return _received.ToArray();
    }

    private static byte[] AssertEndpoint(byte[] data, MessageBrokerClientEndpoint endpoint)
    {
        (( MessageBrokerClientEndpoint )data[0]).TestEquals( endpoint ).Go();
        return data;
    }
}
