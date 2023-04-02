using System.Collections.Generic;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct ExpressionBuilderResult
{
    internal ExpressionBuilderResult(
        Expression bodyExpression,
        ParameterExpression parameterExpression,
        IReadOnlyList<CompilableInlineDelegate> delegates,
        IReadOnlyDictionary<StringSegment, int> argumentIndexes,
        HashSet<StringSegment> discardedArguments)
    {
        BodyExpression = bodyExpression;
        ParameterExpression = parameterExpression;
        Delegates = delegates;
        ArgumentIndexes = argumentIndexes;
        DiscardedArguments = discardedArguments;
    }

    internal Expression BodyExpression { get; }
    internal ParameterExpression ParameterExpression { get; }
    internal IReadOnlyList<CompilableInlineDelegate> Delegates { get; }
    internal IReadOnlyDictionary<StringSegment, int> ArgumentIndexes { get; }
    internal HashSet<StringSegment> DiscardedArguments { get; }
}
