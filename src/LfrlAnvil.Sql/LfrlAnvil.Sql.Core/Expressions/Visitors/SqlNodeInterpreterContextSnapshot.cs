using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a snapshot of an <see cref="SqlNodeInterpreterContext"/> state.
/// </summary>
public readonly struct SqlNodeInterpreterContextSnapshot
{
    private readonly string? _sql;
    private readonly SqlParameterNode[]? _parameters;

    internal SqlNodeInterpreterContextSnapshot(SqlNodeInterpreterContext context)
    {
        _sql = context.Sql.ToString();
        _parameters = Array.Empty<SqlParameterNode>();

        var parameters = context.Parameters;
        if ( parameters.Count > 0 )
        {
            var i = 0;
            _parameters = new SqlParameterNode[parameters.Count];
            foreach ( var (name, type, index) in parameters )
                _parameters[i++] = SqlNode.Parameter( name, type, index );
        }
    }

    /// <summary>
    /// Underlying SQL statement.
    /// </summary>
    public string Sql => _sql ?? string.Empty;

    /// <summary>
    /// Collection of SQL parameter nodes.
    /// </summary>
    public ReadOnlySpan<SqlParameterNode> Parameters => _parameters;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlNodeInterpreterContextSnapshot"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Sql;
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawExpressionNode"/> instance from this snapshot.
    /// </summary>
    /// <param name="type">Optional runtime type of the result of this expression.</param>
    /// <returns>New <see cref="SqlRawExpressionNode"/> instance.</returns>
    [Pure]
    public SqlRawExpressionNode ToExpression(TypeNullability? type = null)
    {
        return SqlNode.RawExpression( Sql, type, _parameters ?? Array.Empty<SqlParameterNode>() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawConditionNode"/> instance from this snapshot.
    /// </summary>
    /// <returns>New <see cref="SqlRawConditionNode"/> instance.</returns>
    [Pure]
    public SqlRawConditionNode ToCondition()
    {
        return SqlNode.RawCondition( Sql, _parameters ?? Array.Empty<SqlParameterNode>() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawStatementNode"/> instance from this snapshot.
    /// </summary>
    /// <returns>New <see cref="SqlRawStatementNode"/> instance.</returns>
    [Pure]
    public SqlRawStatementNode ToStatement()
    {
        return SqlNode.RawStatement( Sql, _parameters ?? Array.Empty<SqlParameterNode>() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawQueryExpressionNode"/> instance from this snapshot.
    /// </summary>
    /// <returns>New <see cref="SqlRawQueryExpressionNode"/> instance.</returns>
    [Pure]
    public SqlRawQueryExpressionNode ToQuery()
    {
        return SqlNode.RawQuery( Sql, _parameters ?? Array.Empty<SqlParameterNode>() );
    }
}
