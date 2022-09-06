using System;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderAggregateError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderAggregateError(
        ParsedExpressionBuilderErrorType type,
        Chain<ParsedExpressionBuilderError> inner,
        StringSliceOld? token = null)
        : base( type, token )
    {
        Assume.IsNotEmpty( inner, nameof( inner ) );
        Inner = inner;
    }

    public Chain<ParsedExpressionBuilderError> Inner { get; }

    [Pure]
    public override string ToString()
    {
        var headerText = $"{base.ToString()}, with {Inner.Count} inner error(s):";
        var innerTexts = Inner.Select( (e, i) => $"  {i + 1}. {e.ToString().Replace( Environment.NewLine, $"{Environment.NewLine}  " )}" );
        var fullInnerText = string.Join( Environment.NewLine, innerTexts );
        return $"{headerText}{Environment.NewLine}{fullInnerText}";
    }
}
