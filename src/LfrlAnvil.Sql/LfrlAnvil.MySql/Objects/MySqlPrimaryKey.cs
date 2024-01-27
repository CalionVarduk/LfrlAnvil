using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlPrimaryKey : MySqlConstraint, ISqlPrimaryKey
{
    internal MySqlPrimaryKey(MySqlIndex index, MySqlPrimaryKeyBuilder builder)
        : base( index.Table, builder )
    {
        Index = index;
    }

    public MySqlIndex Index { get; }
    public override MySqlDatabase Database => Index.Database;

    ISqlIndex ISqlPrimaryKey.Index => Index;
}
