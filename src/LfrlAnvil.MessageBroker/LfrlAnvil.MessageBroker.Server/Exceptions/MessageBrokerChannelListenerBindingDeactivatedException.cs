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
using System.Threading;

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

/// <summary>
/// Represents an exception thrown when <see cref="MessageBrokerChannelListenerBinding"/> is in a deactivated state.
/// </summary>
public class MessageBrokerChannelListenerBindingDeactivatedException : OperationCanceledException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerChannelListenerBindingDeactivatedException"/> instance.
    /// </summary>
    /// <param name="listener">Deactivated <see cref="MessageBrokerChannelListenerBinding"/>.</param>
    /// <param name="disposed">Specifies whether the publisher has been disposed.</param>
    public MessageBrokerChannelListenerBindingDeactivatedException(MessageBrokerChannelListenerBinding listener, bool disposed)
        : base( Resources.ListenerDeactivated( listener.Client, listener.Channel, disposed ), new CancellationToken( canceled: true ) )
    {
        Listener = listener;
        Disposed = disposed;
    }

    /// <summary>
    /// Deactivated <see cref="MessageBrokerChannelListenerBinding"/>.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// Specifies whether the <see cref="Listener"/> has been disposed.
    /// </summary>
    public bool Disposed { get; }
}
