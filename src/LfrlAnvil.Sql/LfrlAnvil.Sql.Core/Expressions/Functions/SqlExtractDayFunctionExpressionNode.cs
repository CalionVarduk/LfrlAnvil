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
/// Represents an SQL syntax tree expression node that defines an invocation of a function that extracts a day component from its parameter.
/// </summary>
public sealed class SqlExtractDayFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlExtractDayFunctionExpressionNode(SqlExpressionNode argument, SqlTemporalUnit unit)
        : base( SqlFunctionType.ExtractDay, new[] { argument } )
    {
        Assume.True( unit is SqlTemporalUnit.Year or SqlTemporalUnit.Month or SqlTemporalUnit.Week );
        Unit = unit;
    }

    /// <summary>
    /// <see cref="SqlTemporalUnit"/> that specifies the day component to extract. Can be one of the three following values:
    /// <list type="bullet">
    /// <item><description><see cref="SqlTemporalUnit.Year"/> for a day of year,</description></item>
    /// <item><description><see cref="SqlTemporalUnit.Month"/> for a day of month,</description></item>
    /// <item><description><see cref="SqlTemporalUnit.Week"/> for a day of week.</description></item>
    /// </list>
    /// </summary>
    public SqlTemporalUnit Unit { get; }
}
