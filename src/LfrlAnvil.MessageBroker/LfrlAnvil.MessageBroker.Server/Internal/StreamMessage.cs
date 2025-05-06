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
using LfrlAnvil.Chrono;
using LfrlAnvil.Memory;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal readonly struct StreamMessage
{
    private readonly MemoryPoolToken<byte> _poolToken;

    internal StreamMessage(
        ulong id,
        Timestamp timestamp,
        MessageBrokerChannelBinding binding,
        MemoryPoolToken<byte> poolToken,
        ReadOnlyMemory<byte> data)
    {
        Id = id;
        Timestamp = timestamp;
        Binding = binding;
        _poolToken = poolToken;
        Data = data;
    }

    internal readonly ulong Id;
    internal readonly Timestamp Timestamp;
    internal readonly MessageBrokerChannelBinding Binding;
    internal readonly ReadOnlyMemory<byte> Data;
    internal bool BufferClearEnabled => _poolToken.Clear;

    [Pure]
    public override string ToString()
    {
        return $"Id = {Id}, Timestamp = {Timestamp}, Length = {Data.Length}, Binding = ({Binding})";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Return()
    {
        _poolToken.Return( Binding.Client );
    }
}
