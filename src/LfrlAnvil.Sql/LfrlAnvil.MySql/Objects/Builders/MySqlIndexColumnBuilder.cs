using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlIndexColumnBuilder : ISqlIndexColumnBuilder, IEquatable<MySqlIndexColumnBuilder>
{
    private MySqlIndexColumnBuilder(MySqlColumnBuilder column, OrderBy ordering)
    {
        Column = column;
        Ordering = ordering;
    }

    public MySqlColumnBuilder Column { get; }
    public OrderBy Ordering { get; }

    ISqlColumnBuilder ISqlIndexColumnBuilder.Column => Column;

    [Pure]
    public override string ToString()
    {
        return $"{MySqlHelpers.GetFullName( Column.Table.Schema.Name, Column.Table.Name, Column.Name )} {Ordering.Name}";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Column.Id, Ordering.Value );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is MySqlIndexColumnBuilder c && Equals( c );
    }

    [Pure]
    public bool Equals(MySqlIndexColumnBuilder? other)
    {
        return other is not null && ReferenceEquals( Column, other.Column ) && Ordering == other.Ordering;
    }

    [Pure]
    public static MySqlIndexColumnBuilder Asc(MySqlColumnBuilder column)
    {
        return new MySqlIndexColumnBuilder( column, OrderBy.Asc );
    }

    [Pure]
    public static MySqlIndexColumnBuilder Desc(MySqlColumnBuilder column)
    {
        return new MySqlIndexColumnBuilder( column, OrderBy.Desc );
    }
}
