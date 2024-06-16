// Copyright 2024 Łukasz Furlepa
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

using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single common table expression trait.
/// </summary>
public sealed class SqlCommonTableExpressionTraitNode : SqlTraitNode
{
    internal SqlCommonTableExpressionTraitNode(SqlCommonTableExpressionNode[] commonTableExpressions)
        : base( SqlNodeType.CommonTableExpressionTrait )
    {
        CommonTableExpressions = commonTableExpressions;
    }

    /// <summary>
    /// Collection of common table expressions.
    /// </summary>
    public ReadOnlyArray<SqlCommonTableExpressionNode> CommonTableExpressions { get; }

    /// <summary>
    /// Specifies whether or not <see cref="CommonTableExpressions"/> contains at least one <see cref="SqlCommonTableExpressionNode"/>
    /// that is marked as recursive.
    /// </summary>
    public bool ContainsRecursive
    {
        get
        {
            foreach ( var cte in CommonTableExpressions )
            {
                if ( cte.IsRecursive )
                    return true;
            }

            return false;
        }
    }
}
