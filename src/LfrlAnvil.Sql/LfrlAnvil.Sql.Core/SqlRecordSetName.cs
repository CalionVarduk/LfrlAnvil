using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql;

public readonly struct SqlRecordSetName : IEquatable<SqlRecordSetName>
{
    private readonly string? _schemaName;
    private readonly string? _name;

    private SqlRecordSetName(string schemaName, string name, bool isTemporary)
    {
        _schemaName = schemaName;
        _name = name;
        IsTemporary = isTemporary;
    }

    public bool IsTemporary { get; }
    public string SchemaName => _schemaName ?? string.Empty;
    public string Name => _name ?? string.Empty;

    public string Identifier
    {
        get
        {
            if ( IsTemporary )
            {
                Assume.IsEmpty( SchemaName, nameof( SchemaName ) );
                return $"TEMP.{Name}";
            }

            return SchemaName.Length > 0 ? $"{SchemaName}.{Name}" : Name;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecordSetName CreateTemporary(string name)
    {
        return new SqlRecordSetName( string.Empty, name, isTemporary: true );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecordSetName Create(string schemaName, string name)
    {
        return new SqlRecordSetName( schemaName, name, isTemporary: false );
    }

    [Pure]
    public override string ToString()
    {
        return Identifier;
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine(
            SchemaName.GetHashCode( StringComparison.OrdinalIgnoreCase ),
            Name.GetHashCode( StringComparison.OrdinalIgnoreCase ),
            IsTemporary );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlRecordSetName n && Equals( n );
    }

    [Pure]
    public bool Equals(SqlRecordSetName other)
    {
        return SchemaName.Equals( other.SchemaName, StringComparison.OrdinalIgnoreCase ) &&
            Name.Equals( other.Name, StringComparison.OrdinalIgnoreCase ) &&
            IsTemporary == other.IsTemporary;
    }

    [Pure]
    public static bool operator ==(SqlRecordSetName a, SqlRecordSetName b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(SqlRecordSetName a, SqlRecordSetName b)
    {
        return ! a.Equals( b );
    }
}
