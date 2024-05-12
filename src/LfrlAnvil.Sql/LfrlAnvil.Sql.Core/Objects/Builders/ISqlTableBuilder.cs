using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL table builder.
/// </summary>
public interface ISqlTableBuilder : ISqlObjectBuilder
{
    /// <summary>
    /// Schema that this table belongs to.
    /// </summary>
    ISqlSchemaBuilder Schema { get; }

    /// <summary>
    /// Collection of columns that belong to this table.
    /// </summary>
    ISqlColumnBuilderCollection Columns { get; }

    /// <summary>
    /// Collection of constraints that belong to this table.
    /// </summary>
    ISqlConstraintBuilderCollection Constraints { get; }

    /// <summary>
    /// Represents a full name information of this table.
    /// </summary>
    SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Underlying <see cref="SqlTableBuilderNode"/> instance that represents this table.
    /// </summary>
    SqlTableBuilderNode Node { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlTableBuilder SetName(string name);
}
