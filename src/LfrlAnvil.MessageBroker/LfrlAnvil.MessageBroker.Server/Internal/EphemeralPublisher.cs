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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal sealed class EphemeralPublisher : IMessageBrokerMessagePublisher
{
    internal EphemeralPublisher(int clientId, string clientName, MessageBrokerChannel channel, MessageBrokerStream stream)
    {
        ClientId = clientId;
        ClientName = clientName;
        Channel = channel;
        Stream = stream;
    }

    public int ClientId { get; }
    public string ClientName { get; }
    public MessageBrokerChannel Channel { get; }
    public MessageBrokerStream Stream { get; }
    public bool IsClientEphemeral => true;
    public MessageBrokerRemoteClient? Client => null;

    [Pure]
    public override string ToString()
    {
        return
            $"[{ClientId}] '{ClientName}' => [{Channel.Id}] '{Channel.Name}' ephemeral publisher binding (using [{Stream.Id}] '{Stream.Name}' stream) ({MessageBrokerChannelPublisherBindingState.Disposed})";
    }

    internal struct Builder
    {
        internal readonly int ClientId;
        internal readonly string ClientName;
        internal readonly MessageBrokerStream Stream;
        private EphemeralPublisher? _publisher;

        internal Builder(int clientId, string clientName, MessageBrokerStream stream)
        {
            ClientId = clientId;
            ClientName = clientName;
            Stream = stream;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal EphemeralPublisher Build(MessageBrokerChannel channel)
        {
            _publisher ??= new EphemeralPublisher( ClientId, ClientName, channel, Stream );
            return _publisher;
        }
    }
}
