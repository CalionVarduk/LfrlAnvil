using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Tests.Helpers.Data;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlQueryReaderFactoryMock : SqlQueryReaderFactory<DbDataReaderMock>
{
    public SqlQueryReaderFactoryMock(SqlColumnTypeDefinitionProviderMock columnTypeDefinitions)
        : base( SqlDialectMock.Instance, columnTypeDefinitions ) { }
}
