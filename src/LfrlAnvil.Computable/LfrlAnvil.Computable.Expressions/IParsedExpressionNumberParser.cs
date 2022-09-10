using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Computable.Expressions;

public interface IParsedExpressionNumberParser
{
    bool TryParse(StringSlice text, [MaybeNullWhen( false )] out object result);
}
