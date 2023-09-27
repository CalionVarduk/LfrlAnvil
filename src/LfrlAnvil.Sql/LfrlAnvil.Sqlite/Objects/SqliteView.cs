using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteView : SqliteObject, ISqlView
{
    internal SqliteView(SqliteSchema schema, SqliteViewBuilder builder)
        : base( builder )
    {
        Schema = schema;
        FullName = builder.FullName;
        DataFields = new SqliteViewDataFieldCollection( this, builder.Source );
    }

    public SqliteSchema Schema { get; }
    public SqliteViewDataFieldCollection DataFields { get; }
    public override string FullName { get; }
    public override SqliteDatabase Database => Schema.Database;

    ISqlSchema ISqlView.Schema => Schema;
    ISqlViewDataFieldCollection ISqlView.DataFields => DataFields;
}
