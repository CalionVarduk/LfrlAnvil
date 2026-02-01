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
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

internal static class Resources
{
    internal const string ServerDisposed = "Operation has been cancelled because server is disposed.";
    internal const string UnexpectedServerEndpoint = "Received unexpected server endpoint.";
    internal const string MessageRoutingIsAlreadyEnqueued = "Message routing is already enqueued.";

    internal const string ExternalDelaySourceHasBeenDisposed
        = "Operation has been cancelled because external delay value task source has been disposed.";

    internal const string InvalidFileHeader = "File contains invalid metadata header.";
    internal const string FileNameDoesNotContainValidId = "File name doesn't contain a valid ID.";
    internal const string MissingFile = "File is missing.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidServerState(MessageBrokerServerState actual, MessageBrokerServerState expected)
    {
        return $"Expected server to be in {expected} state but found {actual} state.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ClientDeactivated(int id, string name, bool disposed)
    {
        return $"Operation has been cancelled because remote client [{id}] '{name}' is {(disposed ? "disposed" : "deactivated")}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ConnectorDisposed(int id)
    {
        return $"Operation has been cancelled because remote client connector [{id}] is disposed.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ChannelDisposed(int id, string name)
    {
        return $"Operation has been cancelled because channel [{id}] '{name}' is disposed.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StreamDisposed(int id, string name)
    {
        return $"Operation has been cancelled because stream [{id}] '{name}' is disposed.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string QueueDisposed(int id, string name)
    {
        return $"Operation has been cancelled because queue [{id}] '{name}' is disposed.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PublisherDisposed(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        return
            $"Operation has been cancelled because publisher binding between client [{client.Id}] '{client.Name}' and channel [{channel.Id}] '{channel.Name}' is disposed.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerDisposed(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        return
            $"Operation has been cancelled because listener binding between client [{client.Id}] '{client.Name}' and channel [{channel.Id}] '{channel.Name}' is disposed.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NotBoundAsPublisher(MessageBrokerChannel channel, MessageBrokerRemoteClient client)
    {
        return $"Client [{client.Id}] '{client.Name}' is not bound as publisher to channel [{channel.Id}] '{channel.Name}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NotBoundAsListener(MessageBrokerChannel channel, MessageBrokerRemoteClient client)
    {
        return $"Client [{client.Id}] '{client.Name}' is not bound as listener to channel [{channel.Id}] '{channel.Name}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NotBoundAsPublisher(MessageBrokerStream stream, MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        return
            $"Client [{client.Id}] '{client.Name}' is not bound as publisher to channel [{channel.Id}] '{channel.Name}' using stream [{stream.Id}] '{stream.Name}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidEndiannessPayload(uint received)
    {
        return $"Expected endianness verification payload to be {Protocol.Endianness.VerificationPayload:x8} but found {received:x8}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string BatchPacketCountIsInvalid(int received, int max)
    {
        return $"Expected batch packet count to be in [2, {max}] range but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string BatchPacketElementHeaderIsTooShort(int index, int remaining)
    {
        return
            $"Expected length of the packet at index {index} in batch to be greater than or equal to {Protocol.PacketHeader.Length} but found {remaining}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string BatchPacketElementPayloadIsTooLarge(int index, int payload, int remaining)
    {
        return $"Expected payload of the packet at index {index} in batch to be less than or equal to {remaining} but found {payload}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string BatchPacketContainsTooMuchData(int remaining)
    {
        return $"Expected an end of the batch packet but found {remaining} remaining byte(s).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidHeaderPayload(uint actual, uint expected)
    {
        return $"Expected header payload to be {expected} but found {actual}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TooShortHeaderPayload(uint actual, uint expectedMin)
    {
        return $"Expected header payload to be at least {expectedMin} but found {actual}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TooLargeHeaderPayload(int remaining)
    {
        return $"Header payload is too large by {remaining}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidPayloadFromClient(
        int clientId,
        string clientName,
        MessageBrokerServerEndpoint endpoint,
        Chain<string> errors)
    {
        var header = $"Server received an invalid {GetEndpoint( endpoint )} from client [{clientId}] '{clientName}'.";
        if ( errors.Count == 0 )
            return header;

        var reasons = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"{header} Encountered {errors.Count} error(s):{Environment.NewLine}{reasons}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidPayloadFromClient(
        int connectorId,
        MessageBrokerServerEndpoint endpoint,
        Chain<string> errors)
    {
        var header = $"Server-side connector [{connectorId}] received an invalid {GetEndpoint( endpoint )} from client.";
        if ( errors.Count == 0 )
            return header;

        var reasons = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"{header} Encountered {errors.Count} error(s):{Environment.NewLine}{reasons}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidStorage(string filePath, Chain<string> errors)
    {
        var header = $"Server storage file '{filePath}' contains invalid data.";
        if ( errors.Count == 0 )
            return header;

        var reasons = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"{header} Encountered {errors.Count} error(s):{Environment.NewLine}{reasons}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidClientNameLength(int length)
    {
        return
            $"Expected client name length to be in [{Defaults.NameLengthBounds.Min}, {Defaults.NameLengthBounds.Max}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidChannelNameLength(int length)
    {
        return
            $"Expected channel name length to be in [{Defaults.NameLengthBounds.Min}, {Defaults.NameLengthBounds.Max}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidStreamNameLength(int length)
    {
        return
            $"Expected stream name length to be in [{Defaults.NameLengthBounds.Min}, {Defaults.NameLengthBounds.Max}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidQueueNameLength(int length)
    {
        return
            $"Expected queue name length to be in [{Defaults.NameLengthBounds.Min}, {Defaults.NameLengthBounds.Max}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidBinaryClientNameLength(int length)
    {
        return $"Expected binary channel name length to be in [1, {Defaults.Memory.DefaultNetworkPacketLength}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidBinaryChannelNameLength(int length, int maxLength)
    {
        return $"Expected binary channel name length to be in [0, {maxLength}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidBinaryQueueNameLength(int length, int maxLength)
    {
        return $"Expected binary queue name length to be in [0, {maxLength}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ChannelIdIsNotPositive(int received)
    {
        return $"Expected channel ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TargetCountIsNotPositive(short received)
    {
        return $"Expected target count to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TargetIdIsNotPositive(int index, int received)
    {
        return $"Expected target ID at index {index} to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidTargetNameLength(int index, int length)
    {
        return
            $"Expected target name length at index {index} to be in [{Defaults.NameLengthBounds.Min}, {Defaults.NameLengthBounds.Max}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TargetByIdDoesNotExist(int index, int id)
    {
        return $"Target client with ID {id} at index {index} could not be found.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TargetByNameDoesNotExist(int index, string name)
    {
        return $"Target client with name '{name}' at index {index} could not be found.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TargetDuplicateFound(int index, MessageBrokerRemoteClient target)
    {
        return $"Target client [{target.Id}] '{target.Name}' at index {index} is a duplicate.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TargetCountIsTooLarge(int read, int count)
    {
        return $"Target count {count} is larger than actual element count {read}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NotificationsDiscarded(int count)
    {
        return $"{count} stored pending notification(s) have been discarded due to client disposal.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StreamMessagesDiscarded(int count)
    {
        return $"{count} stored pending message(s) have been discarded due to server disposal.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string QueueMessagesDiscarded(int count)
    {
        return $"{count} enqueued message(s) have been discarded due to client disposal.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidPrefetchHint(short value)
    {
        return $"Expected prefetch hint to be greater than 0 but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxRetriesIsNegative(int received)
    {
        return $"Expected max retries to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RetryDelayIsNegative(Duration received)
    {
        return $"Expected retry delay to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DisabledRetryDelayIsNotZero(Duration received)
    {
        return $"Expected disabled retry delay to be equal to 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxRedeliveriesIsNegative(int received)
    {
        return $"Expected max redeliveries to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MinAckTimeoutIsNegative(Duration received)
    {
        return $"Expected min ACK timeout to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string EnabledMinAckTimeoutIsNotPositive(Duration received)
    {
        return $"Expected enabled min ACK timeout to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DeadLetterCapacityIsNegative(int received)
    {
        return $"Expected dead letter capacity hint to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DisabledDeadLetterRetentionIsNotZero(Duration received)
    {
        return $"Expected disabled min dead letter retention to be equal to 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string EnabledDeadLetterRetentionIsNotPositive(Duration received)
    {
        return $"Expected enabled min dead letter retention to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string QueueIdIsNotPositive(int received)
    {
        return $"Expected queue ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StreamIdIsNotPositive(int received)
    {
        return $"Expected stream ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string AckIdIsNotPositive(int received)
    {
        return $"Expected ACK ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RetryIsNegative(int received)
    {
        return $"Expected retry to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RedeliveryIsNegative(int received)
    {
        return $"Expected redelivery to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ExplicitDelayIsNegative(Duration received)
    {
        return $"Expected explicit delay to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DisabledExplicitDelayIsNotZero(Duration received)
    {
        return $"Expected disabled explicit delay to be equal to 0 found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ReadCountIsNegative(int received)
    {
        return $"Expected read count to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ServerIsEphemeral(string clientName)
    {
        return $"Non-ephemeral client with name '{clientName}' cannot be connected because the server is ephemeral.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ClientAlreadyConnected(string name)
    {
        return $"Client with name '{name}' is already connected.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UnexpectedPacketLength(int length, int expectedMax)
    {
        return
            $"Expected total packet length to be in [{Protocol.PacketHeader.Length}, {expectedMax}] range but found {( long )length + Protocol.PacketHeader.Length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UnexpectedPacketElementLength(int index, int remaining, int expected)
    {
        return $"Expected packet element length at index {index} to be at least {expected} but found {remaining}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UnexpectedFilterExpression(string expression)
    {
        return $"Filter expressions are not enabled but found:{Environment.NewLine}{expression}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidFilterExpression(string expression)
    {
        return $"Failed to parse the following filter expression:{Environment.NewLine}{expression}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidFilterExpressionArgumentCount(string expression, ParsedExpressionUnboundArguments args)
    {
        return
            $"Expected at most one filter expression context argument but found {args.Count} ({string.Join( ", ", args.Select( static a => $"'{a.Key}'" ) )}) in the following expression:{Environment.NewLine}{expression}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PublisherAlreadyBound(MessageBrokerChannelPublisherBinding publisher)
    {
        return
            $"Client [{publisher.Client.Id}] '{publisher.Client.Name}' could not be bound as a publisher to channel [{publisher.Channel.Id}] '{publisher.Channel.Name}' because it is already bound as a publisher to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PublisherNotBound(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        return
            $"Client [{client.Id}] '{client.Name}' could not be unbound as a publisher from channel [{channel.Id}] '{channel.Name}' because it is not bound as a publisher to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerAlreadyBound(MessageBrokerChannelListenerBinding listener)
    {
        return
            $"Client [{listener.Client.Id}] '{listener.Client.Name}' could not be bound as a listener to channel [{listener.Channel.Id}] '{listener.Channel.Name}' because it is already bound as a listener to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerNotBound(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        return
            $"Client [{client.Id}] '{client.Name}' could not be unbound as a listener from channel [{channel.Id}] '{channel.Name}' because it is not bound as a listener to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string CannotUnbindPublisherFromNonExistingChannel(MessageBrokerRemoteClient client, int channelId)
    {
        return $"Client [{client.Id}] '{client.Name}' could not be unbound as a publisher from non-existing channel with ID {channelId}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string CannotBindAsListenerToNonExistingChannel(MessageBrokerRemoteClient client, string channelName)
    {
        return $"Client [{client.Id}] '{client.Name}' could not be bound as a listener to a non-existing channel '{channelName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string CannotUnbindListenerFromNonExistingChannel(MessageBrokerRemoteClient client, int channelId)
    {
        return $"Client [{client.Id}] '{client.Name}' could not be unbound as a listener from non-existing channel with ID {channelId}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PublisherNotBound(MessageBrokerRemoteClient client, int channelId)
    {
        return
            $"Client [{client.Id}] '{client.Name}' could not push message to channel with ID {channelId} because it is not bound as a publisher to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string QueueForAckNotFound(MessageBrokerRemoteClient client, int queueId)
    {
        return $"Client [{client.Id}] '{client.Name}' could not process a message ACK for non-existing queue with ID {queueId}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string QueueForDeadLetterQueryNotFound(MessageBrokerRemoteClient client, int queueId)
    {
        return $"Client [{client.Id}] '{client.Name}' could not process a dead letter query for non-existing queue with ID {queueId}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessageNotFound(MessageBrokerQueue queue, int ackId, int streamId, ulong messageId)
    {
        return
            $"Queue [{queue.Id}] '{queue.Name}' for client [{queue.Client.Id}] '{queue.Client.Name}' could not process a (ack ID: {ackId}, stream ID: {streamId}, message ID: {messageId}) message ACK because the message does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessageDataNotFound(MessageBrokerStream stream, int storeKey)
    {
        return $"Stream [{stream.Id}] '{stream.Name}' does not have a message related to store key {storeKey}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessageVersionNotFound(MessageBrokerQueue queue, int streamId, ulong messageId, int retry, int redelivery)
    {
        return
            $"Queue [{queue.Id}] '{queue.Name}' for client [{queue.Client.Id}] '{queue.Client.Name}' could not process a (stream ID: {streamId}, message ID: {messageId}) message ACK because its (retry: {retry}, redelivery: {redelivery}) version does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RequestTimeout(MessageBrokerRemoteClient client, Duration timeout)
    {
        return $"Client [{client.Id}] '{client.Name}' failed to send a request to the server in the specified amount of time ({timeout}).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RequestTimeout(MessageBrokerRemoteClientConnector connector)
    {
        return
            $"Client failed to send a request to the server-side connector [{connector.Id}] in the specified amount of time ({connector.Server.HandshakeTimeout}).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidFileLength(int expected, long actual)
    {
        return $"Expected file length to be equal to {expected} but found {actual}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidFileMinLength(long expectedMin, long actual)
    {
        return $"Expected file length to be greater than or equal to {expectedMin} but found {actual}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidFileLength(int expectedMin, int expectedMax, long actual)
    {
        return $"Expected file length to be in [{expectedMin}, {expectedMax}] range but found {actual}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RecreatedChannelDuplicate(int id, string name)
    {
        return $"Recreated channel [{id}] '{name}' is a duplicate.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RecreatedStreamDuplicate(int id, string name)
    {
        return $"Recreated stream [{id}] '{name}' is a duplicate.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RecreatedClientDuplicate(int id, string name)
    {
        return $"Recreated client [{id}] '{name}' is a duplicate.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RecreatedMessageDuplicate(int storeKey)
    {
        return $"Recreated message with key '{storeKey}' is duplicated.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RecreatedQueueDuplicate(MessageBrokerRemoteClient client, int id, string name)
    {
        return $"Recreated queue [{id}] '{name}' for client [{client.Id}] '{client.Name}' is a duplicate.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RecreatedPublisherDuplicate(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        return $"Recreated publisher for client [{client.Id}] '{client.Name}' and channel [{channel.Id}] '{channel.Name}' is a duplicate.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RecreatedListenerDuplicate(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        return $"Recreated listener for client [{client.Id}] '{client.Name}' and channel [{channel.Id}] '{channel.Name}' is a duplicate.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PublisherChannelDoesNotExist(MessageBrokerRemoteClient client, int channelId)
    {
        return
            $"Publisher for client [{client.Id}] '{client.Name}' and channel with ID {channelId} cannot be recreated because channel does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PublisherStreamDoesNotExist(MessageBrokerRemoteClient client, int channelId, int streamId)
    {
        return
            $"Publisher for client [{client.Id}] '{client.Name}' and channel with ID {channelId} cannot be recreated because stream with ID {streamId} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerChannelDoesNotExist(MessageBrokerRemoteClient client, int channelId)
    {
        return
            $"Listener for client [{client.Id}] '{client.Name}' and channel with ID {channelId} cannot be recreated because channel does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerQueueDoesNotExist(MessageBrokerRemoteClient client, MessageBrokerChannel channel, int queueId)
    {
        return
            $"Listener for client [{client.Id}] '{client.Name}' and channel [{channel.Id}] '{channel.Name}' cannot be recreated because queue with ID {queueId} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessageCountIsNegative(int value)
    {
        return $"Expected message count to not be negative but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RoutingCountIsNegative(int value)
    {
        return $"Expected routing count to not be negative but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string VirtualIdIsNotPositive(int value)
    {
        return $"Expected virtual ID to be positive but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string SenderIdIsNotPositive(int value)
    {
        return $"Expected sender ID to be positive but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StoreKeyIsNegative(int value)
    {
        return $"Expected store key to not be negative but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidDataLength(MessageBrokerServer server, int value)
    {
        return $"Expected data length to be in [0, {server.MaxNetworkMessagePacketBytes}] range but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DiscardedDataLengthIsPositive(int value)
    {
        return $"Expected discarded data length to not be positive but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidRoutingDataLength(int value, int max)
    {
        return $"Expected data length to be in [0, {max}] range but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StreamDoesNotExist(int streamId)
    {
        return $"Stream with ID {streamId} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ChannelDoesNotExist(int channelId)
    {
        return $"Channel with ID {channelId} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ClientDoesNotExist(int clientId)
    {
        return $"Client with ID {clientId} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessageDoesNotExist(ulong messageId)
    {
        return $"Message with ID {messageId} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StreamMessageDoesNotExist(MessageBrokerStream stream, int storeKey)
    {
        return $"Stream [{stream.Id}] '{stream.Name}' does not contain a message with key {storeKey}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PendingStreamMessageCannotBeReferencedByQueue(MessageBrokerStream stream, int storeKey)
    {
        return $"Message with key {storeKey} in stream [{stream.Id}] '{stream.Name}' is pending and cannot be referenced by a queue.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerDoesNotExist(MessageBrokerChannel channel, MessageBrokerStream stream, int storeKey)
    {
        return
            $"Failed to enqueue a message from stream [{stream.Id}] '{stream.Name}' with store key {storeKey} because Listener for channel [{channel.Id}] '{channel.Name}' does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string EphemeralSenderDoesNotExist(int senderId)
    {
        return $"Ephemeral sender with ID {senderId} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NextPendingMessageDoesNotExist(int storeKey)
    {
        return $"Next pending message with key {storeKey} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerMetadataWarnings(MessageBrokerRemoteClient client, int channelId, Chain<string> warnings)
    {
        Assume.IsNotEmpty( warnings );
        var header = $"Listener for client [{client.Id}] '{client.Name}' and channel with ID {channelId} has invalid metadata.";
        var issues = string.Join( Environment.NewLine, warnings.Select( static (w, i) => $"{i + 1}. {w}" ) );
        return $"{header} Encountered {warnings.Count} issue(s):{Environment.NewLine}{issues}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetEndpoint<T>(T value)
        where T : struct, Enum
    {
        return Enum.IsDefined( value ) ? value.ToString() : $"<unrecognized-endpoint-{value}>";
    }
}
