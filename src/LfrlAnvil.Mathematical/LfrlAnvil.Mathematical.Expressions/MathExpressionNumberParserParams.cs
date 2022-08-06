using System;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions;

public readonly struct MathExpressionNumberParserParams
{
    public MathExpressionNumberParserParams(MathExpressionFactoryInternalConfiguration configuration, Type argumentType, Type resultType)
    {
        Configuration = configuration;
        ArgumentType = argumentType;
        ResultType = resultType;
    }

    public MathExpressionFactoryInternalConfiguration Configuration { get; }
    public Type ArgumentType { get; }
    public Type ResultType { get; }
}
