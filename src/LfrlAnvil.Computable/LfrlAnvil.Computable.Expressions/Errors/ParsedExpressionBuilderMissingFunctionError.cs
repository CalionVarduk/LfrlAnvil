﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderMissingFunctionError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMissingFunctionError(
        ParsedExpressionBuilderErrorType type,
        StringSliceOld token,
        IReadOnlyList<Type> parameterTypes)
        : base( type, token )
    {
        ParameterTypes = parameterTypes;
    }

    public IReadOnlyList<Type> ParameterTypes { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, parameter types: [{string.Join( ", ", ParameterTypes.Select( p => p.GetDebugString() ) )}]";
    }
}
