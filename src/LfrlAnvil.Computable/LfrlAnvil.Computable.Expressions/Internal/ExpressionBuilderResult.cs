using System.Collections.Generic;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct ExpressionBuilderResult
{
    internal ExpressionBuilderResult(
        ParameterExpression parameterExpression,
        Expression bodyExpression,
        IReadOnlyDictionary<StringSlice, int> argumentIndexes)
    {
        ParameterExpression = parameterExpression;
        BodyExpression = bodyExpression;
        ArgumentIndexes = argumentIndexes;
    }

    internal ParameterExpression ParameterExpression { get; }
    internal Expression BodyExpression { get; }
    internal IReadOnlyDictionary<StringSlice, int> ArgumentIndexes { get; }
}
