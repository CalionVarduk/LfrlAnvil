using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteView : SqliteObject, ISqlView
{
    private SqlRecordSetInfo? _info;
    private SqlViewNode? _recordSet;

    internal SqliteView(SqliteSchema schema, SqliteViewBuilder builder)
        : base( builder )
    {
        Schema = schema;
        FullName = builder.FullName;
        DataFields = new SqliteViewDataFieldCollection( this, builder.Source );
        _info = builder.GetCachedInfo();
        _recordSet = null;
    }

    public SqliteSchema Schema { get; }
    public SqliteViewDataFieldCollection DataFields { get; }
    public override string FullName { get; }
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlViewNode RecordSet => _recordSet ??= SqlNode.View( this );
    public override SqliteDatabase Database => Schema.Database;

    ISqlSchema ISqlView.Schema => Schema;
    ISqlViewDataFieldCollection ISqlView.DataFields => DataFields;
}
