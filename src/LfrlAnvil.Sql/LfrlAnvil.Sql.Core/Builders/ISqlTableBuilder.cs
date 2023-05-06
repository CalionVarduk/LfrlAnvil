using System;

namespace LfrlAnvil.Sql.Builders;

public interface ISqlTableBuilder : ISqlObjectBuilder
{
    ISqlSchemaBuilder Schema { get; }
    ISqlPrimaryKeyBuilder? PrimaryKey { get; }
    ISqlColumnBuilderCollection Columns { get; }
    ISqlIndexBuilderCollection Indexes { get; }
    ISqlForeignKeyBuilderCollection ForeignKeys { get; }

    new ISqlTableBuilder SetName(string name);
    ISqlPrimaryKeyBuilder SetPrimaryKey(ReadOnlyMemory<ISqlIndexColumnBuilder> columns);
}
