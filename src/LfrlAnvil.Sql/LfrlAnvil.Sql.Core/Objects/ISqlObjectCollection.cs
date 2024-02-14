using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlObjectCollection : IReadOnlyCollection<ISqlObject>
{
    ISqlSchema Schema { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlObject Get(string name);

    [Pure]
    ISqlObject? TryGet(string name);

    [Pure]
    ISqlTable GetTable(string name);

    [Pure]
    ISqlTable? TryGetTable(string name);

    [Pure]
    ISqlIndex GetIndex(string name);

    [Pure]
    ISqlIndex? TryGetIndex(string name);

    [Pure]
    ISqlPrimaryKey GetPrimaryKey(string name);

    [Pure]
    ISqlPrimaryKey? TryGetPrimaryKey(string name);

    [Pure]
    ISqlForeignKey GetForeignKey(string name);

    [Pure]
    ISqlForeignKey? TryGetForeignKey(string name);

    [Pure]
    ISqlCheck GetCheck(string name);

    [Pure]
    ISqlCheck? TryGetCheck(string name);

    [Pure]
    ISqlView GetView(string name);

    [Pure]
    ISqlView? TryGetView(string name);
}
