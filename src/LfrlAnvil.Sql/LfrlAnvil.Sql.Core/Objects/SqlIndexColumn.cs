using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects;

public readonly struct SqlIndexColumn<T> : IEquatable<SqlIndexColumn<T>>
    where T : class, ISqlColumn
{
    internal SqlIndexColumn(T column, OrderBy ordering)
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
        return obj is SqlIndexColumn<T> c && Equals( c );
    }

    [Pure]
    public bool Equals(SqlIndexColumn<T> other)
    {
        return ReferenceEquals( Column, other.Column ) && Ordering == other.Ordering;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlIndexColumn<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlColumn
    {
        return new SqlIndexColumn<TDestination>( ReinterpretCast.To<TDestination>( Column ), Ordering );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlIndexColumn<ISqlColumn>(SqlIndexColumn<T> source)
    {
        return new SqlIndexColumn<ISqlColumn>( source.Column, source.Ordering );
    }

    [Pure]
    public static bool operator ==(SqlIndexColumn<T> a, SqlIndexColumn<T> b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(SqlIndexColumn<T> a, SqlIndexColumn<T> b)
    {
        return ! a.Equals( b );
    }
}
