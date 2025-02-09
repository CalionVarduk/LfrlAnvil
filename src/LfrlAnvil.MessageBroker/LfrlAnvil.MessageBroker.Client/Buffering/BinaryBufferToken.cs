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
using LfrlAnvil.Async;
using LfrlAnvil.Memory;

namespace LfrlAnvil.MessageBroker.Client.Buffering;

internal readonly struct BinaryBufferToken : IDisposable
{
    private readonly MemoryPoolToken<byte> _token;

    internal BinaryBufferToken(MemoryPoolToken<byte> token)
    {
        _token = token;
    }

    public bool Clear => _token.Clear;

    public void Dispose()
    {
        using ( AcquireLock() )
            _token.Dispose();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public BinaryBufferToken EnableClearing(bool enabled = true)
    {
        return new BinaryBufferToken( _token.EnableClearing( enabled ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Memory<byte> SetLength(int length)
    {
        using ( AcquireLock() )
        {
            _token.SetLength( length );
            return _token.AsMemory();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return _token.Owner is not null ? ExclusiveLock.Enter( _token.Owner ) : default;
    }
}
