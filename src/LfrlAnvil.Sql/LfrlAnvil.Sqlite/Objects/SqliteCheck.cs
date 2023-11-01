using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteCheck : SqliteObject, ISqlCheck
{
    internal SqliteCheck(SqliteTable table, SqliteCheckBuilder builder)
        : base( builder )
    {
        Table = table;
        FullName = builder.FullName;
    }

    public SqliteTable Table { get; }
    public override string FullName { get; }
    public override SqliteDatabase Database => Table.Database;

    ISqlTable ISqlCheck.Table => Table;
}
