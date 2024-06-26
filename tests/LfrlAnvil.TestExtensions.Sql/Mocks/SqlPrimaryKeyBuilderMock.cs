﻿using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlPrimaryKeyBuilderMock : SqlPrimaryKeyBuilder
{
    public SqlPrimaryKeyBuilderMock(SqlIndexBuilderMock index, string name)
        : base( index, name ) { }

    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlTableBuilderMock Table => ReinterpretCast.To<SqlTableBuilderMock>( base.Table );
    public new SqlIndexBuilderMock Index => ReinterpretCast.To<SqlIndexBuilderMock>( base.Index );

    public new SqlPrimaryKeyBuilderMock SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqlPrimaryKeyBuilderMock SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
