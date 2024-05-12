using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

#pragma warning disable CS0660, CS0661
/// <summary>
/// Represents an SQL syntax tree expression node.
/// </summary>
public abstract class SqlExpressionNode : SqlNodeBase
#pragma warning restore CS0660, CS0661
{
    internal SqlExpressionNode(SqlNodeType nodeType)
        : base( nodeType ) { }

    /// <summary>
    /// Creates a new <see cref="SqlExpressionNode"/> of <see cref="SqlNodeType.Unknown"/> type.
    /// </summary>
    protected SqlExpressionNode() { }

    /// <summary>
    /// Creates a new <see cref="SqlNegateExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <returns>New <see cref="SqlNegateExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlNegateExpressionNode operator -(SqlExpressionNode node)
    {
        return node.Negate();
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseNotExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <returns>New <see cref="SqlBitwiseNotExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlBitwiseNotExpressionNode operator ~(SqlExpressionNode node)
    {
        return node.BitwiseNot();
    }

    /// <summary>
    /// Creates a new <see cref="SqlAddExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlAddExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlAddExpressionNode operator +(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.Add( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSubtractExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlSubtractExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlSubtractExpressionNode operator -(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.Subtract( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiplyExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlMultiplyExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlMultiplyExpressionNode operator *(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.Multiply( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDivideExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlDivideExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlDivideExpressionNode operator /(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.Divide( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlModuloExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlModuloExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlModuloExpressionNode operator %(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.Modulo( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseAndExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlBitwiseAndExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlBitwiseAndExpressionNode operator &(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.BitwiseAnd( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseOrExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlBitwiseOrExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlBitwiseOrExpressionNode operator |(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.BitwiseOr( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseXorExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlBitwiseXorExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlBitwiseXorExpressionNode operator ^(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.BitwiseXor( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlEqualToConditionNode"/> instance.</returns>
    [Pure]
    public static SqlConditionNode operator ==(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsEqualTo( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNotEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlNotEqualToConditionNode"/> instance.</returns>
    [Pure]
    public static SqlConditionNode operator !=(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsNotEqualTo( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlGreaterThanConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlGreaterThanConditionNode"/> instance.</returns>
    [Pure]
    public static SqlConditionNode operator >(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsGreaterThan( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLessThanConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlLessThanConditionNode"/> instance.</returns>
    [Pure]
    public static SqlConditionNode operator <(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsLessThan( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlGreaterThanOrEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlGreaterThanOrEqualToConditionNode"/> instance.</returns>
    [Pure]
    public static SqlConditionNode operator >=(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsGreaterThanOrEqualTo( right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLessThanOrEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>New <see cref="SqlLessThanOrEqualToConditionNode"/> instance.</returns>
    [Pure]
    public static SqlConditionNode operator <=(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsLessThanOrEqualTo( right );
    }
}
