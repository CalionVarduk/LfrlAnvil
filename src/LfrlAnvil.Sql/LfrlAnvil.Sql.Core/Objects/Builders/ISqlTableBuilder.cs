using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlTableBuilder : ISqlObjectBuilder
{
    ISqlSchemaBuilder Schema { get; }
    ISqlColumnBuilderCollection Columns { get; }
    ISqlConstraintBuilderCollection Constraints { get; }
    IReadOnlyCollection<ISqlViewBuilder> ReferencingViews { get; }
    SqlRecordSetInfo Info { get; }
    SqlTableBuilderNode RecordSet { get; }

    new ISqlTableBuilder SetName(string name);
}
