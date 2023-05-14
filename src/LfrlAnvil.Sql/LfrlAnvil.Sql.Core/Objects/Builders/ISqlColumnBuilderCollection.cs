using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    bool TryGet(string name, [MaybeNullWhen( false )] out ISqlColumnBuilder result);

    ISqlColumnBuilderCollection SetDefaultTypeDefinition(ISqlColumnTypeDefinition definition);
    ISqlColumnBuilder Create(string name);
    ISqlColumnBuilder GetOrCreate(string name);
    bool Remove(string name);
}
