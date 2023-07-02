namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlMaxFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlMaxFunctionExpressionNode(SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Max, arguments )
    {
        Ensure.IsNotEmpty( arguments, nameof( arguments ) );

        Type = arguments[0].Type;
        if ( Type is null )
            return;

        for ( var i = 1; i < arguments.Length; ++i )
        {
            Type = SqlExpressionType.GetCommonType( Type, arguments[i].Type );
            if ( Type is null )
                break;
        }
    }

    public override SqlExpressionType? Type { get; }
}
