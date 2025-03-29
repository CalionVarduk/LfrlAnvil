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
        return Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
    }

    internal byte[] ReadHandshakeRejectedResponse()
    {
        return Read( Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload );
    }

    internal byte[] ReadPingResponse()
    {
        return Read( Protocol.PacketHeader.Length );
    }

    internal byte[] ReadBoundResponse()
    {
        return Read( Protocol.PacketHeader.Length + Protocol.BoundResponse.Payload );
    }

    internal byte[] ReadBindFailureResponse()
    {
        return Read( Protocol.PacketHeader.Length + Protocol.BindFailureResponse.Payload );
    }

    internal byte[] ReadUnboundResponse()
    {
        return Read( Protocol.PacketHeader.Length + Protocol.UnboundResponse.Payload );
    }

    internal byte[] ReadUnbindFailureResponse()
    {
        return Read( Protocol.PacketHeader.Length + Protocol.UnbindFailureResponse.Payload );
    }

    internal byte[] ReadSubscribedResponse()
    {
        return Read( Protocol.PacketHeader.Length + Protocol.SubscribedResponse.Payload );
    }

    internal byte[] ReadSubscribeFailureResponse()
    {
        return Read( Protocol.PacketHeader.Length + Protocol.SubscribeFailureResponse.Payload );
    }

    internal byte[] ReadUnsubscribedResponse()
    {
        return Read( Protocol.PacketHeader.Length + Protocol.UnsubscribedResponse.Payload );
    }

    internal byte[] ReadUnsubscribeFailureResponse()
    {
        return Read( Protocol.PacketHeader.Length + Protocol.UnsubscribeFailureResponse.Payload );
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
}
