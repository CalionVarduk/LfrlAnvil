using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlParameterBinderFactoryMock : SqlParameterBinderFactory<DbCommandMock>
{
    public SqlParameterBinderFactoryMock(SqlColumnTypeDefinitionProviderMock columnTypeDefinitions)
        : base( SqlDialectMock.Instance, columnTypeDefinitions ) { }

    [Pure]
    public static SqlParameterBinderFactoryMock CreateInstance()
    {
        return new SqlParameterBinderFactoryMock(
            new SqlColumnTypeDefinitionProviderMock( new SqlColumnTypeDefinitionProviderBuilderMock() ) );
    }
}
