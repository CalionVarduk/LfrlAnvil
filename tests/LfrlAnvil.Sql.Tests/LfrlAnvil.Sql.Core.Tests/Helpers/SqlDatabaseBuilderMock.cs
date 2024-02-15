using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlDatabaseBuilderMock : SqlDatabaseBuilder
{
    private SqlDatabaseBuilderMock(SqlColumnTypeDefinitionProviderMock typeDefinitions)
        : base(
            SqlDialectMock.Instance,
            "0.0.0",
            "common",
            new SqlDataTypeProviderMock(),
            typeDefinitions,
            new SqlNodeInterpreterFactoryMock(),
            new SqlQueryReaderFactoryMock( typeDefinitions ),
            new SqlParameterBinderFactoryMock( typeDefinitions ),
            new SqlSchemaBuilderCollectionMock(),
            new SqlDatabaseChangeTrackerMock() ) { }

    public new SqlSchemaBuilderCollectionMock Schemas => ReinterpretCast.To<SqlSchemaBuilderCollectionMock>( base.Schemas );
    public new SqlDataTypeProviderMock DataTypes => ReinterpretCast.To<SqlDataTypeProviderMock>( base.DataTypes );

    public new SqlColumnTypeDefinitionProviderMock TypeDefinitions =>
        ReinterpretCast.To<SqlColumnTypeDefinitionProviderMock>( base.TypeDefinitions );

    public new SqlNodeInterpreterFactoryMock NodeInterpreters => ReinterpretCast.To<SqlNodeInterpreterFactoryMock>( base.NodeInterpreters );
    public new SqlQueryReaderFactoryMock QueryReaders => ReinterpretCast.To<SqlQueryReaderFactoryMock>( base.QueryReaders );
    public new SqlParameterBinderFactoryMock ParameterBinders => ReinterpretCast.To<SqlParameterBinderFactoryMock>( base.ParameterBinders );
    public new SqlDatabaseChangeTrackerMock Changes => ReinterpretCast.To<SqlDatabaseChangeTrackerMock>( base.Changes );

    public new SqlDatabaseBuilderMock AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        base.AddConnectionChangeCallback( callback );
        return this;
    }

    public new static void QuickRemove(SqlObjectBuilder obj)
    {
        SqlBuilderApi.QuickRemove( obj );
    }

    public static void AddReference(SqlObjectBuilder obj, SqlObjectBuilderReferenceSource<SqlObjectBuilder> source)
    {
        SqlObjectBuilder.AddReference( obj, source );
    }

    [Pure]
    public static SqlDatabaseBuilderMock Create()
    {
        return new SqlDatabaseBuilderMock( new SqlColumnTypeDefinitionProviderMock( new SqlColumnTypeDefinitionProviderBuilderMock() ) );
    }
}
