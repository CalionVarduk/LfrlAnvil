using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlForeignKeyBuilderCollection : IReadOnlyCollection<ISqlForeignKeyBuilder>
{
    ISqlTableBuilder Table { get; }

    [Pure]
    bool Contains(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);

    [Pure]
    ISqlForeignKeyBuilder Get(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);

    bool TryGet(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex, [MaybeNullWhen( false )] out ISqlForeignKeyBuilder result);

    ISqlForeignKeyBuilder Create(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);
    ISqlForeignKeyBuilder GetOrCreate(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);
    bool Remove(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);
}
