using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlDatabaseChangeTracker
{
    ISqlObjectBuilder? ActiveObject { get; }
    SqlDatabaseCreateMode Mode { get; }
    bool IsAttached { get; }
    ISqlDatabaseBuilder Database { get; }

    ReadOnlySpan<SqlDatabaseBuilderCommandAction> GetPendingActions();

    [Pure]
    SqlObjectExistenceState GetExistenceState(ISqlObjectBuilder target);

    [Pure]
    bool ContainsChange(ISqlObjectBuilder target, SqlObjectChangeDescriptor descriptor);

    bool TryGetOriginalValue(ISqlObjectBuilder target, SqlObjectChangeDescriptor descriptor, out object? result);

    ISqlDatabaseChangeTracker AddAction(Action<IDbCommand> action);
    ISqlDatabaseChangeTracker AddStatement(ISqlStatementNode statement);

    ISqlDatabaseChangeTracker AddParameterizedStatement(
        ISqlStatementNode statement,
        IEnumerable<KeyValuePair<string, object?>> parameters,
        SqlParameterBinderCreationOptions? options = null);

    ISqlDatabaseChangeTracker AddParameterizedStatement<TSource>(
        ISqlStatementNode statement,
        TSource parameters,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull;

    ISqlDatabaseChangeTracker Attach(bool enabled = true);
    ISqlDatabaseChangeTracker CompletePendingChanges();
}
