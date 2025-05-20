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

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents a collection of event callbacks for events emitted by a <see cref="MessageBrokerServer"/>.
/// </summary>
public readonly struct MessageBrokerServerLogger
{
    private MessageBrokerServerLogger(
        Action<MessageBrokerServerTraceEvent>? traceStart,
        Action<MessageBrokerServerTraceEvent>? traceEnd,
        Action<MessageBrokerServerListenerStartingEvent>? listenerStarting,
        Action<MessageBrokerServerListenerStartedEvent>? listenerStarted,
        Action<MessageBrokerServerAwaitClientEvent>? awaitClient,
        Action<MessageBrokerServerClientAcceptedEvent>? clientAccepted,
        Action<MessageBrokerServerDisposingEvent>? disposing,
        Action<MessageBrokerServerDisposedEvent>? disposed,
        Action<MessageBrokerServerErrorEvent>? error)
    {
        TraceStart = traceStart;
        TraceEnd = traceEnd;
        ListenerStarting = listenerStarting;
        ListenerStarted = listenerStarted;
        AwaitClient = awaitClient;
        ClientAccepted = clientAccepted;
        Disposing = disposing;
        Disposed = disposed;
        Error = error;
    }

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerServerTraceEvent"/> emitted during operation trace start.
    /// </summary>
    public readonly Action<MessageBrokerServerTraceEvent>? TraceStart;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerServerTraceEvent"/> emitted during operation trace end.
    /// </summary>
    public readonly Action<MessageBrokerServerTraceEvent>? TraceEnd;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerServerListenerStartingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerServerListenerStartingEvent>? ListenerStarting;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerServerListenerStartedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerServerListenerStartedEvent>? ListenerStarted;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerServerAwaitClientEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerServerAwaitClientEvent>? AwaitClient;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerServerClientAcceptedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerServerClientAcceptedEvent>? ClientAccepted;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerServerDisposingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerServerDisposingEvent>? Disposing;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerServerDisposedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerServerDisposedEvent>? Disposed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerServerErrorEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerServerErrorEvent>? Error;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerServerLogger"/> instance.
    /// </summary>
    /// <param name="traceStart">Optional <see cref="TraceStart"/> callback.</param>
    /// <param name="traceEnd">Optional <see cref="TraceEnd"/> callback.</param>
    /// <param name="listenerStarting">Optional <see cref="ListenerStarting"/> callback.</param>
    /// <param name="listenerStarted">Optional <see cref="ListenerStarted"/> callback.</param>
    /// <param name="awaitClient">Optional <see cref="AwaitClient"/> callback.</param>
    /// <param name="clientAccepted">Optional <see cref="ClientAccepted"/> callback.</param>
    /// <param name="disposing">Optional <see cref="Disposing"/> callback.</param>
    /// <param name="disposed">Optional <see cref="Disposed"/> callback.</param>
    /// <param name="error">Optional <see cref="Error"/> callback.</param>
    /// <returns>New <see cref="MessageBrokerServerLogger"/> instance.</returns>
    [Pure]
    public static MessageBrokerServerLogger Create(
        Action<MessageBrokerServerTraceEvent>? traceStart = null,
        Action<MessageBrokerServerTraceEvent>? traceEnd = null,
        Action<MessageBrokerServerListenerStartingEvent>? listenerStarting = null,
        Action<MessageBrokerServerListenerStartedEvent>? listenerStarted = null,
        Action<MessageBrokerServerAwaitClientEvent>? awaitClient = null,
        Action<MessageBrokerServerClientAcceptedEvent>? clientAccepted = null,
        Action<MessageBrokerServerDisposingEvent>? disposing = null,
        Action<MessageBrokerServerDisposedEvent>? disposed = null,
        Action<MessageBrokerServerErrorEvent>? error = null)
    {
        return new MessageBrokerServerLogger(
            traceStart,
            traceEnd,
            listenerStarting,
            listenerStarted,
            awaitClient,
            clientAccepted,
            disposing,
            disposed,
            error );
    }
}
