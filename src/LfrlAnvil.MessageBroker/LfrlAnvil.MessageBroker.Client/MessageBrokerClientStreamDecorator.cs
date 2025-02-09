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

using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a callback which allows to decorate <see cref="MessageBrokerClient"/>'s network stream.
/// <param name="client">Owner of the <paramref name="stream"/>.</param>
/// <param name="stream"><see cref="System.Net.Sockets.NetworkStream"/> to decorate.</param>
/// <param name="cancellationToken"><see cref="CancellationToken"/> instance.</param>
/// <returns>A <see cref="ValueTask{TResult}"/> which returns a stream, that will be used by the <paramref name="client"/>.</returns>
/// </summary>
public delegate ValueTask<Stream> MessageBrokerClientStreamDecorator(
    MessageBrokerClient client,
    NetworkStream stream,
    CancellationToken cancellationToken);
