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

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that calculates a difference
/// between two date and/or time parameters and converts the result to the given unit.
/// </summary>
public sealed class SqlTemporalDiffFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTemporalDiffFunctionExpressionNode(SqlExpressionNode start, SqlExpressionNode end, SqlTemporalUnit unit)
        : base( SqlFunctionType.TemporalDiff, new[] { start, end } )
    {
        Ensure.IsDefined( unit );
        Unit = unit;
    }

    /// <summary>
    /// <see cref="SqlTemporalUnit"/> that specifies the unit of the returned result.
    /// </summary>
    public SqlTemporalUnit Unit { get; }
}
