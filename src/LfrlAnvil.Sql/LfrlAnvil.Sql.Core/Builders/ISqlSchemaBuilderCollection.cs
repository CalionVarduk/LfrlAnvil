using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Builders;

public interface ISqlSchemaBuilderCollection : IReadOnlyCollection<ISqlSchemaBuilder>
{
    ISqlDatabaseBuilder Database { get; }
    ISqlSchemaBuilder Default { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlSchemaBuilder Get(string name);

    bool TryGet(string name, [MaybeNullWhen( false )] out ISqlSchemaBuilder result);

    ISqlSchemaBuilder Create(string name);
    ISqlSchemaBuilder GetOrCreate(string name);
    bool Remove(string name);
}
