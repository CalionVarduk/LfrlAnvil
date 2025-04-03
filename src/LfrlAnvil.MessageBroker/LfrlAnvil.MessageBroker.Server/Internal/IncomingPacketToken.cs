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
using LfrlAnvil.Memory;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal readonly struct IncomingPacketToken
{
    internal enum Result : byte
    {
        Disposed = 0,
        TimedOut = 1,
        Ok = 2
    }

    internal IncomingPacketToken(Protocol.PacketHeader header, MemoryPoolToken<byte> poolToken, Memory<byte> data, Result type)
    {
        Header = header;
        PoolToken = poolToken;
        Data = data;
        Type = type;
    }

    internal readonly Protocol.PacketHeader Header;
    internal readonly MemoryPoolToken<byte> PoolToken;
    internal readonly Memory<byte> Data;
    internal readonly Result Type;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IncomingPacketToken TimedOut()
    {
        return new IncomingPacketToken( default, default, default, Result.TimedOut );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IncomingPacketToken Ok(Protocol.PacketHeader header, MemoryPoolToken<byte> poolToken, Memory<byte> data)
    {
        return new IncomingPacketToken( header, poolToken, data, Result.Ok );
    }

    [Pure]
    public override string ToString()
    {
        return Type == Result.Disposed ? "<DISPOSED>" : $"[{Type}] Header = ({Header}), DataLength = {Data.Length}";
    }
}
