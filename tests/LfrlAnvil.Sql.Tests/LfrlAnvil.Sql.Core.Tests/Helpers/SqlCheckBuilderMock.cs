using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlCheckBuilderMock : SqlCheckBuilder
{
    public SqlCheckBuilderMock(
        SqlTableBuilderMock table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, condition, referencedColumns ) { }

    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlTableBuilderMock Table => ReinterpretCast.To<SqlTableBuilderMock>( base.Table );

    public new SqlObjectBuilderArray<SqlColumnBuilderMock> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<SqlColumnBuilderMock>();

    public new SqlCheckBuilderMock SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqlCheckBuilderMock SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
