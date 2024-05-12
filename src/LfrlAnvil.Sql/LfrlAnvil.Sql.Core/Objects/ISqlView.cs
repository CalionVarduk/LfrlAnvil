using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL view.
/// </summary>
public interface ISqlView : ISqlObject
{
    /// <summary>
    /// Schema that this view belongs to.
    /// </summary>
    ISqlSchema Schema { get; }

    /// <summary>
    /// Collection of data fields that belong to this view.
    /// </summary>
    ISqlViewDataFieldCollection DataFields { get; }

    /// <summary>
    /// Represents a full name information of this view.
    /// </summary>
    SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Underlying <see cref="SqlViewNode"/> instance that represents this view.
    /// </summary>
    SqlViewNode Node { get; }
}
