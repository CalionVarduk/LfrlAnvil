using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlParameterBinderFactoryMock : SqlParameterBinderFactory<DbCommandMock>
{
    public SqlParameterBinderFactoryMock(SqlColumnTypeDefinitionProviderMock columnTypeDefinitions, bool supportsPositionalParameters)
        : base( SqlDialectMock.Instance, columnTypeDefinitions, supportsPositionalParameters ) { }

    [Pure]
    public static SqlParameterBinderFactoryMock CreateInstance(bool arePositionalParametersSupported = true)
    {
        return new SqlParameterBinderFactoryMock(
            new SqlColumnTypeDefinitionProviderMock( new SqlColumnTypeDefinitionProviderBuilderMock() ),
            arePositionalParametersSupported );
    }
}
