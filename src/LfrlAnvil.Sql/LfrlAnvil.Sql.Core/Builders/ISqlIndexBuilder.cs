﻿using System.Collections.Generic;

namespace LfrlAnvil.Sql.Builders;

public interface ISqlIndexBuilder : ISqlObjectBuilder
{
    ISqlTableBuilder Table { get; }
    IReadOnlyList<ISqlIndexColumnBuilder> Columns { get; }
    IReadOnlyCollection<ISqlForeignKeyBuilder> ReferencingForeignKeys { get; }
    IReadOnlyCollection<ISqlForeignKeyBuilder> ForeignKeys { get; }
    ISqlPrimaryKeyBuilder? PrimaryKey { get; }
    bool IsUnique { get; }

    ISqlIndexBuilder MarkAsUnique(bool enabled = true);
    ISqlIndexBuilder SetDefaultName();
    new ISqlIndexBuilder SetName(string name);
}