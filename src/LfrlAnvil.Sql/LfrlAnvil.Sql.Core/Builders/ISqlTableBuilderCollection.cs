using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Builders;

public interface ISqlObjectBuilderCollection : IReadOnlyCollection<ISqlObjectBuilder>
{
    ISqlSchemaBuilder Schema { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlObjectBuilder Get(string name);

    bool TryGet(string name, [MaybeNullWhen( false )] out ISqlObjectBuilder result);

    [Pure]
    ISqlTableBuilder GetTable(string name);

    bool TryGetTable(string name, [MaybeNullWhen( false )] out ISqlTableBuilder result);

    [Pure]
    ISqlIndexBuilder GetIndex(string name);

    bool TryGetIndex(string name, [MaybeNullWhen( false )] out ISqlIndexBuilder result);

    [Pure]
    ISqlPrimaryKeyBuilder GetPrimaryKey(string name);

    bool TryGetPrimaryKey(string name, [MaybeNullWhen( false )] out ISqlPrimaryKeyBuilder result);

    [Pure]
    ISqlForeignKeyBuilder GetForeignKey(string name);

    bool TryGetForeignKey(string name, [MaybeNullWhen( false )] out ISqlForeignKeyBuilder result);

    ISqlTableBuilder CreateTable(string name);
    ISqlTableBuilder GetOrCreateTable(string name);
    bool Remove(string name);
}
