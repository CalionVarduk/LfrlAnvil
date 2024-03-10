using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlColumnBuilderMock : SqlColumnBuilder
{
    public SqlColumnBuilderMock(SqlTableBuilderMock table, string name, SqlColumnTypeDefinition typeDefinition)
        : base( table, name, typeDefinition ) { }

    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlTableBuilderMock Table => ReinterpretCast.To<SqlTableBuilderMock>( base.Table );

    public new SqlColumnBuilderMock SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqlColumnBuilderMock SetType(SqlColumnTypeDefinition definition)
    {
        base.SetType( definition );
        return this;
    }

    public new SqlColumnBuilderMock MarkAsNullable(bool enabled = true)
    {
        base.MarkAsNullable( enabled );
        return this;
    }

    public new SqlColumnBuilderMock SetDefaultValue(SqlExpressionNode? value)
    {
        base.SetDefaultValue( value );
        return this;
    }
}
