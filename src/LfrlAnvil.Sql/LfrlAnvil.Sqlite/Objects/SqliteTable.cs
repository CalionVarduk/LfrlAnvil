using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteTable : SqliteObject, ISqlTable
{
    private SqlitePrimaryKey? _primaryKey;
    private SqlRecordSetInfo? _info;
    private SqlTableNode? _recordSet;

    internal SqliteTable(SqliteSchema schema, SqliteTableBuilder builder)
        : base( builder )
    {
        Schema = schema;
        FullName = builder.FullName;
        _primaryKey = null;
        Columns = new SqliteColumnCollection( this, builder.Columns );
        Indexes = new SqliteIndexCollection( this, builder.Indexes );
        ForeignKeys = new SqliteForeignKeyCollection( this, builder.ForeignKeys.Count );
        _info = builder.GetCachedInfo();
        _recordSet = null;
    }

    public SqliteSchema Schema { get; }
    public SqliteColumnCollection Columns { get; }
    public SqliteIndexCollection Indexes { get; }
    public SqliteForeignKeyCollection ForeignKeys { get; }
    public override string FullName { get; }
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlTableNode RecordSet => _recordSet ??= SqlNode.Table( this );

    public SqlitePrimaryKey PrimaryKey
    {
        get
        {
            Assume.IsNotNull( _primaryKey );
            return _primaryKey;
        }
    }

    public override SqliteDatabase Database => Schema.Database;

    ISqlSchema ISqlTable.Schema => Schema;
    ISqlPrimaryKey ISqlTable.PrimaryKey => PrimaryKey;
    ISqlColumnCollection ISqlTable.Columns => Columns;
    ISqlIndexCollection ISqlTable.Indexes => Indexes;
    ISqlForeignKeyCollection ISqlTable.ForeignKeys => ForeignKeys;

    internal void SetPrimaryKey(SqliteTableBuilder builder)
    {
        Assume.IsNull( _primaryKey );

        if ( builder.PrimaryKey is null )
            throw new SqliteObjectBuilderException( ExceptionResources.PrimaryKeyIsMissing( builder ) );

        var index = Schema.Objects.GetIndex( builder.PrimaryKey.Index.Name );
        _primaryKey = new SqlitePrimaryKey( index, builder.PrimaryKey );
    }
}
