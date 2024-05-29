using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlDefaultObjectNameProvider" />
public class SqlDefaultObjectNameProvider : ISqlDefaultObjectNameProvider
{
    /// <inheritdoc />
    [Pure]
    public virtual string GetForPrimaryKey(ISqlTableBuilder table)
    {
        return SqlHelpers.GetDefaultPrimaryKeyName( table );
    }

    /// <inheritdoc />
    [Pure]
    public virtual string GetForForeignKey(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex)
    {
        return SqlHelpers.GetDefaultForeignKeyName( originIndex, referencedIndex );
    }

    /// <inheritdoc />
    [Pure]
    public virtual string GetForCheck(ISqlTableBuilder table)
    {
        return SqlHelpers.GetDefaultCheckName( table );
    }

    /// <inheritdoc />
    [Pure]
    public virtual string GetForIndex(ISqlTableBuilder table, SqlIndexBuilderColumns<ISqlColumnBuilder> columns, bool isUnique)
    {
        return SqlHelpers.GetDefaultIndexName( table, columns, isUnique );
    }
}
