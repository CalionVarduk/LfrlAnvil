namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a single connection string entry.
/// </summary>
/// <param name="Key">Key of this entry.</param>
/// <param name="Value">Value of this entry.</param>
/// <param name="IsMutable">Specifies whether or not this entry can be changed.</param>
public readonly record struct SqlConnectionStringEntry(string Key, object Value, bool IsMutable);
