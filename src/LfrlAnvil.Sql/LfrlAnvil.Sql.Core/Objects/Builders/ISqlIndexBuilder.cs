using System;
using System.Collections.Generic;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlIndexBuilder : ISqlObjectBuilder
{
    ISqlTableBuilder Table { get; }
    ReadOnlyMemory<ISqlIndexColumnBuilder> Columns { get; }
    IReadOnlyCollection<ISqlForeignKeyBuilder> ReferencingForeignKeys { get; }
    IReadOnlyCollection<ISqlForeignKeyBuilder> ForeignKeys { get; }
    ISqlPrimaryKeyBuilder? PrimaryKey { get; }
    bool IsUnique { get; }

    ISqlIndexBuilder MarkAsUnique(bool enabled = true);
    ISqlIndexBuilder SetDefaultName();
    new ISqlIndexBuilder SetName(string name);
}
