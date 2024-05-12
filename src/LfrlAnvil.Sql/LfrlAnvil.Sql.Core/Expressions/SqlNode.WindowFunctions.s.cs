using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public static partial class SqlNode
{
    /// <summary>
    /// Creates instances of <see cref="SqlAggregateFunctionExpressionNode"/> type that represent window functions.
    /// </summary>
    public static class WindowFunctions
    {
        private static SqlRowNumberWindowFunctionExpressionNode? _rowNumber;
        private static SqlRankWindowFunctionExpressionNode? _rank;
        private static SqlDenseRankWindowFunctionExpressionNode? _denseRank;
        private static SqlCumulativeDistributionWindowFunctionExpressionNode? _cumulativeDistribution;

        /// <summary>
        /// Creates a new <see cref="SqlRowNumberWindowFunctionExpressionNode"/> instance.
        /// </summary>
        /// <returns>New <see cref="SqlRowNumberWindowFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlRowNumberWindowFunctionExpressionNode RowNumber()
        {
            return _rowNumber ??= new SqlRowNumberWindowFunctionExpressionNode( Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlRankWindowFunctionExpressionNode"/> instance.
        /// </summary>
        /// <returns>New <see cref="SqlRankWindowFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlRankWindowFunctionExpressionNode Rank()
        {
            return _rank ??= new SqlRankWindowFunctionExpressionNode( Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlDenseRankWindowFunctionExpressionNode"/> instance.
        /// </summary>
        /// <returns>New <see cref="SqlDenseRankWindowFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlDenseRankWindowFunctionExpressionNode DenseRank()
        {
            return _denseRank ??= new SqlDenseRankWindowFunctionExpressionNode( Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlCumulativeDistributionWindowFunctionExpressionNode"/> instance.
        /// </summary>
        /// <returns>New <see cref="SqlCumulativeDistributionWindowFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlCumulativeDistributionWindowFunctionExpressionNode CumulativeDistribution()
        {
            return _cumulativeDistribution ??= new SqlCumulativeDistributionWindowFunctionExpressionNode( Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlNTileWindowFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="n">Number of groups.</param>
        /// <returns>New <see cref="SqlNTileWindowFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlNTileWindowFunctionExpressionNode NTile(SqlExpressionNode n)
        {
            return new SqlNTileWindowFunctionExpressionNode( new[] { n }, Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlLagWindowFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to calculate the lag for.</param>
        /// <param name="offset">Optional offset. Equal to SQL literal that represents <b>1</b> by default.</param>
        /// <param name="default">Optional default value. Equal to null by default.</param>
        /// <returns>New <see cref="SqlLagWindowFunctionExpressionNode"/> instance.</returns>
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

        /// <summary>
        /// Creates a new <see cref="SqlLeadWindowFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to calculate the lead for.</param>
        /// <param name="offset">Optional offset. Equal to SQL literal that represents <b>1</b> by default.</param>
        /// <param name="default">Optional default value. Equal to null by default.</param>
        /// <returns>New <see cref="SqlLeadWindowFunctionExpressionNode"/> instance.</returns>
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

        /// <summary>
        /// Creates a new <see cref="SqlFirstValueWindowFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to calculate the first value for.</param>
        /// <returns>New <see cref="SqlFirstValueWindowFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlFirstValueWindowFunctionExpressionNode FirstValue(SqlExpressionNode expression)
        {
            return new SqlFirstValueWindowFunctionExpressionNode( new[] { expression }, Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlLastValueWindowFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to calculate the last value for.</param>
        /// <returns>New <see cref="SqlLastValueWindowFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlLastValueWindowFunctionExpressionNode LastValue(SqlExpressionNode expression)
        {
            return new SqlLastValueWindowFunctionExpressionNode( new[] { expression }, Chain<SqlTraitNode>.Empty );
        }

        /// <summary>
        /// Creates a new <see cref="SqlNthValueWindowFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to calculate the n-th value for.</param>
        /// <param name="n">Row's position.</param>
        /// <returns>New <see cref="SqlNthValueWindowFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlNthValueWindowFunctionExpressionNode NthValue(SqlExpressionNode expression, SqlExpressionNode n)
        {
            return new SqlNthValueWindowFunctionExpressionNode( new[] { expression, n }, Chain<SqlTraitNode>.Empty );
        }
    }
}
