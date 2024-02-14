using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlPrimaryKey : SqlConstraint, ISqlPrimaryKey
{
    protected SqlPrimaryKey(SqlIndex index, SqlPrimaryKeyBuilder builder)
        : base( index.Table, builder )
    {
        Index = index;
    }

    public SqlIndex Index { get; }
    ISqlIndex ISqlPrimaryKey.Index => Index;
}
