using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct ExpressionBuilderResult
{
    internal ExpressionBuilderResult(
        Expression bodyExpression,
        ParameterExpression parameterExpression,
        IReadOnlyDictionary<StringSlice, int> argumentIndexes,
        HashSet<StringSlice> discardedArguments)
        : this( bodyExpression, parameterExpression, Array.Empty<CompilableInlineDelegate>(), argumentIndexes, discardedArguments ) { }

    internal ExpressionBuilderResult(
        Expression bodyExpression,
        ParameterExpression parameterExpression,
        IReadOnlyList<CompilableInlineDelegate> delegates,
        IReadOnlyDictionary<StringSlice, int> argumentIndexes,
        HashSet<StringSlice> discardedArguments)
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
    internal IReadOnlyDictionary<StringSlice, int> ArgumentIndexes { get; }
    internal HashSet<StringSlice> DiscardedArguments { get; }
}
