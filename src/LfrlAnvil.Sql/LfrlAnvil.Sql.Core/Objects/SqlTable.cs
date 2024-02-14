using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlTable : SqlObject, ISqlTable
{
    private SqlTableNode? _node;
    private SqlRecordSetInfo? _info;

    protected SqlTable(SqlSchema schema, SqlTableBuilder builder, SqlColumnCollection columns, SqlConstraintCollection constraints)
        : base( schema.Database, builder )
    {
        Schema = schema;
        _info = builder.GetCachedInfo();
        _node = null;
        Columns = columns;
        Columns.SetTable( this, builder.Columns );
        Constraints = constraints;
        Constraints.SetTable( this );
    }

    public SqlSchema Schema { get; }
    public SqlColumnCollection Columns { get; }
    public SqlConstraintCollection Constraints { get; }
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlTableNode Node => _node ??= SqlNode.Table( this );

    ISqlSchema ISqlTable.Schema => Schema;
    ISqlColumnCollection ISqlTable.Columns => Columns;
    ISqlConstraintCollection ISqlTable.Constraints => Constraints;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Schema.Name, Name )}";
    }
}
