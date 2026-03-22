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

using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents an acquired read lock upgraded to the write level from an <see cref="AsyncReaderWriterLock"/> instance.
/// </summary>
public readonly struct AsyncReaderWriterLockUpgradedReadToken : IDisposable
{
    internal readonly AsyncReaderWriterLock.Entry? Entry;
    internal readonly ulong Version;

    internal AsyncReaderWriterLockUpgradedReadToken(AsyncReaderWriterLock.Entry entry, ulong version)
    {
        Entry = entry;
        Version = version;
    }

    /// <summary>
    /// Associated <see cref="AsyncReaderWriterLock"/> instance.
    /// </summary>
    public AsyncReaderWriterLock? Lock => Entry?.Lock;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">When this upgraded read lock has already been downgraded.</exception>
    public void Dispose()
    {
        Entry?.ExitUpgradedRead( Version );
    }

    /// <summary>
    /// Creates an upgradeable read lock token associated with this upgraded read lock.
    /// </summary>
    /// <returns>New <see cref="AsyncReaderWriterLockUpgradeableReadToken"/> instance.</returns>
    [Pure]
    public AsyncReaderWriterLockUpgradeableReadToken GetReadToken()
    {
        return Entry is not null ? new AsyncReaderWriterLockUpgradeableReadToken( Entry, Version ) : default;
    }
}
