using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlForeignKey : SqlForeignKey
{
    internal MySqlForeignKey(MySqlIndex originIndex, MySqlIndex referencedIndex, MySqlForeignKeyBuilder builder)
        : base( originIndex, referencedIndex, builder ) { }

    public new MySqlIndex OriginIndex => ReinterpretCast.To<MySqlIndex>( base.OriginIndex );
    public new MySqlIndex ReferencedIndex => ReinterpretCast.To<MySqlIndex>( base.ReferencedIndex );
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
