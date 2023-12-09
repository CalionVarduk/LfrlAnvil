using LfrlAnvil.Sql.Statements.Compilers;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public sealed class MySqlQueryReaderFactory : SqlQueryReaderFactory<MySqlDataReader>
{
    internal MySqlQueryReaderFactory(MySqlColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( MySqlDialect.Instance, columnTypeDefinitions ) { }
}
