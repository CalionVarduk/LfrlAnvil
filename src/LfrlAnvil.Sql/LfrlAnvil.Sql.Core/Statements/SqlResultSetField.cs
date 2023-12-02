using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Sql.Statements;

public readonly struct SqlResultSetField
{
    private readonly string? _name;
    private readonly List<string>? _typeNames;

    public SqlResultSetField(int ordinal, string name, bool isUsed = true, bool includeTypeNames = false)
    {
        Ordinal = ordinal;
        _name = name;
        IsUsed = isUsed;
        _typeNames = includeTypeNames ? new List<string>() : null;
    }

    public int Ordinal { get; }
    public bool IsUsed { get; }
    public string Name => _name ?? string.Empty;
    public string? TypeName => _typeNames is null ? null : string.Join( " | ", _typeNames );
    public ReadOnlySpan<string> TypeNames => CollectionsMarshal.AsSpan( _typeNames );

    [Pure]
    public override string ToString()
    {
        var typeName = TypeName;
        var unusedText = IsUsed ? string.Empty : " (unused)";
        return typeName is null
            ? $"[{Ordinal}] '{Name}'{unusedText}"
            : $"[{Ordinal}] '{Name}' : {(typeName.Length > 0 ? typeName : "?")}{unusedText}";
    }

    public bool TryAddTypeName(string? name)
    {
        if ( _typeNames is null || string.IsNullOrEmpty( name ) )
            return false;

        foreach ( var existingName in TypeNames )
        {
            if ( existingName.Equals( name, StringComparison.OrdinalIgnoreCase ) )
                return false;
        }

        _typeNames.Add( name );
        return true;
    }
};
