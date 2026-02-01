// Copyright 2025-2026 Łukasz Furlepa
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
/// Represents a source of a keyed collection of fair asynchronous mutex primitives.
/// </summary>
/// <typeparam name="TKey">Key's type.</typeparam>
/// <remarks>Lock is not reentrant.</remarks>
public sealed class AsyncKeyedMutex<TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, Entry> _entries;
    private StackSlim<Entry> _entryCache;

    /// <summary>
    /// Creates a new <see cref="AsyncKeyedMutex{TKey}"/> instance.
    /// </summary>
    /// <param name="keyComparer">Optional <typeparamref name="TKey"/> comparer. Equal to <b>null</b> by default.</param>
    public AsyncKeyedMutex(IEqualityComparer<TKey>? keyComparer = null)
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
    /// Asynchronously acquires an exclusive lock for the specified <paramref name="key"/> from this mutex.
    /// </summary>
    /// <param name="key">Key for which to acquire an exclusive lock.</param>
    /// <param name="cancellationToken">
    /// Optional <see cref="CancellationToken"/> that can be used to cancel pending mutex acquisition.
    /// </param>
    /// <returns>New <see cref="ValueTask{TResult}"/> instance which returns an <see cref="AsyncKeyedMutexLock{TKey}"/> value.</returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <paramref name="cancellationToken"/> was cancelled before the lock was acquired.
    /// </exception>
    public async ValueTask<AsyncKeyedMutexLock<TKey>> EnterAsync(TKey key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Entry? entry;
        ValueTask<AsyncMutexLock> lockTask;
        using ( AcquireLock() )
        {
            entry = GetOrAddEntry( key );
            lockTask = entry.Mutex.EnterAsync( cancellationToken );
        }

        AsyncMutexLock @lock;
        try
        {
            @lock = await lockTask.ConfigureAwait( false );
        }
        catch
        {
            entry.Exit();
            throw;
        }

        return new AsyncKeyedMutexLock<TKey>( entry, @lock );
    }

    /// <summary>
    /// Returns the total number of lock participants for the provided <paramref name="key"/>,
    /// which includes current lock holder and all waiters.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Total number of lock participants.</returns>
    public int Participants(TKey key)
    {
        using ( AcquireLock() )
            return _entries.TryGetValue( key, out var entry ) ? entry.Mutex.Participants : 0;
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
                entry.Mutex.TrimExcess();
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
        internal Entry(AsyncKeyedMutex<TKey> keyedMutex)
        {
            KeyedMutex = keyedMutex;
            Mutex = new AsyncMutex();
            Key = default!;
            RefCount = 0;
        }

        internal readonly AsyncKeyedMutex<TKey> KeyedMutex;
        internal readonly AsyncMutex Mutex;
        internal TKey Key;
        internal int RefCount;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Exit()
        {
            using ( KeyedMutex.AcquireLock() )
            {
                RefCount = unchecked( RefCount - 1 );
                if ( RefCount > 0 )
                    return;

                Assume.Equals( RefCount, 0 );
                Assume.Equals( Mutex.Participants, 0 );

                KeyedMutex._entries.Remove( Key );
                Key = default!;
                KeyedMutex._entryCache.Push( this );
            }
        }
    }
}
