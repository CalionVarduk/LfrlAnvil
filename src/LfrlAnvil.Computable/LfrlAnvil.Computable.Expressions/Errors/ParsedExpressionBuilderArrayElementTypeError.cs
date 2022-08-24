using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderArrayElementTypeError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderArrayElementTypeError(ParsedExpressionBuilderErrorType type, Type elementType, int index)
        : base( type )
    {
        ElementType = elementType;
        Index = index;
    }

    public Type ElementType { get; }
    public int Index { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, element type: {ElementType.FullName}, element index: {Index}";
    }
}
