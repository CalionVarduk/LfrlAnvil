using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

public interface ISqlDatabase : IDisposable
{
    SqlDialect Dialect { get; }
    ISqlSchemaCollection Schemas { get; }
    ISqlDataTypeProvider DataTypes { get; }
    ISqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    ISqlNodeInterpreterFactory NodeInterpreters { get; }
    ISqlQueryReaderFactory QueryReaders { get; }
    ISqlParameterBinderFactory ParameterBinders { get; }
    ISqlDatabaseConnector Connector { get; }
    Version Version { get; }
    string ServerVersion { get; }
    SqlQueryReaderExecutor<SqlDatabaseVersionRecord> VersionRecordsQuery { get; }

    [Pure]
    SqlDatabaseVersionRecord[] GetRegisteredVersions();
}
