using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlDefaultObjectNameProvider
{
    [Pure]
    string GetForPrimaryKey(ISqlTableBuilder table);

    [Pure]
    string GetForForeignKey(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);

    [Pure]
    string GetForCheck(ISqlTableBuilder table);

    [Pure]
    string GetForIndex(ISqlTableBuilder table, SqlIndexBuilderColumns<ISqlColumnBuilder> columns, bool isUnique);
}
