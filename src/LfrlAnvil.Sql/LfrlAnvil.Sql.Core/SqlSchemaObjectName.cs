using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql;

public readonly struct SqlSchemaObjectName : IEquatable<SqlSchemaObjectName>
{
    private readonly string? _schema;
    private readonly string? _object;

    private SqlSchemaObjectName(string schema, string obj)
    {
        _schema = schema;
        _object = obj;
    }

    public string Schema => _schema ?? string.Empty;
    public string Object => _object ?? string.Empty;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSchemaObjectName Create(string obj)
    {
        return Create( string.Empty, obj );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSchemaObjectName Create(string schema, string obj)
    {
        return new SqlSchemaObjectName( schema, obj );
    }

    [Pure]
    public override string ToString()
    {
        var schema = Schema;
        return schema.Length > 0 ? $"{schema}.{Object}" : Object;
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Schema.GetHashCode( StringComparison.OrdinalIgnoreCase ),
            Object.GetHashCode( StringComparison.OrdinalIgnoreCase ) );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlSchemaObjectName n && Equals( n );
    }

    [Pure]
    public bool Equals(SqlSchemaObjectName other)
    {
        return Schema.Equals( other.Schema, StringComparison.OrdinalIgnoreCase )
            && Object.Equals( other.Object, StringComparison.OrdinalIgnoreCase );
    }

    [Pure]
    public static bool operator ==(SqlSchemaObjectName a, SqlSchemaObjectName b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(SqlSchemaObjectName a, SqlSchemaObjectName b)
    {
        return ! a.Equals( b );
    }
}
