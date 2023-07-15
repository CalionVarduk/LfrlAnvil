using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public static partial class SqlNode
{
    public static class AggregateFunctions
    {
        private static SqlDistinctAggregateFunctionTraitNode? _distinct;

        [Pure]
        public static SqlCountAggregateFunctionExpressionNode Count(SqlExpressionNode argument)
        {
            return new SqlCountAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlAggregateFunctionTraitNode>.Empty );
        }

        [Pure]
        public static SqlMinAggregateFunctionExpressionNode Min(SqlExpressionNode argument)
        {
            return new SqlMinAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlAggregateFunctionTraitNode>.Empty );
        }

        [Pure]
        public static SqlMaxAggregateFunctionExpressionNode Max(SqlExpressionNode argument)
        {
            return new SqlMaxAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlAggregateFunctionTraitNode>.Empty );
        }

        [Pure]
        public static SqlSumAggregateFunctionExpressionNode Sum(SqlExpressionNode argument)
        {
            return new SqlSumAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlAggregateFunctionTraitNode>.Empty );
        }

        [Pure]
        public static SqlAverageAggregateFunctionExpressionNode Average(SqlExpressionNode argument)
        {
            return new SqlAverageAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlAggregateFunctionTraitNode>.Empty );
        }

        [Pure]
        public static SqlStringConcatAggregateFunctionExpressionNode StringConcat(
            SqlExpressionNode argument,
            SqlExpressionNode? separator = null)
        {
            var arguments = separator is null ? new[] { argument } : new[] { argument, separator };
            return new SqlStringConcatAggregateFunctionExpressionNode( arguments, Chain<SqlAggregateFunctionTraitNode>.Empty );
        }

        [Pure]
        public static SqlDistinctAggregateFunctionTraitNode DistinctTrait()
        {
            return _distinct ??= new SqlDistinctAggregateFunctionTraitNode();
        }

        [Pure]
        public static SqlFilterAggregateFunctionTraitNode FilterTrait(SqlConditionNode filter, bool isConjunction)
        {
            return new SqlFilterAggregateFunctionTraitNode( filter, isConjunction );
        }
    }
}
