using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlObjectCollection : IReadOnlyCollection<ISqlObject>
{
    ISqlSchema Schema { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlObject Get(string name);

    bool TryGet(string name, [MaybeNullWhen( false )] out ISqlObject result);

    [Pure]
    ISqlTable GetTable(string name);

    bool TryGetTable(string name, [MaybeNullWhen( false )] out ISqlTable result);

    [Pure]
    ISqlIndex GetIndex(string name);

    bool TryGetIndex(string name, [MaybeNullWhen( false )] out ISqlIndex result);

    [Pure]
    ISqlPrimaryKey GetPrimaryKey(string name);

    bool TryGetPrimaryKey(string name, [MaybeNullWhen( false )] out ISqlPrimaryKey result);

    [Pure]
    ISqlForeignKey GetForeignKey(string name);

    bool TryGetForeignKey(string name, [MaybeNullWhen( false )] out ISqlForeignKey result);

    [Pure]
    ISqlCheck GetCheck(string name);

    bool TryGetCheck(string name, [MaybeNullWhen( false )] out ISqlCheck result);

    [Pure]
    ISqlView GetView(string name);

    bool TryGetView(string name, [MaybeNullWhen( false )] out ISqlView result);
}
