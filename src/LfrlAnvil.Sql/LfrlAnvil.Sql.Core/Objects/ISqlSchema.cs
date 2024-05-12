namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL schema.
/// </summary>
public interface ISqlSchema : ISqlObject
{
    /// <summary>
    /// Collection of objects that belong to this schema.
    /// </summary>
    ISqlObjectCollection Objects { get; }
}
