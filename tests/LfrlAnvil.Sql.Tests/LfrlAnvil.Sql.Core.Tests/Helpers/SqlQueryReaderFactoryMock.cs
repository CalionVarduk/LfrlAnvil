using System.Data.Common;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlQueryReaderFactoryMock : SqlQueryReaderFactory<DbDataReader>
{
    public SqlQueryReaderFactoryMock(SqlColumnTypeDefinitionProviderMock columnTypeDefinitions)
        : base( SqlDialectMock.Instance, columnTypeDefinitions ) { }
}
