using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;
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
            _client.TryDispose();
    }

    [Pure]
    internal static byte[] PreparePing(uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.Ping );
        writer.Write( payload ?? Protocol.Endianness.VerificationPayload );
        return buffer;
    }

    [Pure]
    internal static byte[] PrepareBindPublisherRequest(
        string channelName,
        bool isEphemeral = true,
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

        writer.MoveWrite( ( byte )(isEphemeral ? 0 : 1) );
        writer.MoveWrite( ( ushort )(channelNameLength ?? preparedChannelName.ByteCount) );
        preparedChannelName.Encode( writer.GetSpan( preparedChannelName.ByteCount ) ).ThrowIfError();
        writer.Move( preparedChannelName.ByteCount );
        preparedStreamName.Encode( writer.GetSpan( preparedStreamName.ByteCount ) ).ThrowIfError();

        return buffer;
    }

    [Pure]
    internal static byte[] PrepareUnbindPublisherRequest(int channelId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnbindPublisherRequest.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.UnbindPublisherRequest );
        writer.MoveWrite( payload ?? Protocol.UnbindPublisherRequest.Length );
        writer.Write( ( uint )channelId );
        return buffer;
    }

    [Pure]
    internal static byte[] PrepareUnbindPublisherByNameRequest(string channelName, uint? payload = null)
    {
        var encodedName = TextEncoding.Prepare( channelName ).GetValueOrThrow();
        var buffer = new byte[Protocol.PacketHeader.Length + encodedName.ByteCount];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.UnbindPublisherByNameRequest );
        writer.MoveWrite( payload ?? ( uint )encodedName.ByteCount );
        encodedName.Encode( writer.GetSpan( encodedName.ByteCount ) ).ThrowIfError();
        return buffer;
    }

    [Pure]
    internal static byte[] PrepareBindListenerRequest(
        string channelName,
        bool createChannelIfNotExists,
        bool isEphemeral = true,
        short? prefetchHint = null,
        int? maxRetries = null,
        Duration? retryDelay = null,
        int? maxRedeliveries = null,
        Duration? minAckTimeout = null,
        int? deadLetterCapacityHint = null,
        Duration? minDeadLetterRetention = null,
        string? queueName = null,
        string? filterExpression = null,
        short? channelNameLength = null,
        short? queueNameLength = null,
        uint? payload = null)
    {
        var preparedChannelName = EncodeableText.Create( TextEncoding.Instance, channelName ).GetValueOrThrow();
        var preparedQueueName = EncodeableText.Create( TextEncoding.Instance, queueName ?? string.Empty ).GetValueOrThrow();
        var preparedFilterExpression = EncodeableText.Create( TextEncoding.Instance, filterExpression ?? string.Empty ).GetValueOrThrow();
        var buffer = new byte[Protocol.PacketHeader.Length
            + Protocol.BindListenerRequestHeader.Length
            + preparedChannelName.ByteCount
            + preparedQueueName.ByteCount
            + preparedFilterExpression.ByteCount];

        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.BindListenerRequest );
        writer.MoveWrite(
            payload
            ?? ( uint )(Protocol.BindListenerRequestHeader.Length
                + preparedChannelName.ByteCount
                + preparedQueueName.ByteCount
                + preparedFilterExpression.ByteCount) );

        writer.MoveWrite( ( byte )((isEphemeral ? 0 : 1) | (createChannelIfNotExists ? 2 : 0)) );
        writer.MoveWrite( ( ushort )(prefetchHint ?? 1) );
        writer.MoveWrite( ( uint )(maxRetries ?? 0) );
        writer.MoveWrite( ( uint )(retryDelay?.FullMilliseconds ?? 0) );
        writer.MoveWrite( ( uint )(maxRedeliveries ?? 0) );
        writer.MoveWrite( ( uint )(minAckTimeout?.FullMilliseconds ?? 0) );
        writer.MoveWrite( ( uint )(deadLetterCapacityHint ?? 0) );
        writer.MoveWrite( ( ulong )(minDeadLetterRetention?.FullMilliseconds ?? 0) );
        writer.MoveWrite( ( ushort )(channelNameLength ?? preparedChannelName.ByteCount) );
        writer.MoveWrite( ( ushort )(queueNameLength ?? preparedQueueName.ByteCount) );
        preparedChannelName.Encode( writer.GetSpan( preparedChannelName.ByteCount ) ).ThrowIfError();
        writer.Move( preparedChannelName.ByteCount );
        preparedQueueName.Encode( writer.GetSpan( preparedQueueName.ByteCount ) ).ThrowIfError();
        writer.Move( preparedQueueName.ByteCount );
        preparedFilterExpression.Encode( writer.GetSpan( preparedFilterExpression.ByteCount ) ).ThrowIfError();

        return buffer;
    }

    [Pure]
    internal static byte[] PrepareUnbindListenerRequest(int channelId, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.UnbindListenerRequest.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.UnbindListenerRequest );
        writer.MoveWrite( payload ?? Protocol.UnbindListenerRequest.Length );
        writer.Write( ( uint )channelId );
        return buffer;
    }

    [Pure]
    internal static byte[] PrepareDeadLetterQuery(int queueId, int readCount, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.DeadLetterQuery.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.DeadLetterQuery );
        writer.MoveWrite( payload ?? Protocol.DeadLetterQuery.Length );
        writer.MoveWrite( ( uint )queueId );
        writer.Write( ( uint )readCount );
        return buffer;
    }

    [Pure]
    internal static byte[] PreparePushMessageRouting(IReadOnlyCollection<Routing> routes, short? targetCount = null, uint? payload = null)
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

        return buffer;
    }

    [Pure]
    internal static byte[] PreparePushMessage(int channelId, byte[] data, bool confirm = true, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.PushMessageHeader.Length + data.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.PushMessage );
        writer.MoveWrite( payload ?? ( uint )(Protocol.PushMessageHeader.Length + data.Length) );
        writer.MoveWrite( ( byte )(confirm ? 1 : 0) );
        writer.MoveWrite( ( uint )channelId );
        data.AsSpan().CopyTo( writer.GetSpan( data.Length ) );
        return buffer;
    }

    [Pure]
    internal static byte[] PrepareMessageNotificationAck(
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
        return buffer;
    }

    [Pure]
    internal static byte[] PrepareMessageNotificationNegativeAck(
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
        return buffer;
    }

    [Pure]
    internal static byte[] PrepareBatch(byte[][] packets, short? packetCount = null, uint? payload = null)
    {
        var contentLength = Protocol.BatchHeader.Length + packets.Sum( p => p.Length );
        var buffer = new byte[Protocol.PacketHeader.Length + contentLength];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.Batch );
        writer.MoveWrite( payload ?? ( uint )contentLength );
        writer.MoveWrite( ( ushort )(packetCount ?? packets.Length) );
        foreach ( var p in packets )
        {
            p.AsSpan().CopyTo( writer.GetSpan( p.Length ) );
            writer.Move( p.Length );
        }

        return buffer;
    }

    [Pure]
    internal static byte[] PrepareHeader(MessageBrokerServerEndpoint endpoint, uint payload, bool reverseEndianness = false)
    {
        var buffer = new byte[Protocol.PacketHeader.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )endpoint );
        writer.Write( reverseEndianness ? BinaryPrimitives.ReverseEndianness( payload ) : payload );
        return buffer;
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
        var data = Read( notification.Length );
        AssertEndpoint( data, MessageBrokerClientEndpoint.SystemNotification );
        (( MessageBrokerSystemNotificationType )data[5]).TestEquals( notification.Type ).Go();
        return data;
    }

    internal byte[] ReadPublisherDeletedSystemNotification(string channelName)
    {
        var encodedName = TextEncoding.Prepare( channelName ).GetValueOrThrow();
        var data = Read( Protocol.PacketHeader.Length + encodedName.ByteCount );
        AssertEndpoint( data, MessageBrokerClientEndpoint.SystemNotification );
        (( MessageBrokerSystemNotificationType )data[5]).TestEquals( MessageBrokerSystemNotificationType.PublisherDeleted ).Go();
        return data;
    }

    internal byte[] ReadListenerDeletedSystemNotification(string channelName)
    {
        var encodedName = TextEncoding.Prepare( channelName ).GetValueOrThrow();
        var data = Read( Protocol.PacketHeader.Length + encodedName.ByteCount );
        AssertEndpoint( data, MessageBrokerClientEndpoint.SystemNotification );
        (( MessageBrokerSystemNotificationType )data[5]).TestEquals( MessageBrokerSystemNotificationType.ListenerDeleted ).Go();
        return data;
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

    internal byte[] ReadBatch((MessageBrokerClientEndpoint Endpoint, int Length)[] packets)
    {
        var data = Read( Protocol.PacketHeader.Length + Protocol.BatchHeader.Length + packets.Sum( p => p.Length ) );
        AssertEndpoint( data, MessageBrokerClientEndpoint.Batch );

        var reader = new BinaryContractReader( data );
        reader.Move( Protocol.PacketHeader.Length );
        var packetCount = ( int )reader.ReadInt16();
        packets.Length.TestEquals( packetCount ).Go();

        var index = Protocol.PacketHeader.Length + Protocol.BatchHeader.Length;
        foreach ( var (endpoint, length) in packets )
        {
            (( MessageBrokerClientEndpoint )data[index]).TestEquals( endpoint ).Go();
            index += length;
        }

        return data;
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
        short? maxBatchPacketCount = null,
        MemorySize? maxNetworkBatchPacketLength = null,
        bool synchronizeExternalObjectNames = false,
        bool clearBuffers = false,
        bool isEphemeral = true,
        uint? payload = null)
    {
        var preparedName = EncodeableText.Create( TextEncoding.Instance, name ).GetValueOrThrow();
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.HandshakeRequestHeader.Length + preparedName.ByteCount];

        var payloadToSend = payload ?? Protocol.HandshakeRequestHeader.Length + ( uint )preparedName.ByteCount;
        var messageTimeoutMs = ( uint )messageTimeout.FullMilliseconds;
        var pingIntervalMs = ( uint )pingInterval.FullMilliseconds;
        var maxBatchPacketCountValue = ( ushort )(maxBatchPacketCount ?? 0);
        var maxNetworkBatchPacketLengthValue = ( uint )(maxNetworkBatchPacketLength?.Bytes ?? 0);
        if ( BitConverter.IsLittleEndian )
        {
            payloadToSend = BinaryPrimitives.ReverseEndianness( payloadToSend );
            messageTimeoutMs = BinaryPrimitives.ReverseEndianness( messageTimeoutMs );
            pingIntervalMs = BinaryPrimitives.ReverseEndianness( pingIntervalMs );
            maxBatchPacketCountValue = BinaryPrimitives.ReverseEndianness( maxBatchPacketCountValue );
            maxNetworkBatchPacketLengthValue = BinaryPrimitives.ReverseEndianness( maxNetworkBatchPacketLengthValue );
        }

        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.HandshakeRequest );
        writer.MoveWrite( payloadToSend );
        writer.MoveWrite(
            ( byte )((isEphemeral ? 0 : 1)
                | (BitConverter.IsLittleEndian ? 2 : 0)
                | (synchronizeExternalObjectNames ? 4 : 0)
                | (clearBuffers ? 8 : 0)) );

        writer.MoveWrite( messageTimeoutMs );
        writer.MoveWrite( pingIntervalMs );
        writer.MoveWrite( maxBatchPacketCountValue );
        writer.MoveWrite( maxNetworkBatchPacketLengthValue );
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
        Send( PreparePing( payload ) );
    }

    internal void SendBindPublisherRequest(
        string channelName,
        bool isEphemeral = true,
        string? streamName = null,
        short? channelNameLength = null,
        uint? payload = null)
    {
        Send( PrepareBindPublisherRequest( channelName, isEphemeral, streamName, channelNameLength, payload ) );
    }

    internal void SendUnbindPublisherRequest(int channelId, uint? payload = null)
    {
        Send( PrepareUnbindPublisherRequest( channelId, payload ) );
    }

    internal void SendUnbindPublisherByNameRequest(string channelName, uint? payload = null)
    {
        Send( PrepareUnbindPublisherByNameRequest( channelName, payload ) );
    }

    internal void SendBindListenerRequest(
        string channelName,
        bool createChannelIfNotExists,
        bool isEphemeral = true,
        short? prefetchHint = null,
        int? maxRetries = null,
        Duration? retryDelay = null,
        int? maxRedeliveries = null,
        Duration? minAckTimeout = null,
        int? deadLetterCapacityHint = null,
        Duration? minDeadLetterRetention = null,
        string? queueName = null,
        string? filterExpression = null,
        short? channelNameLength = null,
        short? queueNameLength = null,
        uint? payload = null)
    {
        Send(
            PrepareBindListenerRequest(
                channelName,
                createChannelIfNotExists,
                isEphemeral,
                prefetchHint,
                maxRetries,
                retryDelay,
                maxRedeliveries,
                minAckTimeout,
                deadLetterCapacityHint,
                minDeadLetterRetention,
                queueName,
                filterExpression,
                channelNameLength,
                queueNameLength,
                payload ) );
    }

    internal void SendUnbindListenerRequest(int channelId, uint? payload = null)
    {
        Send( PrepareUnbindListenerRequest( channelId, payload ) );
    }

    internal void SendDeadLetterQuery(int queueId, int readCount, uint? payload = null)
    {
        Send( PrepareDeadLetterQuery( queueId, readCount, payload ) );
    }

    internal void SendPushMessageRouting(IReadOnlyCollection<Routing> routes, short? targetCount = null, uint? payload = null)
    {
        Send( PreparePushMessageRouting( routes, targetCount, payload ) );
    }

    internal void SendPushMessage(int channelId, byte[] data, bool confirm = true, uint? payload = null)
    {
        Send( PreparePushMessage( channelId, data, confirm, payload ) );
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
        Send( PrepareMessageNotificationAck( queueId, ackId, streamId, messageId, retryAttempt, redeliveryAttempt, payload ) );
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
        Send(
            PrepareMessageNotificationNegativeAck(
                queueId,
                ackId,
                streamId,
                messageId,
                retryAttempt,
                redeliveryAttempt,
                noRetry,
                noDeadLetter,
                hasExplicitDelay,
                explicitDelay,
                payload ) );
    }

    internal void SendBatch(byte[][] packets, short? packetCount = null, uint? payload = null)
    {
        Send( PrepareBatch( packets, packetCount, payload ) );
    }

    internal void SendHeader(MessageBrokerServerEndpoint endpoint, uint payload, bool reverseEndianness = false)
    {
        Send( PrepareHeader( endpoint, payload, reverseEndianness ) );
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
        short? maxBatchPacketCount = null,
        MemorySize? maxNetworkBatchPacketLength = null,
        bool synchronizeExternalObjectNames = false,
        bool isEphemeral = true)
    {
        return Task.Factory.StartNew( () =>
        {
            Connect( server.LocalEndPoint );
            SendHandshake(
                name ?? "test",
                messageTimeout ?? Duration.FromSeconds( 1 ),
                pingInterval ?? Duration.FromSeconds( 10 ),
                maxBatchPacketCount,
                maxNetworkBatchPacketLength,
                synchronizeExternalObjectNames,
                isEphemeral: isEphemeral );

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
