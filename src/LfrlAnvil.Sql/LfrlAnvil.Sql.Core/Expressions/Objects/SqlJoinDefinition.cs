using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents a definition of a record set join operation.
/// </summary>
public readonly struct SqlJoinDefinition
{
    private SqlJoinDefinition(
        SqlJoinType joinType,
        SqlRecordSetNode innerRecordSet,
        Func<ExpressionParams, SqlConditionNode> onExpression)
    {
        JoinType = joinType;
        InnerRecordSet = innerRecordSet;
        OnExpression = onExpression;
    }

    /// <summary>
    /// Type of the join operation to perform.
    /// </summary>
    public SqlJoinType JoinType { get; }

    /// <summary>
    /// Inner <see cref="SqlRecordSetNode"/> instance.
    /// </summary>
    public SqlRecordSetNode InnerRecordSet { get; }

    /// <summary>
    /// Callback that creates a condition of the join operation.
    /// </summary>
    public Func<ExpressionParams, SqlConditionNode> OnExpression { get; }

    /// <summary>
    /// Creates a new <see cref="SqlJoinDefinition"/> instance with <see cref="SqlJoinType.Inner"/> type.
    /// </summary>
    /// <param name="inner">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Callback that creates a condition of the join operation.</param>
    /// <returns>New <see cref="SqlJoinDefinition"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlJoinDefinition Inner(SqlRecordSetNode inner, Func<ExpressionParams, SqlConditionNode> onExpression)
    {
        return new SqlJoinDefinition( SqlJoinType.Inner, inner, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlJoinDefinition"/> instance with <see cref="SqlJoinType.Left"/> type.
    /// </summary>
    /// <param name="inner">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Callback that creates a condition of the join operation.</param>
    /// <returns>New <see cref="SqlJoinDefinition"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlJoinDefinition Left(SqlRecordSetNode inner, Func<ExpressionParams, SqlConditionNode> onExpression)
    {
        return new SqlJoinDefinition( SqlJoinType.Left, inner, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlJoinDefinition"/> instance with <see cref="SqlJoinType.Left"/> type.
    /// </summary>
    /// <param name="inner">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Callback that creates a condition of the join operation.</param>
    /// <returns>New <see cref="SqlJoinDefinition"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlJoinDefinition Right(SqlRecordSetNode inner, Func<ExpressionParams, SqlConditionNode> onExpression)
    {
        return new SqlJoinDefinition( SqlJoinType.Right, inner, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlJoinDefinition"/> instance with <see cref="SqlJoinType.Full"/> type.
    /// </summary>
    /// <param name="inner">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Callback that creates a condition of the join operation.</param>
    /// <returns>New <see cref="SqlJoinDefinition"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlJoinDefinition Full(SqlRecordSetNode inner, Func<ExpressionParams, SqlConditionNode> onExpression)
    {
        return new SqlJoinDefinition( SqlJoinType.Full, inner, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlJoinDefinition"/> instance with <see cref="SqlJoinType.Cross"/> type.
    /// </summary>
    /// <param name="inner">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <returns>New <see cref="SqlJoinDefinition"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlJoinDefinition Cross(SqlRecordSetNode inner)
    {
        return new SqlJoinDefinition( SqlJoinType.Cross, inner, static _ => SqlNode.True() );
    }

    /// <summary>
    /// Represents parameters of the <see cref="SqlJoinDefinition.OnExpression"/> callback.
    /// </summary>
    public readonly struct ExpressionParams
    {
        private readonly Dictionary<string, SqlRecordSetNode> _recordSets;

        internal ExpressionParams(Dictionary<string, SqlRecordSetNode> recordSets, SqlRecordSetNode inner)
        {
            _recordSets = recordSets;
            Inner = inner;
        }

        /// <summary>
        /// Inner <see cref="SqlRecordSetNode"/> instance.
        /// </summary>
        public SqlRecordSetNode Inner { get; }

        /// <summary>
        /// Returns an outer <see cref="SqlRecordSetNode"/> instance by the given <paramref name="identifier"/>;
        /// </summary>
        /// <param name="identifier"><see cref="SqlRecordSetNode.Identifier"/> of the outer record set.</param>
        /// <returns>Outer <see cref="SqlRecordSetNode"/> instance associated with the given <paramref name="identifier"/>.</returns>
        /// <exception cref="KeyNotFoundException">When outer record set does not exist.</exception>
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public SqlRecordSetNode GetOuter(string identifier)
        {
            return _recordSets[identifier];
        }
    }
}
