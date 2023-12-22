using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.MySql.Exceptions;

internal static class Resources
{
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
