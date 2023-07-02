using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Decorators;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

public static partial class SqlNode
{
    public static class AggregateFunctions
    {
        private static SqlDistinctAggregateFunctionDecoratorNode? _distinct;

        [Pure]
        public static SqlCountAggregateFunctionExpressionNode Count(SqlExpressionNode argument)
        {
            return new SqlCountAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlAggregateFunctionDecoratorNode>.Empty );
        }

        [Pure]
        public static SqlMinAggregateFunctionExpressionNode Min(SqlExpressionNode argument)
        {
            return new SqlMinAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlAggregateFunctionDecoratorNode>.Empty );
        }

        [Pure]
        public static SqlMaxAggregateFunctionExpressionNode Max(SqlExpressionNode argument)
        {
            return new SqlMaxAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlAggregateFunctionDecoratorNode>.Empty );
        }

        [Pure]
        public static SqlSumAggregateFunctionExpressionNode Sum(SqlExpressionNode argument)
        {
            return new SqlSumAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlAggregateFunctionDecoratorNode>.Empty );
        }

        [Pure]
        public static SqlAverageAggregateFunctionExpressionNode Average(SqlExpressionNode argument)
        {
            return new SqlAverageAggregateFunctionExpressionNode( new[] { argument }, Chain<SqlAggregateFunctionDecoratorNode>.Empty );
        }

        [Pure]
        public static SqlStringConcatAggregateFunctionExpressionNode StringConcat(
            SqlExpressionNode argument,
            SqlExpressionNode? separator = null)
        {
            var arguments = separator is null ? new[] { argument } : new[] { argument, separator };
            return new SqlStringConcatAggregateFunctionExpressionNode( arguments, Chain<SqlAggregateFunctionDecoratorNode>.Empty );
        }

        [Pure]
        public static SqlDistinctAggregateFunctionDecoratorNode DistinctDecorator()
        {
            return _distinct ??= new SqlDistinctAggregateFunctionDecoratorNode();
        }

        [Pure]
        public static SqlFilterAggregateFunctionDecoratorNode FilterDecorator(SqlConditionNode filter, bool isConjunction)
        {
            return new SqlFilterAggregateFunctionDecoratorNode( filter, isConjunction );
        }
    }
}
