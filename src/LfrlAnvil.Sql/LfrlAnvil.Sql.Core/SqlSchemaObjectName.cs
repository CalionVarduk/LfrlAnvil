using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a name of an SQL object that may belong to an SQL schema.
/// </summary>
public readonly struct SqlSchemaObjectName : IEquatable<SqlSchemaObjectName>
{
    private readonly string? _schema;
    private readonly string? _object;

    private SqlSchemaObjectName(string schema, string obj)
    {
        _schema = schema;
        _object = obj;
    }

    /// <summary>
    /// Name of the schema that this SQL object belongs to.
    /// </summary>
    public string Schema => _schema ?? string.Empty;

    /// <summary>
    /// Name of this SQL object.
    /// </summary>
    public string Object => _object ?? string.Empty;

    /// <summary>
    /// Creates a new <see cref="SqlSchemaObjectName"/> instance with empty <see cref="Schema"/>.
    /// </summary>
    /// <param name="obj">Name of the SQL object.</param>
    /// <returns>New <see cref="SqlSchemaObjectName"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSchemaObjectName Create(string obj)
    {
        return Create( string.Empty, obj );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSchemaObjectName"/> instance.
    /// </summary>
    /// <param name="schema">Name of the schema that the SQL object belongs to.</param>
    /// <param name="obj">Name of the SQL object.</param>
    /// <returns>New <see cref="SqlSchemaObjectName"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSchemaObjectName Create(string schema, string obj)
    {
        return new SqlSchemaObjectName( schema, obj );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlSchemaObjectName"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var schema = Schema;
        return schema.Length > 0 ? $"{schema}.{Object}" : Object;
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Schema.GetHashCode( StringComparison.OrdinalIgnoreCase ),
            Object.GetHashCode( StringComparison.OrdinalIgnoreCase ) );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlSchemaObjectName n && Equals( n );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(SqlSchemaObjectName other)
    {
        return Schema.Equals( other.Schema, StringComparison.OrdinalIgnoreCase )
            && Object.Equals( other.Object, StringComparison.OrdinalIgnoreCase );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(SqlSchemaObjectName a, SqlSchemaObjectName b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(SqlSchemaObjectName a, SqlSchemaObjectName b)
    {
        return ! a.Equals( b );
    }
}
