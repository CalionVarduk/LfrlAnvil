using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlPrimaryKey : SqlPrimaryKey
{
    internal MySqlPrimaryKey(MySqlIndex index, MySqlPrimaryKeyBuilder builder)
        : base( index, builder ) { }

    public new MySqlIndex Index => ReinterpretCast.To<MySqlIndex>( base.Index );
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
