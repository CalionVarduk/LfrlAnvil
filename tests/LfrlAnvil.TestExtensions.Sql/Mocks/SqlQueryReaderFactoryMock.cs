using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlQueryReaderFactoryMock : SqlQueryReaderFactory<DbDataReaderMock>
{
    public SqlQueryReaderFactoryMock(SqlColumnTypeDefinitionProviderMock columnTypeDefinitions)
        : base( SqlDialectMock.Instance, columnTypeDefinitions ) { }

    [Pure]
    public static SqlQueryReaderFactoryMock CreateInstance()
    {
        return new SqlQueryReaderFactoryMock( new SqlColumnTypeDefinitionProviderMock( new SqlColumnTypeDefinitionProviderBuilderMock() ) );
    }
}
