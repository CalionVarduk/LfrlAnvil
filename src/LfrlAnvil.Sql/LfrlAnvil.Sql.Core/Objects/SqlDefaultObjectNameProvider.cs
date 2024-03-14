using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public class SqlDefaultObjectNameProvider : ISqlDefaultObjectNameProvider
{
    [Pure]
    public virtual string GetForPrimaryKey(ISqlTableBuilder table)
    {
        return SqlHelpers.GetDefaultPrimaryKeyName( table );
    }

    [Pure]
    public virtual string GetForForeignKey(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex)
    {
        return SqlHelpers.GetDefaultForeignKeyName( originIndex, referencedIndex );
    }

    [Pure]
    public virtual string GetForCheck(ISqlTableBuilder table)
    {
        return SqlHelpers.GetDefaultCheckName( table );
    }

    [Pure]
    public virtual string GetForIndex(ISqlTableBuilder table, SqlIndexBuilderColumns<ISqlColumnBuilder> columns, bool isUnique)
    {
        return SqlHelpers.GetDefaultIndexName( table, columns, isUnique );
    }
}
