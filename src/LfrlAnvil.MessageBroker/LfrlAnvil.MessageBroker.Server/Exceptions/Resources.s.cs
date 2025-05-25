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
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

internal static class Resources
{
    internal const string ServerDisposed = "Operation has been cancelled because server is disposed.";
    internal const string UnexpectedServerEndpoint = "Received unexpected server endpoint.";

    internal const string ExternalDelaySourceHasBeenDisposed
        = "Operation has been cancelled because external delay value task source has been disposed.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidServerState(MessageBrokerServerState actual, MessageBrokerServerState expected)
    {
        return $"Expected server to be in {expected} state but found {actual} state.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ClientDisposed(int id, string name)
    {
        return $"Operation has been cancelled because remote client [{id}] '{name}' is disposed.";
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
    internal static string ChannelIdIsNotPositive(int received)
    {
        return $"Expected channel ID to be greater than 0 but found {received}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MessagesDiscarded(int count)
    {
        return $"{count} stored pending message notification(s) have been discarded due to client disposal.";
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
    internal static string FailedToCreatePublisher(
        int clientId,
        string clientName,
        int channelId,
        string channelName,
        Protocol.BindPublisherFailureResponse.Reasons reason)
    {
        Assume.NotEquals( reason, Protocol.BindPublisherFailureResponse.Reasons.None );
        var reasonText = reason == Protocol.BindPublisherFailureResponse.Reasons.AlreadyBound
            ? "it is already bound as a publisher to it"
            : "the publisher binding process was cancelled";

        return
            $"Client [{clientId}] '{clientName}' could not be bound as a publisher to channel [{channelId}] '{channelName}' because {reasonText}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToUnbindPublisherFromChannel(int clientId, string clientName, int channelId, string channelName)
    {
        return
            $"Client [{clientId}] '{clientName}' could not be unbound as a publisher from channel [{channelId}] '{channelName}' because it is not bound as a publisher to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToUnbindPublisherFromNonExistingChannel(int clientId, string clientName, int channelId)
    {
        return
            $"Client [{clientId}] '{clientName}' could not be unbound as a publisher from non-existing channel with ID {channelId}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToUnbindListenerFromChannel(int clientId, string clientName, int channelId, string channelName)
    {
        return
            $"Client [{clientId}] '{clientName}' could not be unbound as a listener from channel [{channelId}] '{channelName}' because it is not bound as a listener to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToUnbindListenerFromNonExistingChannel(int clientId, string clientName, int channelId)
    {
        return
            $"Client [{clientId}] '{clientName}' could not be unbound as a listener from non-existing channel with ID {channelId}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToPushMessageToUnboundChannel(int clientId, string clientName, int channelId)
    {
        return
            $"Client [{clientId}] '{clientName}' could not push message to channel with ID {channelId} because it is not bound as a publisher to it.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RequestTimeout(int clientId, string clientName, Duration timeout)
    {
        return
            $"Client [{clientId}] '{clientName}' failed to send a request to the server in the specified amount of time ({timeout.FullMilliseconds} milliseconds).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToCreateListenerBinding(
        int clientId,
        string clientName,
        int? channelId,
        string channelName,
        Protocol.BindListenerFailureResponse.Reasons reason)
    {
        Assume.NotEquals( reason, Protocol.BindListenerFailureResponse.Reasons.None );
        var reasonText = reason switch
        {
            Protocol.BindListenerFailureResponse.Reasons.ChannelDoesNotExist => "channel does not exist",
            Protocol.BindListenerFailureResponse.Reasons.AlreadyBound => "it is already bound as a listener to it",
            _ => "the listener binding process was cancelled"
        };

        var channelIdText = channelId is null ? string.Empty : $"[{channelId.Value}] ";
        return
            $"Client [{clientId}] '{clientName}' could not be bound as a listener to channel {channelIdText}'{channelName}' because {reasonText}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetEndpoint<T>(T value)
        where T : struct, Enum
    {
        return Enum.IsDefined( value ) ? value.ToString() : $"<unrecognized-endpoint-{value}>";
    }
}
