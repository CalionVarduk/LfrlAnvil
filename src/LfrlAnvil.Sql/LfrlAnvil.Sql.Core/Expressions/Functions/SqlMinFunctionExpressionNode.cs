namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlMinFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlMinFunctionExpressionNode(SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Min, arguments )
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
