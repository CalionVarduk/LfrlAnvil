using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionNonInvocableTypeExpression : InvalidOperationException
{
    public ParsedExpressionNonInvocableTypeExpression(Type type)
        : base( Resources.NonInvocableType( type ) )
    {
        Type = type;
    }

    public Type Type { get; }
}
