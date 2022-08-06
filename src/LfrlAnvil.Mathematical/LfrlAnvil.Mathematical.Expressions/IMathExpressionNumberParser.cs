using System;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Mathematical.Expressions;

public interface IMathExpressionNumberParser
{
    bool TryParse(ReadOnlySpan<char> text, [MaybeNullWhen( false )] out object result);
}
