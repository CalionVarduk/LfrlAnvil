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

internal readonly struct MessageRouting
{
    internal static MessageRouting Empty => new MessageRouting( MemoryPoolToken<byte>.Empty, ReadOnlyMemory<byte>.Empty, 0 );

    internal readonly MemoryPoolToken<byte> PoolToken;
    internal readonly ReadOnlyMemory<byte> Data;
    internal readonly ulong TraceId;

    internal MessageRouting(MemoryPoolToken<byte> poolToken, ReadOnlyMemory<byte> data, ulong traceId)
    {
        PoolToken = poolToken;
        Data = data;
        TraceId = traceId;
    }

    internal bool IsActive => Data.Length > 0;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool Contains(ReadOnlySpan<byte> data, int id)
    {
        Assume.IsGreaterThan( id, 0 );
        var index = --id >> 3;
        return index < data.Length && ((data[index] >> (id & 7)) & 1) != 0;
    }

    [Pure]
    public override string ToString()
    {
        return $"TraceId = {TraceId}, DataLength = {Data.Length}";
    }

    internal readonly struct Result
    {
        internal readonly MessageRouting Value;
        internal readonly int FoundCount;

        private Result(MemoryPoolToken<byte> poolToken, ReadOnlyMemory<byte> data, ulong traceId, int foundCount)
        {
            Value = new MessageRouting( poolToken, data, traceId );
            FoundCount = foundCount;
        }

        internal bool IsValid => FoundCount >= 0;

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Result CreateInvalid(MemoryPoolToken<byte> poolToken)
        {
            return new Result( poolToken, ReadOnlyMemory<byte>.Empty, 0, -1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Result Create(MemoryPoolToken<byte> poolToken, ReadOnlyMemory<byte> data, ulong traceId, int foundCount)
        {
            Assume.IsGreaterThanOrEqualTo( foundCount, 0 );
            return new Result( poolToken, data, traceId, foundCount );
        }

        [Pure]
        public override string ToString()
        {
            return $"Value = ({Value}), FoundCount = {FoundCount}, IsValid = {IsValid}";
        }
    }
}
