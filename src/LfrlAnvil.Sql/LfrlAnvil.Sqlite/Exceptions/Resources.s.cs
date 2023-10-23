using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sqlite.Exceptions;

internal static class Resources
{
    internal const string ConnectionStringForPermanentDatabaseIsImmutable =
        "Connection string for permanent SQLite database is immutable.";

    internal const string ConnectionForClosedPermanentDatabaseCannotBeReopened =
        "Connection for closed permanent SQLite database cannot be reopened.";

    internal const string UpdateTargetIsNotTable = "update target (" +
        nameof( SqlDataSourceQueryExpressionNode.DataSource ) +
        "." +
        nameof( SqlDataSourceNode.From ) +
        ") is not a table";

    internal const string DeleteTargetIsNotTable = "delete target (" +
        nameof( SqlDataSourceQueryExpressionNode.DataSource ) +
        "." +
        nameof( SqlDataSourceNode.From ) +
        ") is not a table";

    internal const string UpdateTargetIsNotAliased = "update target (" +
        nameof( SqlDataSourceQueryExpressionNode.DataSource ) +
        "." +
        nameof( SqlDataSourceNode.From ) +
        ") is not aliased";

    internal const string DeleteTargetIsNotAliased = "delete target (" +
        nameof( SqlDataSourceQueryExpressionNode.DataSource ) +
        "." +
        nameof( SqlDataSourceNode.From ) +
        ") is not aliased";

    internal const string UpdateTargetDoesNotHaveAnyColumns = "update target (" +
        nameof( SqlDataSourceQueryExpressionNode.DataSource ) +
        "." +
        nameof( SqlDataSourceNode.From ) +
        ") does not have any columns";

    internal const string DeleteTargetDoesNotHaveAnyColumns = "delete target (" +
        nameof( SqlDataSourceQueryExpressionNode.DataSource ) +
        "." +
        nameof( SqlDataSourceNode.From ) +
        ") does not have any columns";

    internal const string NestedUpdateAttempt = "of nested update attempt";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ForeignKeyCheckFailure(Version version, IReadOnlySet<string> failedTableNames)
    {
        var headerText = $"Foreign key check for version {version} failed for {failedTableNames.Count} table(s):";
        var tablesText = string.Join( Environment.NewLine, failedTableNames.Select( (n, i) => $"{i + 1}. \"{n}\"" ) );
        return $"{headerText}{Environment.NewLine}{tablesText}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DeleteOrUpdateTargetPrimaryKeyColumnIsComplexExpression(bool isUpdate, int index, SqlExpressionNode node)
    {
        return (isUpdate ? "update target (" : "delete target (") +
            nameof( SqlDataSourceQueryExpressionNode.DataSource ) +
            "." +
            nameof( SqlDataSourceNode.From ) +
            $") contains a primary key column at index {index} that represents a complex expression [{node}]";
    }
}
