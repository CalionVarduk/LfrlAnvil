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
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a set of attributes that defines a single message received from the server.
/// </summary>
public readonly struct MessageBrokerListenerCallbackArgs
{
    private readonly Int31BoolPair _retry;
    private readonly Int31BoolPair _redelivery;

    internal MessageBrokerListenerCallbackArgs(
        MessageBrokerListener listener,
        int ackId,
        ulong messageId,
        Timestamp pushedAt,
        Timestamp receivedAt,
        MessageBrokerExternalObject sender,
        MessageBrokerExternalObject stream,
        Int31BoolPair retry,
        Int31BoolPair redelivery,
        ReadOnlyMemory<byte> data,
        ulong traceId)
    {
        _retry = retry;
        _redelivery = redelivery;
        Listener = listener;
        AckId = ackId;
        MessageId = messageId;
        PushedAt = pushedAt;
        ReceivedAt = receivedAt;
        Sender = sender;
        Stream = stream;
        Data = data;
        TraceId = traceId;
    }

    /// <summary>
    /// <see cref="MessageBrokerListener"/> that received this message.
    /// </summary>
    public MessageBrokerListener Listener { get; }

    /// <summary>
    /// Id of the ACK associated with the message.
    /// </summary>
    /// <remarks>ACK is expected only when value is greater than <b>0</b>.</remarks>
    public int AckId { get; }

    /// <summary>
    /// Message's unique identifier assigned by the server.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Moment of registration of this message in the server-side stream.
    /// </summary>
    public Timestamp PushedAt { get; }

    /// <summary>
    /// Moment when the client has received this message.
    /// </summary>
    public Timestamp ReceivedAt { get; }

    /// <summary>
    /// Client that published this message.
    /// </summary>
    public MessageBrokerExternalObject Sender { get; }

    /// <summary>
    /// Server-side stream that handled this message.
    /// </summary>
    public MessageBrokerExternalObject Stream { get; }

    /// <summary>
    /// Binary data of this message.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <summary>
    /// Identifier of an internal event trace, with which this message is correlated to.
    /// </summary>
    /// <remarks>Can be used to correlate this message with events emitted by <see cref="MessageBrokerClient"/>'s logger.</remarks>
    public ulong TraceId { get; }

    /// <summary>
    /// Retry attempt of this message.
    /// </summary>
    /// <remarks>Retries are initiated by sending NACK response to the server.</remarks>
    public int Retry => _retry.IntValue;

    /// <summary>
    /// Specifies whether or not this message is a retry.
    /// </summary>
    public bool IsRetry => _retry.BoolValue;

    /// <summary>
    /// Redelivery attempt of this message.
    /// </summary>
    /// <remarks>
    /// Redeliveries are initiated automatically by the server
    /// when clients fail to respond with either ACK or negative ACK in time.
    /// </remarks>
    public int Redelivery => _redelivery.IntValue;

    /// <summary>
    /// Specifies whether or not this message is a redelivery.
    /// </summary>
    public bool IsRedelivery => _redelivery.BoolValue;

    /// <summary>
    /// Specifies whether or not this message is not a retry and not a redelivery.
    /// </summary>
    public bool IsFirst => _retry.Data == 0 && _redelivery.Data == 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerListenerCallbackArgs"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var ackId = AckId > 0 ? $", AckId = {AckId}" : string.Empty;
        var isRetry = IsRetry ? " (active)" : string.Empty;
        var isRedelivery = IsRedelivery ? " (active)" : string.Empty;
        return
            $"Listener = ({Listener}), Stream = ({Stream}){ackId}, Id = {MessageId}, Retry = {Retry}{isRetry}, Redelivery = {Redelivery}{isRedelivery}, Length = {Data.Length}, PushedAt = {PushedAt}, ReceivedAt = {ReceivedAt}, Sender = ({Sender}), TraceId = {TraceId}";
    }

    /// <summary>
    /// Attempts to send a message notification ACK for this message.
    /// </summary>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <exception cref="MessageBrokerClientMessageException">When ACKs are not enabled for the <see cref="Listener"/>.</exception>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="bool"/> result. If the result is equal to <b>true</b>, then ACK sending was successful,
    /// otherwise the <see cref="Listener"/> was no longer bound to the channel.
    /// </returns>
    /// <remarks>
    /// Unexpected errors encountered during ACK sending attempt will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the client has successfully sent the ACK to the server,
    /// or the <see cref="Listener"/> is already locally ubound from the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<bool>> AckAsync()
    {
        return ListenerCollection.SendMessageAckAsync( Listener, AckId, Stream.Id, MessageId, Retry, Redelivery, TraceId );
    }

    /// <summary>
    /// Attempts to send a negative message notification ACK for this message.
    /// </summary>
    /// <param name="nack">
    /// Optional <see cref="MessageBrokerNegativeAck"/> instance that allows to modify the ACK.
    /// Equal to <see cref="MessageBrokerNegativeAck.Default"/> by default.
    /// </param>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <exception cref="MessageBrokerClientMessageException">When ACKs are not enabled for the <see cref="Listener"/>.</exception>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="bool"/> result. If the result is equal to <b>true</b>, then ACK sending was successful,
    /// otherwise the <see cref="Listener"/> was no longer bound to the channel.
    /// </returns>
    /// <remarks>
    /// Unexpected errors encountered during ACK sending attempt will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the client has successfully sent the ACK to the server,
    /// or the <see cref="Listener"/> is already locally ubound from the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<bool>> NegativeAckAsync(MessageBrokerNegativeAck nack = default)
    {
        return ListenerCollection.SendNegativeMessageAckAsync(
            Listener,
            AckId,
            Stream.Id,
            MessageId,
            Retry,
            Redelivery,
            TraceId,
            nack,
            false );
    }
}
