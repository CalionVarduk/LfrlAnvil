using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqlitePrimaryKey : SqliteObject, ISqlPrimaryKey
{
    internal SqlitePrimaryKey(SqliteIndex index, SqlitePrimaryKeyBuilder builder)
        : base( builder )
    {
        Index = index;
        FullName = builder.FullName;
    }

    public SqliteIndex Index { get; }
    public override string FullName { get; }
    public override SqliteDatabase Database => Index.Database;

    ISqlIndex ISqlPrimaryKey.Index => Index;
}
