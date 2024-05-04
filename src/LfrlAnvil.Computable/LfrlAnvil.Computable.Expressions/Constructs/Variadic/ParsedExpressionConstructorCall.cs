﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

/// <summary>
/// Represents a constructor call construct.
/// </summary>
public sealed class ParsedExpressionConstructorCall : ParsedExpressionVariadicFunction
{
    private readonly ParsedExpressionFactoryInternalConfiguration _configuration;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionConstructorCall"/> instance.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <param name="foldConstantsWhenPossible">
    /// Specifies whether or not constructor invocations with all parameters being constant
    /// should be resolved immediately as constant expression. Equal to <b>true</b> by default.
    /// </param>
    public ParsedExpressionConstructorCall(
        ParsedExpressionFactoryInternalConfiguration configuration,
        bool foldConstantsWhenPossible = true)
    {
        _configuration = configuration;
        FoldConstantsWhenPossible = foldConstantsWhenPossible;
    }

    /// <summary>
    /// Specifies whether or not constructor invocations with all parameters being constant
    /// should be resolved immediately as constant expression.
    /// </summary>
    public bool FoldConstantsWhenPossible { get; }

    /// <inheritdoc />
    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, 1 );

        var type = parameters[0].GetConstantCtorTypeValue();
        var callParameters = parameters.Slice( 1 );
        var parameterTypes = callParameters.GetExpressionTypes();

        var ctor = _configuration.TryFindTypeCtor( type, parameterTypes );
        if ( ctor is null )
            throw new ParsedExpressionUnresolvableMemberException( type, ".ctor", parameterTypes );

        return FoldConstantsWhenPossible && callParameters.All( static p => p is ConstantExpression )
            ? ExpressionHelpers.CreateConstantCtorCall( ctor, callParameters )
            : Expression.New( ctor, callParameters );
    }
}
