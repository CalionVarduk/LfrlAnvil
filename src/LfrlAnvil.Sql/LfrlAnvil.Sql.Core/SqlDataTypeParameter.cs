using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents an <see cref="ISqlDataType"/> parameter definition.
/// </summary>
public readonly struct SqlDataTypeParameter
{
    private readonly string? _name;

    /// <summary>
    /// Creates a new <see cref="SqlDataTypeParameter"/> instance.
    /// </summary>
    /// <param name="name">Parameter's name.</param>
    /// <param name="bounds">Range of valid values for this parameter.</param>
    public SqlDataTypeParameter(string name, Bounds<int> bounds)
    {
        _name = name;
        Bounds = bounds;
    }

    /// <summary>
    /// Range of valid values for this parameter.
    /// </summary>
    public Bounds<int> Bounds { get; }

    /// <summary>
    /// Parameter's name.
    /// </summary>
    public string Name => _name ?? string.Empty;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlDataTypeParameter"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"'{Name}' [{Bounds.Min}, {Bounds.Max}]";
    }
}
