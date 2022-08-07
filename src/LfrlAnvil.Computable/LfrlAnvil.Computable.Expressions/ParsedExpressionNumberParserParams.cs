using System;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

public readonly struct ParsedExpressionNumberParserParams
{
    public ParsedExpressionNumberParserParams(
        ParsedExpressionFactoryInternalConfiguration configuration,
        Type argumentType,
        Type resultType)
    {
        Configuration = configuration;
        ArgumentType = argumentType;
        ResultType = resultType;
    }

    public ParsedExpressionFactoryInternalConfiguration Configuration { get; }
    public Type ArgumentType { get; }
    public Type ResultType { get; }
}
