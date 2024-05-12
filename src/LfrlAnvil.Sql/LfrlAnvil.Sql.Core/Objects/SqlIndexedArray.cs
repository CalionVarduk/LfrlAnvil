using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents a collection of indexed SQL expressions.
/// </summary>
/// <typeparam name="T">SQL column type.</typeparam>
public readonly struct SqlIndexedArray<T> : IReadOnlyList<SqlIndexed<T>>
    where T : class, ISqlColumn
{
    private readonly ReadOnlyArray<SqlIndexed<ISqlColumn>> _source;

    private SqlIndexedArray(ReadOnlyArray<SqlIndexed<ISqlColumn>> source)
    {
        _source = source;
    }

    /// <inheritdoc />
    public int Count => _source.Count;

    /// <inheritdoc />
    public SqlIndexed<T> this[int index]
    {
        get
        {
            var source = _source[index];
            return new SqlIndexed<T>( ReinterpretCast.To<T>( source.Column ), source.Ordering );
        }
    }

    /// <summary>
    /// Creates a new <see cref="SqlIndexedArray{T}"/> instance.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <returns>New <see cref="SqlIndexedArray{T}"/> instance.</returns>
    /// <remarks>Be careful while using this method, because it does not actually validate the type's correctness.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexedArray<T> From(ReadOnlyArray<SqlIndexed<ISqlColumn>> source)
    {
        return new SqlIndexedArray<T>( source );
    }

    /// <summary>
    /// Converts this instance to another type that implements the <see cref="ISqlColumn"/> interface.
    /// </summary>
    /// <typeparam name="TDestination">SQL column type to convert to.</typeparam>
    /// <returns>New <see cref="SqlIndexedArray{T}"/> instance.</returns>
    /// <remarks>Be careful while using this method, because it does not actually validate the type's correctness.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlIndexedArray<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlColumn
    {
        return new SqlIndexedArray<TDestination>( _source );
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this array.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _source );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="SqlIndexedArray{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<SqlIndexed<T>>
    {
        private ReadOnlyArray<SqlIndexed<ISqlColumn>>.Enumerator _source;

        internal Enumerator(ReadOnlyArray<SqlIndexed<ISqlColumn>> source)
        {
            _source = source.GetEnumerator();
        }

        /// <inheritdoc />
        public SqlIndexed<T> Current
        {
            get
            {
                var source = _source.Current;
                return new SqlIndexed<T>( ReinterpretCast.To<T>( source.Column ), source.Ordering );
            }
        }

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _source.MoveNext();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _source.Dispose();
        }

        void IEnumerator.Reset()
        {
            (( IEnumerator )_source).Reset();
        }
    }

    [Pure]
    IEnumerator<SqlIndexed<T>> IEnumerable<SqlIndexed<T>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
