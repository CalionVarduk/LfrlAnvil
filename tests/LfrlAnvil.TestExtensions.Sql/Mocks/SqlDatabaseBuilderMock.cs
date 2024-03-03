using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlDatabaseBuilderMock : SqlDatabaseBuilder
{
    private SqlDatabaseBuilderMock(SqlColumnTypeDefinitionProviderMock typeDefinitions, string serverVersion, string defaultSchemaName)
        : base(
            SqlDialectMock.Instance,
            serverVersion,
            defaultSchemaName,
            new SqlDataTypeProviderMock(),
            typeDefinitions,
            new SqlNodeInterpreterFactoryMock(),
            new SqlQueryReaderFactoryMock( typeDefinitions ),
            new SqlParameterBinderFactoryMock( typeDefinitions ),
            new SqlSchemaBuilderCollectionMock(),
            new SqlDatabaseChangeTrackerMock() )
    {
        Changes.SetModeAndAttach( SqlDatabaseCreateMode.DryRun );
    }

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
    public static SqlDatabaseBuilderMock Create(string serverVersion = "0.0.0", string defaultSchemaName = "common")
    {
        return new SqlDatabaseBuilderMock(
            new SqlColumnTypeDefinitionProviderMock( new SqlColumnTypeDefinitionProviderBuilderMock() ),
            serverVersion,
            defaultSchemaName );
    }
}
