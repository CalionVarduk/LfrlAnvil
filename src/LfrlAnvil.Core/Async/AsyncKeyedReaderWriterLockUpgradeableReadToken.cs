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
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents an acquired upgradeable read lock from an <see cref="AsyncKeyedReaderWriterLock{TKey}"/> instance.
/// </summary>
/// <typeparam name="TKey">Key's type.</typeparam>
public readonly struct AsyncKeyedReaderWriterLockUpgradeableReadToken<TKey> : IDisposable
    where TKey : notnull
{
    private readonly AsyncKeyedReaderWriterLock<TKey>.Entry? _entry;
    private readonly AsyncReaderWriterLockUpgradeableReadToken _token;

    internal AsyncKeyedReaderWriterLockUpgradeableReadToken(
        AsyncKeyedReaderWriterLock<TKey>.Entry entry,
        AsyncReaderWriterLockUpgradeableReadToken token)
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
    /// <exception cref="InvalidOperationException">
    /// When this upgradeable read lock is in the process of being upgraded to a write lock, or has already been upgraded.
    /// </exception>
    public void Dispose()
    {
        if ( _entry is null )
            return;

        Assume.IsNotNull( _token.Entry );
        var released = false;
        try
        {
            released = _token.Entry.ExitUpgradeableRead( _token.Version );
        }
        finally
        {
            if ( released )
                _entry.Exit();
        }
    }

    /// <summary>
    /// Asynchronously upgrades this upgradeable read lock to a write lock.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional <see cref="CancellationToken"/> that can be used to cancel pending upgrade.
    /// </param>
    /// <returns>
    /// New <see cref="ValueTask{TResult}"/> instance
    /// which returns an <see cref="AsyncKeyedReaderWriterLockUpgradedReadToken{TKey}"/> value.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <paramref name="cancellationToken"/> was cancelled before the upgrade was completed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// When this upgradeable read lock has been released, or it's already in the process of being upgraded, or has already been upgraded.
    /// </exception>
    public async ValueTask<AsyncKeyedReaderWriterLockUpgradedReadToken<TKey>> UpgradeAsync(CancellationToken cancellationToken = default)
    {
        var token = await _token.UpgradeAsync( cancellationToken );
        Assume.IsNotNull( _entry );
        return new AsyncKeyedReaderWriterLockUpgradedReadToken<TKey>( _entry, token );
    }

    /// <summary>
    /// Attempts to synchronously upgrade this lock to a write lock.
    /// </summary>
    /// <param name="entered"><b>out</b> parameter which specifies whether the upgrade was completed.</param>
    /// <returns>
    /// New <see cref="AsyncKeyedReaderWriterLockUpgradedReadToken{TKey}"/> value. When <paramref name="entered"/> is <b>false</b>,
    /// then returned instanced will be a default value.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// When this upgradeable read lock has been released, or it's already in the process of being upgraded, or has already been upgraded.
    /// </exception>
    public AsyncKeyedReaderWriterLockUpgradedReadToken<TKey> TryUpgrade(out bool entered)
    {
        var token = _token.TryUpgrade( out entered );
        if ( ! entered )
            return default;

        Assume.IsNotNull( _entry );
        return new AsyncKeyedReaderWriterLockUpgradedReadToken<TKey>( _entry, token );
    }
}
