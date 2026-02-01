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
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client.Exceptions;

internal static class Resources
{
    internal const string ClientIsAlreadyConnected = "Client is already connected.";
    internal const string ServerIsEphemeral = "Server is ephemeral.";
    internal const string UnexpectedClientEndpoint = "Received unexpected client endpoint.";
    internal const string ClientNameLengthOutOfBounds = "Server found client's name length to be out of bounds.";
    internal const string ExternalObjectNameSynchronizationIsDisabled = "External object name synchronization is disabled.";
    internal const string MessageCannotBeBothRetryAndRedelivery = "Message notification cannot be marked as both a retry and a redelivery.";
    internal const string ListenerExpectsAckId = "Expected ACK ID to be greater than 0 because listener has ACKs enabled.";
    internal const string ListenerDoesNotExpectAckId = "Expected ACK ID to be equal to 0 because listener has ACKs disabled.";
    internal const string UnexpectedFilterExpression = "Server does not support filter expressions.";
    internal const string FilterExpressionIsNotValid = "Server failed to parse provided filter expression.";

    internal const string ExternalDelaySourceHasBeenDisposed
        = "Operation has been cancelled because external delay value task source has been disposed.";

    internal static readonly string MaxMessageRoutingTargetCountReached
        = $"Max message routing target count of {short.MaxValue} was reached.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ClientDisposed(string name)
    {
        return $"Operation has been cancelled because client '{name}' is disposed.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidClientState(string name, MessageBrokerClientState actual, MessageBrokerClientState expected)
    {
        return $"Expected message broker client '{name}' to be in {expected} state but found {actual} state.";
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
    internal static string UnexpectedPacketLength(int length, int expectedMax)
    {
        return
            $"Expected total packet length to be in [{Protocol.PacketHeader.Length}, {expectedMax}] range but found {( long )length + Protocol.PacketHeader.Length}.";
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
    internal static string ClientIdIsNotPositive(int received)
    {
        return $"Expected client ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ChannelIdIsNotPositive(int received)
    {
        return $"Expected channel ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StreamIdIsNotPositive(int received)
    {
        return $"Expected stream ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string QueueIdIsNotPositive(int received)
    {
        return $"Expected queue ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string SenderIdIsNotPositive(int received)
    {
        return $"Expected sender ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string AckIdIsInvalid(int received)
    {
        return $"Expected ACK ID to not be less than -1 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidRetry(int ackId)
    {
        return $"Message notification with ACK ID {ackId} cannot be marked as retry.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidRedelivery(int ackId)
    {
        return $"Message notification with ACK ID {ackId} cannot be marked as redelivery.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RetryIsNotPositive(int received)
    {
        return $"Expected retry to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RedeliveryIsNotPositive(int received)
    {
        return $"Expected redelivery to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingNonZeroMessageResendAttemptMarker(int retry, int redelivery)
    {
        return $"Message notification with retry {retry} and redelivery {redelivery} is not marked as either a retry or a redelivery.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerDoesNotExist(int channelId)
    {
        return $"Listener for channel with ID {channelId} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxRetriesExceeded(MessageBrokerListener listener, int received)
    {
        return $"Retry {received} exceeds listener's {listener.MaxRetries} max retries.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxRedeliveriesExceeded(MessageBrokerListener listener, int received)
    {
        return $"Redelivery {received} exceeds listener's {listener.MaxRedeliveries} max redeliveries.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidMessageParameters(MessageBrokerListener listener, Chain<string> errors)
    {
        Assume.IsGreaterThan( errors.Count, 0 );
        var reasons = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return
            $"Client [{listener.Client.Id}] '{listener.Client.Name}' received an invalid message notification through channel [{listener.ChannelId}] '{listener.ChannelName}'. Encountered {errors.Count} error(s):{Environment.NewLine}{reasons}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DisabledAcks(MessageBrokerListener listener)
    {
        return
            $"Client [{listener.Client.Id}] '{listener.Client.Name}' cannot send an ACK to the server because listener for a channel [{listener.ChannelId}] '{listener.ChannelName}' has them disabled.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessagesDiscarded(int count)
    {
        return $"{count} locally stored message notification(s) have been discarded due to client disposal.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessagesDiscarded(int channelId, string channelName, int count)
    {
        return
            $"{count} locally stored message notification(s) by [{channelId}] '{channelName}' channel listener have been discarded due to listener disposal.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PublisherAlreadyBound(string channelName)
    {
        return $"Client is already bound as a publisher to channel '{channelName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string BindPublisherCancelled(string channelName)
    {
        return $"Binding client to channel '{channelName}' as a publisher has been cancelled by the server.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ChannelDoesNotExist(string channelName)
    {
        return $"Channel '{channelName}' does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerAlreadyBound(string channelName)
    {
        return $"Client is already bound as a listener to channel '{channelName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string BindListenerCancelled(string channelName)
    {
        return $"Binding client to channel '{channelName}' as a listener has been cancelled by the server.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PublisherNotBound(int channelId, string channelName)
    {
        return $"Client is not bound as a publisher to channel [{channelId}] '{channelName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerNotBound(int channelId, string channelName)
    {
        return $"Client is not bound as a listener to channel [{channelId}] '{channelName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessageCancelled(int streamId, string streamName)
    {
        return $"Message push to stream [{streamId}] '{streamName}' has been cancelled by the server.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TotalCountIsNotValid(int received)
    {
        return $"Expected total count to not be less than -1 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxReadCountIsNotEqualToZero(int received)
    {
        return $"Expected max read count to be equal to 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxReadCountIsNotValid(int received, int totalCount)
    {
        return $"Expected max read count to be in [0, {totalCount}] range but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NextExpirationAtIsNotEqualToZero(Timestamp received)
    {
        return $"Expected next expiration at to be equal to 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string QueueDoesNotExist(int queueId)
    {
        return $"Queue with ID {queueId} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessageTimeoutIsOutOfBounds(Duration timeout)
    {
        return $"Expected received message timeout to be in {GetBounds( Defaults.Temporal.TimeoutBounds )} range but found {timeout}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PingIntervalIsOutOfBounds(Duration interval)
    {
        return $"Expected received ping interval to be in {GetBounds( Defaults.Temporal.PingIntervalBounds )} range but found {interval}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxNetworkPacketLengthIsOutOfBounds(MemorySize length)
    {
        return
            $"Expected received max network packet length to be in {GetBounds( Defaults.Memory.MaxNetworkPacketLengthBounds )} range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxNetworkMessagePacketLengthIsOutOfBounds(MemorySize length, Bounds<MemorySize> range)
    {
        return $"Expected received max network message packet length to be in {GetBounds( range )} range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxNetworkBatchPacketLengthIsOutOfBounds(MemorySize length, Bounds<MemorySize> range)
    {
        return $"Expected received max network batch packet length to be in {GetBounds( range )} range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxNetworkBatchPacketLengthIsNotEqualToZero(MemorySize length)
    {
        return $"Expected received disabled max network batch packet length to be equal to 0 but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxBatchPacketCountIsInvalid(short count)
    {
        return $"Expected received max batch packet count to be greater than 1 or equal to 0 but found {count}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxRoutingPacketLengthLimitReached(MessageBrokerClient client, int length)
    {
        return
            $"Max network packet length of {client.MaxNetworkPacketLength} for message routing has been exceeded by {MemorySize.FromBytes( length - client.MaxNetworkPacketBytes )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MaxMessagePacketLengthLimitReached(MessageBrokerClient client, int length)
    {
        return
            $"Max network message packet length of {MemorySize.FromBytes( client.MaxNetworkPushMessagePacketBytes )} has been exceeded by {MemorySize.FromBytes( length - client.MaxNetworkPushMessagePacketBytes )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToPushMessage(Chain<string> errors)
    {
        Assume.IsGreaterThan( errors.Count, 0 );
        var reasons = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"Message could not be pushed to the server. Encountered {errors.Count} error(s):{Environment.NewLine}{reasons}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ResponseTimeout(string clientName, Duration timeout, MessageBrokerServerEndpoint requestEndpoint)
    {
        return $"Server failed to respond to '{clientName}' client's {requestEndpoint} in the specified amount of time ({timeout}).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidSenderNameLength(int length)
    {
        return
            $"Expected sender name length to be in [{Defaults.NameLengthBounds.Min}, {Defaults.NameLengthBounds.Max}] range but found {length}.";
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
    internal static string UnexpectedSystemNotificationType(MessageBrokerSystemNotificationType type)
    {
        return $"Received unexpected system notification type {GetType( type )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ClientPayloadRejected(string clientName, MessageBrokerServerEndpoint endpoint, Chain<string> errors)
    {
        var header = $"Server rejected an invalid {GetEndpoint( endpoint )} sent by client '{clientName}'.";
        if ( errors.Count == 0 )
            return header;

        var reasons = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"{header} Encountered {errors.Count} error(s):{Environment.NewLine}{reasons}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidPayloadFromServer(string clientName, MessageBrokerClientEndpoint endpoint, Chain<string> errors)
    {
        var header = $"Client '{clientName}' received an invalid {GetEndpoint( endpoint )} from the server.";
        if ( errors.Count == 0 )
            return header;

        var reasons = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"{header} Encountered {errors.Count} error(s):{Environment.NewLine}{reasons}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetEndpoint<T>(T value)
        where T : struct, Enum
    {
        return Enum.IsDefined( value ) ? value.ToString() : $"<unrecognized-endpoint-{value}>";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetType(MessageBrokerSystemNotificationType type)
    {
        return Enum.IsDefined( type ) ? type.ToString() : $"<unrecognized-type-{type}>";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetBounds<T>(Bounds<T> source)
        where T : IComparable<T>
    {
        return $"[{source.Min}, {source.Max}]";
    }
}
