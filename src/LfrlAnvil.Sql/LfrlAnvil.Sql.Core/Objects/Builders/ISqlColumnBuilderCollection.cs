using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlColumnBuilderCollection : IReadOnlyCollection<ISqlColumnBuilder>
{
    ISqlTableBuilder Table { get; }
    ISqlColumnTypeDefinition DefaultTypeDefinition { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlColumnBuilder Get(string name);

    [Pure]
    ISqlColumnBuilder? TryGet(string name);

    ISqlColumnBuilderCollection SetDefaultTypeDefinition(ISqlColumnTypeDefinition definition);
    ISqlColumnBuilder Create(string name);
    ISqlColumnBuilder GetOrCreate(string name);
    bool Remove(string name);
}
