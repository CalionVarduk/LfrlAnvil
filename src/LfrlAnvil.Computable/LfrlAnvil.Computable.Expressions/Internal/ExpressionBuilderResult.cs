using System.Collections.Generic;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct ExpressionBuilderResult
{
    internal ExpressionBuilderResult(
        ParameterExpression parameterExpression,
        Expression bodyExpression,
        IReadOnlyList<CompilableInlineDelegate> delegates,
        IReadOnlyDictionary<StringSlice, int> argumentIndexes)
    {
        ParameterExpression = parameterExpression;
        BodyExpression = bodyExpression;
        Delegates = delegates;
        ArgumentIndexes = argumentIndexes;
    }

    internal ParameterExpression ParameterExpression { get; }
    internal Expression BodyExpression { get; }
    internal IReadOnlyList<CompilableInlineDelegate> Delegates { get; }
    internal IReadOnlyDictionary<StringSlice, int> ArgumentIndexes { get; }
}
