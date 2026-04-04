using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Internal;

internal sealed class EnumerableMemory<T> : IReadOnlyList<T>
{
    private readonly ReadOnlyMemory<T> _source;

    internal EnumerableMemory(ReadOnlyMemory<T> source)
    {
        _source = source;
    }

    public int Count => _source.Length;
    public T this[int index] => _source.Span[index];

    [Pure]
    public IEnumerator<T> GetEnumerator()
    {
        for ( var i = 0; i < _source.Length; ++i )
            yield return _source.Span[i];
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
