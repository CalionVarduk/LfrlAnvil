// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
