using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a definition of a result set's field.
/// </summary>
public readonly struct SqlResultSetField
{
    private readonly string? _name;
    private readonly List<string>? _typeNames;

    /// <summary>
    /// Creates a new <see cref="SqlResultSetField"/> instance.
    /// </summary>
    /// <param name="ordinal">Ordinal of this field.</param>
    /// <param name="name">Name of this field.</param>
    /// <param name="isUsed">Specifies whether or not this field is used in a query.</param>
    /// <param name="includeTypeNames">Specifies whether or not to record type names of values associated with this field.</param>
    public SqlResultSetField(int ordinal, string name, bool isUsed = true, bool includeTypeNames = false)
    {
        Ordinal = ordinal;
        _name = name;
        IsUsed = isUsed;
        _typeNames = includeTypeNames ? new List<string>() : null;
    }

    /// <summary>
    /// Ordinal of this field.
    /// </summary>
    public int Ordinal { get; }

    /// <summary>
    /// Specifies whether or not this field is used in a query.
    /// </summary>
    public bool IsUsed { get; }

    /// <summary>
    /// Name of this field.
    /// </summary>
    public string Name => _name ?? string.Empty;

    /// <summary>
    /// Specifies the full type name of this field.
    /// </summary>
    public string? TypeName => _typeNames is null ? null : string.Join( " | ", _typeNames );

    /// <summary>
    /// Collection of all distinct type names of values associated with this field.
    /// </summary>
    public ReadOnlySpan<string> TypeNames => CollectionsMarshal.AsSpan( _typeNames );

    /// <summary>
    /// Returns a string representation of this <see cref="SqlResultSetField"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var typeName = TypeName;
        var unusedText = IsUsed ? string.Empty : " (unused)";
        return typeName is null
            ? $"[{Ordinal}] '{Name}'{unusedText}"
            : $"[{Ordinal}] '{Name}' : {(typeName.Length > 0 ? typeName : "?")}{unusedText}";
    }

    /// <summary>
    /// Attempts to add a type name.
    /// </summary>
    /// <param name="name">Type's name.</param>
    /// <returns><b>true</b> when type name was added, otherwise <b>false</b>.</returns>
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
