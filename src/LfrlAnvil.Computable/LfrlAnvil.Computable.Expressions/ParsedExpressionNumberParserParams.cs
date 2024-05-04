using System;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents parameters for creating an <see cref="IParsedExpressionNumberParser"/> instance.
/// </summary>
public readonly struct ParsedExpressionNumberParserParams
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionNumberParserParams"/> instance.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <param name="argumentType">Argument type.</param>
    /// <param name="resultType">Result type.</param>
    public ParsedExpressionNumberParserParams(
        ParsedExpressionFactoryInternalConfiguration configuration,
        Type argumentType,
        Type resultType)
    {
        Configuration = configuration;
        ArgumentType = argumentType;
        ResultType = resultType;
    }

    /// <summary>
    /// Underling configuration.
    /// </summary>
    public ParsedExpressionFactoryInternalConfiguration Configuration { get; }

    /// <summary>
    /// Argument type.
    /// </summary>
    public Type ArgumentType { get; }

    /// <summary>
    /// Result type.
    /// </summary>
    public Type ResultType { get; }
}
