using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

/// <summary>
/// Represents an information about a unary construct.
/// </summary>
public readonly struct ParsedExpressionUnaryConstructInfo
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionUnaryConstructInfo"/> instance.
    /// </summary>
    /// <param name="constructType">Construct's type.</param>
    /// <param name="argumentType">Argument's type.</param>
    public ParsedExpressionUnaryConstructInfo(Type constructType, Type argumentType)
    {
        ConstructType = constructType;
        ArgumentType = argumentType;
    }

    /// <summary>
    /// Construct's type.
    /// </summary>
    public Type ConstructType { get; }

    /// <summary>
    /// Argument's type.
    /// </summary>
    public Type ArgumentType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionUnaryConstructInfo"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{ConstructType.GetDebugString()}({ArgumentType.GetDebugString()})";
    }
}
