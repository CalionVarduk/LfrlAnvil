using System.Collections.Generic;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL index constraint.
/// </summary>
public interface ISqlIndex : ISqlConstraint
{
    /// <summary>
    /// Collection of columns that define this index.
    /// </summary>
    IReadOnlyList<SqlIndexed<ISqlColumn>> Columns { get; }

    /// <summary>
    /// Specifies whether or not this index is unique.
    /// </summary>
    bool IsUnique { get; }

    /// <summary>
    /// Specifies whether or not this index is partial.
    /// </summary>
    bool IsPartial { get; }

    /// <summary>
    /// Specifies whether or not this index is virtual.
    /// </summary>
    /// <remarks>Virtual indexes aren't actually created in the database.</remarks>
    bool IsVirtual { get; }
}
