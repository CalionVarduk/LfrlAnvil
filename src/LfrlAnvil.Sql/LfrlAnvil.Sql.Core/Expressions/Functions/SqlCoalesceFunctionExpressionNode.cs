namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCoalesceFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCoalesceFunctionExpressionNode(SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Coalesce, arguments )
    {
        Ensure.IsNotEmpty( arguments, nameof( arguments ) );

        Type = arguments[0].Type;
        if ( Type is null || ! Type.Value.IsNullable )
            return;

        for ( var i = 1; i < arguments.Length; ++i )
        {
            var argType = arguments[i].Type;
            if ( SqlExpressionType.GetCommonType( Type, argType ) is null )
            {
                Type = null;
                break;
            }

            Assume.IsNotNull( argType, nameof( argType ) );
            if ( ! argType.Value.IsNullable )
            {
                Type = argType;
                break;
            }
        }
    }

    public override SqlExpressionType? Type { get; }
}
