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

namespace LfrlAnvil.Async;

/// <summary>
/// Represents an acquired lock from an <see cref="AsyncKeyedMutex{TKey}"/> instance.
/// </summary>
public readonly struct AsyncKeyedMutexToken<TKey> : IDisposable
    where TKey : notnull
{
    private readonly AsyncKeyedMutex<TKey>.Entry? _entry;
    private readonly AsyncMutexToken _token;

    internal AsyncKeyedMutexToken(AsyncKeyedMutex<TKey>.Entry entry, AsyncMutexToken token)
    {
        _entry = entry;
        _token = token;
    }

    /// <summary>
    /// Associated <see cref="AsyncKeyedMutex{TKey}"/> instance.
    /// </summary>
    public AsyncKeyedMutex<TKey>? Mutex => _entry?.KeyedMutex;

    /// <summary>
    /// Associated key.
    /// </summary>
    public TKey? Key => _entry is not null ? _entry.Key : default;

    /// <inheritdoc/>
    public void Dispose()
    {
        if ( _entry is null )
            return;

        Assume.IsNotNull( _token.Entry );
        var released = false;
        try
        {
            released = _token.Entry.Exit( _token.Version );
        }
        finally
        {
            if ( released )
                _entry.Exit();
        }
    }
}
