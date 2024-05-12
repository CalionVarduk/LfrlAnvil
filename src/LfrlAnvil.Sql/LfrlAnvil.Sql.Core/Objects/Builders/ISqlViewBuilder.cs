using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL view builder.
/// </summary>
public interface ISqlViewBuilder : ISqlObjectBuilder
{
    /// <summary>
    /// Schema that this view belongs to.
    /// </summary>
    ISqlSchemaBuilder Schema { get; }

    /// <summary>
    /// Underlying source query expression that defines this view.
    /// </summary>
    SqlQueryExpressionNode Source { get; }

    /// <summary>
    /// Collection of objects (tables, views and columns) referenced by this view's <see cref="Source"/>.
    /// </summary>
    IReadOnlyCollection<ISqlObjectBuilder> ReferencedObjects { get; }

    /// <summary>
    /// Represents a full name information of this view.
    /// </summary>
    SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Underlying <see cref="SqlViewBuilderNode"/> instance that represents this view.
    /// </summary>
    SqlViewBuilderNode Node { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlViewBuilder SetName(string name);
}
