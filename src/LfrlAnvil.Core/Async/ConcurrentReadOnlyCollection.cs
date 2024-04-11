using System.Collections;
using System.Collections.Generic;

namespace LfrlAnvil.Async;

public sealed class ConcurrentReadOnlyCollection<T> : IReadOnlyCollection<T>
{
    private readonly IReadOnlyCollection<T> _collection;
    private readonly object _sync;

    public ConcurrentReadOnlyCollection(IReadOnlyCollection<T> collection, object sync)
    {
        _collection = collection;
        _sync = sync;
    }

    public int Count
    {
        get
        {
            lock ( _sync )
            {
                return _collection.Count;
            }
        }
    }

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
        private ExclusiveLock _lock;

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
