using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Builders;

public interface ISqlForeignKeyBuilderCollection : IReadOnlyCollection<ISqlForeignKeyBuilder>
{
    ISqlTableBuilder Table { get; }

    [Pure]
    bool Contains(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex);

    [Pure]
    ISqlForeignKeyBuilder Get(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex);

    bool TryGet(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex, [MaybeNullWhen( false )] out ISqlForeignKeyBuilder result);

    ISqlForeignKeyBuilder Create(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex);
    ISqlForeignKeyBuilder GetOrCreate(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex);
    bool Remove(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex);
}
