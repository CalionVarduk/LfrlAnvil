using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a name of an SQL record set.
/// </summary>
public readonly struct SqlRecordSetInfo : IEquatable<SqlRecordSetInfo>
{
    private readonly string? _identifier;

    private SqlRecordSetInfo(SqlSchemaObjectName name, bool isTemporary, string identifier)
    {
        Name = name;
        IsTemporary = isTemporary;
        _identifier = identifier;
    }

    /// <summary>
    /// Underlying name of this record set.
    /// </summary>
    public SqlSchemaObjectName Name { get; }

    /// <summary>
    /// Specifies whether or not this record set is temporary.
    /// </summary>
    public bool IsTemporary { get; }

    /// <summary>
    /// Identifier of this record set.
    /// </summary>
    public string Identifier => _identifier ?? string.Empty;

    /// <summary>
    /// Creates a new <see cref="SqlRecordSetInfo"/> instance that is marked as temporary.
    /// </summary>
    /// <param name="name">Underlying name of the record set.</param>
    /// <returns>New <see cref="SqlRecordSetInfo"/> instance</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecordSetInfo CreateTemporary(string name)
    {
        return new SqlRecordSetInfo( name: SqlSchemaObjectName.Create( name ), isTemporary: true, identifier: $"TEMP.{name}" );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRecordSetInfo"/> instance with empty <see cref="SqlSchemaObjectName.Schema"/>.
    /// </summary>
    /// <param name="objectName">Name of the record set object.</param>
    /// <returns>New <see cref="SqlRecordSetInfo"/> instance</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecordSetInfo Create(string objectName)
    {
        return Create( string.Empty, objectName );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRecordSetInfo"/> instance.
    /// </summary>
    /// <param name="schemaName">Name of the SQL schema that the record set belongs to.</param>
    /// <param name="objectName">Name of the record set object.</param>
    /// <returns>New <see cref="SqlRecordSetInfo"/> instance</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecordSetInfo Create(string schemaName, string objectName)
    {
        return Create( SqlSchemaObjectName.Create( schemaName, objectName ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRecordSetInfo"/> instance.
    /// </summary>
    /// <param name="name">Underlying name of the record set.</param>
    /// <returns>New <see cref="SqlRecordSetInfo"/> instance</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecordSetInfo Create(SqlSchemaObjectName name)
    {
        return new SqlRecordSetInfo( name: name, isTemporary: false, identifier: name.ToString() );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlRecordSetInfo"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Identifier;
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Name, IsTemporary );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlRecordSetInfo n && Equals( n );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(SqlRecordSetInfo other)
    {
        return Name == other.Name && IsTemporary == other.IsTemporary;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(SqlRecordSetInfo a, SqlRecordSetInfo b)
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
    public static bool operator !=(SqlRecordSetInfo a, SqlRecordSetInfo b)
    {
        return ! a.Equals( b );
    }
}
