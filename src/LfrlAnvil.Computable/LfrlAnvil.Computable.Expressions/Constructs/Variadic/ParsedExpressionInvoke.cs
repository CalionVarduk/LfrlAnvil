using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

/// <summary>
/// Represents a delegate invocation construct.
/// </summary>
public sealed class ParsedExpressionInvoke : ParsedExpressionVariadicFunction
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionInvoke"/> instance.
    /// </summary>
    /// <param name="foldConstantsWhenPossible">
    /// Specifies whether or not invocations with all parameters being constant and target being constant
    /// should be resolved immediately as constant expression. Equal to <b>true</b> by default.
    /// </param>
    public ParsedExpressionInvoke(bool foldConstantsWhenPossible = true)
    {
        FoldConstantsWhenPossible = foldConstantsWhenPossible;
    }

    /// <summary>
    /// Specifies whether or not invocations with all parameters being constant and target being constant
    /// should be resolved immediately as constant expression.
    /// </summary>
    public bool FoldConstantsWhenPossible { get; }

    /// <inheritdoc />
    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, 1 );

        var target = parameters[0];
        var callParameters = parameters.Slice( 1 );

        return FoldConstantsWhenPossible
            && target is ConstantExpression constantTarget
            && callParameters.All( static p => p is ConstantExpression )
                ? ExpressionHelpers.CreateConstantDelegateInvocation( constantTarget, callParameters )
                : Expression.Invoke( target, callParameters );
    }
}
