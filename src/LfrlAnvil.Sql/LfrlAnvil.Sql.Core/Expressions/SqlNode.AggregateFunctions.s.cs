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

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public static partial class SqlNode
{
    /// <summary>
    /// Creates instances of <see cref="SqlAggregateFunctionExpressionNode"/> type.
    /// </summary>
    public static class AggregateFunctions
    {
        /// <summary>
        /// Creates a new <see cref="SqlNamedAggregateFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="name">Aggregate function's name.</param>
        /// <param name="arguments">Collection of aggregate function's arguments.</param>
        /// <returns>New <see cref="SqlNamedAggregateFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlNamedAggregateFunctionExpressionNode Named(SqlSchemaObjectName name, params SqlExpressionNode[] arguments)
        {
            return new SqlNamedAggregateFunctionExpressionNode( name, arguments, Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlCountAggregateFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the number of records for.</param>
        /// <returns>New <see cref="SqlCountAggregateFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlCountAggregateFunctionExpressionNode Count(SqlExpressionNode argument)
        {
            return new SqlCountAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlMinAggregateFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the minimum value for.</param>
        /// <returns>New <see cref="SqlMinAggregateFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlMinAggregateFunctionExpressionNode Min(SqlExpressionNode argument)
        {
            return new SqlMinAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlMaxAggregateFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the maximum value for.</param>
        /// <returns>New <see cref="SqlMaxAggregateFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlMaxAggregateFunctionExpressionNode Max(SqlExpressionNode argument)
        {
            return new SqlMaxAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlSumAggregateFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the sum value for.</param>
        /// <returns>New <see cref="SqlSumAggregateFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlSumAggregateFunctionExpressionNode Sum(SqlExpressionNode argument)
        {
            return new SqlSumAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlAverageAggregateFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the average value for.</param>
        /// <returns>New <see cref="SqlAverageAggregateFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlAverageAggregateFunctionExpressionNode Average(SqlExpressionNode argument)
        {
            return new SqlAverageAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlStringConcatAggregateFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the concatenated string for.</param>
        /// <param name="separator">Optional separator of concatenated strings. Equal to null by default.</param>
        /// <returns>New <see cref="SqlStringConcatAggregateFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlStringConcatAggregateFunctionExpressionNode StringConcat(
            SqlExpressionNode argument,
            SqlExpressionNode? separator = null)
        {
            var arguments = separator is null ? new[] { argument } : new[] { argument, separator };
            return new SqlStringConcatAggregateFunctionExpressionNode( arguments, Chain<SqlTraitNode>.Empty );
        }
    }
}
