using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteIndexColumn : ISqlIndexColumn, IEquatable<SqliteIndexColumn>
{
    internal SqliteIndexColumn(SqliteColumn column, OrderBy ordering)
    {
        Column = column;
        Ordering = ordering;
    }

    public SqliteColumn Column { get; }
    public OrderBy Ordering { get; }

    ISqlColumn ISqlIndexColumn.Column => Column;

    [Pure]
    public override string ToString()
    {
        return $"{Column.FullName} {Ordering.Name}";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Column.FullName, Ordering.Value );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqliteIndexColumn c && Equals( c );
    }

    [Pure]
    public bool Equals(SqliteIndexColumn? other)
    {
        return other is not null && ReferenceEquals( Column, other.Column ) && Ordering == other.Ordering;
    }

    [Pure]
    public static SqliteIndexColumn Asc(SqliteColumn column)
    {
        return new SqliteIndexColumn( column, OrderBy.Asc );
    }

    [Pure]
    public static SqliteIndexColumn Desc(SqliteColumn column)
    {
        return new SqliteIndexColumn( column, OrderBy.Desc );
    }
}
