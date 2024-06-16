// Copyright 2024 Łukasz Furlepa
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

using System.Collections;
using System.Collections.Generic;

namespace LfrlAnvil.Async;

/// <summary>
/// Wraps an <see cref="IReadOnlyCollection{T}"/> instance in a thread-safe object.
/// </summary>
/// <typeparam name="T">Collection element's type.</typeparam>
public sealed class ConcurrentReadOnlyCollection<T> : IReadOnlyCollection<T>
{
    private readonly IReadOnlyCollection<T> _collection;
    private readonly object _sync;

    /// <summary>
    /// Creates a <see cref="ConcurrentReadOnlyCollection{T}"/> instance.
    /// </summary>
    /// <param name="collection">Wrapped collection.</param>
    /// <param name="sync">An optional object on which the monitor lock will be acquired.</param>
    public ConcurrentReadOnlyCollection(IReadOnlyCollection<T> collection, object? sync = null)
    {
        _collection = collection;
        _sync = sync ?? new object();
    }

    /// <inheritdoc />
    public int Count
    {
        get
        {
            using ( ExclusiveLock.Enter( _sync ) )
                return _collection.Count;
        }
    }

    /// <inheritdoc />
    /// <remarks>The enumerator will acquire and hold the monitor lock until it gets disposed.</remarks>
    public IEnumerator<T> GetEnumerator()
    {
        var @lock = ExclusiveLock.Enter( _sync );
        return new Enumerator( _collection.GetEnumerator(), @lock );
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly ExclusiveLock _lock;

        internal Enumerator(IEnumerator<T> enumerator, ExclusiveLock @lock)
        {
            _enumerator = enumerator;
            _lock = @lock;
        }

        public T Current => _enumerator.Current;
        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        public void Dispose()
        {
            _enumerator.Dispose();
            _lock.Dispose();
        }
    }
}
