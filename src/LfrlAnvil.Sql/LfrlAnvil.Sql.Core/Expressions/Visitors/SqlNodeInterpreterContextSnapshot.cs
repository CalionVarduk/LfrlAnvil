using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Visitors;

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
            foreach ( var (name, type) in parameters )
                _parameters[i++] = SqlNode.Parameter( name, type );
        }
    }

    public string Sql => _sql ?? string.Empty;
    public ReadOnlySpan<SqlParameterNode> Parameters => _parameters;

    [Pure]
    public override string ToString()
    {
        return Sql;
    }

    [Pure]
    public SqlRawExpressionNode ToExpression(TypeNullability? type = null)
    {
        return SqlNode.RawExpression( Sql, type, _parameters ?? Array.Empty<SqlParameterNode>() );
    }

    [Pure]
    public SqlRawConditionNode ToCondition()
    {
        return SqlNode.RawCondition( Sql, _parameters ?? Array.Empty<SqlParameterNode>() );
    }

    [Pure]
    public SqlRawStatementNode ToStatement()
    {
        return SqlNode.RawStatement( Sql, _parameters ?? Array.Empty<SqlParameterNode>() );
    }

    [Pure]
    public SqlRawQueryExpressionNode ToQuery()
    {
        return SqlNode.RawQuery( Sql, _parameters ?? Array.Empty<SqlParameterNode>() );
    }
}
