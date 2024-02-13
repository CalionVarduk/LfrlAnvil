using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlObjectBuilderReferenceSource<T> : IEquatable<SqlObjectBuilderReferenceSource<T>>
    where T : class, ISqlObjectBuilder
{
    internal SqlObjectBuilderReferenceSource(T @object, string? property)
    {
        Object = @object;
        Property = property;
    }

    public T Object { get; }
    public string? Property { get; }

    [Pure]
    public override string ToString()
    {
        return Property is null ? Object.ToString() ?? string.Empty : $"{Object} ({Property})";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Object, Property );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlObjectBuilderReferenceSource<T> s && Equals( s );
    }

    [Pure]
    public bool Equals(SqlObjectBuilderReferenceSource<T> other)
    {
        return ReferenceEquals( Object, other.Object ) && Property == other.Property;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderReferenceSource<T> WithProperty(string? property)
    {
        return new SqlObjectBuilderReferenceSource<T>( Object, property );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderReferenceSource<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderReferenceSource<TDestination>( ReinterpretCast.To<TDestination>( Object ), Property );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlObjectBuilderReferenceSource<ISqlObjectBuilder>(SqlObjectBuilderReferenceSource<T> source)
    {
        return new SqlObjectBuilderReferenceSource<ISqlObjectBuilder>( source.Object, source.Property );
    }

    [Pure]
    public static bool operator ==(SqlObjectBuilderReferenceSource<T> a, SqlObjectBuilderReferenceSource<T> b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(SqlObjectBuilderReferenceSource<T> a, SqlObjectBuilderReferenceSource<T> b)
    {
        return ! a.Equals( b );
    }
}
