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
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents an acquired upgradeable read lock from an <see cref="AsyncReaderWriterLock"/> instance.
/// </summary>
public readonly struct AsyncReaderWriterLockUpgradeableReadToken : IDisposable
{
    internal readonly AsyncReaderWriterLock.Entry? Entry;
    internal readonly ulong Version;

    internal AsyncReaderWriterLockUpgradeableReadToken(AsyncReaderWriterLock.Entry entry, ulong version)
    {
        Entry = entry;
        Version = version;
    }

    /// <summary>
    /// Associated <see cref="AsyncReaderWriterLock"/> instance.
    /// </summary>
    public AsyncReaderWriterLock? Lock => Entry?.Lock;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// When this upgradeable read lock is in the process of being upgraded to a write lock, or has already been upgraded.
    /// </exception>
    public void Dispose()
    {
        Entry?.ExitUpgradeableRead( Version );
    }

    /// <summary>
    /// Asynchronously upgrades this upgradeable read lock to a write lock.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional <see cref="CancellationToken"/> that can be used to cancel pending upgrade.
    /// </param>
    /// <returns>
    /// New <see cref="ValueTask{TResult}"/> instance which returns an <see cref="AsyncReaderWriterLockUpgradedReadToken"/> value.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <paramref name="cancellationToken"/> was cancelled before the upgrade was completed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// When this upgradeable read lock has been released, or it's already in the process of being upgraded, or has already been upgraded.
    /// </exception>
    public ValueTask<AsyncReaderWriterLockUpgradedReadToken> UpgradeAsync(CancellationToken cancellationToken = default)
    {
        if ( Entry is null )
            ExceptionThrower.Throw( new InvalidOperationException( ExceptionResources.CannotUpgradeDisposedReaderWriterReadLock ) );

        return Entry.Lock.UpgradeReadAsync( Entry, Version, cancellationToken );
    }

    /// <summary>
    /// Attempts to synchronously upgrade this lock to a write lock.
    /// </summary>
    /// <param name="entered"><b>out</b> parameter which specifies whether the upgrade was completed.</param>
    /// <returns>
    /// New <see cref="AsyncReaderWriterLockUpgradedReadToken"/> value. When <paramref name="entered"/> is <b>false</b>,
    /// then returned instanced will be a default value.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// When this upgradeable read lock has been released, or it's already in the process of being upgraded, or has already been upgraded.
    /// </exception>
    public AsyncReaderWriterLockUpgradedReadToken TryUpgrade(out bool entered)
    {
        if ( Entry is null )
            ExceptionThrower.Throw( new InvalidOperationException( ExceptionResources.CannotUpgradeDisposedReaderWriterReadLock ) );

        return Entry.Lock.TryUpgradeRead( Entry, Version, out entered );
    }
}
