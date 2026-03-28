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

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions;

public static partial class SqlNode
{
    /// <summary>
    /// Creates placeholder node instances.
    /// </summary>
    public static class Placeholders
    {
        /// <summary>
        /// Creates a new <see cref="SqlExpressionPlaceholderNode"/> instance.
        /// </summary>
        /// <param name="identifier">Optional custom placeholder identifier.</param>
        /// <param name="wrapInParentheses">
        /// Specifies whether to wrap replacement text in parentheses. Equal to <b>true</b> by default.
        /// </param>
        /// <returns>New <see cref="SqlExpressionPlaceholderNode"/> instance.</returns>
        [Pure]
        public static SqlExpressionPlaceholderNode Expression(string? identifier = null, bool wrapInParentheses = true)
        {
            return new SqlExpressionPlaceholderNode( identifier, wrapInParentheses );
        }

        /// <summary>
        /// Creates a new <see cref="SqlConditionPlaceholderNode"/> instance.
        /// </summary>
        /// <param name="identifier">Optional custom placeholder identifier.</param>
        /// <param name="wrapInParentheses">
        /// Specifies whether to wrap replacement text in parentheses. Equal to <b>true</b> by default.
        /// </param>
        /// <returns>New <see cref="SqlConditionPlaceholderNode"/> instance.</returns>
        [Pure]
        public static SqlConditionPlaceholderNode Condition(string? identifier = null, bool wrapInParentheses = true)
        {
            return new SqlConditionPlaceholderNode( identifier, wrapInParentheses );
        }

        /// <summary>
        /// Creates a new <see cref="SqlSortTraitPlaceholderNode"/> instance.
        /// </summary>
        /// <param name="identifier">Optional custom placeholder identifier.</param>
        /// <param name="includeOrderBy">Specifies whether to include the ORDER BY prefix. Equal to <b>true</b> by default.</param>
        /// <returns>New <see cref="SqlSortTraitPlaceholderNode"/> instance.</returns>
        [Pure]
        public static SqlSortTraitPlaceholderNode SortTrait(string? identifier = null, bool includeOrderBy = true)
        {
            return new SqlSortTraitPlaceholderNode( identifier, includeOrderBy );
        }
    }
}
