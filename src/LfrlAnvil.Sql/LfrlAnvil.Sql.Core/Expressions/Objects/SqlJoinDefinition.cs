using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Objects;

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

    public SqlJoinType JoinType { get; }
    public SqlRecordSetNode InnerRecordSet { get; }
    public Func<ExpressionParams, SqlConditionNode> OnExpression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlJoinDefinition Inner(SqlRecordSetNode inner, Func<ExpressionParams, SqlConditionNode> onExpression)
    {
        return new SqlJoinDefinition( SqlJoinType.Inner, inner, onExpression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlJoinDefinition Left(SqlRecordSetNode inner, Func<ExpressionParams, SqlConditionNode> onExpression)
    {
        return new SqlJoinDefinition( SqlJoinType.Left, inner, onExpression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlJoinDefinition Right(SqlRecordSetNode inner, Func<ExpressionParams, SqlConditionNode> onExpression)
    {
        return new SqlJoinDefinition( SqlJoinType.Right, inner, onExpression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlJoinDefinition Full(SqlRecordSetNode inner, Func<ExpressionParams, SqlConditionNode> onExpression)
    {
        return new SqlJoinDefinition( SqlJoinType.Full, inner, onExpression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlJoinDefinition Cross(SqlRecordSetNode inner)
    {
        return new SqlJoinDefinition( SqlJoinType.Cross, inner, static _ => SqlNode.True() );
    }

    public readonly struct ExpressionParams
    {
        private readonly Dictionary<string, SqlRecordSetNode> _recordSets;

        internal ExpressionParams(Dictionary<string, SqlRecordSetNode> recordSets, SqlRecordSetNode inner)
        {
            _recordSets = recordSets;
            Inner = inner;
        }

        public SqlRecordSetNode Inner { get; }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public SqlRecordSetNode GetOuter(string name)
        {
            return _recordSets[name];
        }
    }
}
