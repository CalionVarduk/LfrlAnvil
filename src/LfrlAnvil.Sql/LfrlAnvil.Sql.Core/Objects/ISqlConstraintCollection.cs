using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlConstraintCollection : IReadOnlyCollection<ISqlConstraint>
{
    ISqlTable Table { get; }
    ISqlPrimaryKey PrimaryKey { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlConstraint Get(string name);

    [Pure]
    ISqlConstraint? TryGet(string nam);

    [Pure]
    ISqlIndex GetIndex(string name);

    [Pure]
    ISqlIndex? TryGetIndex(string name);

    [Pure]
    ISqlForeignKey GetForeignKey(string name);

    [Pure]
    ISqlForeignKey? TryGetForeignKey(string name);

    [Pure]
    ISqlCheck GetCheck(string name);

    [Pure]
    ISqlCheck? TryGetCheck(string name);
}
