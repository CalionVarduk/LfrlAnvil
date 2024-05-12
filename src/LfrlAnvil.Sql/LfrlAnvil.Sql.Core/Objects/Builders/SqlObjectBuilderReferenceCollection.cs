using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a collection of <see cref="SqlObjectBuilderReference{T}"/> instances.
/// </summary>
/// <typeparam name="T">SQL object builder type.</typeparam>
public readonly struct SqlObjectBuilderReferenceCollection<T> : IReadOnlyCollection<SqlObjectBuilderReference<T>>
    where T : class, ISqlObjectBuilder
{
    private readonly SqlObjectBuilder _object;

    internal SqlObjectBuilderReferenceCollection(SqlObjectBuilder obj)
    {
        _object = obj;
    }

    /// <inheritdoc />
    public int Count => _object.ReferencedTargets?.Count ?? 0;

    /// <summary>
    /// Checks whether or not the provided <paramref name="source"/> exists in this collection.
    /// </summary>
    /// <param name="source">Source to check.</param>
    /// <returns><b>true</b> when <paramref name="source"/> exists, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(SqlObjectBuilderReferenceSource<T> source)
    {
        return _object.ReferencedTargets?.ContainsKey( source.UnsafeReinterpretAs<SqlObjectBuilder>() ) ?? false;
    }

    /// <summary>
    /// Returns an <see cref="SqlObjectBuilderReference{T}"/> instance associated with the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source to return an <see cref="SqlObjectBuilderReference{T}"/> instance for.</param>
    /// <returns><see cref="SqlObjectBuilderReference{T}"/> instance associated with the provided <paramref name="source"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When <paramref name="source"/> does not exist.</exception>
    [Pure]
    public SqlObjectBuilderReference<T> GetReference(SqlObjectBuilderReferenceSource<T> source)
    {
        return TryGetReference( source )
            ?? throw SqlHelpers.CreateObjectBuilderException( source.Object.Database, ExceptionResources.ReferenceDoesNotExist( source ) );
    }

    /// <summary>
    /// Attempts to return an <see cref="SqlObjectBuilderReference{T}"/> instance associated with the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source to return an <see cref="SqlObjectBuilderReference{T}"/> instance for.</param>
    /// <returns>
    /// <see cref="SqlObjectBuilderReference{T}"/> instance associated with the provided <paramref name="source"/>
    /// or null when it does not exist.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderReference<T>? TryGetReference(SqlObjectBuilderReferenceSource<T> source)
    {
        var referencedTargets = _object.ReferencedTargets;
        var baseSource = source.UnsafeReinterpretAs<SqlObjectBuilder>();
        return referencedTargets is not null && referencedTargets.TryGetValue( baseSource, out var target )
            ? CreateEntry( source, target )
            : null;
    }

    /// <summary>
    /// Converts this instance to another type that implements the <see cref="ISqlObjectBuilder"/> interface.
    /// </summary>
    /// <typeparam name="TDestination">SQL object builder type to convert to.</typeparam>
    /// <returns>New <see cref="SqlObjectBuilderReferenceCollection{T}"/> instance.</returns>
    /// <remarks>Be careful while using this method, because it does not actually validate the type's correctness.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderReferenceCollection<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderReferenceCollection<TDestination>( _object );
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _object );
    }

    /// <summary>
    /// Converts <paramref name="source"/> to the base <see cref="ISqlObjectBuilder"/> type.
    /// </summary>
    /// <param name="source">Source to convert.</param>
    /// <returns>New <see cref="SqlObjectBuilderReferenceCollection{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlObjectBuilderReferenceCollection<ISqlObjectBuilder>(SqlObjectBuilderReferenceCollection<T> source)
    {
        return new SqlObjectBuilderReferenceCollection<ISqlObjectBuilder>( source._object );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="SqlObjectBuilderReferenceCollection{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<SqlObjectBuilderReference<T>>
    {
        private readonly bool _isEmpty;
        private Dictionary<SqlObjectBuilderReferenceSource<SqlObjectBuilder>, SqlObjectBuilder>.Enumerator _enumerator;

        internal Enumerator(SqlObjectBuilder obj)
        {
            if ( obj.ReferencedTargets is null )
            {
                _isEmpty = true;
                _enumerator = default;
            }
            else
            {
                _isEmpty = false;
                _enumerator = obj.ReferencedTargets.GetEnumerator();
            }
        }

        /// <inheritdoc />
        public SqlObjectBuilderReference<T> Current
        {
            get
            {
                Assume.False( _isEmpty );
                var current = _enumerator.Current;
                return SqlObjectBuilderReference.Create( current.Key, current.Value ).UnsafeReinterpretAs<T>();
            }
        }

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return ! _isEmpty && _enumerator.MoveNext();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if ( ! _isEmpty )
                _enumerator.Dispose();
        }

        void IEnumerator.Reset()
        {
            if ( ! _isEmpty )
                (( IEnumerator )_enumerator).Reset();
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlObjectBuilderReference<T> CreateEntry(SqlObjectBuilderReferenceSource<T> source, SqlObjectBuilder target)
    {
        return SqlObjectBuilderReference.Create( source, ReinterpretCast.To<T>( target ) );
    }

    [Pure]
    IEnumerator<SqlObjectBuilderReference<T>> IEnumerable<SqlObjectBuilderReference<T>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
