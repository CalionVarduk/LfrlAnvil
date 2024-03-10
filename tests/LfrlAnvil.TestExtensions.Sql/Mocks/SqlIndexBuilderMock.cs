using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlIndexBuilderMock : SqlIndexBuilder
{
    public SqlIndexBuilderMock(
        SqlTableBuilderMock table,
        string name,
        SqlIndexBuilderColumns<SqlColumnBuilderMock> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, new SqlIndexBuilderColumns<SqlColumnBuilder>( columns.Expressions ), isUnique, referencedColumns ) { }

    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlTableBuilderMock Table => ReinterpretCast.To<SqlTableBuilderMock>( base.Table );
    public new SqlPrimaryKeyBuilderMock? PrimaryKey => ReinterpretCast.To<SqlPrimaryKeyBuilderMock>( base.PrimaryKey );

    public new SqlIndexBuilderColumns<SqlColumnBuilderMock> Columns =>
        new SqlIndexBuilderColumns<SqlColumnBuilderMock>( base.Columns.Expressions );

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

    public new SqlIndexBuilderMock MarkAsVirtual(bool enabled = true)
    {
        base.MarkAsVirtual( enabled );
        return this;
    }

    public new SqlIndexBuilderMock SetFilter(SqlConditionNode? filter)
    {
        base.SetFilter( filter );
        return this;
    }
}
