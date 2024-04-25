using LfrlAnvil.Sql.Statements.Compilers;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public sealed class MySqlParameterBinderFactory : SqlParameterBinderFactory<MySqlCommand>
{
    internal MySqlParameterBinderFactory(MySqlColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( MySqlDialect.Instance, columnTypeDefinitions, supportsPositionalParameters: false ) { }
}
