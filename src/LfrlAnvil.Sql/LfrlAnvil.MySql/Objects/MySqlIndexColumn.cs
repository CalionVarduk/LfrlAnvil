using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlIndexColumn : ISqlIndexColumn, IEquatable<MySqlIndexColumn>
{
    internal MySqlIndexColumn(MySqlColumn column, OrderBy ordering)
    {
        Column = column;
        Ordering = ordering;
    }

    public MySqlColumn Column { get; }
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
        return obj is MySqlIndexColumn c && Equals( c );
    }

    [Pure]
    public bool Equals(MySqlIndexColumn? other)
    {
        return other is not null && ReferenceEquals( Column, other.Column ) && Ordering == other.Ordering;
    }

    [Pure]
    public static MySqlIndexColumn Asc(MySqlColumn column)
    {
        return new MySqlIndexColumn( column, OrderBy.Asc );
    }

    [Pure]
    public static MySqlIndexColumn Desc(MySqlColumn column)
    {
        return new MySqlIndexColumn( column, OrderBy.Desc );
    }
}
