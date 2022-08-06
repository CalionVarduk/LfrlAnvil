using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Errors;

public sealed class MathExpressionBuilderAggregateError : MathExpressionBuilderError
{
    internal MathExpressionBuilderAggregateError(
        MathExpressionBuilderErrorType type,
        IReadOnlyList<MathExpressionBuilderError> inner,
        StringSlice? token = null)
        : base( type, token )
    {
        Debug.Assert( inner.Count > 0, "Inner errors collection should not be empty." );
        Inner = inner;
    }

    public IReadOnlyList<MathExpressionBuilderError> Inner { get; }

    [Pure]
    public override string ToString()
    {
        var headerText = $"{base.ToString()}, with {Inner.Count} inner error(s):";
        var innerTexts = Inner.Select( (e, i) => $"{i + 1}. {e}" );
        var fullInnerText = string.Join( $"{Environment.NewLine}{Environment.NewLine}", innerTexts );
        return $"{headerText}{Environment.NewLine}{fullInnerText}";
    }
}
