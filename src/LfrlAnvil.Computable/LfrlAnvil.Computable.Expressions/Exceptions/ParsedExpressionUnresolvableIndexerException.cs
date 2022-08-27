using System;
using System.Collections.Generic;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionUnresolvableIndexerException : InvalidOperationException
{
    public ParsedExpressionUnresolvableIndexerException(Type targetType, IReadOnlyList<Type> parameterTypes)
        : base( Resources.UnresolvableIndexer( targetType, parameterTypes ) )
    {
        TargetType = targetType;
        ParameterTypes = parameterTypes;
    }

    public Type TargetType { get; }
    public IReadOnlyList<Type> ParameterTypes { get; }
}
