using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

public readonly struct SqlDataTypeParameter
{
    private readonly string? _name;

    public SqlDataTypeParameter(string name, Bounds<int> bounds)
    {
        _name = name;
        Bounds = bounds;
    }

    public Bounds<int> Bounds { get; }
    public string Name => _name ?? string.Empty;

    [Pure]
    public override string ToString()
    {
        return $"'{Name}' [{Bounds.Min}, {Bounds.Max}]";
    }
}
