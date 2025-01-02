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

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

/// <summary>
/// Represents a pool of recyclable objects exposed through <see cref="ObjectRecyclerToken{T}"/> instances.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public abstract class ObjectRecycler<T> : IDisposable
    where T : class
{
    private LinkedEntry<T>[] _entries;
    private NullableIndex _occupiedListTail;
    private NullableIndex _freeListTail;
    private bool _isDisposed;

    /// <summary>
    /// Creates a new <see cref="ObjectRecycler{T}"/> instance.
    /// </summary>
    /// <param name="minCapacity">Minimum initial capacity of the underlying object buffer. Equal to <b>0</b> by default.</param>
    protected ObjectRecycler(int minCapacity = 0)
    {
        _entries = minCapacity <= 0 ? Array.Empty<LinkedEntry<T>>() : new LinkedEntry<T>[Buffers.GetCapacity( minCapacity )];
        _occupiedListTail = _freeListTail = NullableIndex.Null;
        ObjectCount = 0;
        ActiveObjectCount = 0;
        _isDisposed = false;
    }

    /// <summary>
    /// Specifies the total number of currently cached objects.
    /// </summary>
    public int ObjectCount { get; private set; }

    /// <summary>
    /// Specifies the number of currently active objects.
    /// </summary>
    public int ActiveObjectCount { get; private set; }

    /// <inheritdoc/>
    /// <exception cref="AggregateException">
    /// When at least one stored object throws an exception during a call to its <see cref="IDisposable.Dispose()"/> method.
    /// </exception>
    public void Dispose()
    {
        if ( _isDisposed )
            return;

        var objCount = ObjectCount;
        var entries = _entries;

        _isDisposed = true;
        _entries = Array.Empty<LinkedEntry<T>>();
        _occupiedListTail = _freeListTail = NullableIndex.Null;
        ObjectCount = 0;
        ActiveObjectCount = 0;
        DisposeEntries( entries.AsSpan( 0, objCount ) );
    }

    /// <summary>
    /// Creates a new <see cref="ObjectRecyclerToken{T}"/> instance from this recycler.
    /// </summary>
    /// <returns>New <see cref="ObjectRecyclerToken{T}"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">When this recycler has been disposed.</exception>
    public ObjectRecyclerToken<T> Rent()
    {
        ObjectDisposedException.ThrowIf( _isDisposed, this );

        int id;
        if ( _freeListTail.HasValue )
        {
            id = _freeListTail.Value;
            ref var entry = ref GetEntryRef( id );
            Assume.True( entry.IsInFreeList );

            var prevIndex = entry.Prev;
            if ( prevIndex.HasValue )
            {
                ref var prev = ref GetEntryRef( prevIndex.Value );
                Assume.True( prev.IsInFreeList );
                prev.MakeFree( prev.Prev, NullableIndex.Null );
            }

            _freeListTail = prevIndex;
            entry.MakeOccupied( _occupiedListTail, NullableIndex.Null );
        }
        else
        {
            var obj = Create();

            id = ObjectCount;
            if ( ObjectCount >= _entries.Length )
            {
                ObjectCount = checked( ObjectCount + 1 );
                var prevEntries = _entries;
                _entries = new LinkedEntry<T>[Buffers.GetCapacity( ObjectCount )];
                prevEntries.AsSpan().CopyTo( _entries );
            }
            else
                ++ObjectCount;

            ref var entry = ref GetEntryRef( id );
            Assume.True( entry.IsUnused );
            entry.MakeOccupied( _occupiedListTail, NullableIndex.Null );
            entry.Value = obj;
        }

        if ( _occupiedListTail.HasValue )
        {
            ref var tail = ref GetEntryRef( _occupiedListTail.Value );
            Assume.True( tail.IsInOccupiedList );
            tail.MakeOccupied( tail.Prev, NullableIndex.Create( id ) );
        }

        ++ActiveObjectCount;
        _occupiedListTail = NullableIndex.Create( id );
        return new ObjectRecyclerToken<T>( this, id );
    }

    /// <summary>
    /// Attempts to dispose cached objects without modifying internal state.
    /// </summary>
    /// <exception cref="AggregateException">
    /// When at least one stored object throws an exception during a call to its <see cref="IDisposable.Dispose()"/> method.
    /// </exception>
    public void TrimExcess()
    {
        if ( ! _freeListTail.HasValue )
            return;

        var objCount = ObjectCount;
        var entries = _entries;
        if ( ! _occupiedListTail.HasValue )
        {
            Assume.Equals( ActiveObjectCount, 0 );
            _entries = Array.Empty<LinkedEntry<T>>();
            _freeListTail = NullableIndex.Null;
            ObjectCount = 0;
            DisposeEntries( entries.AsSpan( 0, objCount ) );
            return;
        }

        Assume.IsGreaterThan( ObjectCount, 0 );
        var maxOccupiedIndex = -1;
        for ( var i = ObjectCount - 1; i >= 0; --i )
        {
            ref var entry = ref GetEntryRef( i );
            Assume.False( entry.IsUnused );
            if ( entry.IsInOccupiedList )
            {
                maxOccupiedIndex = i;
                break;
            }
        }

        Assume.IsGreaterThanOrEqualTo( maxOccupiedIndex, 0 );
        var firstDisposeIndex = maxOccupiedIndex + 1;
        var disposeCount = objCount - firstDisposeIndex;
        if ( disposeCount == 0 )
            return;

        _freeListTail = NullableIndex.Null;
        ObjectCount -= disposeCount;

        var toClear = Span<LinkedEntry<T>>.Empty;
        var newCapacity = Buffers.GetCapacity( ObjectCount );
        if ( newCapacity != _entries.Length )
        {
            _entries = new LinkedEntry<T>[newCapacity];
            entries.AsSpan( 0, ObjectCount ).CopyTo( _entries );
        }
        else
            toClear = _entries.AsSpan( firstDisposeIndex, disposeCount );

        var remaining = ObjectCount - ActiveObjectCount;
        if ( remaining > 0 )
        {
            var nextIndex = 0;
            ref var tail = ref FindNextFreeEntryRef( ref nextIndex );
            tail.MakeFree( NullableIndex.Null, NullableIndex.Null );
            _freeListTail = NullableIndex.Create( nextIndex );

            if ( --remaining > 0 )
            {
                var index = nextIndex + 1;
                while ( true )
                {
                    ref var entry = ref FindNextFreeEntryRef( ref index );
                    ref var next = ref GetEntryRef( nextIndex );
                    next.MakeFree( NullableIndex.Create( index ), next.Next );
                    entry.MakeFree( NullableIndex.Null, NullableIndex.Create( nextIndex ) );
                    if ( --remaining == 0 )
                        break;

                    nextIndex = index++;
                }
            }
        }

        try
        {
            DisposeEntries( entries.AsSpan( firstDisposeIndex, disposeCount ) );
        }
        finally
        {
            toClear.Clear();
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref LinkedEntry<T> FindNextFreeEntryRef(ref int index)
    {
        ref var entry = ref GetEntryRef( index );
        while ( ! entry.IsInFreeList )
            entry = ref GetEntryRef( ++index );

        return ref entry;
    }

    /// <summary>
    /// Creates a new object instance.
    /// </summary>
    /// <returns>New object instance.</returns>
    [Pure]
    protected abstract T Create();

    /// <summary>
    /// Performs additional <paramref name="obj"/> clean up during its move to the free object stack.
    /// </summary>
    /// <param name="obj">Object to free.</param>
    protected virtual void Free(T obj) { }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetString(ObjectRecycler<T>? recycler, int id)
    {
        if ( recycler is not null && id < recycler._entries.Length )
        {
            ref var entry = ref recycler.GetEntryRef( id );
            if ( entry.IsInOccupiedList )
                return $"(active) [{entry.Value}]";
        }

        return "(disposed)";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Release(int id)
    {
        if ( id >= ObjectCount )
            return;

        ref var entry = ref GetEntryRef( id );
        if ( ! entry.IsInOccupiedList )
            return;

        var prevIndex = entry.Prev;
        var nextIndex = entry.Next;

        if ( prevIndex.HasValue )
        {
            ref var prev = ref GetEntryRef( prevIndex.Value );
            Assume.True( prev.IsInOccupiedList );
            prev.MakeOccupied( prev.Prev, nextIndex );
        }

        if ( _occupiedListTail.Value == id )
        {
            Assume.False( nextIndex.HasValue );
            _occupiedListTail = prevIndex;
        }
        else
        {
            Assume.True( nextIndex.HasValue );
            ref var next = ref GetEntryRef( nextIndex.Value );
            Assume.True( next.IsInOccupiedList );
            next.MakeOccupied( prevIndex, next.Next );
        }

        if ( _freeListTail.HasValue )
        {
            ref var tail = ref GetEntryRef( _freeListTail.Value );
            Assume.True( tail.IsInFreeList );
            tail.MakeFree( tail.Prev, NullableIndex.Create( id ) );
        }

        --ActiveObjectCount;
        entry.MakeFree( _freeListTail, NullableIndex.Null );
        _freeListTail = NullableIndex.Create( id );
        Free( entry.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal T GetObject(int id)
    {
        Ensure.IsLessThan( id, _entries.Length );
        ref var entry = ref GetEntryRef( id );
        if ( ! entry.IsInOccupiedList )
            ObjectDisposedException.ThrowIf( ! entry.IsInOccupiedList, typeof( ObjectRecyclerToken<T> ) );

        return entry.Value;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref LinkedEntry<T> GetEntryRef(int index)
    {
        Assume.IsInRange( index, 0, _entries.Length - 1 );
        ref var first = ref MemoryMarshal.GetArrayDataReference( _entries );
        return ref Unsafe.Add( ref first, index );
    }

    private static void DisposeEntries(ReadOnlySpan<LinkedEntry<T>> entries)
    {
        var exceptions = Chain<Exception>.Empty;
        foreach ( var entry in entries )
        {
            Assume.False( entry.IsUnused );
            if ( entry.Value is not IDisposable d )
                continue;

            try
            {
                d.Dispose();
            }
            catch ( Exception exc )
            {
                exceptions = exceptions.Extend( exc );
            }
        }

        if ( exceptions.Count > 0 )
            ExceptionThrower.Throw( new AggregateException( exceptions ) );
    }
}
