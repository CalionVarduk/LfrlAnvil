using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlView : SqlObject, ISqlView
{
    private SqlViewNode? _node;
    private SqlRecordSetInfo? _info;

    protected SqlView(SqlSchema schema, SqlViewBuilder builder, SqlViewDataFieldCollection dataFields)
        : base( schema.Database, builder )
    {
        Schema = schema;
        _info = builder.GetCachedInfo();
        _node = null;
        DataFields = dataFields;
        DataFields.SetView( this, builder.Source );
    }

    public SqlSchema Schema { get; }
    public SqlViewDataFieldCollection DataFields { get; }
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlViewNode Node => _node ??= SqlNode.View( this );

    ISqlSchema ISqlView.Schema => Schema;
    ISqlViewDataFieldCollection ISqlView.DataFields => DataFields;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Schema.Name, Name )}";
    }
}
