// Copyright 2026 Łukasz Furlepa
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
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression placeholder node.
/// </summary>
public sealed class SqlExpressionPlaceholderNode : SqlExpressionNode
{
    internal SqlExpressionPlaceholderNode(string? identifier, bool wrapInParentheses)
        : base( SqlNodeType.ExpressionPlaceholder )
    {
        Identifier = identifier ?? Guid.NewGuid().ToString( "N" );
        SqlPlaceholder = SqlHelpers.GetExpressionSqlPlaceholder( Identifier );
        WrapInParentheses = wrapInParentheses;
    }

    /// <summary>
    /// Placeholder's identifier.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Text that will be injected into the SQL statement by this placeholder.
    /// </summary>
    public string SqlPlaceholder { get; }

    /// <summary>
    /// Specifies whether to wrap replacement text in parentheses.
    /// </summary>
    public bool WrapInParentheses { get; }

    /// <summary>
    /// Replaces this placeholder in the provided <paramref name="targetSql"/> with the provided <paramref name="replacementSql"/>.
    /// </summary>
    /// <param name="targetSql">Text to replace this placeholder in.</param>
    /// <param name="replacementSql">Text to replace this placeholder with.</param>
    /// <returns>Text with replaced placeholder.</returns>
    [Pure]
    public string Replace(string targetSql, string replacementSql)
    {
        return targetSql.Replace( SqlPlaceholder, WrapInParentheses ? $"({replacementSql})" : replacementSql );
    }
}
