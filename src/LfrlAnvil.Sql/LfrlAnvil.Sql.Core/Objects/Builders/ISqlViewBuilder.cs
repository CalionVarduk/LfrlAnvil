using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlViewBuilder : ISqlObjectBuilder
{
    ISqlSchemaBuilder Schema { get; }
    SqlQueryExpressionNode Source { get; }
    IReadOnlyCollection<ISqlObjectBuilder> ReferencedObjects { get; }
    IReadOnlyCollection<ISqlViewBuilder> ReferencingViews { get; }
    SqlRecordSetInfo Info { get; }
    SqlViewBuilderNode RecordSet { get; }

    new ISqlViewBuilder SetName(string name);
}
