using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlPrimaryKey : SqlPrimaryKey
{
    internal PostgreSqlPrimaryKey(PostgreSqlIndex index, PostgreSqlPrimaryKeyBuilder builder)
        : base( index, builder ) { }

    public new PostgreSqlIndex Index => ReinterpretCast.To<PostgreSqlIndex>( base.Index );
    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
