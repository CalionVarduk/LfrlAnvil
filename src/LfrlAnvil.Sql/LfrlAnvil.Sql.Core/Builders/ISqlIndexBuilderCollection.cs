using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Builders;

public interface ISqlIndexBuilderCollection : IReadOnlyCollection<ISqlIndexBuilder>
{
    ISqlTableBuilder Table { get; }

    [Pure]
    bool Contains(ReadOnlyMemory<ISqlIndexColumnBuilder> columns);

    [Pure]
    ISqlIndexBuilder Get(ReadOnlyMemory<ISqlIndexColumnBuilder> columns);

    bool TryGet(ReadOnlyMemory<ISqlIndexColumnBuilder> columns, [MaybeNullWhen( false )] out ISqlIndexBuilder result);

    ISqlIndexBuilder Create(ReadOnlyMemory<ISqlIndexColumnBuilder> columns);
    ISqlIndexBuilder GetOrCreate(ReadOnlyMemory<ISqlIndexColumnBuilder> columns);
    bool Remove(ReadOnlyMemory<ISqlIndexColumnBuilder> columns);
}
