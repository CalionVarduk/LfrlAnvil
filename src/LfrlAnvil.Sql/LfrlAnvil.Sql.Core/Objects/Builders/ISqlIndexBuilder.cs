using System.Collections.Generic;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL index constraint builder.
/// </summary>
public interface ISqlIndexBuilder : ISqlConstraintBuilder
{
    /// <summary>
    /// Collection of columns that define this index.
    /// </summary>
    SqlIndexBuilderColumns<ISqlColumnBuilder> Columns { get; }

    /// <summary>
    /// Collection of columns referenced by this index's <see cref="Columns"/>.
    /// </summary>
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedColumns { get; }

    /// <summary>
    /// Collection of columns referenced by this index's <see cref="Filter"/>.
    /// </summary>
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedFilterColumns { get; }

    /// <summary>
    /// Optional <see cref="ISqlPrimaryKeyBuilder"/> instance attached to this index.
    /// </summary>
    ISqlPrimaryKeyBuilder? PrimaryKey { get; }

    /// <summary>
    /// Specifies whether or not this index is unique.
    /// </summary>
    bool IsUnique { get; }

    /// <summary>
    /// Specifies whether or not this index is virtual.
    /// </summary>
    /// <remarks>Virtual indexes aren't actually created in the database.</remarks>
    bool IsVirtual { get; }

    /// <summary>
    /// Specifies an optional filter condition of this index.
    /// </summary>
    SqlConditionNode? Filter { get; }

    /// <summary>
    /// Changes <see cref="IsUnique"/> value of this index.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When uniqueness cannot be changed.</exception>
    ISqlIndexBuilder MarkAsUnique(bool enabled = true);

    /// <summary>
    /// Changes <see cref="IsVirtual"/> value of this index.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When virtuality cannot be changed.</exception>
    ISqlIndexBuilder MarkAsVirtual(bool enabled = true);

    /// <summary>
    /// Changes <see cref="Filter"/> value of this index.
    /// </summary>
    /// <param name="filter">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When filter cannot be changed.</exception>
    ISqlIndexBuilder SetFilter(SqlConditionNode? filter);

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlIndexBuilder SetName(string name);

    /// <inheritdoc cref="ISqlConstraintBuilder.SetDefaultName()" />
    new ISqlIndexBuilder SetDefaultName();
}
