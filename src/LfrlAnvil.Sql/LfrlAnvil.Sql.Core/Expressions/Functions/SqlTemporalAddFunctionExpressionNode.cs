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
/// Represents an SQL syntax tree expression node that defines an invocation of a function that adds a value with a given unit
/// to the date and/or time parameter.
/// </summary>
public sealed class SqlTemporalAddFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTemporalAddFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode value, SqlTemporalUnit unit)
        : base( SqlFunctionType.TemporalAdd, new[] { argument, value } )
    {
        Ensure.IsDefined( unit );
        Unit = unit;
    }

    /// <summary>
    /// <see cref="SqlTemporalUnit"/> that specifies the unit of the added value.
    /// </summary>
    public SqlTemporalUnit Unit { get; }
}
