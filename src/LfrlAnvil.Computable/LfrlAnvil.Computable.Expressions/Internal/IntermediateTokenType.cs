﻿namespace LfrlAnvil.Computable.Expressions.Internal;

internal enum IntermediateTokenType : byte
{
    Argument = 0,
    NumberConstant = 1,
    StringConstant = 2,
    BooleanConstant = 3,
    Constructs = 4,
    OpenedParenthesis = 5,
    ClosedParenthesis = 6,
    InlineFunctionSeparator = 7,
    ElementSeparator = 8,
    MemberAccess = 9
}
