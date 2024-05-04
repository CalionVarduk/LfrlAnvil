using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

/// <summary>
/// Represents an array creation construct.
/// </summary>
public sealed class ParsedExpressionMakeArray : ParsedExpressionVariadicFunction
{
    /// <inheritdoc />
    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, 1 );

        var elementType = parameters[0].GetConstantArrayElementTypeValue();
        var elements = parameters.Slice( 1 );
        var containsVariableElements = false;

        for ( var i = 0; i < elements.Length; ++i )
        {
            var element = elements[i];
            var actualType = element.Type;

            if ( ! actualType.IsAssignableTo( elementType ) )
                throw new ParsedExpressionInvalidArrayElementException( elementType, actualType );

            if ( element is not ConstantExpression )
                containsVariableElements = true;
        }

        return containsVariableElements
            ? ExpressionHelpers.CreateVariableArray( elementType, elements )
            : ExpressionHelpers.CreateConstantArray( elementType, elements );
    }
}
