using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
        _client.SendTimeout = ChronoConstants.MillisecondsPerSecond * 15;
        _client.ReceiveTimeout = ChronoConstants.MillisecondsPerSecond * 15;
    }

    public void Dispose()
    {
        lock ( _client )
        {
            _client.TryDispose();
        }
    }

    internal void Connect(EndPoint endPoint)
    {
        lock ( _client )
        {
            _client.Connect( ( IPEndPoint )endPoint );
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

    internal byte[] ReadPong()
    {
        return AssertEndpoint( Read( Protocol.PacketHeader.Length ), MessageBrokerClientEndpoint.Pong );
    }

    internal byte[] ReadPublisherBoundResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.PublisherBoundResponse.Payload ),
            MessageBrokerClientEndpoint.PublisherBoundResponse );
    }

    internal byte[] ReadBindPublisherFailureResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.BindPublisherFailureResponse.Payload ),
            MessageBrokerClientEndpoint.BindPublisherFailureResponse );
    }

    internal byte[] ReadPublisherUnboundResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.PublisherUnboundResponse.Payload ),
            MessageBrokerClientEndpoint.PublisherUnboundResponse );
    }

    internal byte[] ReadUnbindPublisherFailureResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.UnbindPublisherFailureResponse.Payload ),
            MessageBrokerClientEndpoint.UnbindPublisherFailureResponse );
    }

    internal byte[] ReadListenerBoundResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.ListenerBoundResponse.Payload ),
            MessageBrokerClientEndpoint.ListenerBoundResponse );
    }

    internal byte[] ReadBindListenerFailureResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.BindListenerFailureResponse.Payload ),
            MessageBrokerClientEndpoint.BindListenerFailureResponse );
    }

    internal byte[] ReadListenerUnboundResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.ListenerUnboundResponse.Payload ),
            MessageBrokerClientEndpoint.ListenerUnboundResponse );
    }

    internal byte[] ReadUnbindListenerFailureResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.UnbindListenerFailureResponse.Payload ),
            MessageBrokerClientEndpoint.UnbindListenerFailureResponse );
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

    internal byte[] ReadMessageNotification(int length)
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload + length ),
            MessageBrokerClientEndpoint.MessageNotification );
    }

    internal byte[] ReadObjectNameSystemNotification(Protocol.ObjectNameNotification notification)
    {
        return AssertEndpoint( Read( notification.Length ), MessageBrokerClientEndpoint.SystemNotification );
    }

    internal byte[] ReadDeadLetterQueryResponse()
    {
        return AssertEndpoint(
            Read( Protocol.PacketHeader.Length + Protocol.DeadLetterQueryResponse.Payload ),
            MessageBrokerClientEndpoint.DeadLetterQueryResponse );
    }

    internal (byte[] Data, int Index) ReadAny(params (MessageBrokerClientEndpoint Endpoint, int Payload)[] expectations)
    {
        var header = Read( Protocol.PacketHeader.Length );
        var endpoint = ( MessageBrokerClientEndpoint )header[0];
        Assertion.Any( expectations.Select( x => endpoint.TestEquals( x.Endpoint ) ) ).Go();
        var entry = expectations.Select( static (e, i) => (e.Endpoint, e.Payload, Index: i) ).First( x => endpoint == x.Endpoint );
        return (header.Concat( Read( entry.Payload ) ).ToArray(), entry.Index);
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

    internal void SendHandshake(
        string name,
        Duration messageTimeout,
        Duration pingInterval,
        bool synchronizeExternalObjectNames = false,
        uint? payload = null)
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
        writer.MoveWrite( ( byte )((BitConverter.IsLittleEndian ? 2 : 0) | (synchronizeExternalObjectNames ? 4 : 0)) );
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
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.Ping );
        writer.Write( payload ?? Protocol.Endianness.VerificationPayload );
        Send( buffer );
    }

    internal void SendBindPublisherRequest(
        string channelName,
        string? streamName = null,
        short? channelNameLength = null,
        uint? payload = null)
    {
        var preparedChannelName = EncodeableText.Create( TextEncoding.Instance, channelName ).GetValueOrThrow();
        var preparedStreamName = EncodeableText.Create( TextEncoding.Instance, streamName ?? string.Empty ).GetValueOrThrow();
        var buffer = new byte[Protocol.PacketHeader.Length
            + Protocol.BindPublisherRequestHeader.Length
            + preparedChannelName.ByteCount
            + preparedStreamName.ByteCount];

        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.BindPublisherRequest );
        writer.MoveWrite(
            payload
            ?? ( uint )(Protocol.BindPublisherRequestHeader.Length + preparedChannelName.ByteCount + preparedStreamName.ByteCount) );

        writer.MoveWrite( 0 );
        writer.MoveWrite( ( ushort )(channelNameLength ?? preparedChannelName.ByteCount) );
        preparedChannelName.Encode( writer.GetSpan( preparedChannelName.ByteCount ) ).ThrowIfError();
        writer.Move( preparedChannelName.ByteCount );
        preparedStreamName.Encode( writer.GetSpan( preparedStreamName.ByteCount ) ).ThrowIfError();

        Send( buffer );
    }

    internal void SendUnbindPublisherRequest(int channelId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnbindPublisherRequest.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.UnbindPublisherRequest );
        writer.MoveWrite( payload ?? Protocol.UnbindPublisherRequest.Length );
        writer.Write( ( uint )channelId );
        Send( buffer );
    }

    internal void SendBindListenerRequest(
        string channelName,
        bool createChannelIfNotExists,
        short? prefetchHint = null,
        int? maxRetries = null,
        Duration? retryDelay = null,
        int? maxRedeliveries = null,
        Duration? minAckTimeout = null,
        int? deadLetterCapacityHint = null,
        Duration? minDeadLetterRetention = null,
        string? queueName = null,
        short? channelNameLength = null,
        uint? payload = null)
    {
        var preparedChannelName = EncodeableText.Create( TextEncoding.Instance, channelName ).GetValueOrThrow();
        var preparedQueueName = EncodeableText.Create( TextEncoding.Instance, queueName ?? string.Empty ).GetValueOrThrow();
        var buffer = new byte[Protocol.PacketHeader.Length
            + Protocol.BindListenerRequestHeader.Length
            + preparedChannelName.ByteCount
            + preparedQueueName.ByteCount];

        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.BindListenerRequest );
        writer.MoveWrite(
            payload ?? ( uint )(Protocol.BindListenerRequestHeader.Length + preparedChannelName.ByteCount + preparedQueueName.ByteCount) );

        writer.MoveWrite( ( byte )(createChannelIfNotExists ? 1 : 0) );
        writer.MoveWrite( ( ushort )(prefetchHint ?? 1) );
        writer.MoveWrite( ( uint )(maxRetries ?? 0) );
        writer.MoveWrite( ( uint )(retryDelay?.FullMilliseconds ?? 0) );
        writer.MoveWrite( ( uint )(maxRedeliveries ?? 0) );
        writer.MoveWrite( ( uint )(minAckTimeout?.FullMilliseconds ?? 0) );
        writer.MoveWrite( ( uint )(deadLetterCapacityHint ?? 0) );
        writer.MoveWrite( ( ulong )(minDeadLetterRetention?.FullMilliseconds ?? 0) );
        writer.MoveWrite( ( ushort )(channelNameLength ?? preparedChannelName.ByteCount) );
        preparedChannelName.Encode( writer.GetSpan( preparedChannelName.ByteCount ) ).ThrowIfError();
        writer.Move( preparedChannelName.ByteCount );
        preparedQueueName.Encode( writer.GetSpan( preparedQueueName.ByteCount ) ).ThrowIfError();

        Send( buffer );
    }

    internal void SendUnbindListenerRequest(int channelId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnbindListenerRequest.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.UnbindListenerRequest );
        writer.MoveWrite( payload ?? Protocol.UnbindListenerRequest.Length );
        writer.Write( ( uint )channelId );
        Send( buffer );
    }

    internal void SendDeadLetterQuery(int queueId, int readCount, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.DeadLetterQuery.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.DeadLetterQuery );
        writer.MoveWrite( payload ?? Protocol.DeadLetterQuery.Length );
        writer.MoveWrite( ( uint )queueId );
        writer.Write( ( uint )readCount );
        Send( buffer );
    }

    internal void SendPushMessageRouting(IReadOnlyCollection<Routing> routes, short? targetCount = null, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.PushMessageRoutingHeader.Length + routes.Sum( r => r.Length )];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.PushMessageRouting );
        writer.MoveWrite( payload ?? ( uint )(Protocol.PushMessageRoutingHeader.Length + routes.Sum( r => r.Length )) );
        writer.MoveWrite( ( ushort )(targetCount ?? routes.Count) );
        foreach ( var route in routes )
        {
            if ( route.IsName )
            {
                writer.MoveWrite( ( ushort )((route.Name.ByteCount << 1) | 1) );
                route.Name.Encode( writer.GetSpan( route.Name.ByteCount ) ).ThrowIfError();
                writer.Move( route.Name.ByteCount );
            }
            else
            {
                writer.MoveWrite( 0 );
                writer.MoveWrite( ( uint )route.Id );
            }
        }

        Send( buffer );
    }

    internal void SendPushMessage(int channelId, byte[] data, bool confirm = true, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.PushMessageHeader.Length + data.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.PushMessage );
        writer.MoveWrite( payload ?? ( uint )(Protocol.PushMessageHeader.Length + data.Length) );
        writer.MoveWrite( ( byte )(confirm ? 1 : 0) );
        writer.MoveWrite( ( uint )channelId );
        data.AsSpan().CopyTo( writer.GetSpan( data.Length ) );
        Send( buffer );
    }

    internal void SendMessageNotificationAck(
        int queueId,
        int ackId,
        int streamId,
        ulong messageId,
        int retryAttempt = 0,
        int redeliveryAttempt = 0,
        uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.MessageNotificationAck.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.MessageNotificationAck );
        writer.MoveWrite( payload ?? Protocol.MessageNotificationAck.Length );
        writer.MoveWrite( ( uint )queueId );
        writer.MoveWrite( ( uint )ackId );
        writer.MoveWrite( ( uint )streamId );
        writer.MoveWrite( messageId );
        writer.MoveWrite( ( uint )retryAttempt );
        writer.Write( ( uint )redeliveryAttempt );
        Send( buffer );
    }

    internal void SendMessageNotificationNegativeAck(
        int queueId,
        int ackId,
        int streamId,
        ulong messageId,
        int retryAttempt = 0,
        int redeliveryAttempt = 0,
        bool noRetry = false,
        bool noDeadLetter = false,
        bool hasExplicitDelay = false,
        Duration? explicitDelay = null,
        uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.MessageNotificationNegativeAck.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.MessageNotificationNack );
        writer.MoveWrite( payload ?? Protocol.MessageNotificationNegativeAck.Length );
        writer.MoveWrite( ( byte )((noRetry ? 1 : 0) | (noDeadLetter ? 2 : 0) | (hasExplicitDelay ? 4 : 0)) );
        writer.MoveWrite( ( uint )queueId );
        writer.MoveWrite( ( uint )ackId );
        writer.MoveWrite( ( uint )streamId );
        writer.MoveWrite( messageId );
        writer.MoveWrite( ( uint )retryAttempt );
        writer.MoveWrite( ( uint )redeliveryAttempt );
        writer.Write( ( uint )(explicitDelay?.FullMilliseconds ?? 0) );
        Send( buffer );
    }

    internal void SendHeader(MessageBrokerServerEndpoint endpoint, uint payload, bool reverseEndianness = false)
    {
        var buffer = new byte[Protocol.PacketHeader.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )endpoint );
        writer.Write( reverseEndianness ? BinaryPrimitives.ReverseEndianness( payload ) : payload );
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
        Duration? pingInterval = null,
        bool synchronizeExternalObjectNames = false)
    {
        return Task.Factory.StartNew(
            () =>
            {
                Connect( server.LocalEndPoint );
                SendHandshake(
                    name ?? "test",
                    messageTimeout ?? Duration.FromSeconds( 1 ),
                    pingInterval ?? Duration.FromSeconds( 10 ),
                    synchronizeExternalObjectNames );

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
