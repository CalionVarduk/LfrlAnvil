using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlIndexBuilderMock : SqlIndexBuilder
{
    public SqlIndexBuilderMock(
        SqlTableBuilderMock table,
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique)
        : base( table, name, columns, isUnique ) { }

    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlTableBuilderMock Table => ReinterpretCast.To<SqlTableBuilderMock>( base.Table );
    public new SqlPrimaryKeyBuilderMock? PrimaryKey => ReinterpretCast.To<SqlPrimaryKeyBuilderMock>( base.PrimaryKey );
    public new SqlIndexColumnBuilderArray<SqlColumnBuilderMock> Columns => base.Columns.UnsafeReinterpretAs<SqlColumnBuilderMock>();

    public new SqlObjectBuilderArray<SqlColumnBuilderMock> ReferencedFilterColumns =>
        base.ReferencedFilterColumns.UnsafeReinterpretAs<SqlColumnBuilderMock>();

    public new SqlIndexBuilderMock SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqlIndexBuilderMock SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public new SqlIndexBuilderMock MarkAsUnique(bool enabled = true)
    {
        base.MarkAsUnique( enabled );
        return this;
    }

    public new SqlIndexBuilderMock SetFilter(SqlConditionNode? filter)
    {
        base.SetFilter( filter );
        return this;
    }
}
