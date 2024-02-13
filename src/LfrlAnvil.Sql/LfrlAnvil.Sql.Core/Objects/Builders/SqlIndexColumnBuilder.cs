using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlIndexColumnBuilder<T> : IEquatable<SqlIndexColumnBuilder<T>>
    where T : class, ISqlColumnBuilder
{
    internal SqlIndexColumnBuilder(T column, OrderBy ordering)
    {
        Column = column;
        Ordering = ordering;
    }

    public T Column { get; }
    public OrderBy Ordering { get; }

    [Pure]
    public override string ToString()
    {
        return $"{Column} {Ordering.Name}";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Column, Ordering.Value );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlIndexColumnBuilder<T> c && Equals( c );
    }

    [Pure]
    public bool Equals(SqlIndexColumnBuilder<T> other)
    {
        return ReferenceEquals( Column, other.Column ) && Ordering == other.Ordering;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlIndexColumnBuilder<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlColumnBuilder
    {
        return new SqlIndexColumnBuilder<TDestination>( ReinterpretCast.To<TDestination>( Column ), Ordering );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlIndexColumnBuilder<ISqlColumnBuilder>(SqlIndexColumnBuilder<T> source)
    {
        return new SqlIndexColumnBuilder<ISqlColumnBuilder>( source.Column, source.Ordering );
    }

    [Pure]
    public static bool operator ==(SqlIndexColumnBuilder<T> a, SqlIndexColumnBuilder<T> b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(SqlIndexColumnBuilder<T> a, SqlIndexColumnBuilder<T> b)
    {
        return ! a.Equals( b );
    }
}
