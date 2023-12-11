using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public static partial class SqlNode
{
    public static class WindowFunctions
    {
        private static SqlRowNumberWindowFunctionExpressionNode? _rowNumber;
        private static SqlRankWindowFunctionExpressionNode? _rank;
        private static SqlDenseRankWindowFunctionExpressionNode? _denseRank;
        private static SqlCumulativeDistributionWindowFunctionExpressionNode? _cumulativeDistribution;

        [Pure]
        public static SqlRowNumberWindowFunctionExpressionNode RowNumber()
        {
            return _rowNumber ??= new SqlRowNumberWindowFunctionExpressionNode( Chain<SqlTraitNode>.Empty );
        }

        [Pure]
        public static SqlRankWindowFunctionExpressionNode Rank()
        {
            return _rank ??= new SqlRankWindowFunctionExpressionNode( Chain<SqlTraitNode>.Empty );
        }

        [Pure]
        public static SqlDenseRankWindowFunctionExpressionNode DenseRank()
        {
            return _denseRank ??= new SqlDenseRankWindowFunctionExpressionNode( Chain<SqlTraitNode>.Empty );
        }

        [Pure]
        public static SqlCumulativeDistributionWindowFunctionExpressionNode CumulativeDistribution()
        {
            return _cumulativeDistribution ??= new SqlCumulativeDistributionWindowFunctionExpressionNode( Chain<SqlTraitNode>.Empty );
        }

        [Pure]
        public static SqlNTileWindowFunctionExpressionNode NTile(SqlExpressionNode n)
        {
            return new SqlNTileWindowFunctionExpressionNode( new[] { n }, Chain<SqlTraitNode>.Empty );
        }

        [Pure]
        public static SqlLagWindowFunctionExpressionNode Lag(
            SqlExpressionNode expression,
            SqlExpressionNode? offset = null,
            SqlExpressionNode? @default = null)
        {
            var arguments = @default is not null
                ? new[] { expression, offset ?? Literal( 1 ), @default }
                : offset is not null
                    ? new[] { expression, offset }
                    : new[] { expression, Literal( 1 ) };

            return new SqlLagWindowFunctionExpressionNode( arguments, Chain<SqlTraitNode>.Empty );
        }

        [Pure]
        public static SqlLeadWindowFunctionExpressionNode Lead(
            SqlExpressionNode expression,
            SqlExpressionNode? offset = null,
            SqlExpressionNode? @default = null)
        {
            var arguments = @default is not null
                ? new[] { expression, offset ?? Literal( 1 ), @default }
                : offset is not null
                    ? new[] { expression, offset }
                    : new[] { expression, Literal( 1 ) };

            return new SqlLeadWindowFunctionExpressionNode( arguments, Chain<SqlTraitNode>.Empty );
        }

        [Pure]
        public static SqlFirstValueWindowFunctionExpressionNode FirstValue(SqlExpressionNode expression)
        {
            return new SqlFirstValueWindowFunctionExpressionNode( new[] { expression }, Chain<SqlTraitNode>.Empty );
        }

        [Pure]
        public static SqlLastValueWindowFunctionExpressionNode LastValue(SqlExpressionNode expression)
        {
            return new SqlLastValueWindowFunctionExpressionNode( new[] { expression }, Chain<SqlTraitNode>.Empty );
        }

        [Pure]
        public static SqlNthValueWindowFunctionExpressionNode NthValue(SqlExpressionNode expression, SqlExpressionNode n)
        {
            return new SqlNthValueWindowFunctionExpressionNode( new[] { expression, n }, Chain<SqlTraitNode>.Empty );
        }
    }
}
