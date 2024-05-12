using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents an SQL database.
/// </summary>
public interface ISqlDatabase : IDisposable
{
    /// <summary>
    /// Specifies the SQL dialect of this database.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// Collection of schemas defined in this database.
    /// </summary>
    ISqlSchemaCollection Schemas { get; }

    /// <summary>
    /// Provider of SQL data types.
    /// </summary>
    ISqlDataTypeProvider DataTypes { get; }

    /// <summary>
    /// Provider of column type definitions.
    /// </summary>
    ISqlColumnTypeDefinitionProvider TypeDefinitions { get; }

    /// <summary>
    /// Factory of node interpreters.
    /// </summary>
    ISqlNodeInterpreterFactory NodeInterpreters { get; }

    /// <summary>
    /// Factory of query readers.
    /// </summary>
    ISqlQueryReaderFactory QueryReaders { get; }

    /// <summary>
    /// Factory of parameter binders.
    /// </summary>
    ISqlParameterBinderFactory ParameterBinders { get; }

    /// <summary>
    /// Connector object that can be used to connect to this database.
    /// </summary>
    ISqlDatabaseConnector Connector { get; }

    /// <summary>
    /// Current version of this database.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Current <see cref="DbConnection.ServerVersion"/> of this database.
    /// </summary>
    string ServerVersion { get; }

    /// <summary>
    /// Query reader's executor capable of reading metadata of all versions applied to this database.
    /// </summary>
    SqlQueryReaderExecutor<SqlDatabaseVersionRecord> VersionRecordsQuery { get; }

    /// <summary>
    /// Returns a collection of all versions applied to this database.
    /// </summary>
    /// <returns>Collection of all versions applied to this database.</returns>
    [Pure]
    SqlDatabaseVersionRecord[] GetRegisteredVersions();
}
