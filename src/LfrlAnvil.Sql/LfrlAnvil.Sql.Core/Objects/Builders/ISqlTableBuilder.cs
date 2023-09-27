using System;
using System.Collections.Generic;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlTableBuilder : ISqlObjectBuilder
{
    ISqlSchemaBuilder Schema { get; }
    ISqlPrimaryKeyBuilder? PrimaryKey { get; }
    ISqlColumnBuilderCollection Columns { get; }
    ISqlIndexBuilderCollection Indexes { get; }
    ISqlForeignKeyBuilderCollection ForeignKeys { get; }
    IReadOnlyCollection<ISqlViewBuilder> ReferencingViews { get; }

    new ISqlTableBuilder SetName(string name);
    ISqlPrimaryKeyBuilder SetPrimaryKey(ReadOnlyMemory<ISqlIndexColumnBuilder> columns);
}
