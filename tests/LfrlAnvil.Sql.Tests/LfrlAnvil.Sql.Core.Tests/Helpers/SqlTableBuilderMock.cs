using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlTableBuilderMock : SqlTableBuilder
{
    public SqlTableBuilderMock(SqlSchemaBuilderMock schema, string name)
        : base(
            schema,
            name,
            new SqlColumnBuilderCollectionMock( schema.Database.TypeDefinitions.GetByDataType( SqlDataTypeMock.Object ) ),
            new SqlConstraintBuilderCollectionMock() ) { }

    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlSchemaBuilderMock Schema => ReinterpretCast.To<SqlSchemaBuilderMock>( base.Schema );
    public new SqlColumnBuilderCollectionMock Columns => ReinterpretCast.To<SqlColumnBuilderCollectionMock>( base.Columns );
    public new SqlConstraintBuilderCollectionMock Constraints => ReinterpretCast.To<SqlConstraintBuilderCollectionMock>( base.Constraints );

    public new SqlTableBuilderMock SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
