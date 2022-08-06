using System.Collections.Generic;
using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

internal readonly struct MathExpressionBuilderResult
{
    internal MathExpressionBuilderResult(
        Expression bodyExpression,
        ParameterExpression parameterExpression,
        IReadOnlyDictionary<StringSlice, int> argumentIndexes)
    {
        BodyExpression = bodyExpression;
        ParameterExpression = parameterExpression;
        ArgumentIndexes = argumentIndexes;
    }

    internal Expression BodyExpression { get; }
    internal ParameterExpression ParameterExpression { get; }
    internal IReadOnlyDictionary<StringSlice, int> ArgumentIndexes { get; }
}
