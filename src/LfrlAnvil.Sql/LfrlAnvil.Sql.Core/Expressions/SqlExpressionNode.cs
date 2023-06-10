using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

#pragma warning disable CS0660, CS0661
public abstract class SqlExpressionNode : SqlNodeBase
#pragma warning restore CS0660, CS0661
{
    protected SqlExpressionNode(SqlNodeType nodeType)
        : base( nodeType ) { }

    public abstract SqlExpressionType? Type { get; }

    [Pure]
    public static SqlNegateExpressionNode operator -(SqlExpressionNode node)
    {
        return node.Negate();
    }

    [Pure]
    public static SqlBitwiseNotExpressionNode operator ~(SqlExpressionNode node)
    {
        return node.BitwiseNot();
    }

    [Pure]
    public static SqlAddExpressionNode operator +(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.Add( right );
    }

    [Pure]
    public static SqlSubtractExpressionNode operator -(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.Subtract( right );
    }

    [Pure]
    public static SqlMultiplyExpressionNode operator *(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.Multiply( right );
    }

    [Pure]
    public static SqlDivideExpressionNode operator /(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.Divide( right );
    }

    [Pure]
    public static SqlModuloExpressionNode operator %(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.Modulo( right );
    }

    [Pure]
    public static SqlBitwiseAndExpressionNode operator &(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.BitwiseAnd( right );
    }

    [Pure]
    public static SqlBitwiseOrExpressionNode operator |(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.BitwiseOr( right );
    }

    [Pure]
    public static SqlBitwiseXorExpressionNode operator ^(SqlExpressionNode left, SqlExpressionNode right)
    {
        return left.BitwiseXor( right );
    }

    [Pure]
    public static SqlConditionNode operator ==(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsEqualTo( right );
    }

    [Pure]
    public static SqlConditionNode operator !=(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsNotEqualTo( right );
    }

    [Pure]
    public static SqlConditionNode operator >(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsGreaterThan( right );
    }

    [Pure]
    public static SqlConditionNode operator <(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsLessThan( right );
    }

    [Pure]
    public static SqlConditionNode operator >=(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsGreaterThanOrEqualTo( right );
    }

    [Pure]
    public static SqlConditionNode operator <=(SqlExpressionNode? left, SqlExpressionNode? right)
    {
        return left.IsLessThanOrEqualTo( right );
    }

    protected void AppendTypeTo(StringBuilder builder)
    {
        builder.Append( ' ' ).Append( ':' ).Append( ' ' );
        if ( Type is null )
            builder.Append( '?' );
        else
            builder.Append( Type.Value.ToString() );
    }
}
