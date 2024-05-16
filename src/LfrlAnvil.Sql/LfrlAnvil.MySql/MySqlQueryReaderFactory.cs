using LfrlAnvil.Sql.Statements.Compilers;
using MySqlConnector;

namespace LfrlAnvil.MySql;

/// <summary>
/// Represents a factory of delegates used by query reader expression instances.
/// </summary>
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlQueryReaderFactory : SqlQueryReaderFactory<MySqlDataReader>
{
    internal MySqlQueryReaderFactory(MySqlColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( MySqlDialect.Instance, columnTypeDefinitions ) { }
}
