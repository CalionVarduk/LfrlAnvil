﻿using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
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
    Version Version { get; }
    string ServerVersion { get; }
    SqlQueryReaderExecutor<SqlDatabaseVersionRecord> VersionRecordsQuery { get; }

    [Pure]
    IDbConnection Connect();

    [Pure]
    ValueTask<IDbConnection> ConnectAsync(CancellationToken cancellationToken = default);

    [Pure]
    SqlDatabaseVersionRecord[] GetRegisteredVersions();
}
