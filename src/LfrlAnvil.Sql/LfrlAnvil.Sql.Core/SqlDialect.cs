using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a specific SQL dialect.
/// </summary>
public class SqlDialect : IEquatable<SqlDialect>
{
    /// <summary>
    /// Creates a new <see cref="SqlDialect"/> instance.
    /// </summary>
    /// <param name="name">Name of this dialect.</param>
    public SqlDialect(string name)
    {
        Ensure.IsNotEmpty( name );
        Name = name;
    }

    /// <summary>
    /// Name of this dialect.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlDialect"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Name;
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlDialect d && Equals( d );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(SqlDialect? other)
    {
        return other is not null && Name.Equals( other.Name );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(SqlDialect? a, SqlDialect? b)
    {
        return a?.Equals( b ) ?? b is null;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(SqlDialect? a, SqlDialect? b)
    {
        return ! (a?.Equals( b ) ?? b is null);
    }
}
