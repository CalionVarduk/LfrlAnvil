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

using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight, disposable object representing an acquired spin lock.
/// </summary>
public ref struct SpinLockEntry
{
    // TODO: implement IDisposable
    private ref SpinLock _lock;
    private readonly bool _taken;

    private SpinLockEntry(ref SpinLock @lock, bool taken)
    {
        _lock = ref @lock;
        _taken = taken;
    }

    /// <summary>
    /// Acquires a spin lock and creates a new <see cref="SpinLockEntry"/>.
    /// </summary>
    /// <param name="lock">Spin lock to acquire.</param>
    /// <returns>A disposable <see cref="SpinLockEntry"/> instance.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SpinLockEntry Enter(ref SpinLock @lock)
    {
        var taken = false;
        @lock.Enter( ref taken );
        return new SpinLockEntry( ref @lock, taken );
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    /// <remarks>Releases previously acquired spin lock.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        if ( _taken )
            _lock.Exit();
    }
}
