using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

/// <summary>
/// Represents information about a construct.
/// </summary>
public readonly struct ParsedExpressionConstructInfo
{
    internal ParsedExpressionConstructInfo(StringSegment symbol, ParsedExpressionConstructType type, object construct)
    {
        Symbol = symbol;
        Type = type;
        Construct = construct;
    }

    /// <summary>
    /// Construct's symbol.
    /// </summary>
    public StringSegment Symbol { get; }

    /// <summary>
    /// Construct's type.
    /// </summary>
    public ParsedExpressionConstructType Type { get; }

    /// <summary>
    /// Construct's instance.
    /// </summary>
    public object Construct { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionConstructInfo"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] '{Symbol}' -> {Construct.GetType().GetDebugString()}";
    }
}
