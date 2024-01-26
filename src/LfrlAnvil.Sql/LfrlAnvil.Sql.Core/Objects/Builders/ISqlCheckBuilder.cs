using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlCheckBuilder : ISqlConstraintBuilder
{
    SqlConditionNode Condition { get; }
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedColumns { get; }

    new ISqlCheckBuilder SetName(string name);
    new ISqlCheckBuilder SetDefaultName();
}
