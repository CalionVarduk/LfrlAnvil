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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents an object capable of finalizing the process of pushing previously enqueued message to the server.
/// </summary>
public readonly struct MessageBrokerPushMessageFinalizer
{
    private readonly MessageBrokerPushContext? _context;
    private readonly ManualResetValueTaskSource<WriterSourceResult>? _routingWriterSource;
    private readonly ManualResetValueTaskSource<WriterSourceResult>? _writerSource;
    private readonly ManualResetValueTaskSource<IncomingPacketToken>? _responseSource;
    private readonly MessageBrokerPushResult? _result;
    private readonly bool _reverseEndianness;

    private MessageBrokerPushMessageFinalizer(
        MessageBrokerPushContext context,
        ManualResetValueTaskSource<WriterSourceResult>? routingWriterSource,
        ManualResetValueTaskSource<WriterSourceResult>? writerSource,
        ManualResetValueTaskSource<IncomingPacketToken>? responseSource,
        MessageBrokerPushResult? result,
        bool reverseEndianness,
        ulong traceId)
    {
        _context = context;
        _routingWriterSource = routingWriterSource;
        _writerSource = writerSource;
        _responseSource = responseSource;
        _result = result;
        _reverseEndianness = reverseEndianness;
        TraceId = traceId;
    }

    /// <summary>
    /// Identifier of an internal trace.
    /// </summary>
    public ulong TraceId { get; }

    /// <summary>
    /// <see cref="MessageBrokerPushContext"/> that created this finalizer.
    /// </summary>
    public MessageBrokerPushContext Context
    {
        get
        {
            Ensure.IsNotNull( _context );
            return _context;
        }
    }

    /// <summary>
    /// Pushes the enqueued message to the publisher's bound channel.
    /// </summary>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerPushResult"/> instance.
    /// </returns>
    /// <exception cref="ObjectDisposedException">When <see cref="Context"/> has been disposed.</exception>
    /// <remarks>
    /// Unexpected errors encountered during pushing will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the message has been successfully enqueued on the server side,
    /// or the publisher is already locally unbound from the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<MessageBrokerPushResult>> PushAsync()
    {
        if ( _result is not null )
            return ValueTask.FromResult( Result.Create( _result.Value ) );

        var context = Context;
        Assume.IsNotNull( _writerSource );
        return PublisherCollection.PushAsync(
            context,
            _routingWriterSource,
            _writerSource,
            _responseSource,
            _reverseEndianness,
            TraceId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerPushMessageFinalizer Create(
        MessageBrokerPushContext context,
        ManualResetValueTaskSource<WriterSourceResult>? routingWriterSource,
        ManualResetValueTaskSource<WriterSourceResult> writerSource,
        ManualResetValueTaskSource<IncomingPacketToken>? responseSource,
        bool reverseEndianness,
        ulong traceId)
    {
        return new MessageBrokerPushMessageFinalizer(
            context,
            routingWriterSource,
            writerSource,
            responseSource,
            null,
            reverseEndianness,
            traceId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerPushMessageFinalizer CreateNotBound(MessageBrokerPushContext context, bool confirm)
    {
        return new MessageBrokerPushMessageFinalizer(
            context,
            null,
            null,
            null,
            MessageBrokerPushResult.CreateNotBound( confirm ),
            false,
            0 );
    }
}
