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
/// Represents an acquired lock from an <see cref="AsyncMutex"/> instance.
/// </summary>
public readonly struct AsyncMutexToken : IDisposable
{
    internal readonly AsyncMutex.Entry? Entry;
    internal readonly ulong Version;

    internal AsyncMutexToken(AsyncMutex.Entry entry, ulong version)
    {
        Entry = entry;
        Version = version;
    }

    /// <summary>
    /// Associated <see cref="AsyncMutex"/> instance.
    /// </summary>
    public AsyncMutex? Mutex => Entry?.Mutex;

    /// <inheritdoc/>
    public void Dispose()
    {
        Entry?.Exit( Version );
    }
}
