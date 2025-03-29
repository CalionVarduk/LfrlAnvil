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
using LfrlAnvil.MessageBroker.Server.Buffering;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal readonly struct EnqueuedMessage
{
    internal EnqueuedMessage(ulong id, MessageBrokerChannelBinding binding, BinaryBufferToken bufferToken, ReadOnlyMemory<byte> data)
    {
        Id = id;
        Binding = binding;
        BufferToken = bufferToken;
        Data = data;
    }

    internal readonly ulong Id;
    internal readonly MessageBrokerChannelBinding Binding;
    internal readonly BinaryBufferToken BufferToken;
    internal readonly ReadOnlyMemory<byte> Data;

    [Pure]
    public override string ToString()
    {
        return $"Id = {Id}, Length = {Data.Length}, Binding = ({Binding})";
    }
}
