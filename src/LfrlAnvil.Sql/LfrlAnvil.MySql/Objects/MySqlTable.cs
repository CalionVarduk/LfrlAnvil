using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlTable : MySqlObject, ISqlTable
{
    private SqlRecordSetInfo? _info;
    private SqlTableNode? _recordSet;

    internal MySqlTable(MySqlSchema schema, MySqlTableBuilder builder)
        : base( builder )
    {
        Schema = schema;
        Columns = new MySqlColumnCollection( this, builder.Columns );
        Constraints = new MySqlConstraintCollection( this, builder.Constraints.Count );
        _info = builder.GetCachedInfo();
        _recordSet = null;
    }

    public MySqlSchema Schema { get; }
    public MySqlColumnCollection Columns { get; }
    public MySqlConstraintCollection Constraints { get; }
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlTableNode RecordSet => _recordSet ??= SqlNode.Table( this );
    public override MySqlDatabase Database => Schema.Database;

    ISqlSchema ISqlTable.Schema => Schema;
    ISqlColumnCollection ISqlTable.Columns => Columns;
    ISqlConstraintCollection ISqlTable.Constraints => Constraints;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {MySqlHelpers.GetFullName( Schema.Name, Name )}";
    }
}
