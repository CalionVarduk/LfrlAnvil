using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL table.
/// </summary>
public interface ISqlTable : ISqlObject
{
    /// <summary>
    /// Schema that this table belongs to.
    /// </summary>
    ISqlSchema Schema { get; }

    /// <summary>
    /// Collection of columns that belong to this table.
    /// </summary>
    ISqlColumnCollection Columns { get; }

    /// <summary>
    /// Collection of constraints that belong to this table.
    /// </summary>
    ISqlConstraintCollection Constraints { get; }

    /// <summary>
    /// Represents a full name information of this table.
    /// </summary>
    SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Underlying <see cref="SqlTableNode"/> instance that represents this table.
    /// </summary>
    SqlTableNode Node { get; }
}
