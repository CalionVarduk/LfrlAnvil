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
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal static class MessageBrokerExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void Emit<T>(this Action<T> emitter, T @event)
        where T : struct
    {
        try
        {
            emitter( @event );
        }
        catch
        {
            // NOTE: do nothing
        }
    }

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
    internal static void SetLength(this MemoryPoolToken<byte> token, int length, out Memory<byte> data, bool trimStart = false)
    {
        using ( token.AcquireLock() )
        {
            token.SetLength( length, trimStart );
            data = token.AsMemory();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Exception? Return(this MemoryPoolToken<byte> token)
    {
        using ( token.AcquireLock() )
            return token.TryDispose().Exception;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void Return(this MemoryPoolToken<byte> token, MessageBrokerRemoteClient client, ulong traceId)
    {
        var exception = token.Return();
        if ( exception is not null && client.Logger.Error is { } error )
            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exception ) );
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
