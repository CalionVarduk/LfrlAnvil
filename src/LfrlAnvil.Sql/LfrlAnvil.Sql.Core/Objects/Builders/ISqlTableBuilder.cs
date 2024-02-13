using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlTableBuilder : ISqlObjectBuilder
{
    ISqlSchemaBuilder Schema { get; }
    ISqlColumnBuilderCollection Columns { get; }
    ISqlConstraintBuilderCollection Constraints { get; }
    SqlRecordSetInfo Info { get; }
    SqlTableBuilderNode Node { get; }

    new ISqlTableBuilder SetName(string name);
}
