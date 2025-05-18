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
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a set of attributes that defines a single message received from the server.
/// </summary>
public readonly struct MessageBrokerListenerCallbackArgs
{
    internal MessageBrokerListenerCallbackArgs(
        MessageBrokerListener listener,
        ulong messageId,
        Timestamp enqueuedAt,
        Timestamp receivedAt,
        int senderId,
        int streamId,
        int retryAttempt,
        int redeliveryAttempt,
        ReadOnlyMemory<byte> data,
        ulong traceId)
    {
        Listener = listener;
        MessageId = messageId;
        EnqueuedAt = enqueuedAt;
        ReceivedAt = receivedAt;
        SenderId = senderId;
        StreamId = streamId;
        RetryAttempt = retryAttempt;
        RedeliveryAttempt = redeliveryAttempt;
        Data = data;
        TraceId = traceId;
    }

    /// <summary>
    /// <see cref="MessageBrokerListener"/> that received this message.
    /// </summary>
    public MessageBrokerListener Listener { get; }

    /// <summary>
    /// Message's unique identifier assigned by the server.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Moment of registration of this message in the server-side stream.
    /// </summary>
    public Timestamp EnqueuedAt { get; }

    /// <summary>
    /// Moment when the client has received this message.
    /// </summary>
    public Timestamp ReceivedAt { get; }

    /// <summary>
    /// Identifier of the client that published this message.
    /// </summary>
    public int SenderId { get; }

    /// <summary>
    /// Identifier of the server-side stream that handled this message.
    /// </summary>
    public int StreamId { get; }

    /// <summary>
    /// Retry attempt number of this message.
    /// </summary>
    /// <remarks>Retries are initiated by sending NACK response to the server.</remarks>
    public int RetryAttempt { get; }

    /// <summary>
    /// Redelivery attempt number of this message.
    /// </summary>
    /// <remarks>
    /// Redeliveries are initiated automatically by the server
    /// when clients fail to respond with either ACK or NACK after enough time has passed.
    /// </remarks>
    public int RedeliveryAttempt { get; }

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
    /// Returns a string representation of this <see cref="MessageBrokerListenerCallbackArgs"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"Listener = ({Listener}), Id = {MessageId}, Retry = {RetryAttempt}, Redelivery = {RedeliveryAttempt}, Length = {Data.Length}, EnqueuedAt = {EnqueuedAt}, ReceivedAt = {ReceivedAt}, Sender = {SenderId}, Stream = {StreamId}, TraceId = {TraceId}";
    }
}
