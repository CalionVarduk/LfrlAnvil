using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlObjectBuilderReferenceCollection<T> : IReadOnlyCollection<SqlObjectBuilderReference<T>>
    where T : class, ISqlObjectBuilder
{
    private readonly SqlObjectBuilder _object;

    internal SqlObjectBuilderReferenceCollection(SqlObjectBuilder obj)
    {
        _object = obj;
    }

    public int Count => _object.ReferencedTargets?.Count ?? 0;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(SqlObjectBuilderReferenceSource<T> source)
    {
        return _object.ReferencedTargets?.ContainsKey( source.UnsafeReinterpretAs<SqlObjectBuilder>() ) ?? false;
    }

    [Pure]
    public SqlObjectBuilderReference<T> GetReference(SqlObjectBuilderReferenceSource<T> source)
    {
        return TryGetReference( source )
            ?? throw SqlHelpers.CreateObjectBuilderException( source.Object.Database, ExceptionResources.ReferenceDoesNotExist( source ) );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderReferenceCollection<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderReferenceCollection<TDestination>( _object );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _object );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlObjectBuilderReferenceCollection<ISqlObjectBuilder>(SqlObjectBuilderReferenceCollection<T> source)
    {
        return new SqlObjectBuilderReferenceCollection<ISqlObjectBuilder>( source._object );
    }

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

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return ! _isEmpty && _enumerator.MoveNext();
        }

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
