using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL table column builder.
/// </summary>
public interface ISqlColumnBuilder : ISqlObjectBuilder
{
    /// <summary>
    /// Table that this column belongs to.
    /// </summary>
    ISqlTableBuilder Table { get; }

    /// <summary>
    /// <see cref="ISqlColumnTypeDefinition"/> instance that defines the data type of this column.
    /// </summary>
    ISqlColumnTypeDefinition TypeDefinition { get; }

    /// <summary>
    /// Specifies whether or not this column accepts null values.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    /// Underlying optional default value expression.
    /// </summary>
    SqlExpressionNode? DefaultValue { get; }

    /// <summary>
    /// Underlying optional <see cref="SqlColumnComputation"/> of this column.
    /// </summary>
    SqlColumnComputation? Computation { get; }

    /// <summary>
    /// Collection of columns referenced by this column's <see cref="Computation"/>.
    /// </summary>
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedComputationColumns { get; }

    /// <summary>
    /// Underlying <see cref="SqlColumnBuilderNode"/> instance that represents this column.
    /// </summary>
    SqlColumnBuilderNode Node { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlColumnBuilder SetName(string name);

    /// <summary>
    /// Changes <see cref="IsNullable"/> value of this column.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When nullability cannot be changed.</exception>
    ISqlColumnBuilder MarkAsNullable(bool enabled = true);

    /// <summary>
    /// Changes <see cref="TypeDefinition"/> value of this column.
    /// </summary>
    /// <param name="definition">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="DefaultValue"/> to null.</remarks>
    ISqlColumnBuilder SetType(ISqlColumnTypeDefinition definition);

    /// <summary>
    /// Changes <see cref="DefaultValue"/> value of this column.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When default value cannot be changed.</exception>
    ISqlColumnBuilder SetDefaultValue(SqlExpressionNode? value);

    /// <summary>
    /// Changes <see cref="Computation"/> value of this column.
    /// </summary>
    /// <param name="computation">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When computation cannot be changed.</exception>
    /// <remarks>Changing the computation to non-null will reset the <see cref="DefaultValue"/> to null.</remarks>
    ISqlColumnBuilder SetComputation(SqlColumnComputation? computation);

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance from this column with <see cref="OrderBy.Asc"/> ordering.
    /// </summary>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    SqlOrderByNode Asc();

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance from this column with <see cref="OrderBy.Desc"/> ordering.
    /// </summary>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    SqlOrderByNode Desc();
}
