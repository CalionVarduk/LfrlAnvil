using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlObjectBuilderCollection : IReadOnlyCollection<ISqlObjectBuilder>
{
    ISqlSchemaBuilder Schema { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlObjectBuilder GetObject(string name);

    [Pure]
    ISqlObjectBuilder? TryGetObject(string name);

    [Pure]
    ISqlTableBuilder GetTable(string name);

    [Pure]
    ISqlTableBuilder? TryGetTable(string name);

    [Pure]
    ISqlIndexBuilder GetIndex(string name);

    [Pure]
    ISqlIndexBuilder? TryGetIndex(string name);

    [Pure]
    ISqlPrimaryKeyBuilder GetPrimaryKey(string name);

    [Pure]
    ISqlPrimaryKeyBuilder? TryGetPrimaryKey(string name);

    [Pure]
    ISqlForeignKeyBuilder GetForeignKey(string name);

    [Pure]
    ISqlForeignKeyBuilder? TryGetForeignKey(string name);

    [Pure]
    ISqlCheckBuilder GetCheck(string name);

    [Pure]
    ISqlCheckBuilder? TryGetCheck(string name);

    [Pure]
    ISqlViewBuilder GetView(string name);

    [Pure]
    ISqlViewBuilder? TryGetView(string name);

    ISqlTableBuilder CreateTable(string name);
    ISqlTableBuilder GetOrCreateTable(string name);
    ISqlViewBuilder CreateView(string name, SqlQueryExpressionNode source);
    bool Remove(string name);
}
