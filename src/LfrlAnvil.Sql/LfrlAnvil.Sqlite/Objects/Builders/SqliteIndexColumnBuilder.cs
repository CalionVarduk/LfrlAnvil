using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteIndexColumnBuilder : ISqlIndexColumnBuilder, IEquatable<SqliteIndexColumnBuilder>
{
    private SqliteIndexColumnBuilder(SqliteColumnBuilder column, OrderBy ordering)
    {
        Column = column;
        Ordering = ordering;
    }

    public SqliteColumnBuilder Column { get; }
    public OrderBy Ordering { get; }

    ISqlColumnBuilder ISqlIndexColumnBuilder.Column => Column;

    [Pure]
    public override string ToString()
    {
        return $"{Column.FullName} {Ordering.Name}";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Column.Id, Ordering.Value );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqliteIndexColumnBuilder c && Equals( c );
    }

    [Pure]
    public bool Equals(SqliteIndexColumnBuilder? other)
    {
        return other is not null && ReferenceEquals( Column, other.Column ) && Ordering == other.Ordering;
    }

    [Pure]
    public static SqliteIndexColumnBuilder Asc(SqliteColumnBuilder column)
    {
        return new SqliteIndexColumnBuilder( column, OrderBy.Asc );
    }

    [Pure]
    public static SqliteIndexColumnBuilder Desc(SqliteColumnBuilder column)
    {
        return new SqliteIndexColumnBuilder( column, OrderBy.Desc );
    }
}
