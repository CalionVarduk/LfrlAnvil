using System;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Computable.Expressions;

public interface IMathExpressionNumberParser
{
    bool TryParse(ReadOnlySpan<char> text, [MaybeNullWhen( false )] out object result);
}
