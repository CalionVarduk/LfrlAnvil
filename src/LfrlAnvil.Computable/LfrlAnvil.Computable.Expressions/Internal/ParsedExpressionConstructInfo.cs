using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

public readonly struct ParsedExpressionConstructInfo
{
    internal ParsedExpressionConstructInfo(ReadOnlyMemory<char> symbol, ParsedExpressionConstructType type, object construct)
    {
        Symbol = symbol;
        Type = type;
        Construct = construct;
    }

    public ReadOnlyMemory<char> Symbol { get; }
    public ParsedExpressionConstructType Type { get; }
    public object Construct { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] '{Symbol}' -> {Construct.GetType().GetDebugString()}";
    }
}
