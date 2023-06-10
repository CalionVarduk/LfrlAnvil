using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public abstract class SqlDataSourceDecoratorNode : SqlNodeBase
{
    internal SqlDataSourceDecoratorNode(SqlNodeType nodeType, SqlDataSourceDecoratorNode? @base)
        : base( nodeType )
    {
        Base = @base;
        Level = @base is null ? 1 : @base.Level + 1;
    }

    public abstract SqlDataSourceNode DataSource { get; }
    public SqlDataSourceDecoratorNode? Base { get; }
    public int Level { get; }

    [Pure]
    public SqlDataSourceDecoratorNode[] Reduce()
    {
        var result = new SqlDataSourceDecoratorNode[Level];
        var index = Level - 1;
        var next = this;

        do
        {
            result[index--] = next;
            next = next.Base;
        }
        while ( next is not null );

        Assume.Equals( index, -1, nameof( index ) );
        return result;
    }
}

public abstract class SqlDataSourceDecoratorNode<TDataSourceNode> : SqlDataSourceDecoratorNode
    where TDataSourceNode : SqlDataSourceNode
{
    protected SqlDataSourceDecoratorNode(SqlNodeType nodeType, TDataSourceNode dataSource)
        : base( nodeType, @base: null )
    {
        DataSource = dataSource;
    }

    protected SqlDataSourceDecoratorNode(SqlNodeType nodeType, SqlDataSourceDecoratorNode<TDataSourceNode> @base)
        : base( nodeType, @base )
    {
        DataSource = @base.DataSource;
    }

    public sealed override TDataSourceNode DataSource { get; }
}
