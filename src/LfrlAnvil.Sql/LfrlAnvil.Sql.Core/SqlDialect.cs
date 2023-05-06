using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

public class SqlDialect : IEquatable<SqlDialect>
{
    public SqlDialect(string name)
    {
        Name = name;
    }

    public string Name { get; }

    [Pure]
    public override string ToString()
    {
        return Name;
    }

    [Pure]
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlDialect d && Equals( d );
    }

    [Pure]
    public bool Equals(SqlDialect? other)
    {
        return other is not null && Name.Equals( other.Name );
    }

    [Pure]
    public static bool operator ==(SqlDialect? a, SqlDialect? b)
    {
        return a?.Equals( b ) ?? b is null;
    }

    [Pure]
    public static bool operator !=(SqlDialect? a, SqlDialect? b)
    {
        return ! (a?.Equals( b ) ?? b is null);
    }
}
