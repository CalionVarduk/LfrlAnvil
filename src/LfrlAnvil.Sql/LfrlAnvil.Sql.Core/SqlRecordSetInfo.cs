using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql;

public readonly struct SqlRecordSetInfo : IEquatable<SqlRecordSetInfo>
{
    private readonly string? _identifier;

    private SqlRecordSetInfo(SqlSchemaObjectName name, bool isTemporary, string identifier)
    {
        Name = name;
        IsTemporary = isTemporary;
        _identifier = identifier;
    }

    public SqlSchemaObjectName Name { get; }
    public bool IsTemporary { get; }
    public string Identifier => _identifier ?? string.Empty;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecordSetInfo CreateTemporary(string name)
    {
        return new SqlRecordSetInfo( name: SqlSchemaObjectName.Create( name ), isTemporary: true, identifier: $"TEMP.{name}" );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecordSetInfo Create(string objectName)
    {
        return Create( string.Empty, objectName );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecordSetInfo Create(string schemaName, string objectName)
    {
        return Create( SqlSchemaObjectName.Create( schemaName, objectName ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecordSetInfo Create(SqlSchemaObjectName name)
    {
        return new SqlRecordSetInfo( name: name, isTemporary: false, identifier: name.Identifier );
    }

    [Pure]
    public override string ToString()
    {
        return Identifier;
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Name, IsTemporary );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlRecordSetInfo n && Equals( n );
    }

    [Pure]
    public bool Equals(SqlRecordSetInfo other)
    {
        return Name == other.Name && IsTemporary == other.IsTemporary;
    }

    [Pure]
    public static bool operator ==(SqlRecordSetInfo a, SqlRecordSetInfo b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(SqlRecordSetInfo a, SqlRecordSetInfo b)
    {
        return ! a.Equals( b );
    }
}
