using System;
using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlTableBuilder : ISqlObjectBuilder
{
    ISqlSchemaBuilder Schema { get; }
    ISqlPrimaryKeyBuilder? PrimaryKey { get; }
    ISqlColumnBuilderCollection Columns { get; }
    ISqlIndexBuilderCollection Indexes { get; }
    ISqlForeignKeyBuilderCollection ForeignKeys { get; }
    IReadOnlyCollection<ISqlViewBuilder> ReferencingViews { get; }
    SqlRecordSetInfo Info { get; }
    SqlTableBuilderNode RecordSet { get; }

    new ISqlTableBuilder SetName(string name);
    ISqlPrimaryKeyBuilder SetPrimaryKey(ReadOnlyMemory<ISqlIndexColumnBuilder> columns);
}
