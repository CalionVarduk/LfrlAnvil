using System;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

public readonly struct ParsedExpressionUnaryConstructInfo
{
    public ParsedExpressionUnaryConstructInfo(Type constructType, Type argumentType)
    {
        ConstructType = constructType;
        ArgumentType = argumentType;
    }

    public Type ConstructType { get; }
    public Type ArgumentType { get; }

    public override string ToString()
    {
        return $"{ConstructType.GetDebugString()}({ArgumentType.GetDebugString()})";
    }
}
