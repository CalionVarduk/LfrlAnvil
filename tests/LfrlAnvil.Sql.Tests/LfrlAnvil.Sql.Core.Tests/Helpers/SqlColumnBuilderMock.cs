using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

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

    [Pure]
    public new SqlIndexColumnBuilder<SqlColumnBuilderMock> Asc()
    {
        return base.Asc().UnsafeReinterpretAs<SqlColumnBuilderMock>();
    }

    [Pure]
    public new SqlIndexColumnBuilder<SqlColumnBuilderMock> Desc()
    {
        return base.Desc().UnsafeReinterpretAs<SqlColumnBuilderMock>();
    }
}
