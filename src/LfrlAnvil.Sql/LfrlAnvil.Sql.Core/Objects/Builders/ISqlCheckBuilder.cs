using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlCheckBuilder : ISqlObjectBuilder
{
    ISqlTableBuilder Table { get; }
    SqlConditionNode Condition { get; }
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedColumns { get; }

    new ISqlCheckBuilder SetName(string name);
}
