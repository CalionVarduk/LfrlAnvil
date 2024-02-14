using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlIndexBuilder : ISqlConstraintBuilder
{
    IReadOnlyList<SqlIndexColumnBuilder<ISqlColumnBuilder>> Columns { get; }
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedFilterColumns { get; }
    ISqlPrimaryKeyBuilder? PrimaryKey { get; }
    bool IsUnique { get; }
    SqlConditionNode? Filter { get; }

    ISqlIndexBuilder MarkAsUnique(bool enabled = true);
    ISqlIndexBuilder SetFilter(SqlConditionNode? filter);
    new ISqlIndexBuilder SetName(string name);
    new ISqlIndexBuilder SetDefaultName();
}
