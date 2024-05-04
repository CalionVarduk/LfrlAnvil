using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents a collection of errors that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation.
/// </summary>
public sealed class ParsedExpressionBuilderAggregateError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderAggregateError(
        ParsedExpressionBuilderErrorType type,
        Chain<ParsedExpressionBuilderError> inner,
        StringSegment? token = null)
        : base( type, token )
    {
        Assume.IsNotEmpty( inner );
        Inner = inner;
    }

    /// <summary>
    /// Collection of inner errors.
    /// </summary>
    public Chain<ParsedExpressionBuilderError> Inner { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderAggregateError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var headerText = $"{base.ToString()}, with {Inner.Count} inner error(s):";
        var innerTexts = Inner.Select(
            static (e, i) => $"  {i + 1}. {e.ToString().Replace( Environment.NewLine, $"{Environment.NewLine}  " )}" );

        var fullInnerText = string.Join( Environment.NewLine, innerTexts );
        return $"{headerText}{Environment.NewLine}{fullInnerText}";
    }
}
