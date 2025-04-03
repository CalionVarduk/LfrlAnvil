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

    internal static readonly string ServerFailedToDecodeClientName
        = $"Server failed to decode client's name using {TextEncoding.Instance.EncodingName} encoding.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ClientDisposed(string name)
    {
        return $"Operation has been cancelled because message broker client '{name}' is disposed.";
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
    internal static string QueueIdIsNotPositive(int received)
    {
        return $"Expected queue ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string AlreadyBound(string channelName)
    {
        return $"Client is already bound to channel '{channelName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string BindCancelled(string channelName)
    {
        return $"Binding client to channel '{channelName}' has been cancelled by the server.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ChannelDoesNotExist(string channelName)
    {
        return $"Channel '{channelName}' does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string AlreadySubscribed(string channelName)
    {
        return $"Client is already subscribed to channel '{channelName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string SubscribeCancelled(string channelName)
    {
        return $"Subscribing client to channel '{channelName}' has been cancelled by the server.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NotBound(int channelId, string channelName)
    {
        return $"Client is not bound to channel [{channelId}] '{channelName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NotSubscribed(int channelId, string channelName)
    {
        return $"Client is not subscribed to channel [{channelId}] '{channelName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessageCancelled(int queueId, string queueName)
    {
        return $"Message enqueue to queue [{queueId}] '{queueName}' has been cancelled by the server.";
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
            $"Message broker server failed to respond to '{clientName}' client's {requestEndpoint} request in the specified amount of time ({timeout.FullMilliseconds} milliseconds).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ClientPayloadRejected(string clientName, MessageBrokerServerEndpoint endpoint, Chain<string> errors)
    {
        var header = $"Message broker server rejected an invalid {GetEndpoint( endpoint )} sent by client '{clientName}'.";
        if ( errors.Count == 0 )
            return header;

        var reasons = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"{header} Encountered {errors.Count} error(s):{Environment.NewLine}{reasons}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidPayloadFromServer(string clientName, MessageBrokerClientEndpoint endpoint, Chain<string> errors)
    {
        var header = $"Message broker client '{clientName}' received an invalid {GetEndpoint( endpoint )} from the server.";
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
    private static string GetBounds<T>(Bounds<T> source)
        where T : IComparable<T>
    {
        return $"[{source.Min}, {source.Max}]";
    }
}
