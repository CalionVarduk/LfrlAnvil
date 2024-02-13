using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlForeignKeyBuilderMock : SqlForeignKeyBuilder
{
    public SqlForeignKeyBuilderMock(SqlIndexBuilderMock originIndex, SqlIndexBuilderMock referencedIndex, string name)
        : base( originIndex, referencedIndex, name ) { }

    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlTableBuilderMock Table => ReinterpretCast.To<SqlTableBuilderMock>( base.Table );

    public new SqlForeignKeyBuilderMock SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqlForeignKeyBuilderMock SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public new SqlForeignKeyBuilderMock SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        base.SetOnDeleteBehavior( behavior );
        return this;
    }

    public new SqlForeignKeyBuilderMock SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        base.SetOnUpdateBehavior( behavior );
        return this;
    }
}
