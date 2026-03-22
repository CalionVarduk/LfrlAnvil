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
/// Represents an acquired read lock upgraded to the write level from an <see cref="AsyncKeyedReaderWriterLock{TKey}"/> instance.
/// </summary>
/// <typeparam name="TKey">Key's type.</typeparam>
public readonly struct AsyncKeyedReaderWriterLockUpgradedReadToken<TKey> : IDisposable
    where TKey : notnull
{
    private readonly AsyncKeyedReaderWriterLock<TKey>.Entry? _entry;
    private readonly AsyncReaderWriterLockUpgradedReadToken _token;

    internal AsyncKeyedReaderWriterLockUpgradedReadToken(
        AsyncKeyedReaderWriterLock<TKey>.Entry entry,
        AsyncReaderWriterLockUpgradedReadToken token)
    {
        _entry = entry;
        _token = token;
    }

    /// <summary>
    /// Associated <see cref="AsyncKeyedReaderWriterLock{TKey}"/> instance.
    /// </summary>
    public AsyncKeyedReaderWriterLock<TKey>? Lock => _entry?.KeyedLock;

    /// <summary>
    /// Associated key.
    /// </summary>
    public TKey? Key => _entry is not null ? _entry.Key : default;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">When this upgraded read lock has already been downgraded.</exception>
    public void Dispose()
    {
        if ( _entry is null )
            return;

        Assume.IsNotNull( _token.Entry );
        _token.Entry.ExitUpgradedRead( _token.Version );
    }

    /// <summary>
    /// Creates an upgradeable read lock token associated with this upgraded read lock.
    /// </summary>
    /// <returns>New <see cref="AsyncKeyedReaderWriterLockUpgradeableReadToken{TKey}"/> instance.</returns>
    [Pure]
    public AsyncKeyedReaderWriterLockUpgradeableReadToken<TKey> GetReadToken()
    {
        return _entry is not null ? new AsyncKeyedReaderWriterLockUpgradeableReadToken<TKey>( _entry, _token.GetReadToken() ) : default;
    }
}
