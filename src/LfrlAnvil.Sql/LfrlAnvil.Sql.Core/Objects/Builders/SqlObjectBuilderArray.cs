using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an array of <see cref="ISqlObjectBuilder"/> instances.
/// </summary>
/// <typeparam name="T">SQL object builder type.</typeparam>
public readonly struct SqlObjectBuilderArray<T> : IReadOnlyList<T>
    where T : class, ISqlObjectBuilder
{
    private readonly ReadOnlyArray<SqlObjectBuilder> _source;

    private SqlObjectBuilderArray(ReadOnlyArray<SqlObjectBuilder> source)
    {
        _source = source;
    }

    /// <inheritdoc />
    public int Count => _source.Count;

    /// <inheritdoc />
    public T this[int index] => ReinterpretCast.To<T>( _source[index] );

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderArray{T}"/> instance.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="TSource">Source SQL object builder type.</typeparam>
    /// <returns>New <see cref="SqlObjectBuilderArray{T}"/> instance.</returns>
    /// <remarks>Be careful while using this method, because it does not actually validate the type's correctness.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderArray<T> From<TSource>(ReadOnlyArray<TSource> source)
        where TSource : SqlObjectBuilder
    {
        return new SqlObjectBuilderArray<T>( ReadOnlyArray<SqlObjectBuilder>.From( source ) );
    }

    /// <summary>
    /// Converts this instance to another type that implements the <see cref="ISqlObjectBuilder"/> interface.
    /// </summary>
    /// <typeparam name="TDestination">SQL object builder type to convert to.</typeparam>
    /// <returns>New <see cref="SqlObjectBuilderArray{T}"/> instance.</returns>
    /// <remarks>Be careful while using this method, because it does not actually validate the type's correctness.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderArray<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderArray<TDestination>( _source );
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
    /// Lightweight enumerator implementation for <see cref="SqlObjectBuilderArray{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private ReadOnlyArray<SqlObjectBuilder>.Enumerator _source;

        internal Enumerator(ReadOnlyArray<SqlObjectBuilder> source)
        {
            _source = source.GetEnumerator();
        }

        /// <inheritdoc />
        public T Current => ReinterpretCast.To<T>( _source.Current );

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
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
