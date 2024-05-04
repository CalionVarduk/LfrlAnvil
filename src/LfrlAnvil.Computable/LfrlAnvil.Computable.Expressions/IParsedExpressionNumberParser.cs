using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents a type-erased expression's number parser.
/// </summary>
public interface IParsedExpressionNumberParser
{
    /// <summary>
    /// Attempts to parse a number from the provided <paramref name="text"/>.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <param name="result"><b>out</b> parameter that returns the result of parsing.</param>
    /// <returns><b>true</b> when parsing was successful, otherwise <b>false</b>.</returns>
    bool TryParse(StringSegment text, [MaybeNullWhen( false )] out object result);
}
