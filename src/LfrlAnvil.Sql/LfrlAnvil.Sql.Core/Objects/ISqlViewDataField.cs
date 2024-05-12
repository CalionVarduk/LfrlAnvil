using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL view data field.
/// </summary>
public interface ISqlViewDataField : ISqlObject
{
    /// <summary>
    /// View that this data field belongs to.
    /// </summary>
    ISqlView View { get; }

    /// <summary>
    /// Underlying <see cref="SqlViewDataFieldNode"/> instance that represents this data field.
    /// </summary>
    SqlViewDataFieldNode Node { get; }
}
