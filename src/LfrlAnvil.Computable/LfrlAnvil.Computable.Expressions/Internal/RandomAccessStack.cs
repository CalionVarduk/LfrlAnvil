using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class RandomAccessStack<T> : IReadOnlyList<T>
    where T : notnull
{
    private const int BaseCapacity = 7;

    private T?[] _objects;

    internal RandomAccessStack()
    {
        _objects = new T?[BaseCapacity];
        Count = 0;
    }

    public int Count { get; private set; }

    public T this[int index] => _objects[Count - index - 1]!;

    internal void Push(T value)
    {
        if ( Count == _objects.Length )
            Array.Resize( ref _objects, (_objects.Length << 1) + 1 );

        _objects[Count++] = value;
    }

    internal void Replace(T value)
    {
        Assume.IsGreaterThan( Count, 0 );
        _objects[Count - 1] = value;
    }

    [Pure]
    internal T Peek()
    {
        Assume.IsGreaterThan( Count, 0 );
        return _objects[Count - 1]!;
    }

    internal T Pop()
    {
        Assume.IsGreaterThan( Count, 0 );

        var index = Count-- - 1;
        var result = _objects[index];
        _objects[index] = default;
        Assume.IsNotNull( result );
        return result;
    }

    internal bool TryPeek([MaybeNullWhen( false )] out T result)
    {
        if ( Count == 0 )
        {
            result = default;
            return false;
        }

        result = Peek();
        return true;
    }

    internal bool TryPop([MaybeNullWhen( false )] out T result)
    {
        if ( Count == 0 )
        {
            result = default;
            return false;
        }

        result = Pop();
        return true;
    }

    internal void Pop(int count)
    {
        Assume.IsInRange( count, 1, Count );

        Count -= count;
        Array.Clear( _objects, Count, count );
    }

    internal void PopInto(int count, T[] buffer, int startIndex)
    {
        Assume.IsInRange( count, 0, Count );
        Assume.IsLessThanOrEqualTo( startIndex + count, buffer.Length );

        if ( count == 0 )
            return;

        var oldCount = Count;
        Count -= count;

        for ( var i = Count; i < oldCount; ++i )
            buffer[i - Count + startIndex] = _objects[i]!;

        Array.Clear( _objects, Count, count );
    }

    [Pure]
    public IEnumerator<T> GetEnumerator()
    {
        return _objects.Take( Count ).GetEnumerator()!;
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
