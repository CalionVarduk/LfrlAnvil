using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlLiteralNode : SqlExpressionNode
{
    internal SqlLiteralNode(TypeNullability type)
        : base( SqlNodeType.Literal )
    {
        Type = type;
    }

    public TypeNullability Type { get; }

    [Pure]
    public abstract object GetValue();

    [Pure]
    public abstract string GetSql(ISqlColumnTypeDefinitionProvider typeDefinitionProvider);
}

public sealed class SqlLiteralNode<T> : SqlLiteralNode
    where T : notnull
{
    internal SqlLiteralNode(T value)
        : base( TypeNullability.Create<T>() )
    {
        Value = value;
    }

    public T Value { get; }

    [Pure]
    public override object GetValue()
    {
        return Value;
    }

    [Pure]
    public override string GetSql(ISqlColumnTypeDefinitionProvider typeDefinitionProvider)
    {
        var definition = typeDefinitionProvider.GetByType<T>();
        var result = definition.ToDbLiteral( Value );
        return result;
    }
}
