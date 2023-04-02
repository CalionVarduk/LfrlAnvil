using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Computable.Expressions;

public interface IParsedExpressionNumberParser
{
    bool TryParse(StringSegment text, [MaybeNullWhen( false )] out object result);
}
