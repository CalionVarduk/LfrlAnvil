using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteTable : SqliteObject, ISqlTable
{
    private SqlRecordSetInfo? _info;
    private SqlTableNode? _recordSet;

    internal SqliteTable(SqliteSchema schema, SqliteTableBuilder builder)
        : base( builder )
    {
        Schema = schema;
        FullName = builder.FullName;
        Columns = new SqliteColumnCollection( this, builder.Columns );
        Constraints = new SqliteConstraintCollection( this, builder.Constraints.Count );
        _info = builder.GetCachedInfo();
        _recordSet = null;
    }

    public SqliteSchema Schema { get; }
    public SqliteColumnCollection Columns { get; }
    public SqliteConstraintCollection Constraints { get; }
    public override string FullName { get; }
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlTableNode RecordSet => _recordSet ??= SqlNode.Table( this );
    public override SqliteDatabase Database => Schema.Database;

    ISqlSchema ISqlTable.Schema => Schema;
    ISqlColumnCollection ISqlTable.Columns => Columns;
    ISqlConstraintCollection ISqlTable.Constraints => Constraints;
}
