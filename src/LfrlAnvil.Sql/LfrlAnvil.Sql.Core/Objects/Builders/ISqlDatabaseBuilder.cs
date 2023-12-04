using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlDatabaseBuilder
{
    SqlDialect Dialect { get; }
    ISqlDataTypeProvider DataTypes { get; }
    ISqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    ISqlNodeInterpreterFactory NodeInterpreters { get; }
    ISqlQueryReaderFactory QueryReaders { get; }
    ISqlParameterBinderFactory ParameterBinders { get; }
    ISqlSchemaBuilderCollection Schemas { get; }
    SqlDatabaseCreateMode Mode { get; }
    bool IsAttached { get; }
    string ServerVersion { get; }

    [Pure]
    ReadOnlySpan<SqlDatabaseBuilderStatement> GetPendingStatements();

    void AddStatement(ISqlStatementNode statement);

    void AddParameterizedStatement(
        ISqlStatementNode statement,
        IEnumerable<KeyValuePair<string, object?>> parameters,
        SqlParameterBinderCreationOptions? options = null);

    void AddParameterizedStatement<TSource>(
        ISqlStatementNode statement,
        TSource parameters,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull;

    ISqlDatabaseBuilder SetNodeInterpreterFactory(ISqlNodeInterpreterFactory factory);
    ISqlDatabaseBuilder SetAttachedMode(bool enabled = true);
    ISqlDatabaseBuilder SetDetachedMode(bool enabled = true);
    ISqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback);
}
