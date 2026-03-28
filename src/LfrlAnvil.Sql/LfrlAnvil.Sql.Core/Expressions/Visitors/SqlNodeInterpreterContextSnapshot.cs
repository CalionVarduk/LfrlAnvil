// Copyright 2024-2026 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
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
        : this( context.Sql.ToString(), context.Parameters ) { }

    private SqlNodeInterpreterContextSnapshot(string sql, IReadOnlyCollection<SqlNodeInterpreterContextParameter>? parameters)
    {
        _sql = sql;
        _parameters = Array.Empty<SqlParameterNode>();

        if ( parameters is not null && parameters.Count > 0 )
        {
            var i = 0;
            _parameters = new SqlParameterNode[parameters.Count];
            foreach ( var (name, type, index) in parameters )
                _parameters[i++] = SqlNode.Parameter( name, type, index );
        }
    }

    private SqlNodeInterpreterContextSnapshot(string sql, SqlParameterNode[]? parameters)
    {
        _sql = sql;
        _parameters = parameters ?? Array.Empty<SqlParameterNode>();
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

    /// <summary>
    /// Creates a new <see cref="SqlNodeInterpreterContext"/> from this snapshot.
    /// </summary>
    /// <returns>New <see cref="SqlNodeInterpreterContext"/> instance.</returns>
    [Pure]
    public SqlNodeInterpreterContext Restore()
    {
        var result = SqlNodeInterpreterContext.Create( capacity: Sql.Length );

        result.Sql.Append( Sql );
        foreach ( var p in Parameters )
            result.AddParameter( p.Name, p.Type, p.Index );

        return result;
    }

    /// <summary>
    /// Replaces provided placeholder <paramref name="node"/> in this snapshot with the provided <paramref name="replacementSql"/>.
    /// </summary>
    /// <param name="node">Node to replace.</param>
    /// <param name="replacementSql">Text to replace the placeholder with.</param>
    /// <returns>New context snapshot with replaced placeholder.</returns>
    [Pure]
    public SqlNodeInterpreterContextSnapshot Replace(SqlExpressionPlaceholderNode node, string replacementSql)
    {
        return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacementSql ), _parameters );
    }

    /// <summary>
    /// Replaces provided placeholder <paramref name="node"/> in this snapshot
    /// with the provided <paramref name="replacement"/> snapshot's SQL.
    /// </summary>
    /// <param name="node">Node to replace.</param>
    /// <param name="replacement">Context to replace the placeholder with.</param>
    /// <returns>New context snapshot with replaced placeholder.</returns>
    [Pure]
    public SqlNodeInterpreterContextSnapshot Replace(SqlExpressionPlaceholderNode node, SqlNodeInterpreterContextSnapshot replacement)
    {
        if ( replacement.Parameters.IsEmpty )
            return Replace( node, replacement.Sql );

        if ( Parameters.IsEmpty )
            return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql ), replacement._parameters );

        var parameters = MergeParameters( Parameters, replacement.Parameters );
        return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql ), parameters?.Values );
    }

    /// <summary>
    /// Replaces provided placeholder <paramref name="node"/> in this snapshot with the provided <paramref name="replacement"/>'s SQL.
    /// </summary>
    /// <param name="node">Node to replace.</param>
    /// <param name="replacement">Context to replace the placeholder with.</param>
    /// <returns>New context snapshot with replaced placeholder.</returns>
    [Pure]
    public SqlNodeInterpreterContextSnapshot Replace(SqlExpressionPlaceholderNode node, SqlNodeInterpreterContext replacement)
    {
        if ( replacement.Parameters.Count == 0 )
            return Replace( node, replacement.Sql.ToString() );

        if ( Parameters.IsEmpty )
            return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql.ToString() ), replacement.Parameters );

        var parameters = MergeParameters( Parameters, replacement.Parameters );
        return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql.ToString() ), parameters?.Values );
    }

    /// <summary>
    /// Replaces provided placeholder <paramref name="node"/> in this snapshot with the provided <paramref name="replacementSql"/>.
    /// </summary>
    /// <param name="node">Node to replace.</param>
    /// <param name="replacementSql">Text to replace the placeholder with.</param>
    /// <returns>New context snapshot with replaced placeholder.</returns>
    [Pure]
    public SqlNodeInterpreterContextSnapshot Replace(SqlConditionPlaceholderNode node, string replacementSql)
    {
        return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacementSql ), _parameters );
    }

    /// <summary>
    /// Replaces provided placeholder <paramref name="node"/> in this snapshot
    /// with the provided <paramref name="replacement"/> snapshot's SQL.
    /// </summary>
    /// <param name="node">Node to replace.</param>
    /// <param name="replacement">Context to replace the placeholder with.</param>
    /// <returns>New context snapshot with replaced placeholder.</returns>
    [Pure]
    public SqlNodeInterpreterContextSnapshot Replace(SqlConditionPlaceholderNode node, SqlNodeInterpreterContextSnapshot replacement)
    {
        if ( replacement.Parameters.IsEmpty )
            return Replace( node, replacement.Sql );

        if ( Parameters.IsEmpty )
            return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql ), replacement._parameters );

        var parameters = MergeParameters( Parameters, replacement.Parameters );
        return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql ), parameters?.Values );
    }

    /// <summary>
    /// Replaces provided placeholder <paramref name="node"/> in this snapshot with the provided <paramref name="replacement"/>'s SQL.
    /// </summary>
    /// <param name="node">Node to replace.</param>
    /// <param name="replacement">Context to replace the placeholder with.</param>
    /// <returns>New context snapshot with replaced placeholder.</returns>
    [Pure]
    public SqlNodeInterpreterContextSnapshot Replace(SqlConditionPlaceholderNode node, SqlNodeInterpreterContext replacement)
    {
        if ( replacement.Parameters.Count == 0 )
            return Replace( node, replacement.Sql.ToString() );

        if ( Parameters.IsEmpty )
            return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql.ToString() ), replacement.Parameters );

        var parameters = MergeParameters( Parameters, replacement.Parameters );
        return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql.ToString() ), parameters?.Values );
    }

    /// <summary>
    /// Replaces provided placeholder <paramref name="node"/> in this snapshot with the provided <paramref name="replacementSql"/>.
    /// </summary>
    /// <param name="node">Node to replace.</param>
    /// <param name="replacementSql">Text to replace the placeholder with.</param>
    /// <returns>New context snapshot with replaced placeholder.</returns>
    [Pure]
    public SqlNodeInterpreterContextSnapshot Replace(SqlSortTraitPlaceholderNode node, string replacementSql)
    {
        return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacementSql ), _parameters );
    }

    /// <summary>
    /// Replaces provided placeholder <paramref name="node"/> in this snapshot
    /// with the provided <paramref name="replacement"/> snapshot's SQL.
    /// </summary>
    /// <param name="node">Node to replace.</param>
    /// <param name="replacement">Context to replace the placeholder with.</param>
    /// <returns>New context snapshot with replaced placeholder.</returns>
    [Pure]
    public SqlNodeInterpreterContextSnapshot Replace(SqlSortTraitPlaceholderNode node, SqlNodeInterpreterContextSnapshot replacement)
    {
        if ( replacement.Parameters.IsEmpty )
            return Replace( node, replacement.Sql );

        if ( Parameters.IsEmpty )
            return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql ), replacement._parameters );

        var parameters = MergeParameters( Parameters, replacement.Parameters );
        return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql ), parameters?.Values );
    }

    /// <summary>
    /// Replaces provided placeholder <paramref name="node"/> in this snapshot with the provided <paramref name="replacement"/>'s SQL.
    /// </summary>
    /// <param name="node">Node to replace.</param>
    /// <param name="replacement">Context to replace the placeholder with.</param>
    /// <returns>New context snapshot with replaced placeholder.</returns>
    [Pure]
    public SqlNodeInterpreterContextSnapshot Replace(SqlSortTraitPlaceholderNode node, SqlNodeInterpreterContext replacement)
    {
        if ( replacement.Parameters.Count == 0 )
            return Replace( node, replacement.Sql.ToString() );

        if ( Parameters.IsEmpty )
            return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql.ToString() ), replacement.Parameters );

        var parameters = MergeParameters( Parameters, replacement.Parameters );
        return new SqlNodeInterpreterContextSnapshot( node.Replace( Sql, replacement.Sql.ToString() ), parameters?.Values );
    }

    [Pure]
    private static Dictionary<string, SqlNodeInterpreterContextParameter>? MergeParameters(
        ReadOnlySpan<SqlParameterNode> first,
        ReadOnlySpan<SqlParameterNode> second)
    {
        Dictionary<string, SqlNodeInterpreterContextParameter>? result = null;
        foreach ( var p in first )
            SqlNodeInterpreterContext.AddParameter( ref result, p.Name, p.Type, p.Index );

        foreach ( var p in second )
            SqlNodeInterpreterContext.AddParameter( ref result, p.Name, p.Type, p.Index );

        return result;
    }

    [Pure]
    private static Dictionary<string, SqlNodeInterpreterContextParameter>? MergeParameters(
        ReadOnlySpan<SqlParameterNode> first,
        IReadOnlyCollection<SqlNodeInterpreterContextParameter> second)
    {
        Dictionary<string, SqlNodeInterpreterContextParameter>? result = null;
        foreach ( var p in first )
            SqlNodeInterpreterContext.AddParameter( ref result, p.Name, p.Type, p.Index );

        foreach ( var p in second )
            SqlNodeInterpreterContext.AddParameter( ref result, p.Name, p.Type, p.Index );

        return result;
    }
}
