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
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal static class PoolExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MemoryPoolToken<byte> Rent(this MemoryPool<byte> pool, int length, out Memory<byte> data)
    {
        using ( pool.AcquireLock() )
        {
            var token = pool.Rent( length );
            data = token.AsMemory();
            return token;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void SetLength(this MemoryPoolToken<byte> token, int length, out Memory<byte> data)
    {
        using ( token.AcquireLock() )
        {
            token.SetLength( length );
            data = token.AsMemory();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void Return(this MemoryPoolToken<byte> token, MessageBrokerClient client)
    {
        Exception? exception;
        using ( token.AcquireLock() )
            exception = token.TryDispose().Exception;

        if ( exception is not null )
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exception ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireLock(this MemoryPool<byte> pool)
    {
        return ExclusiveLock.SpinWaitEnter( pool, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireLock(this MemoryPoolToken<byte> token)
    {
        return token.Owner is not null ? token.Owner.AcquireLock() : default;
    }
}
