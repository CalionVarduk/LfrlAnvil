using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlForeignKey : SqlForeignKey
{
    internal PostgreSqlForeignKey(PostgreSqlIndex originIndex, PostgreSqlIndex referencedIndex, PostgreSqlForeignKeyBuilder builder)
        : base( originIndex, referencedIndex, builder ) { }

    public new PostgreSqlIndex OriginIndex => ReinterpretCast.To<PostgreSqlIndex>( base.OriginIndex );
    public new PostgreSqlIndex ReferencedIndex => ReinterpretCast.To<PostgreSqlIndex>( base.ReferencedIndex );
    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
