using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlPrimaryKeyBuilder : SqlConstraintBuilder, ISqlPrimaryKeyBuilder
{
    protected SqlPrimaryKeyBuilder(SqlIndexBuilder index, string name)
        : base( index.Table, SqlObjectType.PrimaryKey, name )
    {
        Index = index;
    }

    public SqlIndexBuilder Index { get; }
    public override bool CanRemove => Index.CanRemove;
    ISqlIndexBuilder ISqlPrimaryKeyBuilder.Index => Index;

    public new SqlPrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqlPrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    [Pure]
    protected override string GetDefaultName()
    {
        return SqlHelpers.GetDefaultPrimaryKeyName( Table );
    }

    protected override void BeforeRemove()
    {
        Index.Remove();
    }

    protected override void AfterRemove() { }

    ISqlPrimaryKeyBuilder ISqlPrimaryKeyBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlPrimaryKeyBuilder ISqlPrimaryKeyBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }
}
