using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL check constraint builder.
/// </summary>
public interface ISqlCheckBuilder : ISqlConstraintBuilder
{
    /// <summary>
    /// Underlying condition of this check constraint.
    /// </summary>
    SqlConditionNode Condition { get; }

    /// <summary>
    /// Collection of columns referenced by this check constraint.
    /// </summary>
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedColumns { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlCheckBuilder SetName(string name);

    /// <inheritdoc cref="ISqlConstraintBuilder.SetDefaultName()" />
    new ISqlCheckBuilder SetDefaultName();
}
