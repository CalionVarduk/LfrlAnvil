using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlColumnBuilder : ISqlObjectBuilder
{
    ISqlTableBuilder Table { get; }
    ISqlColumnTypeDefinition TypeDefinition { get; }
    bool IsNullable { get; }
    SqlExpressionNode? DefaultValue { get; }
    SqlColumnComputation? Computation { get; }
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedComputationColumns { get; }
    SqlColumnBuilderNode Node { get; }

    new ISqlColumnBuilder SetName(string name);
    ISqlColumnBuilder MarkAsNullable(bool enabled = true);
    ISqlColumnBuilder SetType(ISqlColumnTypeDefinition definition);
    ISqlColumnBuilder SetDefaultValue(SqlExpressionNode? value);
    ISqlColumnBuilder SetComputation(SqlColumnComputation? computation);

    [Pure]
    SqlOrderByNode Asc();

    [Pure]
    SqlOrderByNode Desc();
}
