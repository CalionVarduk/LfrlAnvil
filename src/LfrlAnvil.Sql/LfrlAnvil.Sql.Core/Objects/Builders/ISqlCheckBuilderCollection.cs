using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlCheckBuilderCollection : IReadOnlyCollection<ISqlCheckBuilder>
{
    ISqlTableBuilder Table { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlCheckBuilder Get(string name);

    bool TryGet(string name, [MaybeNullWhen( false )] out ISqlCheckBuilder result);

    ISqlCheckBuilder Create(SqlConditionNode condition);
    bool Remove(string name);
}
