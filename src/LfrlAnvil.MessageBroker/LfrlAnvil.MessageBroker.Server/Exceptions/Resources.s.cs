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
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

internal static class Resources
{
    internal const string ServerDisposed = "Operation has been cancelled because server is disposed.";
    internal const string UnexpectedServerEndpoint = "Received unexpected server endpoint.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidServerState(MessageBrokerServerState actual, MessageBrokerServerState expected)
    {
        return $"Expected message broker server to be in {expected} state but found {actual} state.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ClientDisposed(int id, string name)
    {
        return $"Operation has been cancelled because remote message broker client [{id}] '{name}' is disposed.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidEndiannessPayload(uint received)
    {
        return $"Expected endianness verification payload to be {Protocol.Endianness.VerificationPayload:x8} but found {received:x8}.";
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
    internal static string ServerPayloadRejected(
        int clientId,
        string clientName,
        MessageBrokerClientEndpoint endpoint,
        Chain<string> errors)
    {
        var header
            = $"Message broker client rejected an invalid {GetEndpoint( endpoint )} sent by server's remote client [{clientId}] '{clientName}'.";

        if ( errors.Count == 0 )
            return header;

        var reasons = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"{header} Encountered {errors.Count} error(s):{Environment.NewLine}{reasons}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidPayloadFromClient(
        int clientId,
        string clientName,
        MessageBrokerServerEndpoint endpoint,
        Chain<string> errors)
    {
        var header = $"Message broker server received an invalid {GetEndpoint( endpoint )} from client [{clientId}] '{clientName}'.";
        if ( errors.Count == 0 )
            return header;

        var reasons = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"{header} Encountered {errors.Count} error(s):{Environment.NewLine}{reasons}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidNameLength(int length)
    {
        return
            $"Expected name length to be in [{Defaults.NameLengthBounds.Min}, {Defaults.NameLengthBounds.Max}] range but found {length}.";
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
    internal static string InvalidBinaryChannelNameLength(int length, int maxLength)
    {
        return $"Expected binary channel name length to be in [0, {maxLength}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidPrefetchHint(int value)
    {
        return $"Expected prefetch hint to be greater than 0 but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DuplicateClientName(string name)
    {
        return $"Client with name '{name}' already exists.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UnexpectedPacketLength(int length)
    {
        return $"Expected packet length to be in [0, {int.MaxValue}] range but found {length}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToCreateChannelBinding(
        int clientId,
        string clientName,
        int channelId,
        string channelName,
        Protocol.BindFailureResponse.Reasons reason)
    {
        Assume.NotEquals( reason, Protocol.BindFailureResponse.Reasons.None );
        var reasonText = reason == Protocol.BindFailureResponse.Reasons.AlreadyBound
            ? "it is already bound to it"
            : "the binding process was cancelled";

        return
            $"Message broker client [{clientId}] '{clientName}' could not be bound to channel [{channelId}] '{channelName}' because {reasonText}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToUnbindFromChannel(int clientId, string clientName, int channelId, string channelName)
    {
        return
            $"Message broker client [{clientId}] '{clientName}' could not be unbound from channel [{channelId}] '{channelName}' because it is not bound to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToUnbindFromNonExistingChannel(int clientId, string clientName, int channelId)
    {
        return $"Message broker client [{clientId}] '{clientName}' could not be unbound from non-existing channel with ID {channelId}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToUnsubscribeFromChannel(int clientId, string clientName, int channelId, string channelName)
    {
        return
            $"Message broker client [{clientId}] '{clientName}' could not be unsubscribed from channel [{channelId}] '{channelName}' because it is not subscribed to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToUnsubscribeFromNonExistingChannel(int clientId, string clientName, int channelId)
    {
        return
            $"Message broker client [{clientId}] '{clientName}' could not be unsubscribed from non-existing channel with ID {channelId}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToAddMessageToUnboundChannel(int clientId, string clientName, int channelId)
    {
        return
            $"Message broker client [{clientId}] '{clientName}' could not add message to channel with ID {channelId} because it is not bound to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToCreateSubscription(
        int clientId,
        string clientName,
        int? channelId,
        string channelName,
        Protocol.SubscribeFailureResponse.Reasons reason)
    {
        Assume.NotEquals( reason, Protocol.SubscribeFailureResponse.Reasons.None );
        var reasonText = reason switch
        {
            Protocol.SubscribeFailureResponse.Reasons.ChannelDoesNotExist => "channel does not exist",
            Protocol.SubscribeFailureResponse.Reasons.AlreadySubscribed => "it is already subscribed to it",
            _ => "the subscription process was cancelled"
        };

        var channelIdText = channelId is null ? string.Empty : $"[{channelId.Value}] ";
        return
            $"Message broker client [{clientId}] '{clientName}' failed to create a subscription to channel {channelIdText}'{channelName}' because {reasonText}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetEndpoint<T>(T value)
        where T : struct, Enum
    {
        return Enum.IsDefined( value ) ? value.ToString() : $"<unrecognized-endpoint-{value}>";
    }
}
