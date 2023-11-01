using System;
using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlIndexBuilder : ISqlObjectBuilder
{
    ISqlTableBuilder Table { get; }
    ReadOnlyMemory<ISqlIndexColumnBuilder> Columns { get; }
    IReadOnlyCollection<ISqlForeignKeyBuilder> ReferencingForeignKeys { get; }
    IReadOnlyCollection<ISqlForeignKeyBuilder> OriginatingForeignKeys { get; }
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedFilterColumns { get; }
    ISqlPrimaryKeyBuilder? PrimaryKey { get; }
    bool IsUnique { get; }
    SqlConditionNode? Filter { get; }

    ISqlIndexBuilder MarkAsUnique(bool enabled = true);
    ISqlIndexBuilder SetFilter(SqlConditionNode? filter);
    ISqlIndexBuilder SetDefaultName();
    new ISqlIndexBuilder SetName(string name);
}
