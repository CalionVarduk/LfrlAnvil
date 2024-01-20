using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlTable : MySqlObject, ISqlTable
{
    private MySqlPrimaryKey? _primaryKey;
    private SqlRecordSetInfo? _info;
    private SqlTableNode? _recordSet;

    internal MySqlTable(MySqlSchema schema, MySqlTableBuilder builder)
        : base( builder )
    {
        Schema = schema;
        FullName = builder.FullName;
        _primaryKey = null;
        Columns = new MySqlColumnCollection( this, builder.Columns );
        Indexes = new MySqlIndexCollection( this, builder.Indexes );
        ForeignKeys = new MySqlForeignKeyCollection( this, builder.ForeignKeys.Count );
        Checks = new MySqlCheckCollection( this, builder.Checks );
        _info = builder.GetCachedInfo();
        _recordSet = null;
    }

    public MySqlSchema Schema { get; }
    public MySqlColumnCollection Columns { get; }
    public MySqlIndexCollection Indexes { get; }
    public MySqlForeignKeyCollection ForeignKeys { get; }
    public MySqlCheckCollection Checks { get; }
    public override string FullName { get; }
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlTableNode RecordSet => _recordSet ??= SqlNode.Table( this );

    public MySqlPrimaryKey PrimaryKey
    {
        get
        {
            Assume.IsNotNull( _primaryKey );
            return _primaryKey;
        }
    }

    public override MySqlDatabase Database => Schema.Database;

    ISqlSchema ISqlTable.Schema => Schema;
    ISqlPrimaryKey ISqlTable.PrimaryKey => PrimaryKey;
    ISqlColumnCollection ISqlTable.Columns => Columns;
    ISqlIndexCollection ISqlTable.Indexes => Indexes;
    ISqlForeignKeyCollection ISqlTable.ForeignKeys => ForeignKeys;
    ISqlCheckCollection ISqlTable.Checks => Checks;

    internal void SetPrimaryKey(MySqlTableBuilder builder)
    {
        Assume.IsNull( _primaryKey );

        if ( builder.PrimaryKey is null )
            throw new MySqlObjectBuilderException( ExceptionResources.PrimaryKeyIsMissing( builder ) );

        var index = Schema.Objects.GetIndex( builder.PrimaryKey.Index.Name );
        _primaryKey = new MySqlPrimaryKey( index, builder.PrimaryKey );
    }
}
