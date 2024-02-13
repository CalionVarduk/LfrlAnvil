﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlSchemaBuilderCollection : IReadOnlyCollection<ISqlSchemaBuilder>
{
    ISqlDatabaseBuilder Database { get; }
    ISqlSchemaBuilder Default { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlSchemaBuilder Get(string name);

    [Pure]
    ISqlSchemaBuilder? TryGet(string name);

    ISqlSchemaBuilder Create(string name);
    ISqlSchemaBuilder GetOrCreate(string name);
    bool Remove(string name);
}
