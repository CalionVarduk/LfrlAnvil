using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlSchemaBuilderMock : SqlSchemaBuilder
{
    public SqlSchemaBuilderMock(SqlDatabaseBuilderMock database, string name)
        : base( database, name, new SqlObjectBuilderCollectionMock() ) { }

    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlObjectBuilderCollectionMock Objects => ReinterpretCast.To<SqlObjectBuilderCollectionMock>( base.Objects );

    public new SqlSchemaBuilderMock SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
