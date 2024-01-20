using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlPrimaryKey : MySqlObject, ISqlPrimaryKey
{
    internal MySqlPrimaryKey(MySqlIndex index, MySqlPrimaryKeyBuilder builder)
        : base( builder )
    {
        Index = index;
        FullName = builder.FullName;
    }

    public MySqlIndex Index { get; }
    public override string FullName { get; }
    public override MySqlDatabase Database => Index.Database;

    ISqlIndex ISqlPrimaryKey.Index => Index;
}
