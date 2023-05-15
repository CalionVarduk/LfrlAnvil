using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlForeignKeyCollection : IReadOnlyCollection<ISqlForeignKey>
{
    ISqlTable Table { get; }

    [Pure]
    bool Contains(ISqlIndex index, ISqlIndex referencedIndex);

    [Pure]
    ISqlForeignKey Get(ISqlIndex index, ISqlIndex referencedIndex);

    bool TryGet(ISqlIndex index, ISqlIndex referencedIndex, [MaybeNullWhen( false )] out ISqlForeignKey result);
}
