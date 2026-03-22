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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents a source of a keyed collection of fair asynchronous reader-writer locks.
/// </summary>
/// <typeparam name="TKey">Key's type.</typeparam>
/// <remarks>Lock is not reentrant.</remarks>
public sealed class AsyncKeyedReaderWriterLock<TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, Entry> _entries;
    private StackSlim<Entry> _entryCache;

    /// <summary>
    /// Creates a new <see cref="AsyncKeyedReaderWriterLock{TKey}"/> instance.
    /// </summary>
    /// <param name="keyComparer">Optional <typeparamref name="TKey"/> comparer. Equal to <b>null</b> by default.</param>
    public AsyncKeyedReaderWriterLock(IEqualityComparer<TKey>? keyComparer = null)
    {
        _entries = new Dictionary<TKey, Entry>( keyComparer );
        _entryCache = StackSlim<Entry>.Create();
    }

    /// <summary>
    /// Used key comparer.
    /// </summary>
    public IEqualityComparer<TKey> KeyComparer => _entries.Comparer;

    /// <summary>
    /// Returns the collection of all active keys.
    /// </summary>
    public TKey[] ActiveKeys
    {
        get
        {
            using ( AcquireLock() )
                return _entries.Select( static kv => kv.Key ).ToArray();
        }
    }

    /// <summary>
    /// Asynchronously acquires a read lock for the specified <paramref name="key"/> from this reader-writer lock.
    /// </summary>
    /// <param name="key">Key for which to acquire a read lock.</param>
    /// <param name="cancellationToken">
    /// Optional <see cref="CancellationToken"/> that can be used to cancel pending read lock acquisition.
    /// </param>
    /// <returns>
    /// New <see cref="ValueTask"/> instance which returns an <see cref="AsyncKeyedReaderWriterLockReadToken{TKey}"/> value.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <paramref name="cancellationToken"/> was cancelled before the read lock was acquired.
    /// </exception>
    public async ValueTask<AsyncKeyedReaderWriterLockReadToken<TKey>> EnterReadAsync(
        TKey key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Entry? entry;
        ValueTask<AsyncReaderWriterLockReadToken> lockTask;
        using ( AcquireLock() )
        {
            entry = GetOrAddEntry( key );
            lockTask = entry.Lock.EnterReadAsync( cancellationToken );
        }

        AsyncReaderWriterLockReadToken token;
        try
        {
            token = await lockTask.ConfigureAwait( false );
        }
        catch
        {
            using ( AcquireLock() )
                entry.ExitUnsafe();

            throw;
        }

        return new AsyncKeyedReaderWriterLockReadToken<TKey>( entry, token );
    }

    /// <summary>
    /// Attempts to synchronously acquire a read lock for the specified <paramref name="key"/> from this reader-writer lock.
    /// </summary>
    /// <param name="key">Key for which to acquire a read lock.</param>
    /// <param name="entered"><b>out</b> parameter which specifies whether the read lock was acquired.</param>
    /// <returns>
    /// New <see cref="AsyncKeyedReaderWriterLockReadToken{TKey}"/> value. When <paramref name="entered"/> is <b>false</b>,
    /// then returned instanced will be a default value.
    /// </returns>
    public AsyncKeyedReaderWriterLockReadToken<TKey> TryEnterRead(TKey key, out bool entered)
    {
        Entry? entry;
        AsyncReaderWriterLockReadToken token;
        using ( AcquireLock() )
        {
            entry = GetOrAddEntry( key );
            token = entry.Lock.TryEnterRead( out entered );
            if ( ! entered )
            {
                entry.ExitUnsafe();
                return default;
            }
        }

        return new AsyncKeyedReaderWriterLockReadToken<TKey>( entry, token );
    }

    /// <summary>
    /// Asynchronously acquires an upgradeable read lock for the specified <paramref name="key"/> from this reader-writer lock.
    /// </summary>
    /// <param name="key">Key for which to acquire an upgradeable lock.</param>
    /// <param name="cancellationToken">
    /// Optional <see cref="CancellationToken"/> that can be used to cancel pending upgradeable read lock acquisition.
    /// </param>
    /// <returns>
    /// New <see cref="ValueTask{TResult}"/> instance
    /// which returns an <see cref="AsyncKeyedReaderWriterLockUpgradeableReadToken{TKey}"/> value.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <paramref name="cancellationToken"/> was cancelled before the upgradeable read lock was acquired.
    /// </exception>
    public async ValueTask<AsyncKeyedReaderWriterLockUpgradeableReadToken<TKey>> EnterUpgradeableReadAsync(
        TKey key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Entry? entry;
        ValueTask<AsyncReaderWriterLockUpgradeableReadToken> lockTask;
        using ( AcquireLock() )
        {
            entry = GetOrAddEntry( key );
            lockTask = entry.Lock.EnterUpgradeableReadAsync( cancellationToken );
        }

        AsyncReaderWriterLockUpgradeableReadToken token;
        try
        {
            token = await lockTask.ConfigureAwait( false );
        }
        catch
        {
            using ( AcquireLock() )
                entry.ExitUnsafe();

            throw;
        }

        return new AsyncKeyedReaderWriterLockUpgradeableReadToken<TKey>( entry, token );
    }

    /// <summary>
    /// Attempts to synchronously acquire an upgradeable read lock for the specified <paramref name="key"/> from this reader-writer lock.
    /// </summary>
    /// <param name="key">Key for which to acquire an upgradeable lock.</param>
    /// <param name="entered"><b>out</b> parameter which specifies whether the upgradeable read lock was acquired.</param>
    /// <returns>
    /// New <see cref="AsyncKeyedReaderWriterLockUpgradeableReadToken{TKey}"/> value. When <paramref name="entered"/> is <b>false</b>,
    /// then returned instanced will be a default value.
    /// </returns>
    public AsyncKeyedReaderWriterLockUpgradeableReadToken<TKey> TryEnterUpgradeableRead(TKey key, out bool entered)
    {
        Entry? entry;
        AsyncReaderWriterLockUpgradeableReadToken token;
        using ( AcquireLock() )
        {
            entry = GetOrAddEntry( key );
            token = entry.Lock.TryEnterUpgradeableRead( out entered );
            if ( ! entered )
            {
                entry.ExitUnsafe();
                return default;
            }
        }

        return new AsyncKeyedReaderWriterLockUpgradeableReadToken<TKey>( entry, token );
    }

    /// <summary>
    /// Asynchronously acquires a write lock for the specified <paramref name="key"/> from this reader-writer lock.
    /// </summary>
    /// <param name="key">Key for which to acquire a write lock.</param>
    /// <param name="cancellationToken">
    /// Optional <see cref="CancellationToken"/> that can be used to cancel pending write lock acquisition.
    /// </param>
    /// <returns>
    /// New <see cref="ValueTask{TResult}"/> instance which returns an <see cref="AsyncKeyedReaderWriterLockWriteToken{TKey}"/> value.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <paramref name="cancellationToken"/> was cancelled before the write lock was acquired.
    /// </exception>
    public async ValueTask<AsyncKeyedReaderWriterLockWriteToken<TKey>> EnterWriteAsync(
        TKey key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Entry? entry;
        ValueTask<AsyncReaderWriterLockWriteToken> lockTask;
        using ( AcquireLock() )
        {
            entry = GetOrAddEntry( key );
            lockTask = entry.Lock.EnterWriteAsync( cancellationToken );
        }

        AsyncReaderWriterLockWriteToken token;
        try
        {
            token = await lockTask.ConfigureAwait( false );
        }
        catch
        {
            using ( AcquireLock() )
                entry.ExitUnsafe();

            throw;
        }

        return new AsyncKeyedReaderWriterLockWriteToken<TKey>( entry, token );
    }

    /// <summary>
    /// Attempts to synchronously acquire a write lock for the specified <paramref name="key"/> from this reader-writer lock.
    /// </summary>
    /// <param name="key">Key for which to acquire a write lock.</param>
    /// <param name="entered"><b>out</b> parameter which specifies whether the write lock was acquired.</param>
    /// <returns>
    /// New <see cref="AsyncKeyedReaderWriterLockWriteToken{TKey}"/> value. When <paramref name="entered"/> is <b>false</b>,
    /// then returned instanced will be a default value.
    /// </returns>
    public AsyncKeyedReaderWriterLockWriteToken<TKey> TryEnterWrite(TKey key, out bool entered)
    {
        Entry? entry;
        AsyncReaderWriterLockWriteToken token;
        using ( AcquireLock() )
        {
            entry = GetOrAddEntry( key );
            token = entry.Lock.TryEnterWrite( out entered );
            if ( ! entered )
            {
                entry.ExitUnsafe();
                return default;
            }
        }

        return new AsyncKeyedReaderWriterLockWriteToken<TKey>( entry, token );
    }

    /// <summary>
    /// Returns the total number of lock participants for the provided <paramref name="key"/>,
    /// which includes current lock holders and all waiters.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Total number of lock participants.</returns>
    public int Participants(TKey key)
    {
        using ( AcquireLock() )
            return _entries.TryGetValue( key, out var entry ) ? entry.Lock.Participants : 0;
    }

    /// <summary>
    /// Attempts to discard unused resources.
    /// </summary>
    public void TrimExcess()
    {
        using ( AcquireLock() )
        {
            _entryCache = StackSlim<Entry>.Create();
            _entries.TrimExcess();
            foreach ( var (_, entry) in _entries )
                entry.Lock.TrimExcess();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Entry GetOrAddEntry(TKey key)
    {
        ref var entryRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _entries, key, out var exists )!;
        if ( ! exists )
        {
            if ( ! _entryCache.TryPop( out var entry ) )
                entry = new Entry( this );

            entry.Key = key;
            entryRef = entry;
        }

        entryRef.RefCount = checked( entryRef.RefCount + 1 );
        return entryRef;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _entries );
    }

    internal sealed class Entry
    {
        internal Entry(AsyncKeyedReaderWriterLock<TKey> keyedLock)
        {
            KeyedLock = keyedLock;
            Lock = new AsyncReaderWriterLock();
            Key = default!;
            RefCount = 0;
        }

        internal readonly AsyncKeyedReaderWriterLock<TKey> KeyedLock;
        internal readonly AsyncReaderWriterLock Lock;
        internal TKey Key;
        internal int RefCount;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Exit()
        {
            using ( KeyedLock.AcquireLock() )
                ExitUnsafe();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ExitUnsafe()
        {
            RefCount = unchecked( RefCount - 1 );
            if ( RefCount > 0 )
                return;

            Assume.Equals( RefCount, 0 );
            Assume.Equals( Lock.Participants, 0 );

            KeyedLock._entries.Remove( Key );
            Key = default!;
            KeyedLock._entryCache.Push( this );
        }
    }
}
