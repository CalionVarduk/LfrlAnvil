// Copyright 2026 Łukasz Furlepa
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

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

/// <summary>
/// Represents an exception thrown when message broker server storage contains invalid data.
/// </summary>
public class MessageBrokerServerStorageException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerServerProtocolException"/> instance.
    /// </summary>
    /// <param name="server"><see cref="MessageBrokerServer"/> that encountered this storage error.</param>
    /// <param name="filePath">Path to the file which contains invalid data.</param>
    /// <param name="errors">Collection of storage errors.</param>
    public MessageBrokerServerStorageException(MessageBrokerServer server, string filePath, Chain<string> errors)
        : base( Resources.InvalidStorage( filePath, errors ) )
    {
        Server = server;
        FilePath = filePath;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> that encountered this storage error.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Path to the file which contains invalid data.
    /// </summary>
    public string FilePath { get; }
}
