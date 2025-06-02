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
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client.Exceptions;

internal static class Resources
{
    internal const string ClientNameAlreadyExists = "Client name already exists.";
    internal const string UnexpectedClientEndpoint = "Received unexpected client endpoint.";
    internal const string ClientNameLengthOutOfBounds = "Server found client's name length to be out of bounds.";
    internal const string ExternalObjectNameSynchronizationIsDisabled = "External object name synchronization is disabled.";

    internal const string ExternalDelaySourceHasBeenDisposed
        = "Operation has been cancelled because external delay value task source has been disposed.";

    internal static readonly string ServerFailedToDecodeClientName
        = $"Server failed to decode client's name using {TextEncoding.Instance.EncodingName} encoding.";

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
    internal static string UnexpectedPacketLength(int length)
    {
        return $"Expected packet length to be in [0, {int.MaxValue}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidEndiannessPayload(uint received)
    {
        return $"Expected endianness verification payload to be {Protocol.Endianness.VerificationPayload:x8} but found {received:x8}.";
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
    internal static string SenderIdEqualsClientId(int received)
    {
        return $"Expected sender ID {received} to not be equal to client's ID.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string SenderIdIsNegative(int received)
    {
        return $"Expected sender ID to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StreamIdIsNegative(int received)
    {
        return $"Expected stream ID to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RetryAttemptIsNegative(int received)
    {
        return $"Expected retry attempt to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RedeliveryAttemptIsNegative(int received)
    {
        return $"Expected redelivery attempt to not be negative but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ListenerDoesNotExist(int channelId)
    {
        return $"Listener for channel with ID {channelId} does not exist.";
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
    internal static string ResponseTimeout(string clientName, Duration timeout, MessageBrokerServerEndpoint requestEndpoint)
    {
        return
            $"Server failed to respond to '{clientName}' client's {requestEndpoint} in the specified amount of time ({timeout.FullMilliseconds} milliseconds).";
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
