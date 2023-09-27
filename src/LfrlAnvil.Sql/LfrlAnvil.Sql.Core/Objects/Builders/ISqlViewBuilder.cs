using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlViewBuilder : ISqlObjectBuilder
{
    ISqlSchemaBuilder Schema { get; }
    SqlQueryExpressionNode Source { get; }
    IReadOnlyCollection<ISqlObjectBuilder> ReferencedObjects { get; }
    IReadOnlyCollection<ISqlViewBuilder> ReferencingViews { get; }

    new ISqlViewBuilder SetName(string name);
}
