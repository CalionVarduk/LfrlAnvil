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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a context capable of replacing placeholder nodes.
/// </summary>
public sealed class SqlPlaceholderNodeReplacementContext : SqlNodeMutatorContext
{
    private readonly List<(SqlNodeBase Placeholder, SqlNodeBase Replacement)> _replacements;
    private readonly int _count;

    private SqlPlaceholderNodeReplacementContext(List<(SqlNodeBase, SqlNodeBase)> replacements)
    {
        _replacements = replacements;
        _count = replacements.Count;
    }

    /// <inheritdoc/>
    protected internal override MutationResult Mutate(SqlNodeBase node, SqlNodeAncestors ancestors)
    {
        for ( var i = 0; i < _count; ++i )
        {
            var (placeholder, replacement) = _replacements[i];
            if ( ReferenceEquals( node, placeholder ) )
                return replacement;
        }

        return base.Mutate( node, ancestors );
    }

    /// <summary>
    /// Represents a builder of contexts capable of replacing placeholder nodes.
    /// </summary>
    public sealed class Builder
    {
        private readonly List<(SqlNodeBase Placeholder, SqlNodeBase Replacement)> _replacements;

        /// <summary>
        /// Creates a new <see cref="Builder"/> instance.
        /// </summary>
        /// <param name="capacity">Optional initial capacity of the collection of placeholders and their replacements.</param>
        public Builder(int capacity = 0)
        {
            _replacements = new List<(SqlNodeBase, SqlNodeBase)>( capacity );
        }

        /// <summary>
        /// Adds a placeholder and its replacement to this builder.
        /// </summary>
        /// <param name="placeholder">Placeholder node to replace.</param>
        /// <param name="replacement">Expression node to replace the placeholder with.</param>
        /// <returns><b>this</b>.</returns>
        public Builder Add(SqlExpressionPlaceholderNode placeholder, SqlExpressionNode replacement)
        {
            _replacements.Add( (placeholder, replacement) );
            return this;
        }

        /// <summary>
        /// Adds a placeholder and its replacement to this builder.
        /// </summary>
        /// <param name="placeholder">Placeholder node to replace.</param>
        /// <param name="replacement">Condition node to replace the placeholder with.</param>
        /// <returns><b>this</b>.</returns>
        public Builder Add(SqlConditionPlaceholderNode placeholder, SqlConditionNode replacement)
        {
            _replacements.Add( (placeholder, replacement) );
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="SqlPlaceholderNodeReplacementContext"/> based on the current state of this builder.
        /// </summary>
        /// <returns>New <see cref="SqlPlaceholderNodeReplacementContext"/> instance.</returns>
        [Pure]
        public SqlPlaceholderNodeReplacementContext Build()
        {
            return new SqlPlaceholderNodeReplacementContext( _replacements );
        }
    }
}
